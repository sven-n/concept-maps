namespace ConceptMaps.DataModel;

using ConceptMaps.DataModel.Spacy;

public static class RelationshipExtensions
{
    public static bool IsUndefined(this Relationship relationship)
    {
        return SpacyRelationLabel.Undefined.Equals(relationship.RelationshipType, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsChildren(this Relationship relationship)
    {
        return SpacyRelationLabel.Children.Equals(relationship.RelationshipType, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsSpouse(this Relationship relationship)
    {
        return SpacyRelationLabel.Spouse.Equals(relationship.RelationshipType, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsSiblings(this Relationship relationship)
    {
        return SpacyRelationLabel.Siblings.Equals(relationship.RelationshipType, StringComparison.InvariantCultureIgnoreCase);
    }

    public static IEnumerable<Triple> ToTriples(this IEnumerable<Relationship>? relationships)
    {
        return relationships?.Select(rel => new Triple(rel.FirstEntity, rel.RelationshipType, rel.SecondEntity, rel.Score))
            ?? Enumerable.Empty<Triple>();
    }
}