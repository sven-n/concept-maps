namespace ConceptMaps.UI.Services;

using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using QuikGraph;

/// <summary>
/// Simplified class definition for the <see cref="StandardLayoutAlgorithmFactory{TVertex,TEdge,TGraph}"/>
/// </summary>
public class StandardLayoutAlgorithmFactory : StandardLayoutAlgorithmFactory<
        NodeModel,
        TaggedEdge<NodeModel, string>,
        BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>>>,
    ILayoutAlgorithmFactory
{
}
