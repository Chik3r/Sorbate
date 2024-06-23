using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sorbate.DiscordBot;
using Sorbate.Storage;
using Sorbate.Storage.Analyzers;
using Sorbate.Storage.Models;

namespace Sorbate;

class Program {
    private static async Task Main(string[] args) {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, false)
            .AddUserSecrets(typeof(Program).Assembly)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        builder.Services.AddLogging();
        builder.Services.AddHttpClient();

        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                                  throw new Exception("Missing connection string");
        builder.Services.AddDbContextFactory<StorageContext>(options => options.UseNpgsql(connectionString));

        builder.Services.AddHostedService<DiscordService>();
        builder.Services.AddSingleton<AnalyzerService>()
            .AddHostedService<AnalyzerService>(provider => provider.GetService<AnalyzerService>()!);
        builder.Services.AddSingleton<StorageHandler>();

        using IHost host = builder.Build();
        await host.RunAsync();
    }
}