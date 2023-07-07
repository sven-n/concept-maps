namespace ConceptMaps.Crawler;

using System.Text;
using System.Text.Json;
using ConceptMaps.DataModel;

/// <summary>
/// Analyzes the text for possible sentences which include hints to the relationships
/// between persons.
/// It's using a naive approach, by simply checking for every possible relationship
/// in a sentence.
/// If both (fore) names of a relationship are included in the sentence, we assume the
/// sentence is about describing the relationship.
/// </summary>
public class RelationshipAnalyzer
{
    private readonly List<Relationship> _relationships;
    private readonly HashSet<string> _uniqueFirstNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationshipAnalyzer"/> class.
    /// </summary>
    /// <param name="relationshipFilePath">The relationship file path.</param>
    public RelationshipAnalyzer(string relationshipFilePath)
    {
        this._relationships = File.ReadAllLines(relationshipFilePath)
            .Select(line => line.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(tokens => tokens.Length == 3)
            // now we have a line <Entity A> <relationtype> <Entity B>, which means: "A has <relationtype> B".
            // Examples: "A is mother of B", "A has child B".
            // In our further training data, the meaning is slightly different.
            // For spouses and siblings the order doesn't matter. For the parent-child connection we have to
            // normalize that. There we only got the convention "A is child of B".
            .Select(tokens => new Relationship
                {
                    FirstEntity = tokens[2],
                    RelationshipType = tokens[1].ToLower(),
                    SecondEntity = tokens[0],
                })
            .Select(NormalizeRelationship)
            .Distinct()
            .ToList();

        var names = this._relationships.SelectMany(r => new[] { r.FirstEntity, r.SecondEntity }).Distinct();
        var firstNames = names.Select(name => name.Split(' ').First()).ToList();
        this._uniqueFirstNames = firstNames.Where(firstName => firstNames.Count(n => n == firstName) == 1).ToHashSet();

    }

    /// <summary>
    /// Analyzes the text with the possible relationships and stores the results
    /// as a json file in a result file.
    /// </summary>
    /// <param name="textFilePath">The text file path.</param>
    /// <param name="resultFilePath">The result file path.</param>
    public void AnalyzeAndStoreResults(string textFilePath, string resultFilePath)
    {
        var text = File.ReadAllText(textFilePath);
        var foundSentences = this.FindSentences(text);
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
        };

        File.WriteAllText(resultFilePath, JsonSerializer.Serialize(foundSentences, serializerOptions), Encoding.UTF8);
    }

    /// <summary>
    /// Normalizes the relationship to be able to filter out duplicate information.
    /// E.g. A is father of B -> B is child of A.
    /// </summary>
    /// <param name="relationship">The relationship.</param>
    /// <returns>The normalized relationship.</returns>
    private static Relationship NormalizeRelationship(Relationship relationship)
    {
        switch (relationship.RelationshipType)
        {
            case "mother":
            case "father":
                return new Relationship
                {
                    FirstEntity = relationship.SecondEntity,
                    RelationshipType = "children",
                    SecondEntity = relationship.FirstEntity,
                };
            default:
                return relationship;
        }
    }

    private List<SentenceRelationships> FindSentences(string text)
    {
        var sentences = text.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return sentences
            .Select(this.ProcessSentence)
            .Where(result => result.Relationships.Any())
            .ToList();
    }

    /// <summary>
    /// Processes the sentence. A sentence contains the relationship, if the forename of each entity is included.
    /// If a name contains more than 2 tokens, n-1 tokens must 
    /// </summary>
    /// <param name="sentence">The sentence.</param>
    /// <returns>The relationships for a sentence.</returns>
    private SentenceRelationships ProcessSentence(string sentence)
    {
        return new SentenceRelationships(
            sentence,
            this._relationships
                .Where(rel => SentenceContainsName(sentence, rel.FirstEntity, rel.FirstEntityForeName)
                              && SentenceContainsName(sentence, rel.SecondEntity, rel.SecondEntityForeName))
                .Select(rel => rel with
                    {
                        FirstEntity = NameInSentence(sentence, rel.FirstEntity, rel.FirstEntityForeName),
                        SecondEntity = NameInSentence(sentence, rel.SecondEntity, rel.SecondEntityForeName),
                    })
                .ToList());
    }

    private string NameInSentence(string sentence, string fullName, string foreName)
    {
        if (sentence.Contains(fullName))
        {
            return fullName;
        }

        return foreName;
    }

    private bool SentenceContainsName(string sentence, string fullName, string foreName)
    {
        return sentence.Contains(this._uniqueFirstNames.Contains(foreName) ? foreName : fullName);
    }
}