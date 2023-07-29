namespace ConceptMaps.UI.Components;

using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;

/// <summary>
/// A <see cref="LinkLabelModel"/> for relations between entities.
/// </summary>
public class RelationLabel : LinkLabelModel
{
    /// <summary>
    /// Gets the type of the relation.
    /// </summary>
    public string RelationType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationLabel"/> class.
    /// </summary>
    /// <param name="parent">The parent link between two entities.</param>
    /// <param name="content">The textual content.</param>
    /// <param name="relationType">Type of the relation.</param>
    public RelationLabel(BaseLinkModel parent, string content, string relationType)
        : base(parent, content)
    {
        this.RelationType = relationType;
    }
}