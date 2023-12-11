using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SecretSanta.Domain.Data;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SecretSanta.Domain;

public class BotWrapper : IDisposable
{
    private TelegramBotClient _bot;
    private readonly CancellationTokenSource _stopCts;
    private readonly string _botKey;
    private readonly Persistence _persistence;
    private readonly ILogger<BotWrapper> _log;

    public BotWrapper(string botKey, Persistence persistence, ILogger<BotWrapper> log)
    {
        _botKey = botKey;
        _persistence = persistence;
        _log = log;
        _bot = new TelegramBotClient(botKey);
        _stopCts = new CancellationTokenSource();
    }

    public void StartReceivingMessages()
    {
        var updateReceiver = new QueuedUpdateReceiver(_bot, new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }
        });

        Task.Run(async () =>
        {
            await foreach (var message in updateReceiver.WithCancellation(_stopCts.Token))
            {
                await OnNewTelegramMessage(message);
            }
        }, _stopCts.Token);
    }

    private async Task OnNewTelegramMessage(Update update)
    {
        var message = update.Message;

        if (message is null)
        {
            _log.LogWarning("Message is null for @{update}", update);
            return;
        }

        var chatId = new ChatId(message.Chat.Id);
        if (message.Text == "/start")
        {
            _bot.SendTextMessageAsync(chatId,
                "Write /whoamisantafor to find who to buy presents for!").GetAwaiter().GetResult();
        }

        if (message.Text == "/whoamisantafor")
        {
            if (message.From is null)
            {
                _log.LogWarning("From is null for @{update}", update);
                return;
            }

            var events = _persistence.GetEventsFor(message.From.Username).GetAwaiter().GetResult();

            var sb = new StringBuilder();

            sb.AppendLine("----- Current ----");

            foreach (var santaEvent in events.Where(e => !e.Archived))
            {
                var opponent = santaEvent.GetOpponentFor(message.From.Username);

                if (opponent is null)
                {
                    _log.LogWarning("opponent is null for @{update}", update);
                    return;
                }

                sb.AppendLine(
                    $"For event '{santaEvent.Name}', buy a present for {opponent.Name} (@{opponent.TelegramLogin})!");
            }

            var archived = events.Where(e => e.Archived).ToArray();
            if (archived.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine("----- Archived (old) ----");

                foreach (var archivedEvent in archived)
                {
                    var opponent = archivedEvent.GetOpponentFor(message.From.Username);

                    if (opponent is null)
                    {
                        _log.LogWarning("opponent is null for @{update}", update);
                        return;
                    }

                    sb.AppendLine(
                        $"For event '{archivedEvent.Name}', you bought a present for {opponent.Name} (@{opponent.TelegramLogin})!");
                }
            }

            if (events.Length == 0)
            {
                sb.AppendLine("Sorry, I do not know any events for you.");
            }

            await _bot.SendTextMessageAsync(chatId, sb.ToString());
            _log.LogInformation("Sent a message to @{username}", message.Chat.Username);
        }
    }

    public async Task<bool> UserExists(string login)
    {
        var chat = await _bot.GetChatAsync(new ChatId(login));

        return chat != null;
    }

    public bool IsValidPayload(JsonElement receivedTelegramInfo)
    {
        List<string> fields = new();

        foreach (var prop in receivedTelegramInfo.EnumerateObject())
        {
            if (prop.Name != "hash")
            {
                fields.Add($"{prop.Name}={prop.Value}");
            }
        }

        fields.Sort();

        var hashedKey = SHA256.HashData(Encoding.UTF8.GetBytes(_botKey));
        var hashAlgorithm = new HMACSHA256(hashedKey);

        var joinedData = string.Join("\n", fields);
        var dataBytes = Encoding.UTF8.GetBytes(joinedData);
        var hashedFields = hashAlgorithm.ComputeHash(dataBytes);
        var hexData = BitConverter.ToString(hashedFields).Replace("-", string.Empty).ToLower();

        var receivedHash = receivedTelegramInfo.GetProperty("hash").GetString();

        return receivedHash == hexData;
    }

    public void Dispose()
    {
        if (!_stopCts.IsCancellationRequested)
        {
            _stopCts.Cancel();
        }
    }
}