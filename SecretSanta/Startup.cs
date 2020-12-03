using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecretSanta.Domain;
using SecretSanta.Domain.Data;
using SecretSanta.Domain.State;

namespace SecretSanta
{
    public class Startup
    {
        private readonly IHostEnvironment _hostEnvironment;

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
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
            services.AddSingleton(p => new BotWrapper(Configuration.GetValue<string>("DOTNET_BOT_KEY"),
                p.GetRequiredService<Persistence>(), p.GetRequiredService<ILogger<BotWrapper>>()));

            services.AddHttpsRedirection(opt => { opt.RedirectStatusCode = 301; });

            var rootFolder =
                _hostEnvironment.IsDevelopment()
                    ? Path.GetTempPath()
                    : Configuration.GetValue<string>("DOTNET_SQLITE_DB_PATH");
            var subDirectory = "SecretSanta";
            var fileName = "secretSanta.db";
            var fullPath = Path.Combine(rootFolder, subDirectory, fileName);
            Directory.CreateDirectory(Path.Combine(rootFolder, subDirectory));
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
}