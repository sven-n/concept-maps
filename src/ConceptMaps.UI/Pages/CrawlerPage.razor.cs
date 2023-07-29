namespace ConceptMaps.UI.Pages;

using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;

/// <summary>
/// Webpage for the <see cref="ICrawler"/>.
/// </summary>
public partial class CrawlerPage
{
    /// <summary>
    /// The injected <see cref="ICrawler"/>.
    /// </summary>
    [Inject]
    private ICrawler Crawler { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected <see cref="IWebsiteSettingsProvider"/>.
    /// </summary>
    [Inject]
    private IWebsiteSettingsProvider SettingsProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected navigation manager.
    /// </summary>
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected <see cref="ICrawledDataProvider"/>.
    /// </summary>
    [Inject]
    private ICrawledDataProvider CrawledDataProvider { get; set; }


    /// <summary>
    /// The <see cref="IWebsiteSettings"/> per website file name.
    /// </summary>
    private SortedList<string, IWebsiteSettings> _websiteSettings = new();

    /// <summary>
    /// The file name of the currently selected <see cref="IWebsiteSettings"/>.
    /// </summary>
    private string? _selectedFile;

    /// <summary>
    /// The progress log of the crawler.
    /// </summary>
    private List<string> _progressLog = new();

    /// <summary>
    /// The flag, if the page is currently crawling.
    /// </summary>
    private bool _isCrawling = false;

    /// <summary>
    /// The cancellation token source, which allows to cancel the crawling.
    /// </summary>
    private CancellationTokenSource? _crawlCts;

    /// <summary>
    /// The result files, which are set after crawling has completed.
    /// </summary>
    private (string textFilePath, string relationshipFilePath, string sentencesFilePath, string trainingDataFilePath) _resultFiles;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        this._websiteSettings = new SortedList<string, IWebsiteSettings>(this.SettingsProvider.AvailableSettings.Select(path => (Path.GetFileName(path), this.SettingsProvider.LoadSettings(path))).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2));
        this._selectedFile = this._websiteSettings.Keys.FirstOrDefault()?.ToString();
        base.OnInitialized();
    }

    /// <summary>
    /// Starts the crawling of the selected website.
    /// </summary>
    private void StartCrawling()
    {
        var cts = this._crawlCts = new CancellationTokenSource();
        var progressLog = new List<string>();
        this._progressLog = progressLog;
        this._resultFiles = default;
        var progress = new Progress<string>(s =>
        {
            progressLog.Add($"{DateTime.Now}: {s}");
            this.InvokeAsync(this.StateHasChanged);
        });
        _ = Task.Run(() => this.DoCrawlAsync(progress, cts.Token));
    }

    /// <summary>
    /// Cancels the crawling.
    /// </summary>
    private void CancelCrawling()
    {
        this._crawlCts?.Cancel();
    }

    /// <summary>
    /// Does the crawling.
    /// </summary>
    /// <param name="progress">The progress.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="System.InvalidOperationException">No configuration selected.</exception>
    private async Task DoCrawlAsync(IProgress<string> progress, CancellationToken cancellationToken)
    {
        this._isCrawling = true;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.Register(() => progress.Report("Cancelled crawler, please wait until the already started pages are done..."));

            if (_selectedFile is null || !this._websiteSettings.TryGetValue(_selectedFile, out var settings))
            {
                throw new InvalidOperationException("No configuration selected.");
            }

            var timestamp = DateTime.Now;
            var configName = this._selectedFile.Split('.').First();
            var fileNamePrefix = $"{configName}_{timestamp:s}".Replace(':', '_').Replace('-', '_');

            var folderPath = this.CrawledDataProvider.FolderPath;

            var textFilePath = Path.Combine(folderPath,$"{fileNamePrefix}_Text.txt");
            var relationshipFilePath = Path.Combine(folderPath, $"{fileNamePrefix}_Relationships.txt");
            var sentencesFilePath = Path.Combine(folderPath, $"{fileNamePrefix}_SentenceRelationships.json");

            progress.Report("Started Crawling ...");
            await this.Crawler.CrawlAsync(settings, textFilePath, relationshipFilePath, progress, cancellationToken);
            progress.Report("Finished Crawling, starting analyzing for relationship sentences ...");

            // After crawling analyze the sentences for possible relationships
            var relationshipAnalyzer = new RelationshipAnalyzer(relationshipFilePath);
            relationshipAnalyzer.AnalyzeAndStoreResults(textFilePath, sentencesFilePath);

            progress.Report("Generating NER training data ...");
            var trainingDataGenerator = new NerTrainingDataGenerator(relationshipFilePath);
            var trainingDataFile = trainingDataGenerator.GenerateTrainingDataFile(textFilePath);
            progress.Report("Finished");
            this._resultFiles = (textFilePath, relationshipFilePath, sentencesFilePath, trainingDataFile);
        }
        catch (OperationCanceledException)
        {
            progress.Report("Cancelled");
        }
        catch (Exception ex)
        {
            progress.Report($"Unexpected Error: {ex}");
        }
        finally
        {
            this._isCrawling = false;
            this._crawlCts?.Dispose();
            this._crawlCts = null;
        }
    }
}
