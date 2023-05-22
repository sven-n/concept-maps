namespace ConceptMaps.Crawler;

using System.Collections.Immutable;

/// <summary>
/// Website specific settings for the crawler.
/// </summary>
public class WebsiteSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebsiteSettings"/> class.
    /// </summary>
    /// <param name="entryUri">The URI of the entry point for the crawler.</param>
    /// <param name="baseUri">The base URI. All pages which should be crawled should have this address as base.</param>
    /// <param name="blockUris">The URIs which should not be crawled.</param>
    public WebsiteSettings(string entryUri, string baseUri, IEnumerable<string> blockUris)
    {
        EntryUri = new Uri(entryUri);
        BaseUri = new Uri(baseUri);
        BlockUris = blockUris.Select(u => Uri.IsWellFormedUriString(u, UriKind.Absolute) ? new Uri(u) : new Uri(BaseUri, u)).ToImmutableHashSet();
    }

    /// <summary>
    /// Gets the URI of the entry point for the crawler.
    /// </summary>
    public Uri EntryUri { get; }

    /// <summary>
    /// Gets the base URI. All pages which should be crawled should have this address as base.
    /// </summary>
    public Uri BaseUri { get; }

    /// <summary>
    /// Gets the URIs which should not be crawled.
    /// </summary>
    public IReadOnlySet<Uri> BlockUris { get; }
}