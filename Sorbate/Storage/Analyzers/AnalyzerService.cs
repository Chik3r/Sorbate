using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage.Analyzers;

public class AnalyzerService : BackgroundService {
    private readonly List<ModAnalyzer> _analyzers = [];
    private readonly ILogger _logger;
    private readonly IDbContextFactory<StorageContext> _dbFactory;

    public AnalyzerService(ILogger<AnalyzerService> logger, IDbContextFactory<StorageContext> dbFactory) {
        _logger = logger;
        _dbFactory = dbFactory;
        
        foreach (Type type in typeof(ModAnalyzer).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ModAnalyzer)))) {
            ModAnalyzer analyzer = (ModAnalyzer) Activator.CreateInstance(type)!;
            _analyzers.Add(analyzer);
        }
    }
    
    public async Task<ModFile> AnalyzeMod(Stream modFileStream, ModFile modInfo, TmodFile? tmodFile = null) {
        if (tmodFile is null && !TmodFile.TryReadFromStream(modFileStream, out tmodFile)) {
            _logger.LogWarning("Unable to read .tmod file from stream");
            return modInfo;
        }
        
        _logger.LogDebug("Analyzing mod with name '{ModName}'", modInfo.InternalModName);
        foreach (ModAnalyzer analyzer in _analyzers)
            modInfo = await analyzer.AnalyzeModFile(modFileStream, modInfo, tmodFile);

        return modInfo;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Searching for mods missing metadata");
        await using StorageContext db = await _dbFactory.CreateDbContextAsync(stoppingToken);

        // Build a sequence of where queries that are OR'ed together
        ExpressionStarter<ModFile>? predicate = PredicateBuilder.New<ModFile>(false);
        foreach (ModAnalyzer analyzer in _analyzers) predicate = analyzer.BuildSearchPredicate(predicate);
        
        foreach (ModFile source in db.Files.AsExpandable().Where(predicate)) {
            string path = Path.Combine(StorageHandler.ModFileDirectory, source.Id.ToString());
            await using Stream fileStream = File.OpenRead(path);
            ModFile info = await AnalyzeMod(fileStream, source);
            
            db.Files.Update(info);
        }

        await db.SaveChangesAsync(stoppingToken);
    }
}