namespace ConceptMaps.DummyTripleService;

using Catalyst.Models;
using Catalyst;
using Mosaik.Core;

/// <summary>
/// A service which generates <see cref="Triple"/>s for a given text.
/// </summary>
public class TripleService
{
    private const int ModelVersion = 0;

    private const string ModelTag = "Reuters-Classifier";
    private static readonly Language English = Language.English;
    private Task<FastText> _prepareTask = Task.FromException<FastText>(new InvalidOperationException("Not yet prepared."));

    /// <summary>
    /// Initializes a new instance of the <see cref="TripleService"/> class.
    /// </summary>
    public TripleService()
    {
        Catalyst.Models.English.Register();
        Storage.Current = new DiskStorage("catalyst-models");
    }

    /// <summary>
    /// Prepares this service by starting a training, if required.
    /// </summary>
    public void PrepareModel()
    {
        this._prepareTask = this.GetModelAsync();
    }

    /// <summary>
    /// Gets the triples for a specified text input.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The generated triples.</returns>
    public async ValueTask<IList<Triple>> GetTriplesAsync(string text)
    {
        await _prepareTask.ConfigureAwait(false);

        // We need a trained model to determine the POS of each word in a text.
        var model = await this.GetModelAsync();

        var tokenizer = new FastTokenizer(English);

        // This HashSet is somehow empty. Maybe only relevant for other languages?
        var stopWords = StopWords.Spacy.For(English);

        var tokens = tokenizer.Parse(text);
        var lemmatizer = LemmatizerStore.Get(Language.English);
        var processedTokens = tokens.Select(t => (
            Text: lemmatizer.GetLemma(t),
            POS: model.GetMostProbablePOSforWord(t.Value)
        ));

        var filteredTokens = processedTokens
            .Where(t => !stopWords.Contains(t.Text))
            .Where(t => t.POS is not (PartOfSpeech.DET or PartOfSpeech.CCONJ or PartOfSpeech.ADP));

        return TokensToTriples(filteredTokens).ToList();
    }



    /// <summary>
    /// Creates tokens out of triples.
    /// This is a very naive approach and only works for very simple sentences.
    /// </summary>
    /// <param name="tokens">The tokens.</param>
    /// <returns></returns>
    private IEnumerable<Triple> TokensToTriples(IEnumerable<(string Text, PartOfSpeech Pos)> tokens)
    {
        string? subj = null;
        string? pred = null;
        string? obj = null;
        foreach (var token in tokens)
        {
            switch (token.Pos)
            {
                // NONE is wrong, but the net is determining it...
                case PartOfSpeech.PROPN when subj is null:
                case PartOfSpeech.NOUN when subj is null:
                case PartOfSpeech.NONE when subj is null:
                    subj = token.Text;
                    break;
                case PartOfSpeech.ADJ when obj is null:
                case PartOfSpeech.PROPN when obj is null:
                case PartOfSpeech.NOUN when obj is null:
                case PartOfSpeech.NONE when obj is null:
                    obj = token.Text;
                    break;
                case PartOfSpeech.VERB:
                    pred = token.Text;
                    break;
                case PartOfSpeech.PUNCT when token.Text == ".": // End of sentence
                    if (subj is not null && obj is not null)
                    {
                        yield return new Triple(subj, pred, obj);
                    }

                    subj = null;
                    pred = null;
                    obj = null;
                    break;
            }

            if (subj is not null && pred is not null && obj is not null)
            {
                yield return new Triple(subj, pred, obj);
                subj = null;
                pred = null;
                obj = null;
            }
        }
    }

    private async Task<FastText> GetModelAsync(bool forceRetrain = false)
    {
        try
        {
            // First try to load it from the storage.
            if (!forceRetrain)
            {
                return await FastText.FromStoreAsync(English, ModelVersion, ModelTag);
            }
        }
        catch (FileNotFoundException)
        {
            // Expected, when we didn't train the model yet.
        }

        return await TrainModel();
    }

    private async ValueTask<FastText> TrainModel()
    {
        var (train, test) = await Corpus.Reuters.GetAsync();
        var nlp = await Pipeline.ForAsync(English);
        var trainResult = nlp.Process(train);
        var testResult = nlp.Process(test);
        var fastText = new FastText(English, ModelVersion, ModelTag);
        fastText.Data.Type = FastText.ModelType.Supervised;
        fastText.Data.Loss = FastText.LossType.OneVsAll;
        fastText.Data.LearningRate = 1f;
        fastText.Data.Dimensions = 256;
        fastText.Data.Epoch = 100;
        fastText.Data.MinimumWordNgramsCounts = 5;
        fastText.Data.MaximumWordNgrams = 3;
        fastText.Data.MinimumCount = 5;
        fastText.Train(trainResult);
        await fastText.StoreAsync();
        return fastText;
    }
}