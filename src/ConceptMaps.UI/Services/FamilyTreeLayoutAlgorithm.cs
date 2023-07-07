namespace ConceptMaps.UI.Services;

using ConceptMaps.DataModel.Spacy;
using GraphShape;
using GraphShape.Algorithms.Layout;
using Node = Blazor.Diagrams.Core.Models.NodeModel;
using Edge = QuikGraph.TaggedEdge<Blazor.Diagrams.Core.Models.NodeModel, string>;
using Graph = QuikGraph.BidirectionalGraph<Blazor.Diagrams.Core.Models.NodeModel, QuikGraph.TaggedEdge<Blazor.Diagrams.Core.Models.NodeModel, string>>;

public class FamilyTreeLayoutAlgorithmFactory : ILayoutAlgorithmFactory
{
    public IEnumerable<string> AlgorithmTypes { get; } = new[] { "FamilyTree" };

    public ILayoutAlgorithm<Node, Edge, Graph> CreateAlgorithm(string algorithmType, ILayoutContext<Node, Edge, Graph> context, ILayoutParameters parameters)
    {
        return new FamilyTreeLayoutAlgorithm(
            context.Graph,
            context.Positions,
            context.Sizes,
            parameters as SimpleTreeLayoutParameters);
    }

    public ILayoutParameters CreateParameters(string algorithmType, ILayoutParameters parameters)
    {
        return new SimpleTreeLayoutParameters
        {
            LayerGap = 100,
            VertexGap = 100,
            Direction = LayoutDirection.BottomToTop,
            SpanningTreeGeneration = SpanningTreeGeneration.DFS
        };
    }

    public bool IsValidAlgorithm(string algorithmType)
    {
        throw new NotImplementedException();
    }

    public string GetAlgorithmType(ILayoutAlgorithm<Node, Edge, Graph> algorithm)
    {
        throw new NotImplementedException();
    }

    public bool NeedEdgeRouting(string algorithmType)
    {
        throw new NotImplementedException();
    }

    public bool NeedOverlapRemoval(string algorithmType)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Family tree layout algorithm.
/// </summary>
public class FamilyTreeLayoutAlgorithm : DefaultParameterizedLayoutAlgorithmBase<Node, Edge, Graph, SimpleTreeLayoutParameters>
{
    private sealed class Layer
    {
        public double Size { get; set; }

        public double NextPosition { get; set; }

        public IList<Node> Vertices { get; } = new List<Node>();

        public double LastTranslate { get; set; }

        public Layer()
        {
            this.LastTranslate = 0;
        }
    }

    private sealed class VertexData
    {
        public (Node?, Node?) Parents { get; init; }

        public double Translate { get; set; }

        public double Position { get; set; }
    }

    private readonly IDictionary<Node, Size> _verticesSizes;

    private readonly IDictionary<Node, VertexData> _data = new Dictionary<Node, VertexData>();

    private readonly IList<Layer> _layers = new List<Layer>();

    private int _direction;

    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyTreeLayoutAlgorithm"/> class.
    /// </summary>
    /// <param name="visitedGraph">Graph to layout.</param>
    /// <param name="verticesSizes">Vertices sizes.</param>
    /// <param name="parameters">Optional algorithm parameters.</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="verticesSizes"/> is <see langword="null"/>.</exception>
    public FamilyTreeLayoutAlgorithm(
        Graph visitedGraph,
        IDictionary<Node, Size> verticesSizes,
        SimpleTreeLayoutParameters? parameters = null)
        : this(visitedGraph, verticesPositions: null, verticesSizes, parameters)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FamilyTreeLayoutAlgorithm"/> class.
    /// </summary>
    /// <param name="visitedGraph">Graph to layout.</param>
    /// <param name="verticesPositions">Vertices positions.</param>
    /// <param name="verticesSizes">Vertices sizes.</param>
    /// <param name="parameters">Optional algorithm parameters.</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="visitedGraph"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="verticesSizes"/> is <see langword="null"/>.</exception>
    public FamilyTreeLayoutAlgorithm(
        Graph visitedGraph,
        IDictionary<Node, Point>? verticesPositions,
        IDictionary<Node, Size> verticesSizes,
        SimpleTreeLayoutParameters? parameters = null)
        : base(visitedGraph, verticesPositions, parameters)
    {
        this._verticesSizes = new Dictionary<Node, Size>(verticesSizes);
    }
    
