namespace ConceptMaps.UI.Data;

using System.Text.Json.Serialization;
using ConceptMaps.DataModel;

public class SentenceContext
{
    public SentenceContext()
    {
        this.Sentence = string.Empty;
    }

    public SentenceContext(string sentence)
    {
        Sentence = sentence;
        if (!sentence.EndsWith('.') && !sentence.EndsWith('?') && !sentence.EndsWith('!'))
        {
            Sentence += ".";
        }
    }

    public SentenceState State { get; set; }

    public string Sentence { get; set; }

    /// <summary>
    /// Gets or sets the relationships which are known by crawling (e.g. a fandom).
    /// </summary>
    public List<Relationship> KnownRelationships { get; set; } = new();

    public List<Relationship> Relationships { get; set; } = new();

    [JsonIgnore]
    public Exception? LastException { get; set; }
}
