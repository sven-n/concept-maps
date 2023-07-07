namespace ConceptMaps.UI.Services;

using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using ConceptMaps.UI.Components;
using ConceptMaps.DataModel;
using ConceptMaps.DataModel.Spacy;
using GraphShape;
using GraphShape.Algorithms.Layout;
using QuikGraph;
using Size = Blazor.Diagrams.Core.Geometry.Size;

/// <summary>
/// Provides services for <see cref="Diagram"/>.
/// </summary>
public class DiagramService
{
    private readonly ILayoutAlgorithmFactory _layoutAlgorithmFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramService"/> class.
    /// </summary>
    /// <param name="layoutAlgorithmFactory">The layout algorithm factory.</param>
    public DiagramService(ILayoutAlgorithmFactory layoutAlgorithmFactory)
    {
        this._layoutAlgorithmFactory = layoutAlgorithmFactory;
    }

    /// <summary>
    /// Creates a new diagram based on the given triples.
    /// </summary>
    /// <param name="relationships">The triples.</param>
    /// <returns>The created diagram.</returns>
    public Diagram CreateDiagram(IList<Relationship> relationships)
    {
        var diagram = new Diagram();
        diagram.RegisterModelComponent<SpouseLinkLabel, SpouseLinkLabelWidget>();

        // temporary fix for zooming exceptions, see https://github.com/Blazor-Diagrams/Blazor.Diagrams/issues/322
        diagram.Options.EnableVirtualization = false;
        diagram.Options.GridSize = 10; // For the snapping

        var nodes = new Dictionary<string, NodeModel>();
        NodeModel AddIfNotExists(string word)
        {
            if (!nodes.TryGetValue(word, out var nodeModel))
            {
                nodeModel = new NodeModel(word)
                {
                    Title = word,
                };

                nodes.Add(word, nodeModel);
                diagram.Nodes.Add(nodeModel);
            }

            return nodeModel;
        }

        foreach (var relationship in relationships)
        {
            var fromNode = AddIfNotExists(relationship.FirstEntity);
            var toNode = AddIfNotExists(relationship.SecondEntity);

            LinkModel link;
            if (relationship.IsSpouse())
            {
                link = new LinkModel(relationship.ToString(), fromNode, toNode)
                {
                    PathGenerator = PathGenerators.Straight,
                };
                link.Labels.Add(new SpouseLinkLabel(link, "💍"));
            }
            else if (relationship.IsChildren())
            {
                link = new LinkModel(relationship.ToString(), fromNode, toNode);
                link.SourceMarker = LinkMarker.Arrow;
                link.Labels.Add(new RelationLabel(link, GetEdgeCaption(relationship.RelationshipType), relationship.RelationshipType));
            }
            else if (relationship.IsSiblings())
            {
                link = new LinkModel(relationship.ToString(), fromNode, toNode)
                {
                    Color = "LightGray",
                };
                link.Labels.Add(new RelationLabel(link, GetEdgeCaption(relationship.RelationshipType), relationship.RelationshipType));
            }
            else
            {
                link = new LinkModel(relationship.ToString(), fromNode, toNode);
                link.Labels.Add(new RelationLabel(link, GetEdgeCaption(relationship.RelationshipType), relationship.RelationshipType));
            }

            diagram.Links.Add(link);
        }

        return diagram;
    }

    public void ArrangeNodes(Diagram diagram, string algorithm)
    {
        var graph = MakeGraph(diagram);

        var nodePositions = CalculateNodePositions(graph, algorithm);
        foreach (var (node, position) in nodePositions)
        {
            node.Position = new Blazor.Diagrams.Core.Geometry.Point(position.X, position.Y);
            node.Refresh();
        }

        RemoveSiblingLinksWithParents(diagram);
        RemoveChildrenLabels(diagram);

        diagram.ZoomToFit();
        diagram.Refresh();
    }

