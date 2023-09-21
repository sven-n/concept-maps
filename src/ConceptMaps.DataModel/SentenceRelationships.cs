namespace ConceptMaps.DataModel;

/// <summary>
/// Defines a sentences with it's contained relationships.
/// </summary>
public record SentenceRelationships(string Sentence, List<Relationship> Relationships);
