namespace ConceptMaps.UI.Data;

using System.Text.Json.Serialization;
using ConceptMaps.DataModel;

/// <summary>
/// The context of a sentence.
/// </summary>
public class SentenceContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SentenceContext"/> class.
    /// </summary>
    public SentenceContext()
    {
        this.Sentence = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SentenceContext"/> class.
    /// </summary>
    /// <param name="sentence">The sentence.</param>
    public SentenceContext(string sentence)
    {
        Sentence = sentence;
        if (!sentence.EndsWith('.') && !sentence.EndsWith('?') && !sentence.EndsWith('!'))
        {
            Sentence += ".";
        }
    }

    /// <summary>
    /// Gets or sets the state of the sentence.
    /// </summary>
    public SentenceState State { get; set; }

    /// <summary>
    /// Gets or sets the sentence as text.
    /// </summary>
    public string Sentence { get; set; }

    /// <summary>
    /// Gets or sets the relationships which are known by crawling (e.g. a fandom).
    /// </summary>
    public List<Relationship> KnownRelationships { get; set; } = new();

    /// <summary>
    /// Gets or sets the relationships which are actually included in the sentence.
    /// </summary>
    public List<Relationship> Relationships { get; set; } = new();

    /// <summary>
    /// Gets or sets the exception which occurred when processing the sentence
    /// the last time.
    /// </summary>
    [JsonIgnore]
    public Exception? LastException { get; set; }
}
