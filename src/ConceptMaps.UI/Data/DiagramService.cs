namespace ConceptMaps.UI.Data;

using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
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
    /// Gets the available algorithm types.
    /// </summary>
    public IEnumerable<string> AlgorithmTypes => this._layoutAlgorithmFactory.AlgorithmTypes;

    /// <summary>
    /// Creates a new diagram based on the given triples.
    /// </summary>
    /// <param name="triples">The triples.</param>
    /// <returns>The created diagram.</returns>
    public Diagram CreateDiagram(IList<Triple> triples)
    {
        var diagram = new Diagram();

        var nodes = new Dictionary<string, NodeModel>();
        NodeModel AddIfNotExists(string word)
        {
            if (!nodes.TryGetValue(word, out var nodeModel))
            {
                nodeModel = new NodeModel(word)
                {
                    Title = word
                };

                

                nodes.Add(word, nodeModel);
                diagram.Nodes.Add(nodeModel);
            }

            return nodeModel;
        }

        foreach (var triple in triples)
        {
            var fromNode = AddIfNotExists(triple.FromWord);
            var toNode = AddIfNotExists(triple.ToWord);

            var link = new LinkModel(triple.ToString(), fromNode, toNode)
            {
                TargetMarker = LinkMarker.Arrow,
            };

            if (!string.IsNullOrWhiteSpace(triple.EdgeName))
            {
                link.Labels.Add(new LinkLabelModel(link, triple.EdgeName)
                {
                    Content = triple.EdgeName,
                });
            }

            diagram.Links.Add(link);
        }

        // diagram.ZoomToFit();
        return diagram;
    }

    public void ArrangeNodes(Diagram diagram, string algorithm)
    {
        var graph = MakeGraph(diagram);
        var nodes = diagram.Nodes.ToDictionary(node => node.Id, node => node);

        var nodePositions = CalculateNodePositions(graph, nodes, algorithm);
        foreach (var (id, node) in nodes)
        {
            if (nodePositions.TryGetValue(id, out var position))
            {
                node.Position = new Blazor.Diagrams.Core.Geometry.Point(position.X, position.Y);
                node.RefreshAll();
            }
        }

        diagram.Refresh();
        diagram.ZoomToFit();
    }

    private static BidirectionalGraph<string, TaggedEdge<string, string>> MakeGraph(Diagram diagram)
    {
        var graph = new BidirectionalGraph<string, TaggedEdge<string, string>>(true);

        foreach (var node in diagram.Nodes)
        {
            graph.AddVertex(node.Id);
        }

        foreach (var diagramLink in diagram.Links.Where(l => l.TargetNode is not null))
        {
            var edgeTitle = diagramLink.Labels.FirstOrDefault()?.Content ?? string.Empty;
            graph.AddEdge(new TaggedEdge<string, string>(diagramLink.SourceNode.Id, diagramLink.TargetNode!.Id, edgeTitle));
        }

        return graph;
    }

    private IDictionary<string, Point> CalculateNodePositions(BidirectionalGraph<string, TaggedEdge<string, string>> graph, Dictionary<string, NodeModel> nodes, string algorithmName)
    {
        var layoutContext = CreateLayoutContext(nodes, graph);
        var layoutParameters = this._layoutAlgorithmFactory.CreateParameters(algorithmName, null);
        if (this._layoutAlgorithmFactory.CreateAlgorithm(algorithmName, layoutContext, layoutParameters) is { } algorithm)
        {
            algorithm.Compute();
            return algorithm.VerticesPositions;
        }

        return new Dictionary<string, Point>();
    }
    
    private static LayoutContext<string, TaggedEdge<string, string>, BidirectionalGraph<string, TaggedEdge<string, string>>> CreateLayoutContext(Dictionary<string, NodeModel> nodes, BidirectionalGraph<string, TaggedEdge<string, string>> graph)
    {
        var positions = new Dictionary<string, Point>();
        var sizes = nodes.ToDictionary(kvp => kvp.Key, kvp => ConvertSize(kvp.Value.Size!));

        return new LayoutContext<string, TaggedEdge<string, string>, BidirectionalGraph<string, TaggedEdge<string, string>>>(graph, positions, sizes, LayoutMode.Simple);
    }

    private static GraphShape.Size ConvertSize(Size size) => new (size?.Width ?? 100, size?.Height ?? 80);
}
