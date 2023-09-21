namespace ConceptMaps.UI.Services;

using ConceptMaps.UI.Data;
using ConceptMaps.DataModel;

/// <summary>
/// Analyzes sentences of <see cref="DataPrepareContext"/> and finds relationships between entities.
/// </summary>
public class SentenceAnalyzer
{
    private readonly RemoteTripleService _remoteTripleService;
    private CancellationTokenSource? _resolveCts;
    private bool _isAnalyzing;

    /// <summary>
    /// Initializes a new instance of the <see cref="SentenceAnalyzer"/> class.
    /// </summary>
    /// <param name="remoteTripleService">The remote triple service.</param>
    public SentenceAnalyzer(RemoteTripleService remoteTripleService)
    {
        this._remoteTripleService = remoteTripleService;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is analyzing a sentence.
    /// </summary>
    public bool IsAnalyzing
    {
        get => this._isAnalyzing;
        set
        {
            if (this._isAnalyzing == value)
            {
                return;
            }

            this._isAnalyzing = value;
            this.IsAnalyzingChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Occurs when <see cref="IsAnalyzing"/> changed.
    /// </summary>
    public event EventHandler? IsAnalyzingChanged;

    /// <summary>
    /// Gets a value indicating whether the analysis is cancelled.
    /// </summary>
    public bool IsCancelled => this._resolveCts?.IsCancellationRequested ?? false;

    /// <summary>
    /// Cancels the analysis.
    /// </summary>
    public void Cancel()
    {
        this._resolveCts?.Cancel();
    }

    /// <summary>
    /// Starts to analyze all sentences in the state <see cref="SentenceState.Initial"/>.
    /// </summary>
    /// <param name="prepareContext">The prepare context.</param>
    /// <param name="progress">The progress.</param>
    public void StartAnalyzeAllAsync(DataPrepareContext prepareContext, IProgress<int> progress)
    {
        if (this.IsAnalyzing)
        {
            throw new InvalidOperationException("Can only start one resolve action at a time.");
        }

        var cts = this._resolveCts = new CancellationTokenSource();
        _ = Task.Run(() => DoAnalyzeAllAsync(prepareContext, progress, cts.Token));
    }

    /// <summary>
    /// Starts to analyze one sentence.
    /// </summary>
    /// <param name="sentence">The sentence.</param>
    /// <param name="i">The index of the sentence.</param>
    /// <param name="progress">The object to report progress back to the caller.</param>
    /// <exception cref="System.InvalidOperationException">Can only start one resolve action at a time.</exception>
    public void StartAnalyzeSingle(SentenceContext sentence, int i, IProgress<int> progress)
    {
        if (this.IsAnalyzing)
        {
            throw new InvalidOperationException("Can only start one resolve action at a time.");
        }

        this.IsAnalyzing = true;
        var cts = this._resolveCts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessSentenceAsync(sentence, i, progress, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
            catch
            {
                // should never happen... we handle it just in case.
            }
            finally
            {
                progress.Report(i);
                this.IsAnalyzing = false;
            }
        });
    }

    /// <summary>
    /// Analyzes all sentences in the state <see cref="SentenceState.Initial"/>.
    /// </summary>
    /// <param name="prepareContext">The prepare context.</param>
    /// <param name="progress">The progress.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task DoAnalyzeAllAsync(DataPrepareContext prepareContext, IProgress<int> progress, CancellationToken cancellationToken)
    {
        this.IsAnalyzing = true;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sentences = prepareContext.Sentences;
            for (var i = 0; i < sentences.Count; i++)
            {
                var currentSentence = sentences[i];
                if (currentSentence.State is SentenceState.Initial)
                {
                    await ProcessSentenceAsync(currentSentence, i, progress, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // do nothing
        }
        catch (Exception ex)
        {
            // progress.Report($"Unexpected Error: {ex}");
        }
        finally
        {
            this.IsAnalyzing = false;
            this._resolveCts?.Dispose();
            this._resolveCts = null;
            progress.Report(-2);
        }
    }

    private async ValueTask ProcessSentenceAsync(SentenceContext currentSentence, int i, IProgress<int> progress, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            currentSentence.State = SentenceState.Processing;
            progress.Report(i - 1);

            var triples = await _remoteTripleService.GenerateTriplesAsync(currentSentence.Sentence, cancellationToken);
            var foundRelationships = new List<Relationship>();
            foreach (var triple in triples)
            {
                var relation = new Relationship
                {
                    FirstEntity = triple.FromWord,
                    SecondEntity = triple.ToWord,
                    RelationshipType = triple.EdgeName,
                };

                foundRelationships.Add(relation);
            }

            currentSentence.State = SentenceState.Processed;
            currentSentence.Relationships = foundRelationships;
            progress.Report(i);
        }
        catch (OperationCanceledException)
        {
            currentSentence.State = SentenceState.Initial;
            throw;
        }
        catch (Exception ex)
        {
            currentSentence.State = SentenceState.Initial;
            currentSentence.LastException = ex;
        }
    }
}