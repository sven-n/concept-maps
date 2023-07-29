namespace ConceptMaps.UI.Data;

using ConceptMaps.DataModel;

/// <summary>
/// Context/Session for the data preparation page.
/// It holds all the sentences with their relationships and states.
/// </summary>
public class DataPrepareContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataPrepareContext"/> class.
    /// </summary>
    public DataPrepareContext()
    {
        this.Sentences = new();
    }

    /// <summary>
    /// Gets or sets the name of the context/session.
    /// </summary>
    public string Name { get; set; } = DateTime.Now.ToString("yyyyMMdd_hhmmss");

    /// <summary>
    /// Gets or sets the sentences of the context/session.
    /// </summary>
    public List<SentenceContext> Sentences { get; set; }

    /// <summary>
    /// Gets the reviewed sentences.
    /// </summary>
    public int ReviewedSentences => this.Sentences.Count(s => s.State == SentenceState.Reviewed);

    /// <summary>
    /// Gets the reviewed sentences as <see cref="SentenceRelationships"/> items.
    /// </summary>
    /// <returns>The reviewed sentences as <see cref="SentenceRelationships"/> items.</returns>
    public IList<SentenceRelationships> GetReviewedData()
    {
        return this.Sentences
            .Where(s => s.State == SentenceState.Reviewed)
            .Select(s => new SentenceRelationships(
                s.Sentence,
                s.Relationships))
            .ToList();
    }
}