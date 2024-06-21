using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage.Analyzers;

public class HashAnalyzer : ModAnalyzer {
    protected override Task<ModFile> AnalyzeModFile(Stream modFileStream, ModFile modInfo, TmodFile tmodFile) {
        // Skip header (4 bytes) and version (string)
        modFileStream.Position = 4;
        BinaryReader reader = new(modFileStream);
        _ = reader.ReadString();
        
        // Read the SHA-1 hash of the file
        byte[] hash = reader.ReadBytes(20);
        modInfo.FileHash = hash;
        
        return Task.FromResult(modInfo);
    }

    protected override bool ShouldBeAnalyzed(ModFile modInfo) {
        throw new NotImplementedException();
    }
}