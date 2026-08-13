using Sorbate.Data;
using Sorbate.Scraping;

namespace Sorbate.Services;

public class BackgroundScrapeService(IServiceScopeFactory scopeFactory, IConfiguration config, IStorage storage) : BackgroundService {
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(config.GetValue<int>("Scraping:IntervalSeconds"));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await RunLatest(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }
    }

    public async Task RunLatest(CancellationToken token = default) {
        using IServiceScope scope = scopeFactory.CreateScope();

        foreach (IScraper scraper in scope.ServiceProvider.GetServices<IScraper>()) {
            IAsyncEnumerable<ModRecord> data = await scraper.ScrapeLatest(token);
            await storage.UploadRange(data);
        }
    }
}