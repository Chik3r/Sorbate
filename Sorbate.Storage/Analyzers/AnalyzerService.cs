using LinqKit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage.Analyzers;

public class AnalyzerService : BackgroundService {
    private readonly List<ModAnalyzer> _analyzers = [];
    private readonly ILogger _logger;

    public AnalyzerService(ILogger<AnalyzerService> logger) {
        _logger = logger;
        
        foreach (Type type in typeof(ModAnalyzer).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ModAnalyzer)))) {
            ModAnalyzer analyzer = (ModAnalyzer) Activator.CreateInstance(type)!;
            _analyzers.Add(analyzer);
        }
    }
    
    public async Task<ModFile> AnalyzeMod(Stream modFileStream, ModFile modInfo, TmodFile? tmodFile = null) {
        if (tmodFile is null && !TmodFile.TryReadFromStream(modFileStream, out tmodFile)) {
            throw new Exception("Unable to read .tmod file from stream");
        }
        
        foreach (ModAnalyzer analyzer in _analyzers)
            modInfo = await analyzer.AnalyzeModFile(modFileStream, modInfo, tmodFile);

        return modInfo;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await using StorageContext db = new();

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