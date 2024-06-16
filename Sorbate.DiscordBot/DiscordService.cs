using Microsoft.Extensions.Hosting;
using Sorbate.DiscordBot.Data;

namespace Sorbate.DiscordBot;

public class DiscordService : BackgroundService {
    private readonly DiscordClient _discordClient = new();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _discordClient.Start();

        await foreach (Attachment attachment in _discordClient.SearchForFiles().WithCancellation(stoppingToken)) {
            // process the attachments
        }
        
        // We have finished processing all attachments
        // Now we simply run in the background processing any incoming websocket messages (in the DiscordClient)
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(1000, stoppingToken);
        }
            
        _discordClient.Stop();
    }
}