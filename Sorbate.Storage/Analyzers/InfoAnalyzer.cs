using System.Text;
using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage.Analyzers;

public class InfoAnalyzer : ModAnalyzer {
    protected override Task<ModFile> AnalyzeModFile(Stream modFileStream, ModFile modInfo, TmodFile tmodFile) {
        foreach (TmodFileEntry entry in tmodFile.Entries) {
            if (entry.Path != "Info") continue;

            TmodFileData data = TmodFile.ProcessModEntry(entry);
            string dataText = Encoding.UTF8.GetString(data.Data.Span);

            string[] lines = dataText.Split('\n');
            foreach (string line in lines) {
                string[] parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
                if (parts.Length != 2) continue;

                string key = parts[0];
                string value = parts[1];
                if (string.Equals(key, "author", StringComparison.OrdinalIgnoreCase))
                    modInfo.Author = value;
            }
            
            break;
        }

        return Task.FromResult(modInfo);
    }

    protected override bool ShouldBeAnalyzed(ModFile modInfo) {
        throw new NotImplementedException();
    }
}