namespace ConceptMaps.UI.Spacy;

/// <summary>
/// Defines the possible values for <see cref="SpacyRelation.RelationLabel"/>.
/// </summary>
public static class SpacyRelationLabel
{
    /// <summary>
    /// Gets the <see cref="SpacyRelation.RelationLabel"/> for an undefined relation,
    /// when there isn't a relation between two entities.
    /// </summary>
    public static string Undefined { get; } = "undefined";

    /// <summary>
    /// Gets the <see cref="SpacyRelation.RelationLabel"/> for entities which are spouses.
    /// </summary>
    public static string Spouse { get; } = "SPOUSE";

    /// <summary>
    /// Gets the <see cref="SpacyRelation.RelationLabel"/> for entites which are siblings.
    /// </summary>
    public static string Siblings { get; } = "SIBLINGS";

    /// <summary>
    /// Gets the <see cref="SpacyRelation.RelationLabel"/> for entities which are children.
    /// </summary>
    public static string Children { get; } = "CHILDREN";
}