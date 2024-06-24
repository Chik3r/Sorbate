using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sorbate.DiscordBot.Data;
using Sorbate.Storage;

namespace Sorbate.DiscordBot;

public class DiscordService : BackgroundService {
    private readonly ILogger<DiscordService> _logger;
    private readonly StorageHandler _storageHandler;
    private readonly DiscordClient? _discordClient;

    public DiscordService(ILogger<DiscordService> logger, ILogger<DiscordClient> clientLogger, HttpClient client,
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
        if (_discordClient is null) {
            _logger.LogInformation("Discord client not initialized, stopping service");
            return;
        }
        
        _discordClient.Start();

        _logger.LogInformation("Starting .tmod file search");
        await foreach (Attachment attachment in _discordClient.SearchForFiles().WithCancellation(stoppingToken)) {
            // process the attachments
            string extension = Path.GetExtension(attachment.Filename);
            if (extension != ".tmod") // TODO: This should be a constant that can be easily swapped globally
                continue;
            
            // TODO: Ignore file if over a certain size (e.g. 300MB) and warn
            
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