namespace ConceptMaps.UI.Pages;

using Blazor.Diagrams.Core;
using ConceptMaps.DataModel;
using ConceptMaps.DataModel.Spacy;
using ConceptMaps.UI.Services;

using Microsoft.AspNetCore.Components;

public partial class Generate
{
    private Diagram? _diagram;
    private bool _isRunning;
    private string _selectedEntity = string.Empty;

    private string? TextInput { get; set; } = "As siblings, Bob and Alice share a special bond as the children of Jeff and Mary.";

    [Inject]
    private RemoteTripleService RemoteTripleService { get; set; } = null!;

    [Inject]
    private DiagramService DiagramService { get; set; } = null!;

    private List<string> Entities { get; set; } = new();

    private List<Relationship>? Relationships { get; set; }

    private IEnumerable<Relationship> FilteredRelationships
    {
        get
        {
            if (this.Relationships is null)
            {
                return Enumerable.Empty<Relationship>();
            }

            if (string.IsNullOrWhiteSpace(this.SelectedEntity))
            {
                return this.Relationships
                    .Where(rel => !rel.IsUndefined());
            }

            var filteredRelationships = this.Relationships
                .Where(rel => !rel.IsUndefined())
                .Where(rel => rel.FirstEntity == this.SelectedEntity || rel.SecondEntity == this.SelectedEntity)
                .ToList();
            var additionalLinks = this.Relationships
                .Where(rel => this.Entities.Contains(rel.FirstEntity)
                              && this.Entities.Contains(rel.SecondEntity)
                              && !filteredRelationships.Contains(rel))
                .ToList();
            filteredRelationships.AddRange(additionalLinks);
            return filteredRelationships;
        }
    }

    private string SelectedEntity
    {
        get => _selectedEntity;
        set
        {
            if (value == this._selectedEntity)
            {
                return;
            }

            this._selectedEntity = value;
            this.InvokeAsync(this.CreateDiagramAsync);
        }
    }

    private RenderFragment? ErrorMessage { get; set; }

    private async Task OnApplyAsync()
    {
        this._diagram = null;
        await this.InvokeAsync(this.StateHasChanged);
        await Task.Delay(100);
        await this.CreateDiagramAsync();
    }

    private async Task OnRunAsync()
    {
        this._diagram = null;
        this.ErrorMessage = null;

        this._isRunning = true;
        try
        {
            this.StateHasChanged();
            try
            {
                var triples = await this.RemoteTripleService.GenerateTriplesAsync(this.TextInput);
                this.Entities = triples.Select(t => t.FromWord).Concat(triples.Select(t => t.ToWord)).Distinct().Order().ToList();
                this.Relationships = triples
                    .Select(t => new Relationship
                    {
                        FirstEntity = t.FromWord,
                        RelationshipType = t.EdgeName ?? SpacyRelationLabel.Undefined,
                        SecondEntity = t.ToWord,
                        Score = t.Score,
                    })
                    .ToList();
                await this.CreateDiagramAsync();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = builder => builder.AddMarkupContent(
                    0,
                    $"Unexpected error occurred when generating triples: {ex}".ReplaceLineEndings("<br>"));
            }
        }
        finally
        {
            this._isRunning = false;
        }
    }

    private async Task CreateDiagramAsync()
    {
        try
        {
            if (this._diagram is { } oldDiagram)
            {
                this._diagram = null;
                oldDiagram.Links.Clear();
                oldDiagram.Nodes.Clear();
                await this.InvokeAsync(this.StateHasChanged);
                await Task.Delay(200);
            }
            this._diagram = this.DiagramService.CreateDiagram(this.FilteredRelationships.ToList());

            // This is required to render it once, so we can arrange the elements in the next step
            await this.InvokeAsync(this.StateHasChanged);
            await Task.Delay(200);
            await this.ArrangeNodesAsync();
        }
        catch (Exception ex)
        {
            this.ErrorMessage = builder => builder.AddMarkupContent(
                0,
                $"Unexpected error occurred when generating or arranging the diagram: {ex}".ReplaceLineEndings("<br>"));
        }
    }

    private async Task ArrangeNodesAsync()
    {
        if (this._diagram is { } diagram)
        {
            diagram.Batch(() => this.DiagramService.ArrangeNodes(diagram, "FamilyTree"));
        }

        await this.InvokeAsync(this.StateHasChanged);
    }
}
