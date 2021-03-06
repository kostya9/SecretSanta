using dotenv.net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecretSanta.Domain;
using SecretSanta.Domain.Data;

namespace SecretSanta
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotEnv.Config();
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
                ctx.Database.EnsureCreated();
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