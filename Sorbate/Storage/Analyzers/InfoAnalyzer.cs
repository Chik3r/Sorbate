using System.Text;
using LinqKit;
using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage.Analyzers;

internal class InfoAnalyzer : ModAnalyzer {
    internal override Task<ModFile> AnalyzeModFile(Stream modFileStream, ModFile modInfo, TmodFile tmodFile) {
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
                else if (string.Equals(key, "displayName", StringComparison.OrdinalIgnoreCase))
                    modInfo.DisplayName = value;
            }
            
            break;
        }

        return Task.FromResult(modInfo);
    }

    internal override ExpressionStarter<ModFile> BuildSearchPredicate(ExpressionStarter<ModFile> expression) =>
        expression.Or(f => f.DisplayName == null || f.Author == null);
}