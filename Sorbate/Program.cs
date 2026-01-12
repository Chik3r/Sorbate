using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sorbate.Data;
// using Sorbate.FileScrapers.Discord;
// using Sorbate.FileScrapers.ModBrowser;
using Sorbate.Scraping;
using Sorbate.Services;
// using Sorbate.Storage;
// using Sorbate.Storage.Analyzers;
// using Sorbate.Storage.Models;

namespace Sorbate;

class Program {
    private static async Task Main(string[] args) {
        // HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        //
        // builder.Services.AddLogging();
        // builder.Services.AddHttpClient();
        //
        // string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
        //                           throw new Exception("Missing connection string");
        // builder.Services.AddDbContextFactory<StorageContext>(options => options.UseNpgsql(connectionString));
        //
        // builder.Services.AddHostedService<DiscordScraperService>();
        // builder.Services.AddHostedService<ModBrowserScraperService>();
        // builder.Services.AddSingleton<AnalyzerService>()
        //     .AddHostedService<AnalyzerService>(provider => provider.GetService<AnalyzerService>()!);
        // builder.Services.AddSingleton<StorageHandler>();
        //
        // using IHost host = builder.Build();
        // await host.RunAsync();

        var builder = WebApplication.CreateBuilder(args);
        
        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                                  throw new Exception("Missing connection string");
        // TODO: use DB factory instead
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        builder.Services.AddHttpClient();
        
        builder.Services.AddScoped<IScraper, SteamScraper>();
        builder.Services.AddScoped<ScrapeService>();

        builder.Services.AddHostedService<StartupBackfillService>();
        builder.Services.AddHostedService<ScrapeService>();

        builder.Services.AddControllers();

        ///////
        
        WebApplication app = builder.Build();
        
        // TODO: Do migrations in CLI instead of here
        // using IServiceScope scope = app.Services.CreateScope();
        // AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // await db.Database.MigrateAsync();
        
        app.MapControllers();
        
        await app.RunAsync();
    }
}