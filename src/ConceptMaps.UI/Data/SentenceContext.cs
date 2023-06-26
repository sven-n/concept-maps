namespace ConceptMaps.UI.Data;

using ConceptMaps.Crawler;

public class SentenceContext
{
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

    public List<Relationship> Relationships { get; set; } = new();

    public Exception? LastException { get; set; }
}