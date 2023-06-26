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
    private ICrawler Crawler { get; set; }

    /// <summary>
    /// THe injected <see cref="IWebsiteSettingsProvider"/>.
    /// </summary>
    [Inject]
    private IWebsiteSettingsProvider SettingsProvider { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private SortedList<string, IWebsiteSettings> _websiteSettings = new();

    private string? _selectedFile;

    private List<string> _progressLog = new();

    private bool _isCrawling = false;

    private CancellationTokenSource? _crawlCts;
    private (string textFilePath, string relationshipFilePath, string sentencesFilePath, string trainingDataFilePath) _resultFiles;

    protected override void OnInitialized()
    {
        this._websiteSettings = new SortedList<string, IWebsiteSettings>(this.SettingsProvider.AvailableSettings.Select(path => (Path.GetFileName(path), this.SettingsProvider.LoadSettings(path))).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2));
        this._selectedFile = this._websiteSettings.Keys.FirstOrDefault()?.ToString();
        base.OnInitialized();
    }

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
        _ = Task.Run(() => DoCrawlAsync(progress, cts.Token));
    }

    private void CancelCrawling()
    {
        this._crawlCts?.Cancel();
    }

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

            var textFilePath = $"crawl-results\\{fileNamePrefix}_Text.txt";
            var relationshipFilePath = $"crawl-results\\{fileNamePrefix}_Relationships.txt";
            var sentencesFilePath = $"crawl-results\\{fileNamePrefix}_SentenceRelationships.json";
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
