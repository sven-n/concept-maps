namespace ConceptMaps.UI.Services;

public interface IModelProvider
{
    IEnumerable<string> RelationsModels { get; }

    IEnumerable<string> NerModels { get; }

    string? ActiveRelationsModel { get; }

    string? ActiveNrtModel { get; }
}