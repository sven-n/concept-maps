namespace ConceptMaps.UI.Services;

using System.Text.Json;
using ConceptMaps.UI.Data;

/// <summary>
/// A manager for the prepared sentence training data (sessions on the data
/// preparation page) which saves the data in a json format.
/// </summary>
public class PrepareDataManager : IPrepareDataManager
{
    internal static readonly string SubFolder = "prepare-data";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets the available prepared sentence training data files.
    /// </summary>
    public IEnumerable<string> DataFiles
    {
        get
        {
            // todo: this must be improved...
            var folderPath = Path.Combine(Environment.CurrentDirectory, SubFolder, ModelType.Relation.AsString());
            if (!Directory.Exists(folderPath))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.EnumerateFiles(folderPath, "*.json").Where(name => !string.IsNullOrWhiteSpace(name)).Select(path => Path.GetFileName(path)!);
        }
    }

    /// <inheritdoc />
    public string GetFolderPath(ModelType modelType) => Path.Combine(Environment.CurrentDirectory, SubFolder, modelType.AsString());

    /// <inheritdoc />
    string IPrepareDataManager.SubFolder => SubFolder;

    /// <inheritdoc />
    public async Task SaveAsync(DataPrepareContext prepareContext)
    {
        var targetFolder = Path.Combine(Environment.CurrentDirectory, SubFolder, ModelType.Relation.AsString());
        Directory.CreateDirectory(targetFolder); // Ensure that the directory exists.
        await using var fileStream = File.Create(Path.Combine(targetFolder, prepareContext.Name + ".json"));
        await JsonSerializer.SerializeAsync(fileStream, prepareContext, SerializerOptions);
    }

    /// <inheritdoc />
    public async Task<DataPrepareContext?> LoadAsync(string fileName)
    {
        var targetFilePath = Path.Combine(Environment.CurrentDirectory, SubFolder, ModelType.Relation.AsString(), fileName);
        await using var fileStream = File.OpenRead(targetFilePath);
        var result = await JsonSerializer.DeserializeAsync<DataPrepareContext>(fileStream, SerializerOptions);
        return result;
    }
}