namespace ConceptMaps.UI.Services;

public interface IModelProvider
{
    IEnumerable<string> RelationsModels { get; }

    IEnumerable<string> NrtModels { get; }

    string? ActiveRelationsModel { get; }

    string? ActiveNrtModel { get; }
}