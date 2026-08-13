using Sorbate.Data;

namespace Sorbate.Scraping;

public interface IScraper {
    public string SourceName { get; }
    public Task<IAsyncEnumerable<ModRecord>> ScrapeLatest(CancellationToken token = default);
    public Task<IAsyncEnumerable<ModRecord>> ScrapeHistorical(CancellationToken token = default);
}