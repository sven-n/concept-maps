namespace ConceptMaps.UI.Data;

using System.Text.Json;
using ConceptMaps.Crawler;

public class DataPrepareContext
{
    public DataPrepareContext()
    {
        this.Sentences = new();
    }

    public List<SentenceContext> Sentences { get; set; }

    public int ReviewedSentences => this.Sentences.Count(s => s.State == SentenceState.Reviewed);

    public void Save()
    {
        // todo: save this instance, so someone can continue to work after a crash or break
    }

    public async Task LoadCrawlDataAsync(string selectedFile)
    {
        // todo: check if file already loaded?
        await using var fileStream = File.OpenRead(selectedFile);
        var crawledData = await JsonSerializer.DeserializeAsync<SentenceRelationships[]>(fileStream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        crawledData ??= Array.Empty<SentenceRelationships>();
        Sentences.AddRange(crawledData.Select(cd => new SentenceContext(cd.Sentence)
        {
            Relationships = cd.Relationships,
            State = SentenceState.Processed,
        }));
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