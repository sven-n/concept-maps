using System.Text.Json.Serialization;

namespace ConceptMaps.Crawler;

public class Relationship
{
    public string FirstEntity { get; set; }

    public string SecondEntity { get; set; }

    [JsonIgnore]
    public string RelationshipTypeInSentence { get; set; } = "undefined";

    [JsonPropertyName("relationshipType")]
    public string? KnownRelationshipType { get; set; }

    public Relationship Clone()
    {
        return new Relationship
        {
            FirstEntity = this.FirstEntity,
            SecondEntity = this.SecondEntity,
            RelationshipTypeInSentence = this.RelationshipTypeInSentence,
            KnownRelationshipType = this.KnownRelationshipType,
        };
    }
}