    /// <inheritdoc />
    protected override void InternalCompute()
    {
        if (this.VisitedGraph.VertexCount == 0)
        {
            return;
        }

        this._direction = this.Parameters.Direction == LayoutDirection.BottomToTop ? -1 : 1;

        var layeredNodes = this.GetParentsWithoutBeingChild();
        this.AddChildren(layeredNodes);
        this.AddSpouses(layeredNodes);
        this.AddLeftovers(layeredNodes);

        for (var index = 0; index < layeredNodes.Count; index++)
        {
            var layerNodes = layeredNodes[index];
            var nodesGroupedByParents = layerNodes
                .GroupBy(n => this.GetTuple(this.VisitedGraph.InEdges(n).Where(e => e.Tag == SpacyRelationLabel.Children).Select(e => e.Source).OrderBy(e => e.Id)))
                .ToList();
            foreach (var grouped in nodesGroupedByParents)
            {
                var isFirstChild = true;
                foreach (var child in grouped)
                {
                    this.CalculatePosition(child, grouped.Key, index, !isFirstChild, isFirstChild && grouped.Key != default);
                    isFirstChild = false;
                    var spouse = this.VisitedGraph.OutEdges(child).FirstOrDefault(n => n.Tag == SpacyRelationLabel.Spouse)?.Target
                                 ?? this.VisitedGraph.InEdges(child).FirstOrDefault(n => n.Tag == SpacyRelationLabel.Spouse)?.Source;
                    if (spouse is not null)
                    {
                        this.CalculatePosition(spouse, grouped.Key, index, true, false);
                    }
                }
            }
        }

        this.AssignPositions();
    }

    private (Node?, Node?) GetTuple(IEnumerable<Node> nodes)
    {
        nodes = nodes.Take(2);
        var first = nodes.FirstOrDefault();
        var second = nodes.LastOrDefault();
        return (first, second);
    }

    internal void AddChildren(List<HashSet<Node>> layeredNodes)
    {
        for (var layerIndex = 0; layerIndex < layeredNodes.Count; layerIndex++)
        {
            var nodes = layeredNodes[layerIndex];
            HashSet<Node>? nextLayer = null;
            foreach (var parentNode in nodes)
            {
                var childNodes = this.VisitedGraph
                    .OutEdges(parentNode)
                    .Where(edge => edge.Tag == SpacyRelationLabel.Children)
                    .Select(edge => edge.Target);

                foreach (var child in childNodes)
                {
                    nextLayer ??= GetOrCreateLayer(layerIndex + 1);
                    nextLayer.Add(child);
                }
            }
        }

        HashSet<Node> GetOrCreateLayer(int layerIndex)
        {
            if (layerIndex < layeredNodes.Count)
            {
                return layeredNodes[layerIndex];
            }

            var hashSet = new HashSet<Node>();
            layeredNodes.Add(hashSet);
            return hashSet;
        }
    }

    internal void AddLeftovers(List<HashSet<Node>> layeredNodes)
    {
        var processedNodes = layeredNodes.SelectMany(n => n).ToHashSet();
        
        var leftOvers = this.VisitedGraph.Vertices.Where(n => !processedNodes.Contains(n)).ToList();
        if (leftOvers.Count == 0)
        {
            return;
        }

        var layer = new HashSet<Node>(leftOvers);
        layeredNodes.Add(layer);
    }

