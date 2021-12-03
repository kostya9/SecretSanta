using dotenv.net;
using Microsoft.EntityFrameworkCore;
using SecretSanta.Domain;
using SecretSanta.Domain.Data;

namespace SecretSanta
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotEnv.Load();
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
                ctx.Database.Migrate();
            }

            using var bot = host.Services.GetRequiredService<BotWrapper>();
            if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                bot.StartReceivingMessages();
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}