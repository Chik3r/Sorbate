using Sorbate.Data;
using Sorbate.Scraping;

namespace Sorbate.Services;

public class StartupBackfillService(IServiceScopeFactory scopeFactory, IConfiguration config) : IHostedService {
    public async Task StartAsync(CancellationToken token) {
        if (!config.GetValue<bool>("Scraping:RunBackfillOnStartup")) {
            return;
        }

        using IServiceScope scope = scopeFactory.CreateScope();
        AppDbContext
            db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // TODO: Replace with DB factory instead

        foreach (IScraper scraper in scope.ServiceProvider.GetServices<IScraper>()) {
            IEnumerable<ModRecord> data = await scraper.ScrapeHistorical(token);
            db.AddRange(data);
        }

        await db.SaveChangesAsync(token);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}