using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SecretSanta.Domain
{
    public class Persistence
    {
        private readonly SqliteDbContext _dbContext;

        public Persistence(SqliteDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SecretSantaEvent[]> GetEventsFor(string telegramLogin)
        {
            var userMembership = await _dbContext.Memberships.Where(m => m.TelegramLogin == telegramLogin).ToArrayAsync();

            var eventUids = userMembership.Select(m => m.EventUid).ToArray();
            var allEvents = await _dbContext.Events.Where(e => eventUids.Contains(e.Uid)).ToArrayAsync();

            var allEventMemberships = await _dbContext.Memberships.Where(m => eventUids.Contains(m.EventUid)).ToArrayAsync();
            var membershipLookup = allEventMemberships.ToLookup(a => a.EventUid);

            List<SecretSantaEvent> events = new();
            foreach (var persistedSantaEvent in allEvents)
            {
                var members = membershipLookup[persistedSantaEvent.Uid].ToArray();
                var mappedMembers = members.Select(m => new SecretSantaMember(m.Name, m.TelegramLogin)).ToArray();
                var loginMapping = mappedMembers.ToDictionary(m => m.TelegramLogin, m => m, StringComparer.OrdinalIgnoreCase);
                var opponentMapping = members.ToDictionary(m => loginMapping[m.TelegramLogin],
                    m => loginMapping[m.OpponentTelegramLogin]);
                

                var santaEvent = new SecretSantaEvent(persistedSantaEvent.Uid, persistedSantaEvent.Name, mappedMembers, opponentMapping);
                events.Add(santaEvent);
            }

            return events.ToArray();
        }
        
        public async Task SaveEvent(SecretSantaEvent santaEvent)
        {
            await _dbContext.Events.AddAsync(new SqliteDbContext.PersistedSantaEvent()
            {
                Name = santaEvent.Name,
                Uid = santaEvent.Uid
            });

            var mappings = santaEvent.LoginMappings;
            foreach (var member in santaEvent.TelegramUsers)
            {
                await _dbContext.Memberships.AddAsync(new SqliteDbContext.PersistedSantaEventMembership()
                {
                    Name = member.Name,
                    EventUid = santaEvent.Uid,
                    TelegramLogin = member.TelegramLogin,
                    OpponentTelegramLogin = mappings[member.TelegramLogin]
                });
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}