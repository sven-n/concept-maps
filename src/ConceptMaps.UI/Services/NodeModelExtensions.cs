namespace ConceptMaps.UI.Services;

using Blazor.Diagrams.Core.Models;

public static class NodeModelExtensions
{
    public static PortModel GetOrAddPort(this NodeModel nodeModel, PortAlignment portAlignment)
    {
        return nodeModel.GetPort(portAlignment) ?? nodeModel.AddPort(portAlignment);
    }
}