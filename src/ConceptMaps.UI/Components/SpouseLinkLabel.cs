namespace ConceptMaps.UI.Components;

using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using ConceptMaps.UI.Spacy;

public class SpouseLinkLabel : RelationLabel
{
    public SpouseLinkLabel(BaseLinkModel parent, string content)
        : base(parent, content, SpacyRelationLabel.Spouse)
    {
    }
}

public class RelationLabel : LinkLabelModel
{
    public string RelationType { get; }

    public RelationLabel(BaseLinkModel parent, string content, string relationType)
        : base(parent, content)
    {
        this.RelationType = relationType;
    }
}