namespace ConceptMaps.Crawler;

using Microsoft.Extensions.DependencyInjection;
using System.Text;

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
    /// <param name="progress">The progress callback object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task CrawlAsync(IWebsiteSettings settings, TextWriter contentWriter, TextWriter relationsWriter, IProgress<string>? progress = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Extension methods for <see cref="ICrawler"/>.
/// </summary>
public static class CrawlerExtensions
{
    /// <summary>
    /// Crawls the website and writes the results into the text writers.
    /// </summary>
    /// <param name="crawler">The crawler.</param>
    /// <param name="settings">The settings for the website which should be crawled.</param>
    /// <param name="textFilePath">The target text file path.</param>
    /// <param name="relationshipFilePath">The target relationship file path.</param>
    /// <param name="progress">The progress callback object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static async ValueTask CrawlAsync(this ICrawler crawler, IWebsiteSettings settings, string textFilePath, string relationshipFilePath, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        await using var contentWriter = CreateUtf8StreamWriter(textFilePath);
        await using var relationsWriter = CreateUtf8StreamWriter(relationshipFilePath);
        await crawler.CrawlAsync(settings, contentWriter, relationsWriter, progress, cancellationToken);
    }

    public static IServiceCollection AddCrawler(this IServiceCollection serviceCollection)
    {
        var relationshipExtractorFactory = new RelationshipExtractorFactory();
        relationshipExtractorFactory.Register(new FandomWithDataSourceAttributesRelationshipExtractor());
        relationshipExtractorFactory.Register(new HarryPotterFandomRelationshipExtractor());
        return serviceCollection.AddTransient<IWebsiteSettingsProvider, JsonWebsiteSettingsProvider>()
            .AddSingleton(relationshipExtractorFactory)
            .AddTransient<ICrawler, Crawler>();
    }

    private static StreamWriter CreateUtf8StreamWriter(string path)
    {
        var fileStream = File.Create(path);
        fileStream.Write(Encoding.UTF8.GetPreamble());
        return new StreamWriter(fileStream, Encoding.UTF8, leaveOpen: false);
    }
}