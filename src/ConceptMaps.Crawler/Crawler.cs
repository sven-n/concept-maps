namespace ConceptMaps.Crawler;

using System.Text.RegularExpressions;

/// <summary>
/// The implementation of a crawler.
/// </summary>
public class Crawler : ICrawler
{
    /// <summary>
    /// A regex which finds the citation-brackets, so they can be removed.
    /// </summary>
    private static readonly Regex RemoveCitationBrackets = new Regex(@"\[\d+\]", RegexOptions.Compiled);

    /// <summary>
    /// The logger for this class.
    /// </summary>
    private readonly ILogger<Crawler> _logger;

    /// <summary>
    /// The factory to get the <see cref="IRelationshipExtractor"/> for the specific website.
    /// </summary>
    private readonly RelationshipExtractorFactory _relationshipExtractorFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Crawler" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="relationshipExtractorFactory">The relationship extractor.</param>
    public Crawler(ILogger<Crawler> logger, RelationshipExtractorFactory relationshipExtractorFactory)
    {
        this._logger = logger;
        this._relationshipExtractorFactory = relationshipExtractorFactory;
    }
    
    /// <inheritdoc />
    public async Task CrawlAsync(WebsiteSettings settings, TextWriter contentWriter, TextWriter relationsWriter, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var crawledPages = new HashSet<Uri>();
        var config = new CrawlConfiguration
        {
            MinCrawlDelayPerDomainMilliSeconds = 1000,
            IsExternalPageCrawlingEnabled = false,
            CrawlTimeoutSeconds = (int)TimeSpan.FromMinutes(10).TotalSeconds,
            MaxPagesToCrawl = 10,
            MaxLinksPerPage = int.MaxValue,
        };

        using var crawler = new PoliteWebCrawler(config);
        crawler.PageCrawlCompleted += (_, args) =>
        {
            OnPageCrawlCompleted(args.CrawledPage, contentWriter, relationsWriter);
            if (args.CrawledPage.HttpResponseMessage?.IsSuccessStatusCode is not true)
            {
                // we could try again later
                crawledPages.Remove(args.CrawledPage.Uri);
            }
            else
            {
                progress?.Report($"Crawled page {args.CrawledPage.Uri} ...");
            }
        };
        crawler.PageCrawlStarting += (sender, args) =>
        {
            progress?.Report($"Crawling page {args.PageToCrawl.Uri} ...");
            this._logger.LogInformation("Crawling page {page} ...", args.PageToCrawl.Uri);
            crawledPages.Add(args.PageToCrawl.Uri);
        };

        crawler.ShouldCrawlPageDecisionMaker += OnShouldCrawlPage;
        
        foreach (var entryUri in settings.EntryUris)
        {
            // We are not passing the cancellationToken here, because that would
            // crash the program.
            // Unfortunately, Abot doesn't catch the OperationCancelledExceptions
            // in it's started threads.
            await crawler.CrawlAsync(entryUri);
        }

        CrawlDecision OnShouldCrawlPage(PageToCrawl page, CrawlContext context)
        {
            return new CrawlDecision
            {
                Allow = settings.BaseUri.IsBaseOf(page.Uri)
                        && !settings.BlockUris.Contains(page.Uri)
                        && !crawledPages.Contains(page.Uri)
                        && !page.Uri.AbsoluteUri.Contains("/File:")
                        && !cancellationToken.IsCancellationRequested
            };
        }
    }

    /// <summary>
    /// Called when a page has been crawled. The page can then be analyzed.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="textWriter">The text writer.</param>
    /// <param name="relationsWriter">The writer for relationships.</param>
    private void OnPageCrawlCompleted(CrawledPage page, TextWriter textWriter, TextWriter relationsWriter)
    {
        if (page.HttpResponseMessage?.IsSuccessStatusCode is not true)
        {
            this._logger.LogError(
                "Couldn't crawl page {page}: {error}",
                page.Uri, page.HttpResponseMessage?.ToString()
                          ?? page.HttpRequestException?.ToString());
            return;
        }

        this._logger.LogInformation("Crawled page {page} successfully.", page.Uri);

        var document = page.AngleSharpHtmlDocument;

        var paragraphs = document.Body
            .GetElementsByTagName("p") // search for all <p> elements
            .Where(p => !p.HasTableAsParent()) // we don't want children of tables (e.g. at the bottom of the page)
            .Select(p => p.TextContent.Trim()) // remove whitespace from the start and end of the content.
            .Select(text => text.Replace("READ MORE", string.Empty)) // filter out "READ MORE" (LOTR)
            .Select(text => text.Replace("Point me!", string.Empty)) // filter out "Point me!" (HP)
            .Where(text => !string.IsNullOrWhiteSpace(text)) // we don't want empty or white space 
            .Select(text => text.Replace(".\"[", "\".[")) // correct sentence endings where the point was included in the citation.
            .Select(text => RemoveCitationBrackets.Replace(text, string.Empty)); // remove the citation marks, e.g. '[2]'

        foreach (var text in paragraphs)
        {
            textWriter.WriteLine(text);
        }

        // Now we try to extract the relationships. A suitable extractor is selected based on the URI of the page.
        var relationshipExtractor = this._relationshipExtractorFactory.GetExtractor(page.Uri);
        foreach (var (currentPerson, relationType, relativeName) in relationshipExtractor.ExtractRelationships(document))
        {
            relationsWriter.WriteLine($"{currentPerson};{relationType};{relativeName}");
        }
    }
}