namespace ConceptMaps.DataModel.Spacy;

/// <summary>
/// Defines the possible values for <see cref="SpacyRelation.RelationLabel"/>.
/// </summary>
public static class SpacyRelationLabel
{
    /// <summary>
    /// Gets the <see cref="SpacyRelation.RelationLabel"/> for an undefined relation,
    /// when there isn't a relation between two entities.
    /// </summary>
    public static string Undefined { get; } = "UNDEFINED";

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
    
    private static readonly Dictionary<string, string> DisplayNames = new()
    {
        { Undefined, "is unrelated with" },
        { Spouse, "is married with" },
        { Siblings, "is sibling of" },
        { Children, "is child of" },
    };

    public static string GetDisplayName(string label)
    {
        if (DisplayNames.TryGetValue(label, out var displayName))
        {
            return displayName;
        }

        return label;
    }
}