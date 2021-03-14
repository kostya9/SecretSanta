using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Domain
{
    public record SecretSantaMember(string Name, string TelegramLogin);

    public class SecretSantaEvent
    {
        private readonly SecretSantaMember[] _users;
        private readonly Dictionary<string, SecretSantaMember> _opponents;

        public IEnumerable<SecretSantaMember> TelegramUsers => _users.ToArray();

        public IReadOnlyDictionary<string, string> Opponents =>
            _opponents.ToDictionary(m => m.Key, m => m.Value.TelegramLogin);

        public string Uid { get; }
        public string Name { get; }

        public SecretSantaEvent(string uid, string name, SecretSantaMember[] members,
            Dictionary<SecretSantaMember, SecretSantaMember> mapping)
        {
            _users = members;
            _opponents =
                mapping.ToDictionary(m => m.Key.TelegramLogin, m => m.Value, StringComparer.OrdinalIgnoreCase);
            Uid = uid;
            Name = name;
        }

        public SecretSantaMember? GetOpponentFor(string? username)
        {
            if (username == null)
                return null;

            return _opponents.GetValueOrDefault(username);
        }

        public static SecretSantaEvent Create(string name, IList<SecretSantaMember> members)
        {
            Random r = new();
            while (true)
            {
                var candidateToCandidateNumber = Enumerable.Range(0, members.Count).OrderBy(i => r.Next()).ToArray();

                bool successful = true;
                for (var i = 0; i < candidateToCandidateNumber.Length; i++)
                {
                    var j = candidateToCandidateNumber[i];
                    if (i == j)
                    {
                        successful = false;
                        break;
                    }
                }

                if (successful)
                {
                    var mapping = candidateToCandidateNumber.Select((i, j) => (i, j))
                        .ToDictionary(pair => members[pair.i], pair => members[pair.j]);
                    return new(Guid.NewGuid().ToString(), name, members.ToArray(), mapping);
                }
            }
        }
    }
}