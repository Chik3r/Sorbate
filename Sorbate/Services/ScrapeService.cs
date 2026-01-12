using Sorbate.Data;
using Sorbate.Scraping;

namespace Sorbate.Services;

public class ScrapeService : BackgroundService {
    private readonly IEnumerable<IScraper> _scrapers;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval;

    public ScrapeService(IEnumerable<IScraper> scrapers, IServiceScopeFactory scopeFactory, IConfiguration config) {
        _scrapers = scrapers;
        _scopeFactory = scopeFactory;
        _interval = TimeSpan.FromSeconds(config.GetValue<int>("Scraping:IntervalSeconds"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunLatest();
            await Task.Delay(_interval, stoppingToken);
        }
    }
    
    public async Task RunLatest() {
        using IServiceScope scope = _scopeFactory.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // TODO: Replace with DB factory instead
        
        foreach (IScraper scraper in _scrapers) {
            IEnumerable<ModRecord> data = await scraper.ScrapeLatest();
            db.AddRange(data);
        }
        
        await db.SaveChangesAsync();
    }
    
    public async Task RunHistorical() {
        using IServiceScope scope = _scopeFactory.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // TODO: Replace with DB factory instead
        
        foreach (IScraper scraper in _scrapers) {
            IEnumerable<ModRecord> data = await scraper.ScrapeHistorical();
            db.AddRange(data);
        }
        
        await db.SaveChangesAsync();
    }
}