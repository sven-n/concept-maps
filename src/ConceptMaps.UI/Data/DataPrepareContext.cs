namespace ConceptMaps.UI.Data;

using System.Text.Json;
using ConceptMaps.Crawler;

public class DataPrepareContext
{
    public DataPrepareContext(string selectedFile)
    {
        SelectedFile = selectedFile;
        this.Sentences = new();
        if (!string.IsNullOrWhiteSpace(this.SelectedFile))
        {
            var crawledData = JsonSerializer.Deserialize<SentenceRelationships[]>(File.ReadAllText(selectedFile), new JsonSerializerOptions(JsonSerializerDefaults.Web))?.ToList() ?? new();
            Sentences.AddRange(crawledData.Select(cd => new SentenceContext(cd.Sentence)
            {
                Relationships = cd.Relationships,
                State = SentenceState.Processed,
            }));
        }
    }

    public string SelectedFile { get; }

    public List<SentenceContext> Sentences { get; set; }

    public int ReviewedSentences => this.Sentences.Count(s => s.State == SentenceState.Reviewed);

    public void Save()
    {
        // todo: save this instance, so someone can continue to work after a crash or break
    }

    public IList<SentenceRelationships> AsCrawlerData()
    {
        return this.Sentences
            .Where(s => s.State == SentenceState.Reviewed)
            .Select(s => new SentenceRelationships(
                s.Sentence,
                s.Relationships))
            .ToList();
    }
}