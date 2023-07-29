namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ConceptMaps.DataModel;
using ConceptMaps.DataModel.Spacy;

/// <summary>
/// A table which displays a list of relationships.
/// </summary>
public partial class RelationsTable
{
    /// <summary>
    /// Gets or sets the relationships which should be displayed.
    /// </summary>
    [Parameter]
    [Required]
    public List<Relationship> Relationships { get; set; } = null!;

    /// <summary>
    /// Gets or sets the known relationships, usually retrieved by the web crawler.
    /// </summary>
    [Parameter]
    public List<Relationship>? KnownRelationships { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this component is read only.
    /// </summary>
    [Parameter]
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Called when the button to add a relationship is clicked.
    /// </summary>
    private void OnAddRelationshipClick()
    {
        this.Relationships.Add(new Relationship());
    }

    /// <summary>
    /// Called when the button to switch the entities is clicked.
    /// </summary>
    private void OnSwitchEntities(Relationship relationship)
    {
        (relationship.SecondEntity, relationship.FirstEntity)
            = (relationship.FirstEntity, relationship.SecondEntity);
    }

    /// <summary>
    /// Determines whether the selected <see cref="Relationship.RelationshipType"/> is probably wrong,
    /// by comparing it with the one found in <see cref="KnownRelationships"/>.
    /// <see cref="Relationship.RelationshipType"/> should either be <see cref="SpacyRelationLabel.Undefined"/>
    /// or match with the one in <see cref="KnownRelationships"/>.
    /// </summary>
    /// <param name="relationship">The relationship.</param>
    /// <returns>
    ///   <c>true</c> if the selected <see cref="Relationship.RelationshipType"/> is probably wrong; otherwise, <c>false</c>.
    /// </returns>
    private bool IsProbablyWrong(Relationship relationship)
    {
        var expectedRelationshipType = this.GetExpectedRelationshipType(relationship);
        return relationship.RelationshipType != expectedRelationshipType;
    }

    /// <summary>
    /// Gets the expected type of the relationship based on <see cref="KnownRelationships"/>.
    /// </summary>
    /// <param name="relationship">The relationship.</param>
    /// <returns>The expected type of the relationship based on <see cref="KnownRelationships"/>.</returns>
    private string GetExpectedRelationshipType(Relationship relationship)
    {
        if (relationship.IsUndefined())
        {
            return string.Empty;
        }

        if (this.KnownRelationships?.Count is null or 0)
        {
            return string.Empty;
        }

        var knownRelationship = this.KnownRelationships.Find(relationship.FirstEntity, relationship.SecondEntity);
        if (knownRelationship is null)
        {
            return string.Empty;
        }

        return knownRelationship.RelationshipType;
    }
}
