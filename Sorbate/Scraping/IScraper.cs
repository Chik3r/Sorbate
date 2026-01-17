using Sorbate.Data;

namespace Sorbate.Scraping;

public interface IScraper {
    public string SourceName { get; }
    public Task<IEnumerable<ModRecord>> ScrapeLatest(CancellationToken token = default);
    public Task<IEnumerable<ModRecord>> ScrapeHistorical(CancellationToken token = default);
}