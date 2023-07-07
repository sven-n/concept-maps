namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ConceptMaps.DataModel;

public partial class RelationsTable
{
    [Parameter]
    [Required]
    public List<Relationship> Relationships { get; set; }

    [Parameter]
    public List<Relationship>? KnownRelationships { get; set; }

    [Parameter]
    public bool IsReadOnly { get; set; }

    private void OnAddRelationshipClick()
    {
        this.Relationships.Add(new Relationship());
    }

    private void OnSwitchEntities(Relationship relationship)
    {
        (relationship.SecondEntity, relationship.FirstEntity)
            = (relationship.FirstEntity, relationship.SecondEntity);
    }

    private bool IsProbablyWrong(Relationship relationship)
    {
        if (relationship.IsUndefined())
        {
            return false;
        }

        if (this.KnownRelationships?.Count is null or 0)
        {
            return false;
        }

        var knownRelationship = this.KnownRelationships.Find(relationship.FirstEntity, relationship.SecondEntity);
        if (knownRelationship is null)
        {
            return false;
        }

        return relationship.RelationshipType != knownRelationship.RelationshipType && !relationship.IsUndefined();
    }

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
