namespace ConceptMaps.UI.Pages;

using Blazor.Diagrams.Core;
using ConceptMaps.DataModel;
using ConceptMaps.DataModel.Spacy;
using ConceptMaps.UI.Services;

using Microsoft.AspNetCore.Components;

/// <summary>
/// The page which offers to generate the concept map based on a text.
/// </summary>
public partial class Generate
{
    /// <summary>
    /// The diagram with the generated graph.
    /// </summary>
    private Diagram? _diagram;

    /// <summary>
    /// Flag, if the generation is currently running.
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// The selected entity of the filter. Backing-Field for <see cref="SelectedEntity"/>.
    /// </summary>
    private string _selectedEntity = string.Empty;

    /// <summary>
    /// Gets or sets the text input which is bound to the input element.
    /// </summary>
    private string? TextInput { get; set; } = "As siblings, Bob and Alice share a special bond as the children of Jeff and Mary.";

    /// <summary>
    /// Gets or sets the injected triple service.
    /// </summary>
    [Inject]
    private RemoteTripleService RemoteTripleService { get; set; } = null!;

    /// <summary>
    /// Gets or sets the injected diagram service.
    /// </summary>
    [Inject]
    private DiagramService DiagramService { get; set; } = null!;

    /// <summary>
    /// Gets or sets the entities which are contained in the graph.
    /// </summary>
    private List<string> Entities { get; set; } = new();

    /// <summary>
    /// Gets or sets the relationships between the entities.
    /// </summary>
    private List<Relationship>? Relationships { get; set; }

    /// <summary>
    /// Gets the <see cref="Relationships"/>, filtered by <see cref="SelectedEntity"/>.
    /// It returns the relationships of all entities which are directly connected
    /// to the <see cref="SelectedEntity"/>.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the selected entity after which the graph should be filtered.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the error message which was generated when calling the triple service.
    /// </summary>
    private RenderFragment? ErrorMessage { get; set; }

    /// <summary>
    /// Called when the apply button was clicked.
    /// Applies the current state of the <see cref="FilteredRelationships"/> to the graph.
    /// </summary>
    private async Task OnApplyAsync()
    {
        this._diagram = null;
        await this.InvokeAsync(this.StateHasChanged);
        await Task.Delay(100);
        await this.CreateDiagramAsync();
    }

    /// <summary>
    /// Called when the run button was clicked. Generates the concept map by
    /// first getting the triples and then creating the diagram.
    /// </summary>
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

    /// <summary>
    /// Creates the diagram based on the <see cref="FilteredRelationships"/>.
    /// </summary>
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

    /// <summary>
    /// Arranges the nodes by applying the <see cref="FamilyTreeLayoutAlgorithm"/>.
    /// </summary>
    private async Task ArrangeNodesAsync()
    {
        if (this._diagram is { } diagram)
        {
            diagram.Batch(() => this.DiagramService.ArrangeNodes(diagram, "FamilyTree"));
        }

        await this.InvokeAsync(this.StateHasChanged);
    }
}
