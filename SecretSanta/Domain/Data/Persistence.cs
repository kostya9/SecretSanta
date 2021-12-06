using Microsoft.EntityFrameworkCore;

namespace SecretSanta.Domain.Data;

public class Persistence
{
    private readonly IDbContextFactory<SqliteDbContext> _dbContextFactory;

    public Persistence(IDbContextFactory<SqliteDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<SecretSantaEvent[]> GetEventsFor(string? telegramLogin)
    {
        if (string.IsNullOrWhiteSpace(telegramLogin))
            return Array.Empty<SecretSantaEvent>();

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var userMembership = await dbContext.Memberships.AsNoTracking().Where(m => m.TelegramLogin == telegramLogin).ToArrayAsync();

        var eventUids = userMembership.Select(m => m.EventUid).ToArray();
        var allEvents = await dbContext.Events.AsNoTracking().Where(e => eventUids.Contains(e.Uid)).ToArrayAsync();

        var allEventMemberships = await dbContext.Memberships.AsNoTracking().Where(m => eventUids.Contains(m.EventUid)).ToArrayAsync();
        var membershipLookup = allEventMemberships.ToLookup(a => a.EventUid);

        List<SecretSantaEvent> events = new();
        foreach (var persistedSantaEvent in allEvents)
        {
            var members = membershipLookup[persistedSantaEvent.Uid].ToArray();
            var mappedMembers = members.Select(m => new SecretSantaMember(m.Name, m.TelegramLogin)).ToArray();
            var loginMapping = mappedMembers.ToDictionary(m => m.TelegramLogin, m => m, StringComparer.OrdinalIgnoreCase);
            var opponentMapping = members.ToDictionary(m => loginMapping[m.TelegramLogin],
                m => loginMapping[m.OpponentTelegramLogin]);
            var metadata = persistedSantaEvent.Metadata;

            var owner = mappedMembers.First(m => m.TelegramLogin == persistedSantaEvent.OwnerId);

            var santaEvent = new SecretSantaEvent(persistedSantaEvent.Uid,
                persistedSantaEvent.Name,
                mappedMembers,
                opponentMapping,
                owner,
                persistedSantaEvent.Archived,
                new(metadata));
            events.Add(santaEvent);
        }

        return events.ToArray();
    }

    public async Task SaveArchived(SecretSantaEvent santaEvent)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        var persisted = await dbContext.Events.FindAsync(santaEvent.Uid);

        persisted.Archived = santaEvent.Archived;

        await dbContext.SaveChangesAsync();
    }

    public async Task SaveEvent(SecretSantaEvent santaEvent)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        await dbContext.Events.AddAsync(new SqliteDbContext.PersistedSantaEvent()
        {
            Name = santaEvent.Name,
            Uid = santaEvent.Uid,
            Metadata = santaEvent.Metadata.Content
        });

        var opponents = santaEvent.Opponents;
        foreach (var member in santaEvent.TelegramUsers)
        {
            await dbContext.Memberships.AddAsync(new SqliteDbContext.PersistedSantaEventMembership()
            {
                Name = member.Name,
                EventUid = santaEvent.Uid,
                TelegramLogin = member.TelegramLogin,
                OpponentTelegramLogin = opponents[member.TelegramLogin]
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
