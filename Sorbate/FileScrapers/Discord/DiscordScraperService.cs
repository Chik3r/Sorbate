using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sorbate.FileScrapers.Discord.Data;
using Sorbate.Storage;

namespace Sorbate.FileScrapers.Discord;

public class DiscordScraperService : BackgroundService {
    private readonly ILogger<DiscordScraperService> _logger;
    private readonly StorageHandler _storageHandler;
    private readonly DiscordClient? _discordClient;

    public DiscordScraperService(ILogger<DiscordScraperService> logger, ILogger<DiscordClient> clientLogger, HttpClient client,
        StorageHandler storageHandler, IConfiguration configuration) {
        _logger = logger;
        _storageHandler = storageHandler;

        string? discordAuthToken = configuration.GetSection("Discord")["AuthToken"];
        if (string.IsNullOrWhiteSpace(discordAuthToken)) {
            logger.LogError("Missing discord authorization token");
            return;
        }

        _discordClient = new DiscordClient(clientLogger, client, discordAuthToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        return;
        if (_discordClient is null) {
            _logger.LogInformation("Discord client not initialized, stopping service");
            return;
        }
        
        _discordClient.Start();

        _logger.LogInformation("Starting .tmod file search");
        await foreach (Attachment attachment in _discordClient.SearchForFiles().WithCancellation(stoppingToken)) {
            // process the attachments
            string extension = Path.GetExtension(attachment.Filename);
            if (extension != ".tmod")
                continue;
            
            // Store the attachment
            await using Stream file = await _storageHandler.DownloadModFile(attachment.Url);
            await _storageHandler.StoreModFile(file);
        }
        _logger.LogInformation("Finished .tmod file search");
    }

    public override Task StopAsync(CancellationToken cancellationToken) {
        _discordClient?.Stop();
        
        return base.StopAsync(cancellationToken);
    }
}