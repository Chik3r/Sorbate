using Sorbate.Data;

namespace Sorbate.Scraping;

public interface IScraper {
    public string SourceName { get; }
    public Task<IEnumerable<ModRecord>> ScrapeLatest();
    public Task<IEnumerable<ModRecord>> ScrapeHistorical();
}