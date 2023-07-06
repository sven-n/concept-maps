namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;

public partial class RelationsTable
{
    [Parameter]
    [Required]
    public List<Relationship> Relationships { get; set; }

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
}
