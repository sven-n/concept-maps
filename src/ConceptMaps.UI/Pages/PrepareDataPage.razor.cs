namespace ConceptMaps.UI.Pages;

using System.Text;
using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Services;
using ConceptMaps.UI.Data;

/// <summary>
/// Webpage for the <see cref="ICrawler"/>.
/// </summary>
public partial class PrepareDataPage
{
    /// <summary>
    /// THe injected <see cref="ICrawledDataProvider"/>.
    /// </summary>
    [Inject]
    private ICrawledDataProvider DataProvider { get; set; }

    [Inject]
    private SentenceAnalyzer SentenceAnalyzer { get; set; }

    [Inject]
    private RemoteTrainingDataConversionService ConversionService { get; set; }

    private string? DownloadPath { get; set; }
    private DataPrepareContext? PrepareContext { get; set; }

    private string? SelectedFile
    {
        get => this.PrepareContext?.SelectedFile;
        set
        {
            if (this.SelectedFile != value)
            {
                this.PrepareContext = new DataPrepareContext(value);
                this.DownloadPath = null;
            }
        }
    }

    public void StartAnalyzeAll()
    {
        var progress = new Progress<int>(completedIndex =>
        {
            this.InvokeAsync(this.StateHasChanged);
        });
        _ = Task.Run(() => this.SentenceAnalyzer.StartAnalyzeAllAsync(this.PrepareContext, progress));
    }

    protected override void OnInitialized()
    {
        //this.SelectedFile = this.DataProvider.AvailableRelationalData.FirstOrDefault();
        base.OnInitialized();
    }

    private void AddNewSentence()
    {
        this.PrepareContext ??= new DataPrepareContext(string.Empty);
        this.PrepareContext.Sentences.Add(new SentenceContext(string.Empty));
    }

    private async Task GetTrainingDataAsync(CancellationToken cancellationToken)
    {
        if (this.PrepareContext is null)
        {
            return;
        }

        this.DownloadPath = null;
        await this.InvokeAsync(this.StateHasChanged);

        var crawlerData = this.PrepareContext.AsCrawlerData();
        var result = await this.ConversionService.ConvertToJsonTrainingData(crawlerData, cancellationToken);
        var targetFolderPath = Path.Combine("training-data", "relations");
        Directory.CreateDirectory(targetFolderPath);
        var fileName = Path.GetFileName(this.PrepareContext.SelectedFile);
        var targetPath = Path.Combine(targetFolderPath, fileName.Replace(".json", "TrainingData.json"));
        await File.WriteAllTextAsync(targetPath, result, Encoding.UTF8, cancellationToken);
        this.DownloadPath = targetPath;
    }
}
