using System.Security.AccessControl;

namespace ConceptMaps.UI.Pages;

using System.Text;
using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Components;
using ConceptMaps.UI.Data;
using ConceptMaps.UI.Services;
using System.Text.Json;

/// <summary>
/// Webpage for the <see cref="ICrawler"/>.
/// </summary>
public partial class PrepareDataPage
{
    private bool _showBatchAdding;

    private bool _showCrawledDataSelection;

    private bool _isLoadingCrawledData;

    private bool _showPrepareDataSelection;

    private readonly PaginationState<SentenceContext> _paginationState = new PaginationState<SentenceContext>();

    private HashSet<SentenceState> _visibleStates = new HashSet<SentenceState>
    {
        { SentenceState.Initial },
        { SentenceState.Processed },
        { SentenceState.Reviewed },
        { SentenceState.Processing },
        { SentenceState.Hidden },
    };

    private bool IsDialogOpen => this._showBatchAdding || this._showCrawledDataSelection || this._isLoadingCrawledData || this._showPrepareDataSelection;

    /// <summary>
    /// The injected <see cref="ICrawledDataProvider"/>.
    /// </summary>
    [Inject]
    private ICrawledDataProvider DataProvider { get; set; } = null!;

    [Inject]
    private SentenceAnalyzer SentenceAnalyzer { get; set; } = null!;

    [Inject]
    private IPrepareDataManager PrepareDataManager { get; set; } = null!;

    private DataPrepareContext PrepareContext { get; set; } = new();

    private bool ShowInitial
    {
        get => this._visibleStates.Contains(SentenceState.Initial);
        set => _ = value ? this._visibleStates.Add(SentenceState.Initial) : this._visibleStates.Remove(SentenceState.Initial);
    }

    private bool ShowProcessed
    {
        get => this._visibleStates.Contains(SentenceState.Processed);
        set => _ = value ? this._visibleStates.Add(SentenceState.Processed) : this._visibleStates.Remove(SentenceState.Processed);
    }

    private bool ShowReviewed
    {
        get => this._visibleStates.Contains(SentenceState.Reviewed);
        set => _ = value ? this._visibleStates.Add(SentenceState.Reviewed) : this._visibleStates.Remove(SentenceState.Reviewed);
    }

    private bool ShowHidden
    {
        get => this._visibleStates.Contains(SentenceState.Hidden);
        set => _ = value ? this._visibleStates.Add(SentenceState.Hidden) : this._visibleStates.Remove(SentenceState.Hidden);
    }

    protected override void OnInitialized()
    {
        this._paginationState.Items = this.FilteredSentences;
        this._paginationState.PropertyChanged += (_, _) => this.InvokeAsync(this.StateHasChanged);
        base.OnInitialized();
    }

    private IEnumerable<SentenceContext> FilteredSentences
    {
        get
        {
            return this.PrepareContext.Sentences
                .Where(c => this._visibleStates.Contains(c.State));
        }
    }

    private void OnLoadContextAsync()
    {
        //this.PrepareDataManager.LoadAsync()
    }

    private async Task OnSaveContextAsync()
    {
        await PrepareDataManager.SaveAsync(this.PrepareContext);
    }

    private void OnClearContext()
    {
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
        this.PrepareContext.Sentences.Add(new SentenceContext());
    }

    private void OnDeleteSentenceClick(SentenceContext sentence)
    {
        this.PrepareContext.Sentences.Remove(sentence);
        this.StateHasChanged();
    }

    private async Task OnLoadCrawledDataClick(string path)
    {
        _isLoadingCrawledData = true;
        this.StateHasChanged();
        try
        {
            await this.PrepareContext.LoadCrawlDataAsync(path);
            this._showCrawledDataSelection = false;
        }
        catch
        {
            // ignore for now...
        }
        finally
        {
            _isLoadingCrawledData = false;
        }
    }

    private void OnAddedBatchSentences()
    {
        this._showBatchAdding = false;
        this.StateHasChanged();
    }

    private void OnCancelBatchSentences()
    {
        this._showBatchAdding = false;
        this.StateHasChanged();
    }

    private async Task SaveTrainingDataAsync(CancellationToken cancellationToken)
    {
        if (this.PrepareContext.ReviewedSentences == 0)
        {
            return;
        }

        await this.InvokeAsync(this.StateHasChanged);

        var crawlerData = this.PrepareContext.AsCrawlerData();
        var serializedData = JsonSerializer.Serialize(crawlerData.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var targetFolderPath = Path.Combine("training-data", ModelType.Relation.AsString());
        Directory.CreateDirectory(targetFolderPath);
        var fileName = DateTime.Now.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty) + "_Sentences.json";
        var targetPath = Path.Combine(targetFolderPath, fileName);
        await File.WriteAllTextAsync(targetPath, serializedData, Encoding.UTF8, cancellationToken);
    }
}
