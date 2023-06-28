namespace ConceptMaps.UI.Services;

// TODO: Get them over http from the python service, as the models may not run on the same system.
public class ModelProvider : IModelProvider
{
    public IEnumerable<string> RelationsModels
    {
        get
        {
            var folderPath = Path.Combine("../../models", ModelType.Relation.AsString());
            return Directory.EnumerateDirectories(folderPath).Select(Path.GetDirectoryName).Where(name => !string.IsNullOrWhiteSpace(name))!;
        }
    }

    public IEnumerable<string> NrtModels
    {
        get
        {
            var folderPath = Path.Combine("../../models", ModelType.Nrt.AsString());
            return Directory.EnumerateDirectories(folderPath).Select(Path.GetDirectoryName).Where(name => !string.IsNullOrWhiteSpace(name))!;
        }
    }

    public string? ActiveRelationsModel { get; set; }

    public string? ActiveNrtModel { get; set; }
}