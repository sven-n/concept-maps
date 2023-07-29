namespace ConceptMaps.UI.Pages;

using System.Diagnostics;
using System.Text.Json;
using ConceptMaps.DataModel;
using ConceptMaps.DataModel.Spacy;
using Microsoft.AspNetCore.Components;
using ConceptMaps.UI.Services;

/// <summary>
/// Page to evaluate the trained model.
/// </summary>
public partial class EvaluationPage : IDisposable
{
    /// <summary>
    /// Flag, if the page is currently evaluating.
    /// </summary>
    private bool _isEvaluating;

    /// <summary>
    /// The evaluation summary.
    /// </summary>
    private EvaluationSummary? _summary;

    /// <summary>
    /// The cancellation token source, which allows to cancel the evaluation.
    /// </summary>
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Gets or sets the injected sentence analyzer.
    /// </summary>
    [Inject]
    private RemoteTripleService SentenceAnalyzer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected training data manager.
    /// </summary>
    [Inject]
    private ITrainingDataManager TrainingDataManager { get; set; } = null!;

    /// <summary>
    /// Gets the evaluated sentences.
    /// </summary>
    private List<EvaluationResult> EvaluatedSentences { get; } = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// Called when the button to start the evaluation has been clicked.
    /// It reads the prepared training sentences from the selected file,
    /// calls the <see cref="RemoteTripleService.GenerateTriplesAsync"/> for each
    /// sentence and compares the result with the expected relations.
    /// </summary>
    /// <param name="filePath">The file path.</param>
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

    /// <summary>
    /// Retries the task again, in case there was an unexpected error.
    /// </summary>
    /// <typeparam name="T">The type of the result of the task.</typeparam>
    /// <param name="task">The task.</param>
    /// <param name="maxAttempts">The maximum number of attempts.</param>
    /// <returns>The result of the task.</returns>
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

    /// <summary>
    /// Processes the specified sentence by calling the <see cref="RemoteTripleService.GenerateTriplesAsync"/>
    /// and comparing the result with the expected relations.
    /// </summary>
    /// <param name="sentence">The sentence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the evaluation of the specified sentence.</returns>
    private async Task<EvaluationResult> ProcessSentenceAsync(SentenceRelationships sentence, CancellationToken cancellationToken)
    {
        var result = await this.SentenceAnalyzer.GenerateTriplesAsync(sentence.Sentence, cancellationToken);
        var evaluationResult = new EvaluationResult(sentence.Sentence);
        foreach (var triple in result)
        {
            var expectedRelation = sentence.Relationships
                .FirstOrDefault(r => r.FirstEntity == triple.FromWord && r.SecondEntity == triple.ToWord);
            var expectedReverseRelation = sentence.Relationships
                .FirstOrDefault(r => r.SecondEntity == triple.FromWord && r.FirstEntity == triple.ToWord && (r.IsSiblings() || r.IsSpouse()));
            var expected = (expectedRelation ?? expectedReverseRelation)?.RelationshipType ?? SpacyRelationLabel.Undefined;
            var actual = triple.EdgeName ?? SpacyRelationLabel.Undefined;
            evaluationResult.Relationships.Add(new EvaluatedRelationship(triple.FromWord, triple.ToWord, expected, actual, triple.Score));
        }

        this.EvaluatedSentences.Add(evaluationResult);
        return evaluationResult;
    }

    /// <summary>
    /// The evaluation result of a sentence.
    /// </summary>
    private class EvaluationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationResult"/> class.
        /// </summary>
        /// <param name="sentence">The sentence which has been evaluated.</param>
        public EvaluationResult(string sentence)
        {
            Sentence = sentence;
        }

        /// <summary>
        /// Gets the sentence which has been evaluated.
        /// </summary>
        public string Sentence { get; }

        /// <summary>
        /// Gets the relationships which have been evaluated.
        /// </summary>
        public List<EvaluatedRelationship> Relationships { get; } = new();

        /// <summary>
        /// Gets a value indicating whether all relations were determined correctly.
        /// </summary>
        public bool AreAllCorrect => this.Relationships.TrueForAll(rel => string.Equals(rel.ActualRelationship, rel.ExpectedRelationship, StringComparison.InvariantCultureIgnoreCase));

        /// <summary>
        /// Gets the minimum score across all determined relationships.
        /// </summary>
        public double MinScore => this.Relationships.Any() ? this.Relationships.Min(rel => rel.Score) : double.NaN;
    }

    /// <summary>
    /// The summary of an evaluation.
    /// </summary>
    private record EvaluationSummary(string FilePath, int TotalSentences, int ProcessedSentences, int CorrectSentences)
    {
        public int ProcessedPercentage => this.TotalSentences != 0 ? this.ProcessedSentences * 100 / this.TotalSentences : 0;

        public int CorrectPercentage => this.ProcessedSentences != 0 ? this.CorrectSentences * 100 / this.ProcessedSentences : 0;

        public int FailedSentences => this.ProcessedSentences - this.CorrectSentences;

        public int FailedPercentage => this.ProcessedSentences != 0 ? this.FailedSentences * 100 / this.ProcessedSentences : 0;
    }

    /// <summary>
    /// An evaluated relationship of a sentence.
    /// </summary>
    private record EvaluatedRelationship(string FirstEntity, string SecondEntity, string ExpectedRelationship, string ActualRelationship, double Score);
}
