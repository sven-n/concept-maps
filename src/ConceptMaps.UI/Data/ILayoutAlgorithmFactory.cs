namespace ConceptMaps.UI.Data;

using GraphShape.Algorithms.Layout;
using QuikGraph;

/// <summary>
/// Simplified interface definition for the <see cref="ILayoutAlgorithmFactory{TVertex,TEdge,TGraph}"/>
/// </summary>
public interface ILayoutAlgorithmFactory : ILayoutAlgorithmFactory<string, TaggedEdge<string, string>, BidirectionalGraph<string, TaggedEdge<string, string>>>
{
}