    /// <summary>
    /// Clean up unneeded children labels. The relations between parents and childrens are
    /// visible through the arrow marker.
    /// </summary>
    /// <param name="diagram">The diagram.</param>
    private static void RemoveChildrenLabels(Diagram diagram)
    {
        var links = diagram.Links
            .Select(l => (Link: l, l.SourceNode, TargetNode: l.TargetNode!, RelationType: l.Labels.OfType<RelationLabel>().FirstOrDefault()?.RelationType));
        var childrenLinks = links
            .Where(l => l.RelationType == SpacyRelationLabel.Children)
            .ToList();
        foreach (var childrenLink in childrenLinks)
        {
            childrenLink.Link.Labels.Clear();
        }
    }

    /// <summary>
    /// lean up unneeded links. The relation between siblings is already visible
    /// through their parent-child connections, so we can remove them.
    /// </summary>
    /// <param name="diagram">The diagram.</param>
    private static void RemoveSiblingLinksWithParents(Diagram diagram)
    {
        var links = diagram.Links
            .Select(l => (Link: l, l.SourceNode, TargetNode: l.TargetNode!, RelationType: l.Labels.OfType<RelationLabel>().FirstOrDefault()?.RelationType))
            .ToList();
        var childrenWithParents = links
            .Where(l => l.RelationType == SpacyRelationLabel.Children)
            .Select(l => l.SourceNode)
            .ToHashSet();
        var siblingLinksWithParents = links
            .Where(l => l.RelationType == SpacyRelationLabel.Siblings)
            .Where(l => childrenWithParents.Contains(l.SourceNode) && childrenWithParents.Contains(l.TargetNode))
            .Select(l => l.Link)
            .ToList();
        diagram.Links.Remove(siblingLinksWithParents);
        
        foreach (var valueTuple in links.Where(l => l.RelationType == SpacyRelationLabel.Siblings))
        {
            valueTuple.Link.Labels.Clear();
        }
    }

    private static Blazor.Diagrams.Core.Geometry.Point GetVerticePosition(PortModel port, int addX, int addY)
    {
        var sourcePosition = port.Parent.Position.Add(port.MiddlePosition.X, port.MiddlePosition.Y);
        return sourcePosition.Add(addX, addY);
    }

    private static string GetEdgeCaption(string label)
    {
        switch (label)
        {
            case "CHILDREN": return "child";
            case "SPOUSE": return "is married with";
            case "SIBLINGS": return "siblings";
        }

        return label;
    }

    private static BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>> MakeGraph(Diagram diagram)
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

    private IDictionary<NodeModel, Point> CalculateNodePositions(BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>> graph, string algorithmName)
    {
        var layoutContext = CreateLayoutContext(graph);
        var layoutParameters = _layoutAlgorithmFactory.CreateParameters(algorithmName, null);
        AdaptLayoutParameters(layoutParameters);

        if (_layoutAlgorithmFactory.CreateAlgorithm(algorithmName, layoutContext, layoutParameters) is { } algorithm)
        {
            algorithm.Compute();
            return algorithm.VerticesPositions;
        }

        return new Dictionary<NodeModel, Point>();
    }

    private static void AdaptLayoutParameters(ILayoutParameters layoutParameters)
    {
        if (layoutParameters is SimpleTreeLayoutParameters treeParameters)
        {
            treeParameters.LayerGap = 100;
            treeParameters.VertexGap = 100;
            treeParameters.Direction = LayoutDirection.TopToBottom;
            treeParameters.SpanningTreeGeneration = SpanningTreeGeneration.DFS;
        }
    }

    private static LayoutContext<NodeModel, TaggedEdge<NodeModel, string>, BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>>> CreateLayoutContext(BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>> graph)
    {
        var positions = new Dictionary<NodeModel, Point>();
        var sizes = graph.Vertices.ToDictionary(node => node, node => ConvertSize(node.Size!));

        return new LayoutContext<NodeModel, TaggedEdge<NodeModel, string>, BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>>>(graph, positions, sizes, LayoutMode.Simple);
    }

    private static GraphShape.Size ConvertSize(Size size) => new(size?.Width ?? 100, size?.Height ?? 80);
}