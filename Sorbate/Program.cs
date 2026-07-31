using Microsoft.EntityFrameworkCore;
using Sorbate.Data;
using Sorbate.Scraping;
using Sorbate.Services;

namespace Sorbate;

class Program {
    private static async Task Main(string[] args) {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                                  throw new Exception("Missing connection string");
        // TODO: use DB factory instead
        builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IScraper, SteamScraper>();
        builder.Services.AddSingleton<IStorage, StorageHandler>();

        builder.Services.AddHostedService<BackgroundScrapeService>();
        builder.Services.AddHostedService<StartupBackfillService>();

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