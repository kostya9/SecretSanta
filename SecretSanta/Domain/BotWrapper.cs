using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecretSanta.Domain.Data;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SecretSanta.Domain
{
    public class BotWrapper : IDisposable
    {
        private TelegramBotClient _bot;
        private readonly string _botKey;
        private readonly Persistence _persistence;
        private readonly ILogger<BotWrapper> _log;

        public BotWrapper(string botKey, Persistence persistence, ILogger<BotWrapper> log)
        {
            _botKey = botKey;
            _persistence = persistence;
            _log = log;
            _bot = new TelegramBotClient(botKey);
        }

        public void StartReceivingMessages()
        {
            if(_bot.IsReceiving)
                return;
            
            _bot.OnMessage += OnNewTelegramMessage;
            _bot.StartReceiving(new[] {UpdateType.Message});
        }

        private void OnNewTelegramMessage(object sender, MessageEventArgs args)
        {
            var chatId = new ChatId(args.Message.Chat.Id);
            if (args.Message.Text == "/start")
            {
                _bot.SendTextMessageAsync(chatId,
                    "Write /whoamisantafor to find who to buy presents for!").GetAwaiter().GetResult();
            }

            if (args.Message.Text == "/whoamisantafor")
            {
                var events = _persistence.GetEventsFor(args.Message.From.Username).GetAwaiter().GetResult();

                var sb = new StringBuilder();

                foreach (var santaEvent in events)
                {
                    var opponent = santaEvent.GetFor(args.Message.From.Username);
                    sb.AppendLine(
                        $"For event '{santaEvent.Name}', buy a present for {opponent.Name} (@{opponent.TelegramLogin})!");
                }

                if (events.Length == 0)
                {
                    sb.AppendLine("Sorry, I do not know any events for you.");
                }
                
                _bot.SendTextMessageAsync(chatId, sb.ToString()).GetAwaiter().GetResult();
                _log.LogInformation("Sent a message to @{username}", args.Message.Chat.Username);
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
            if (_bot.IsReceiving)
            {
                _bot.StopReceiving();
                _bot.OnMessage -= OnNewTelegramMessage;
            }
        }
    }
}