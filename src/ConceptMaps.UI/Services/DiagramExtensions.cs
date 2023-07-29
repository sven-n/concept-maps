namespace ConceptMaps.UI.Services;

using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using ConceptMaps.UI.Components;
using QuikGraph;

/// <summary>
/// Extension methods for <see cref="Diagram"/>.
/// </summary>
public static class DiagramExtensions
{
    /// <summary>
    /// Creates a bidirectional graph (QuikGraph) which can be arranged by an algorithm,
    /// based on a blazor diagram.
    /// </summary>
    /// <param name="diagram">The blazor diagram.</param>
    /// <returns>The bidirectional graph.</returns>
    public static BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>> AsGraph(this Diagram diagram)
    {
        var graph = new BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>>(false);

        foreach (var node in diagram.Nodes)
        {
            graph.AddVertex(node);
        }

        foreach (var diagramLink in diagram.Links
                     .Where(l => l.TargetNode is not null)
                     .Where(l => l.Labels.Any())) // Siblings have no label
        {
            var relationType = diagramLink.Labels.OfType<RelationLabel>().FirstOrDefault()?.RelationType ?? string.Empty;
            graph.AddEdge(
                new TaggedEdge<NodeModel, string>(
                    diagramLink.TargetNode!,
                    diagramLink.SourceNode,
                    relationType));
        }

        return graph;
    }
}