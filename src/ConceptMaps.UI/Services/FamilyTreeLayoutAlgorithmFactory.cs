namespace ConceptMaps.UI.Services;

using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using QuikGraph;

/// <summary>
/// Factory for the <see cref="FamilyTreeLayoutAlgorithm"/>.
/// </summary>
public class FamilyTreeLayoutAlgorithmFactory : ILayoutAlgorithmFactory
{
    /// <inheritdoc />
    public IEnumerable<string> AlgorithmTypes { get; } = new[] { "FamilyTree" };

    /// <inheritdoc />
    public ILayoutAlgorithm<NodeModel, TaggedEdge<NodeModel, string>, BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>>> CreateAlgorithm(string algorithmType, ILayoutContext<NodeModel, TaggedEdge<NodeModel, string>, BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>>> context, ILayoutParameters parameters)
    {
        return new FamilyTreeLayoutAlgorithm(
            context.Graph,
            context.Positions,
            context.Sizes,
            parameters as SimpleTreeLayoutParameters);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool IsValidAlgorithm(string algorithmType)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public string GetAlgorithmType(ILayoutAlgorithm<NodeModel, TaggedEdge<NodeModel, string>, BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>>> algorithm)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool NeedEdgeRouting(string algorithmType)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool NeedOverlapRemoval(string algorithmType)
    {
        throw new NotImplementedException();
    }
}