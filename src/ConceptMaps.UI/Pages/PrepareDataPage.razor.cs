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
public partial class PrepareDataPage
{
    /// <summary>
    /// The injected <see cref="ICrawledDataProvider"/>.
    /// </summary>
    [Inject]
    private ICrawledDataProvider DataProvider { get; set; } = null!;

    [Inject]
    private SentenceAnalyzer SentenceAnalyzer { get; set; } = null!;

    private DataPrepareContext? PrepareContext { get; set; }

    private string? SelectedFile
    {
        get => this.PrepareContext?.SelectedFile;
        set
        {
            if (this.SelectedFile != value)
            {
                this.PrepareContext = new DataPrepareContext(value);
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

        await this.InvokeAsync(this.StateHasChanged);

        var crawlerData = this.PrepareContext.AsCrawlerData();
        var serializedData = JsonSerializer.Serialize(crawlerData.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var targetFolderPath = Path.Combine("training-data", ModelType.Relation.AsString());
        Directory.CreateDirectory(targetFolderPath);
        var fileName = Path.GetFileName(this.PrepareContext.SelectedFile);
        var targetPath = Path.Combine(targetFolderPath, fileName);
        await File.WriteAllTextAsync(targetPath, serializedData, Encoding.UTF8, cancellationToken);
    }
}
