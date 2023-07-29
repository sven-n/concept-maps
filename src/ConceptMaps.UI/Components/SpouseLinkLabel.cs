namespace ConceptMaps.UI.Components;

using Blazor.Diagrams.Core.Models.Base;
using ConceptMaps.DataModel.Spacy;

public class SpouseLinkLabel : RelationLabel
{
    public SpouseLinkLabel(BaseLinkModel parent, string content)
        : base(parent, content, SpacyRelationLabel.Spouse)
    {
    }
}