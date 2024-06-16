using Microsoft.Extensions.Hosting;
using Sorbate.DiscordBot.Data;
using Sorbate.Storage;

namespace Sorbate.DiscordBot;

public class DiscordService : BackgroundService {
    private readonly DiscordClient _discordClient;
    private readonly StorageHandler _storageHandler;

    public DiscordService(HttpClient client, StorageHandler storageHandler) {
        _discordClient = new DiscordClient(client);
        _storageHandler = storageHandler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _discordClient.Start();

        await foreach (Attachment attachment in _discordClient.SearchForFiles().WithCancellation(stoppingToken)) {
            // process the attachments
            string extension = Path.GetExtension(attachment.Filename);
            if (extension != ".tmod") // TODO: This should be a constant that can be easily swapped globally
                continue;
            
            // TODO: Ignore file if over a certain size (e.g. 300MB) and warn
            
            // Store the attachment
            Stream file = await _storageHandler.DownloadModFile(attachment.Url);
            await _storageHandler.StoreModFile(file);
        }
        
        // We have finished processing all attachments
        // Now we simply run in the background processing any incoming websocket messages (in the DiscordClient)
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(1000, stoppingToken);
        }
            
        _discordClient.Stop();
    }
}