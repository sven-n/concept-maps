namespace ConceptMaps.UI.Pages;

using System.Diagnostics;
using System.Text.Json;
using ConceptMaps.DataModel;
using ConceptMaps.DataModel.Spacy;
using Microsoft.AspNetCore.Components;
using ConceptMaps.UI.Components;
using ConceptMaps.UI.Services;

/// <summary>
/// Page to evaluate a trained model.
/// </summary>
public partial class EvaluationPage : IDisposable
{
    private class EvaluationResult
    {
        public string Sentence { get; set; }

        public List<EvaluatedRelationship> Relationships { get; } = new();

        public bool AreAllCorrect => this.Relationships.TrueForAll(rel => string.Equals(rel.ActualRelationship, rel.ExpectedRelationship, StringComparison.InvariantCultureIgnoreCase));

        public double MinScore => this.Relationships.Any() ? this.Relationships.Min(rel => rel.Score) : double.NaN;
    }

    private record EvaluationSummary(string FilePath, int TotalSentences, int ProcessedSentences, int CorrectSentences)
    {
        public int ProcessedPercentage => this.TotalSentences != 0 ? this.ProcessedSentences * 100 / this.TotalSentences : 0;

        public int CorrectPercentage => this.ProcessedSentences != 0 ? this.CorrectSentences * 100 / this.ProcessedSentences : 0;

        public int FailedSentences => this.ProcessedSentences - this.CorrectSentences;

        public int FailedPercentage => this.ProcessedSentences != 0 ? this.FailedSentences * 100 / this.ProcessedSentences : 0;
    }

    private record EvaluatedRelationship(string FirstEntity, string SecondEntity, string ExpectedRelationship, string ActualRelationship, double Score);

    private bool _isEvaluating;

    private EvaluationSummary? _summary;

    private readonly PaginationState<EvaluationResult> _paginationState = new PaginationState<EvaluationResult>();
    private CancellationTokenSource? _cts;

    [Inject]
    private RemoteTripleService SentenceAnalyzer { get; set; } = null!;

    [Inject]
    private ITrainingDataManager TrainingDataManager { get; set; } = null!;

    protected override void OnInitialized()
    {
        this._paginationState.PropertyChanged += (_, _) => this.InvokeAsync(this.StateHasChanged);
        base.OnInitialized();
    }

    private List<EvaluationResult> EvaluatedSentences { get; } = new();

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task OnStartEvaluationAsync(string filePath)
    {
        this._isEvaluating = true;
        try
        {
            this.EvaluatedSentences.Clear();
            this._cts = new CancellationTokenSource();
            var cancellationToken = this._cts.Token;
            await using var fileStream = File.OpenRead(filePath);
            var sentences = JsonSerializer.Deserialize<SentenceRelationships[]>(fileStream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (sentences is null)
            {
                return;
            }

            this._summary = new EvaluationSummary(filePath, sentences.Length, 0, 0);
            await this.InvokeAsync(this.StateHasChanged);
            foreach (var sentence in sentences)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var lastResult = await RetryOnError(ProcessSentenceAsync(sentence, cancellationToken));
                this._summary = this._summary with { ProcessedSentences = this._summary.ProcessedSentences + 1 };
                if (lastResult.AreAllCorrect)
                {
                    this._summary = this._summary with { CorrectSentences = this._summary.CorrectSentences + 1 };
                }

                await this.InvokeAsync(this.StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // expected, do nothing.
        }
        catch (Exception ex)
        {
            // todo log
        }
        finally
        {
            this._isEvaluating = false;
            this._cts?.Dispose();
            this._cts = null;
        }
    }

    private async Task<T> RetryOnError<T>(Task<T> task, int maxAttempts = 3)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                return await task;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // Ignore, try again
                if (i == maxAttempts - 1)
                {
                    throw;
                }
            }
        }

        throw new UnreachableException();
    }

    private async Task<EvaluationResult> ProcessSentenceAsync(SentenceRelationships sentence, CancellationToken cancellationToken)
    {
        var result = await this.SentenceAnalyzer.GenerateTriplesAsync(sentence.Sentence, cancellationToken);
        var evaluationResult = new EvaluationResult()
        {
            Sentence = sentence.Sentence,
        };
        foreach (var triple in result)
        {
            var expectedRelation = sentence.Relationships
                .FirstOrDefault(r => r.FirstEntity == triple.FromWord && r.SecondEntity == triple.ToWord);
            var expectedReverseRelation = sentence.Relationships
                .FirstOrDefault(r => r.SecondEntity == triple.FromWord && r.FirstEntity == triple.ToWord && (r.IsSiblings() || r.IsSpouse()));
            var expected = (expectedRelation ?? expectedReverseRelation)?.RelationshipType ?? SpacyRelationLabel.Undefined;
            var actual = triple.EdgeName ?? SpacyRelationLabel.Undefined;
            if (expected != actual)
            {

            }
            evaluationResult.Relationships.Add(new EvaluatedRelationship(triple.FromWord, triple.ToWord, expected, actual, triple.Score));
        }

        this.EvaluatedSentences.Add(evaluationResult);
        return evaluationResult;
    }
}
