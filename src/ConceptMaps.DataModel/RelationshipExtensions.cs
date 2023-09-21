namespace ConceptMaps.DataModel;

using ConceptMaps.DataModel.Spacy;

/// <summary>
/// Extension methods for <see cref="Relationship"/>.
/// </summary>
public static class RelationshipExtensions
{
    /// <summary>
    /// Determines whether this <see cref="Relationship"/> is undefined.
    /// </summary>
    /// <param name="relationship">The relationship.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="Relationship"/> is undefined; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUndefined(this Relationship relationship)
    {
        return SpacyRelationLabel.Undefined.Equals(relationship.RelationshipType, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Determines whether this <see cref="Relationship"/> is a child-to-parent relationship.
    /// </summary>
    /// <param name="relationship">The relationship.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="Relationship"/> is a child-to-parent relationship; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsChildren(this Relationship relationship)
    {
        return SpacyRelationLabel.Children.Equals(relationship.RelationshipType, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Determines whether this <see cref="Relationship"/> is a marriage.
    /// </summary>
    /// <param name="relationship">The relationship.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="Relationship"/> is a marriage; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsSpouse(this Relationship relationship)
    {
        return SpacyRelationLabel.Spouse.Equals(relationship.RelationshipType, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Determines whether this <see cref="Relationship"/> is between siblings.
    /// </summary>
    /// <param name="relationship">The relationship.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="Relationship"/> is between siblings; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsSiblings(this Relationship relationship)
    {
        return SpacyRelationLabel.Siblings.Equals(relationship.RelationshipType, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Searches for the <see cref="Relationship"/> between the specified entities.
    /// </summary>
    /// <param name="relationships">The list of relationships.</param>
    /// <param name="firstEntity">The first entity name.</param>
    /// <param name="secondEntity">The second entity name.</param>
    /// <returns>The <see cref="Relationship"/> between the specified entities, if found.</returns>
    public static Relationship? Find(this List<Relationship> relationships, string firstEntity, string secondEntity)
    {
        var expectedRelation = relationships
            .FirstOrDefault(r => r.FirstEntity == firstEntity && r.SecondEntity == secondEntity);
        var expectedReverseRelation = relationships
            .FirstOrDefault(r => r.SecondEntity == firstEntity && r.FirstEntity == secondEntity && (r.IsSiblings() || r.IsSpouse()));
        return expectedRelation ?? expectedReverseRelation;
    }
}