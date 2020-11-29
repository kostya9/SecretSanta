using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SecretSanta.Domain
{
    public class BotWrapper
    {
        private readonly TelegramBotClient _bot;
        private readonly string _botKey;

        public BotWrapper(string botKey)
        {
            _botKey = botKey;
            _bot = new TelegramBotClient(botKey);
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
    }
}