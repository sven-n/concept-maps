namespace ConceptMaps.UI.Data;

using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using QuikGraph;

/// <summary>
/// Simplified class definition for the <see cref="StandardLayoutAlgorithmFactory{TVertex,TEdge,TGraph}"/>
/// </summary>
public class StandardLayoutAlgorithmFactory : StandardLayoutAlgorithmFactory<
        NodeModel,
        Edge<NodeModel>,
        BidirectionalGraph<NodeModel, Edge<NodeModel>>>,
    ILayoutAlgorithmFactory
{
}