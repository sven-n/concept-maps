namespace ConceptMaps.UI.Data;

using System.Text.Json;
using ConceptMaps.Crawler;

public class DataPrepareContext
{
    public DataPrepareContext()
    {
        this.Sentences = new();
    }

    public string Name { get; set; } = DateTime.Now.ToString("yyyyMMdd_hhmmss");

    public List<SentenceContext> Sentences { get; set; }

    public int ReviewedSentences => this.Sentences.Count(s => s.State == SentenceState.Reviewed);

    public async Task LoadCrawlDataAsync(string selectedFile)
    {
        await using var fileStream = File.OpenRead(selectedFile);
        var crawledData = await JsonSerializer.DeserializeAsync<SentenceRelationships[]>(fileStream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        crawledData ??= Array.Empty<SentenceRelationships>();
        Sentences.AddRange(crawledData.Select(cd => new SentenceContext(cd.Sentence)
        {
            Relationships = cd.Relationships,
            State = SentenceState.Initial,
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