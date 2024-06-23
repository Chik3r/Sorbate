using LinqKit;
using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage.Analyzers;

internal abstract class ModAnalyzer {
    internal abstract Task<ModFile> AnalyzeModFile(Stream modFileStream, ModFile modInfo, TmodFile tmodFile);

    internal abstract ExpressionStarter<ModFile> BuildSearchPredicate(ExpressionStarter<ModFile> expression);
}