using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecretSanta.Domain;
using SecretSanta.Domain.State;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SecretSanta
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<UserState>();
            services.AddScoped(provider => provider.GetRequiredService<UserState>().Auth);
            services.AddScoped(provider => provider.GetRequiredService<UserState>().SantaEvents);
            services.AddScoped<Persistence>();
            services.AddSingleton(_ => new BotWrapper(Configuration.GetValue<string>("DOTNET_BOT_KEY")));

            services.AddHttpsRedirection(opt => { opt.RedirectStatusCode = 301; });

            var temp = Path.GetTempPath();
            var subDirectory = "SecretSanta";
            var fileName = "secretSanta.db";
            var fullPath = Path.Combine(temp, subDirectory, fileName);
            Directory.CreateDirectory(Path.Combine(temp, subDirectory));
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={fullPath};"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }

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