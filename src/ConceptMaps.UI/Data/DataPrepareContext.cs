namespace ConceptMaps.UI.Data;

using System.Text.Json;
using ConceptMaps.DataModel;

public class DataPrepareContext
{
    public DataPrepareContext()
    {
        this.Sentences = new();
    }

    public string Name { get; set; } = DateTime.Now.ToString("yyyyMMdd_hhmmss");

    public List<SentenceContext> Sentences { get; set; }

    public int ReviewedSentences => this.Sentences.Count(s => s.State == SentenceState.Reviewed);

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