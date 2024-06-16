using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sorbate.DiscordBot;
using Sorbate.Storage.Models;

namespace Sorbate;

class Program {
    private static async Task Main(string[] args) {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Services.AddLogging();
        builder.Services.AddHostedService<DiscordService>();

        using IHost host = builder.Build();
        await host.RunAsync();
    }
}