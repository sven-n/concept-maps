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

        // temporary fix for zooming exceptions, see https://github.com/Blazor-Diagrams/Blazor.Diagrams/issues/322
        diagram.Options.EnableVirtualization = false;

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
        
        var nodePositions = CalculateNodePositions(graph, algorithm);
        foreach (var (node, position) in nodePositions)
        {
            node.Position = new Blazor.Diagrams.Core.Geometry.Point(position.X, position.Y);
            node.RefreshAll();
        }

        diagram.Refresh();
        diagram.ZoomToFit();
    }

    private static BidirectionalGraph<NodeModel, Edge<NodeModel>> MakeGraph(Diagram diagram)
    {
        var graph = new BidirectionalGraph<NodeModel, Edge<NodeModel>>(true);

        foreach (var node in diagram.Nodes)
        {
            graph.AddVertex(node);
        }

        foreach (var diagramLink in diagram.Links.Where(l => l.TargetNode is not null))
        {
            graph.AddEdge(new Edge<NodeModel>(diagramLink.SourceNode, diagramLink.TargetNode!));
        }

        return graph;
    }

    private IDictionary<NodeModel, Point> CalculateNodePositions(BidirectionalGraph<NodeModel, Edge<NodeModel>> graph, string algorithmName)
    {
        var layoutContext = CreateLayoutContext(graph);
        var layoutParameters = this._layoutAlgorithmFactory.CreateParameters(algorithmName, null);
        AdaptLayoutParameters(layoutParameters);

        if (this._layoutAlgorithmFactory.CreateAlgorithm(algorithmName, layoutContext, layoutParameters) is { } algorithm)
        {
            algorithm.Compute();
            return algorithm.VerticesPositions;
        }

        return new Dictionary<NodeModel, Point>();
    }

    private static void AdaptLayoutParameters(ILayoutParameters layoutParameters)
    {
        // TODO: Make configurable on the UI
        switch (layoutParameters)
        {
            case SimpleTreeLayoutParameters treeParameters:
                treeParameters.LayerGap = 50;
                treeParameters.VertexGap = 50;
                treeParameters.Direction = LayoutDirection.TopToBottom;
                break;
            case BalloonTreeLayoutParameters balloonTreeParameters:
                balloonTreeParameters.MinRadius = 50;
                break;
            case BoundedFRLayoutParameters boundedFrLayoutParameters:
                boundedFrLayoutParameters.Height = GraphicConstants.GraphicsHeight;
                boundedFrLayoutParameters.Width = GraphicConstants.GraphicsWidth;
                break;
            case ISOMLayoutParameters isoMLayoutParameters:
                isoMLayoutParameters.MinRadius = 300;
                isoMLayoutParameters.InitialRadius = 300;
                isoMLayoutParameters.Height = GraphicConstants.GraphicsHeight;
                isoMLayoutParameters.Width = GraphicConstants.GraphicsWidth;
                break;
            case KKLayoutParameters kkLayoutParameters:
                kkLayoutParameters.Height = GraphicConstants.GraphicsHeight;
                kkLayoutParameters.Width = GraphicConstants.GraphicsWidth;
                break;
            case LinLogLayoutParameters linLogLayoutParameters:
                // LinLogLayoutParameters.AttractionExponent = 20;
                linLogLayoutParameters.GravitationMultiplier = 0.01;
                break;
            case SugiyamaLayoutParameters sugiyamaLayoutParameters:
                sugiyamaLayoutParameters.LayerGap = 50;
                sugiyamaLayoutParameters.SliceGap = 50;
                sugiyamaLayoutParameters.WidthPerHeight = GraphicConstants.GraphicsWidthCm / GraphicConstants.GraphicsHeightCm;
                break;
            case RandomLayoutParameters randomLayoutParameters:
                randomLayoutParameters.Width = GraphicConstants.GraphicsWidth;
                randomLayoutParameters.Height = GraphicConstants.GraphicsHeight;
                break;
            case CompoundFDPLayoutParameters compoundFdpLayoutParameters:
                compoundFdpLayoutParameters.IdealEdgeLength = 75;
                break;
        }
    }

    private static LayoutContext<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>> CreateLayoutContext(BidirectionalGraph<NodeModel, Edge<NodeModel>> graph)
    {
        var positions = new Dictionary<NodeModel, Point>();
        var sizes = graph.Vertices.ToDictionary(node => node, node => ConvertSize(node.Size!));

        return new LayoutContext<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>(graph, positions, sizes, LayoutMode.Simple);
    }

    private static GraphShape.Size ConvertSize(Size size) => new (size?.Width ?? 100, size?.Height ?? 80);
}
