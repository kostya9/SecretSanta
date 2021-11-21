using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SecretSanta.Domain.Data
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<PersistedSantaEventMembership> Memberships { get; set; } = null!;

        public DbSet<PersistedSantaEvent> Events { get; set; } = null!;

        public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            var eventTable = modelBuilder.Entity<PersistedSantaEvent>().ToTable("santa_event");
            eventTable.HasKey(e => e.Uid);
            eventTable.Property(p => p.Name).HasColumnName("name");
            eventTable.Property(p => p.Uid).HasColumnName("uid");
            eventTable.Property(p => p.Metadata).HasColumnName("metadata")
                .HasConversion(
                    m => m.ToString(),
                    s => JsonDocument.Parse(string.IsNullOrWhiteSpace(s) ? "{}": s, new JsonDocumentOptions()));

            var membershipTable = modelBuilder.Entity<PersistedSantaEventMembership>().ToTable("santa_event_membership");
            membershipTable.HasKey(m => new {m.EventUid, m.TelegramLogin});
            membershipTable.HasIndex(m => new {m.TelegramLogin, m.EventUid});
            membershipTable.Property(m => m.Name).HasColumnName("name");
            membershipTable.Property(m => m.EventUid).HasColumnName("event_uid");
            membershipTable.Property(m => m.TelegramLogin).HasColumnName("telegram_login");
            membershipTable.Property(m => m.OpponentTelegramLogin).HasColumnName("opponent_telegram_login");

            base.OnModelCreating(modelBuilder);
        }

        public class PersistedSantaEventMembership
        {
            public string EventUid { get; set; }

            public string TelegramLogin { get; set; }
            
            public string OpponentTelegramLogin { get; set; }

            public string Name { get; set; }
        }
        
        public class PersistedSantaEvent
        {
            public string Uid { get; set; }

            public string Name { get; set; }

            public JsonDocument Metadata { get; set; }
        }
    }
}