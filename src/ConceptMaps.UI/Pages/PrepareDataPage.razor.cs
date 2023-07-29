namespace ConceptMaps.UI.Pages;

using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Components;
using ConceptMaps.UI.Data;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// Webpage for the training data preparation.
/// </summary>
public partial class PrepareDataPage
{
    /// <summary>
    /// Flag, if the batch adding component should be displayed.
    /// </summary>
    private bool _showBatchAdding;

    /// <summary>
    /// Flag, if the crawled data is currently loading.
    /// </summary>
    private bool _isLoadingCrawledData;

    /// <summary>
    /// The pagination state of the sentences.
    /// </summary>
    private readonly PaginationState<SentenceContext> _paginationState = new();

    /// <summary>
    /// The visible states, which can be modified by the filter checkboxes.
    /// </summary>
    private readonly HashSet<SentenceState> _visibleStates = new()
    {
        SentenceState.Initial,
        SentenceState.Processed,
        SentenceState.Reviewed,
        SentenceState.Processing,
        SentenceState.Hidden,
    };

    /// <summary>
    /// Gets a value indicating whether any dialog component is open.
    /// </summary>
    private bool IsDialogOpen => this._showBatchAdding || this._isLoadingCrawledData;

    /// <summary>
    /// The injected <see cref="ICrawledDataProvider"/>.
    /// </summary>
    [Inject]
    private ICrawledDataProvider DataProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected sentence analyzer.
    /// </summary>
    [Inject]
    private SentenceAnalyzer SentenceAnalyzer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected <see cref="IPrepareDataManager"/>.
    /// </summary>
    [Inject]
    private IPrepareDataManager PrepareDataManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected <see cref="ITrainingDataManager"/>.
    /// </summary>
    [Inject]
    private ITrainingDataManager TrainingDataManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets the prepare context/session.
    /// </summary>
    private DataPrepareContext PrepareContext { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to show sentences of <see cref="SentenceState.Initial"/>.
    /// Bound to the corresponding check box.
    /// </summary>
    private bool ShowInitial
    {
        get => this._visibleStates.Contains(SentenceState.Initial);
        set => _ = value ? this._visibleStates.Add(SentenceState.Initial) : this._visibleStates.Remove(SentenceState.Initial);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show sentences of <see cref="SentenceState.Processed"/>.
    /// Bound to the corresponding check box.
    /// </summary>
    private bool ShowProcessed
    {
        get => this._visibleStates.Contains(SentenceState.Processed);
        set => _ = value ? this._visibleStates.Add(SentenceState.Processed) : this._visibleStates.Remove(SentenceState.Processed);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show sentences of <see cref="SentenceState.Reviewed"/>.
    /// Bound to the corresponding check box.
    /// </summary>
    private bool ShowReviewed
    {
        get => this._visibleStates.Contains(SentenceState.Reviewed);
        set => _ = value ? this._visibleStates.Add(SentenceState.Reviewed) : this._visibleStates.Remove(SentenceState.Reviewed);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show sentences of <see cref="SentenceState.Hidden"/>.
    /// Bound to the corresponding check box.
    /// </summary>
    private bool ShowHidden
    {
        get => this._visibleStates.Contains(SentenceState.Hidden);
        set => _ = value ? this._visibleStates.Add(SentenceState.Hidden) : this._visibleStates.Remove(SentenceState.Hidden);
    }

    /// <summary>
    /// Gets the sentences, filtered by <see cref="_visibleStates"/>.
    /// </summary>
    private IEnumerable<SentenceContext> FilteredSentences
    {
        get
        {
            return this.PrepareContext.Sentences
                .Where(c => this._visibleStates.Contains(c.State));
        }
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        this._paginationState.Items = this.FilteredSentences;
        this._paginationState.PropertyChanged += (_, _) => this.InvokeAsync(this.StateHasChanged);
        base.OnInitialized();
    }

    /// <summary>
    /// Called when a context/session should be loaded.
    /// It loads the data and resets the pagination state.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    private async Task OnLoadContextAsync(string fileName)
    {
        if (await this.PrepareDataManager.LoadAsync(fileName) is { } loaded)
        {
            this.PrepareContext = loaded;
            this._paginationState.Items = this.FilteredSentences;
            this._paginationState.CurrentPageIndex = 0;
        }
    }

    /// <summary>
    /// Called when the context/session should be saved.
    /// </summary>
    private async Task OnSaveContextAsync()
    {
        await this.PrepareDataManager.SaveAsync(this.PrepareContext);
    }

    /// <summary>
    /// Handles the event of the inputFile element of the import session function.
    /// </summary>
    /// <param name="e">The <see cref="InputFileChangeEventArgs"/> instance containing the event data.</param>
    private async Task OnImportSessions(InputFileChangeEventArgs e)
    {
        var targetFolder = this.PrepareDataManager.GetFolderPath(ModelType.Relation);

        foreach (var file in e.GetMultipleFiles(100))
        {
            var targetPath = Path.Combine(targetFolder, file.Name);
            await using var inputStream = file.OpenReadStream();
            await using var writeStream = File.Create(targetPath);
            await inputStream.CopyToAsync(writeStream);
        }
    }

    /// <summary>
    /// Called when the context/session should be cleared.
    /// It removes all sentences from the current session.
    /// </summary>
    private void OnClearContext()
    {
        this.PrepareContext = new DataPrepareContext();
        this._paginationState.Items = this.FilteredSentences;
        this._paginationState.CurrentPageIndex = 0;
    }

    /// <summary>
    /// Starts to analyze all sentences which are not yet analyzed.
    /// </summary>
    public void StartAnalyzeAll()
    {
        var progress = new Progress<int>(completedIndex =>
        {
            this.InvokeAsync(this.StateHasChanged);
        });

        _ = Task.Run(() => this.SentenceAnalyzer.StartAnalyzeAllAsync(this.PrepareContext, progress));
    }

    /// <summary>
    /// Adds a new sentence to the context.
    /// </summary>
    private void AddNewSentence()
    {
        this.PrepareContext.Sentences.Add(new SentenceContext());
    }

    /// <summary>
    /// Removes a sentence from the context/session.
    /// </summary>
    /// <param name="sentence">The sentence to remove.</param>
    private void OnDeleteSentenceClick(SentenceContext sentence)
    {
        this.PrepareContext.Sentences.Remove(sentence);
        this.StateHasChanged();
    }

    /// <summary>
    /// Called when crawled data has been selected to be loaded.
    /// </summary>
    /// <param name="path">The path.</param>
    private async Task OnLoadCrawledDataClick(string path)
    {
        _isLoadingCrawledData = true;
        this.StateHasChanged();
        try
        {
            var crawledData = (await this.DataProvider.GetRelationshipsAsync(path)).ToList();
            this.PrepareContext.Sentences.AddRange(crawledData.Select(cd => new SentenceContext(cd.Sentence)
            {
                Relationships = cd.Relationships.Select(r => r with { RelationshipType = r.RelationshipType.ToUpperInvariant() }).ToList(),
                KnownRelationships = cd.Relationships.Select(r => r with { RelationshipType = r.RelationshipType.ToUpperInvariant() }).ToList(),
                State = SentenceState.Initial,
            }));
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

    /// <summary>
    /// Called after the batch sentences have been added.
    /// Hides the <see cref="AddSentenceBatch"/> component and updates the state.
    /// </summary>
    private void OnAddedBatchSentences()
    {
        this._showBatchAdding = false;
        this.StateHasChanged();
    }

    /// <summary>
    /// Called after cancelling in the <see cref="AddSentenceBatch"/> component.
    /// Hides the <see cref="AddSentenceBatch"/> component and updates the state.
    /// </summary>
    private void OnCancelBatchSentences()
    {
        this._showBatchAdding = false;
        this.StateHasChanged();
    }

    /// <summary>
    /// Saves the training data with the <see cref="TrainingDataManager"/>.
    /// </summary>
    private async Task SaveTrainingDataAsync()
    {
        if (this.PrepareContext.ReviewedSentences == 0)
        {
            return;
        }

        await this.InvokeAsync(this.StateHasChanged);
        await this.TrainingDataManager.SaveRelationsAsync(this.PrepareContext);
    }
}
