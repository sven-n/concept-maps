namespace ConceptMaps.Crawler;

/// <summary>
/// Interface for a crawler implementation.
/// </summary>
public interface ICrawler
{
    /// <summary>
    /// Crawls the website and writes the results into the text writers.
    /// </summary>
    /// <param name="settings">The settings for the website which should be crawled.</param>
    /// <param name="contentWriter">The text writer where the content of the page is written to.</param>
    /// <param name="relationsWriter">The text writer to which the relationships between persons are written to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CrawlAsync(WebsiteSettings settings, TextWriter contentWriter, TextWriter relationsWriter, CancellationToken cancellationToken = default);
}