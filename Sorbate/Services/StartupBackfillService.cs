using Microsoft.Extensions.Hosting;
using Sorbate.Scraping;

namespace Sorbate.Services;

public class StartupBackfillService : IHostedService {
    private readonly ScrapeService _scrapeService;
    private readonly IConfiguration _config;

    public StartupBackfillService(ScrapeService scrapeService, IConfiguration config) {
        _scrapeService = scrapeService;
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        if (_config.GetValue<bool>("Scraping:RunBackfillOnStartup")) 
            await _scrapeService.RunHistorical();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}