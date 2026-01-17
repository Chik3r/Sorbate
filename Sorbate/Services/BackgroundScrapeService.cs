using Sorbate.Data;
using Sorbate.Scraping;

namespace Sorbate.Services;

public class BackgroundScrapeService(IServiceScopeFactory scopeFactory, IConfiguration config) : BackgroundService {
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(config.GetValue<int>("Scraping:IntervalSeconds"));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await RunLatest(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    public async Task RunLatest(CancellationToken token = default) {
        using IServiceScope scope = scopeFactory.CreateScope();
        AppDbContext
            db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // TODO: Replace with DB factory instead

        foreach (IScraper scraper in scope.ServiceProvider.GetServices<IScraper>()) {
            IEnumerable<ModRecord> data = await scraper.ScrapeLatest(token);
            List<ModRecord> records = data.ToList();
            
            // TODO: Properly store the ModRecord, write the file to where it should go, give the file a name, etc
            foreach (ModRecord record in records) {
                record.FileName = "test_aaa.tmod";
            }
            
            db.AddRange(records);
        }

        await db.SaveChangesAsync(token);
    }
}