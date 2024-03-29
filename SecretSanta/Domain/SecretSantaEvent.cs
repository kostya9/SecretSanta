﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace SecretSanta.Domain;

public record SecretSantaMember(string Name, string TelegramLogin);

public record MaxPrice(int Value, string Currency);

public record Metadata(JsonDocument Content)
{
    public bool TryGetPrice([NotNullWhen(true)] out MaxPrice? price)
    {
        if (Content.RootElement.TryGetProperty(MaxPriceKey, out var maxPrice))
        {
            price = maxPrice.Deserialize<MaxPrice>()!;
            return true;
        }

        price = null;
        return false;
    }

    public static string MaxPriceKey => "maxPrice";
};

public class SecretSantaEvent
{
    private readonly SecretSantaMember[] _users;
    private readonly Dictionary<string, SecretSantaMember> _opponents;

    public SecretSantaMember Owner { get; }

    public IEnumerable<SecretSantaMember> TelegramUsers => _users.ToArray();

    public IReadOnlyDictionary<string, string> Opponents =>
        _opponents.ToDictionary(m => m.Key, m => m.Value.TelegramLogin);

    public string Uid { get; }

    public string Name { get; }

    public Metadata Metadata { get; }

    public bool Archived { get; private set; }

    public SecretSantaEvent(string uid, string name, SecretSantaMember[] members,
        Dictionary<SecretSantaMember, SecretSantaMember> mapping, SecretSantaMember owner, bool archived, Metadata metadata)
    {
        _users = members;
        _opponents =
            mapping.ToDictionary(m => m.Key.TelegramLogin, m => m.Value, StringComparer.OrdinalIgnoreCase);
        Uid = uid;
        Name = name;
        Metadata = metadata;
        Owner = owner;
        Archived = archived;
    }

    public SecretSantaMember? GetOpponentFor(string? username)
    {
        if (username == null)
            return null;

        return _opponents.GetValueOrDefault(username);
    }

    public void SetArchived(bool value)
    {
        Archived = value;
    }

    public bool IsOwner(string userId)
    {
        return Owner.TelegramLogin.Equals(userId, StringComparison.OrdinalIgnoreCase);
    }

    public static SecretSantaEvent Create(string name, IList<SecretSantaMember> members, Metadata metadata)
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
                var owner = members.First();
                return new(Guid.NewGuid().ToString(), name, members.ToArray(), mapping, owner, archived: false, metadata);
            }
        }
    }
}
