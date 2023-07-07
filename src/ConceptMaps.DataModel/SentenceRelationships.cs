namespace ConceptMaps.DataModel;

public record SentenceRelationships(string Sentence, List<Relationship> Relationships);


public static class RelationshipsExtensions
{
    public static Relationship? Find(this List<Relationship> relationships, string firstEntity, string secondEntity)
    {
        var expectedRelation = relationships
            .FirstOrDefault(r => r.FirstEntity == firstEntity && r.SecondEntity == secondEntity);
        var expectedReverseRelation = relationships
            .FirstOrDefault(r => r.SecondEntity == firstEntity && r.FirstEntity == secondEntity && (r.IsSiblings() || r.IsSpouse()));
        return expectedRelation ?? expectedReverseRelation;
    }
}