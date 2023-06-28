namespace ConceptMaps.UI.Pages;

using System.Text;
using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Services;
using ConceptMaps.UI.Data;
using System.Text.Json;

/// <summary>
/// Webpage for the <see cref="ICrawler"/>.
/// </summary>
public partial class ModelsPage
{
    /// <summary>
    /// The injected <see cref="ICrawledDataProvider"/>.
    /// </summary>
    [Inject]
    private ICrawledDataProvider DataProvider { get; set; } = null!;

    [Inject]
    private RemoteTrainingService TrainingService { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected <see cref="IModelProvider"/>.
    /// </summary>
    [Inject]
    private IModelProvider ModelProvider { get; set; } = null!;

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
        var serializedData = JsonSerializer.Serialize(crawlerData.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var targetFolderPath = Path.Combine("training-data", "relations");
        Directory.CreateDirectory(targetFolderPath);
        var fileName = Path.GetFileName(this.PrepareContext.SelectedFile);
        var targetPath = Path.Combine(targetFolderPath, fileName);
        await File.WriteAllTextAsync(targetPath, serializedData, Encoding.UTF8, cancellationToken);
        this.DownloadPath = targetPath;
    }
}

