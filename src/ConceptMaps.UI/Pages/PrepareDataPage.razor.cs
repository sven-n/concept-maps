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
    private bool _showBatchAdding;

    private bool _showCrawledDataSelection;

    private bool _isLoadingCrawledData;

    private int _currentPageIndex;

    private int _pageSize = 10;

    private HashSet<SentenceState> _visibleStates = new HashSet<SentenceState>
    {
        { SentenceState.Initial },
        { SentenceState.Processed },
        { SentenceState.Reviewed },
        { SentenceState.Processing },
        { SentenceState.Removed },
    };

    /// <summary>
    /// The injected <see cref="ICrawledDataProvider"/>.
    /// </summary>
    [Inject]
    private ICrawledDataProvider DataProvider { get; set; } = null!;

    [Inject]
    private SentenceAnalyzer SentenceAnalyzer { get; set; } = null!;

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

    private bool ShowRemoved
    {
        get => this._visibleStates.Contains(SentenceState.Removed);
        set => _ = value ? this._visibleStates.Add(SentenceState.Removed) : this._visibleStates.Remove(SentenceState.Removed);
    }

    private bool NextPageAvailable => this.TotalPageCount > this._currentPageIndex + 1;

    private bool PreviousPageAvailable => this._currentPageIndex > 0;

    private bool CurrentPageAvailable => this._currentPageIndex < this.TotalPageCount;

    private int TotalPageCount
    {
        get
        {
            var sentenceCount = this.FilteredSentences.Count();
            var pageCount = sentenceCount / this._pageSize;
            if (sentenceCount % this._pageSize > 0)
            {
                pageCount++;
            }

            return pageCount;
        }
    }

    private int CurrentPageIndex
    {
        get
        {
            return Math.Min(this._currentPageIndex, this.TotalPageCount - 1);
        }

        set
        {
            this._currentPageIndex = Math.Min(value, this.TotalPageCount - 1);
        }
    }

    private IEnumerable<SentenceContext> FilteredSentences
    {
        get
        {
            return this.PrepareContext.Sentences
                .Where(c => this._visibleStates.Contains(c.State));
        }
    }

    private IEnumerable<SentenceContext> FilteredAndPagedSentences
    {
        get
        {
            if (!this.CurrentPageAvailable)
            {
                this._currentPageIndex = this.TotalPageCount - 1;
            }

            return this.FilteredSentences
                .Skip(this._currentPageIndex * this._pageSize)
                .Take(this._pageSize);
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
        this.PrepareContext.Sentences.Add(new SentenceContext());
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
