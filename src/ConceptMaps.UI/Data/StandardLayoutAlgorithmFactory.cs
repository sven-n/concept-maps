namespace ConceptMaps.UI.Data;

using GraphShape.Algorithms.Layout;
using QuikGraph;

/// <summary>
/// Simplified class definition for the <see cref="StandardLayoutAlgorithmFactory{TVertex,TEdge,TGraph}"/>
/// </summary>
public class StandardLayoutAlgorithmFactory : StandardLayoutAlgorithmFactory<
        string,
        TaggedEdge<string, string>,
        BidirectionalGraph<string, TaggedEdge<string, string>>>,
    ILayoutAlgorithmFactory
{
}