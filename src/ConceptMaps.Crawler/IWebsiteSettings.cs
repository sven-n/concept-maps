namespace ConceptMaps.Crawler;

/// <summary>
/// Website specific settings for the crawler.
/// </summary>
public interface IWebsiteSettings
{
    /// <summary>
    /// Gets the name of the website.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the base URI. All pages which should be crawled should have this address as base.
    /// </summary>
    Uri BaseUri { get; set; }

    /// <summary>
    /// Gets the URI of the entry point for the crawler.
    /// </summary>
    List<Uri> EntryUris { get; }

    /// <summary>
    /// Gets the URIs which should not be crawled.
    /// </summary>
    ISet<Uri> BlockUris { get; }
}