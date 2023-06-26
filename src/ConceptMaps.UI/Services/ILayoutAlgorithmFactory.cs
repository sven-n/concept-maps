namespace ConceptMaps.UI.Services;

using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using QuikGraph;

/// <summary>
/// Simplified interface definition for the <see cref="ILayoutAlgorithmFactory{TVertex,TEdge,TGraph}"/>
/// </summary>
public interface ILayoutAlgorithmFactory : ILayoutAlgorithmFactory<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>
{
}