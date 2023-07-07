namespace ConceptMaps.UI.Services;

using ConceptMaps.UI.Data;
using ConceptMaps.DataModel;

public class SentenceAnalyzer
{
    private readonly RemoteTripleService _remoteTripleService;
    private CancellationTokenSource? _resolveCts;
    private bool _isAnalyzing;

    public SentenceAnalyzer(RemoteTripleService remoteTripleService)
    {
        this._remoteTripleService = remoteTripleService;
    }

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

    public event EventHandler IsAnalyzingChanged;

    public bool IsCancelled => this._resolveCts?.IsCancellationRequested ?? false;

    public void Cancel()
    {
        this._resolveCts?.Cancel();
    }

    public void StartAnalyzeAllAsync(DataPrepareContext prepareContext, IProgress<int> progress)
    {
        if (this.IsAnalyzing)
        {
            throw new InvalidOperationException("Can only start one resolve action at a time.");
        }

        var cts = this._resolveCts = new CancellationTokenSource();
        _ = Task.Run(() => DoAnalyzeAllAsync(prepareContext, progress, cts.Token));
    }

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
                await ProcessSentenceAsync(currentSentence, i, progress, cancellationToken);
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