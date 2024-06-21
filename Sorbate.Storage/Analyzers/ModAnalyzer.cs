using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage.Analyzers;

public abstract class ModAnalyzer {
    private static readonly List<ModAnalyzer> analyzers = [];
    
    static ModAnalyzer() {
        foreach (Type type in typeof(ModAnalyzer).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ModAnalyzer)))) {
            ModAnalyzer analyzer = (ModAnalyzer) Activator.CreateInstance(type)!;
            analyzers.Add(analyzer);
        }
    }
    
    public static async Task<ModFile> AnalyzeMod(Stream modFileStream, ModFile modInfo, TmodFile? tmodFile = null) {
        if (tmodFile is null && !TmodFile.TryReadFromStream(modFileStream, out tmodFile)) {
            throw new Exception("Unable to read .tmod file from stream");
        }
        
        foreach (ModAnalyzer analyzer in analyzers)
            modInfo = await analyzer.AnalyzeModFile(modFileStream, modInfo, tmodFile);

        return modInfo;
    }
    
    protected abstract Task<ModFile> AnalyzeModFile(Stream modFileStream, ModFile modInfo, TmodFile tmodFile);

    protected abstract bool ShouldBeAnalyzed(ModFile modInfo);
}