    internal void AddSpouses(List<HashSet<Node>> layeredNodes)
    {
        for (var layerIndex = 0; layerIndex < layeredNodes.Count; layerIndex++)
        {
            var currentLayer = layeredNodes[layerIndex];
            foreach (var node in currentLayer.ToList())
            {
                var spouseNodesOut = this.VisitedGraph
                    .OutEdges(node)
                    .Where(edge => edge.Tag == SpacyRelationLabel.Spouse)
                    .Select(edge => edge.Target);
                var spouseNodesIn = this.VisitedGraph
                    .InEdges(node)
                    .Where(edge => edge.Tag == SpacyRelationLabel.Spouse)
                    .Select(edge => edge.Source);
                var spouses = spouseNodesOut.Concat(spouseNodesIn).Distinct();

                foreach (var spouse in spouses)
                {
                    currentLayer.Add(spouse);
                }
            }
        }
    }

    internal List<HashSet<Node>> GetParentsWithoutBeingChild()
    {
        // First try to find the longest parent/child chain...
        var parentNodes = this.VisitedGraph.Vertices
            .Where(node => this.VisitedGraph.OutEdges(node).Any(edge => edge.Tag == SpacyRelationLabel.Children)) // has children
            .Where(node => !this.VisitedGraph.InEdges(node).Any(edge => edge.Tag == SpacyRelationLabel.Children)) // without being a child itself
            .Select(node => (Node: node, Deepness: GetParentalDeepness(node))) // count how deep is the hierarchy
            .ToList();
        if (parentNodes.Count == 0)
        {
            return new List<HashSet<Node>>();
        }

        var maxDeepness = parentNodes.Max(node => node.Deepness);
        var result = parentNodes
            .GroupBy(tuple => tuple.Deepness)
            .OrderBy(g => maxDeepness - g.Key)
            .Select(g => g.Select(l => l.Node).Distinct().ToHashSet())
            .ToList();
        return result;

        int GetParentalDeepness(Node node, int current = 0)
        {
            int maximum = current;
            foreach (var edge in this.VisitedGraph.OutEdges(node).Where(e => e.Tag == SpacyRelationLabel.Children))
            {
                maximum = Math.Max(current, GetParentalDeepness(edge.Target, current + 1));
            }

            return maximum;
        }
    }

    private double CalculatePosition(Node node, (Node?, Node?) parents, int layerIndex, bool isSpouse, bool isSiblingOfSameParents)
    {
        if (this._data.ContainsKey(node))
        {
            return -1; // This vertex is already layed out
        }

        while (layerIndex >= this._layers.Count)
        {
            this._layers.Add(new Layer());
        }

        var layer = this._layers[layerIndex];
        var size = this._verticesSizes[node];
        var vertexData = new VertexData { Parents = parents };
        this._data[node] = vertexData;

        layer.NextPosition += size.Width / 2.0;
        if (isSpouse || isSiblingOfSameParents)
        {
            layer.NextPosition -= this.Parameters.VertexGap / 2;
        }

        layer.Size = Math.Max(layer.Size, size.Height + this.Parameters.LayerGap);
        layer.Vertices.Add(node);

        vertexData.Translate = Math.Max(layer.NextPosition - vertexData.Position, 0);

        layer.LastTranslate = vertexData.Translate;
        vertexData.Position += vertexData.Translate;
        layer.NextPosition =vertexData.Position + size.Width / 2.0 + this.Parameters.VertexGap;

        return vertexData.Position;
    }

    private void AssignPositions()
    {
        double layerOffset = 0;

        foreach (var layer in this._layers)
        {
            //double xOffset
            foreach (var vertex in layer.Vertices)
            {
                this.ThrowIfCancellationRequested();

                var size = this._verticesSizes[vertex];
                var vertexData = this._data[vertex];
                this.VerticesPositions[vertex] = new Point(vertexData.Position, this._direction * (layerOffset + size.Height / 2.0));
            }

            layerOffset += layer.Size;
        }

        if (this._direction < 0)
        {
            this.NormalizePositions();
        }
    }
}
