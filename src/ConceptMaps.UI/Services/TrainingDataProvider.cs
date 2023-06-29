using System.Text.Json;
using ConceptMaps.UI.Data;

namespace ConceptMaps.UI.Services;

public class TrainingDataProvider : ITrainingDataProvider
{
    public IEnumerable<string> RelationsDataFiles
    {
        get
        {
            // todo: this must be improved...
            var folderPath = Path.Combine(Environment.CurrentDirectory, "training-data", ModelType.Relation.AsString());
            if (!Directory.Exists(folderPath))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.EnumerateFiles(folderPath, "*.json").Where(name => !string.IsNullOrWhiteSpace(name))!;
        }
    }

    public IEnumerable<string> NrtDataFiles
    {
        get
        {
            var folderPath = Path.Combine(Environment.CurrentDirectory, "training-data", ModelType.Nrt.AsString());
            if (!Directory.Exists(folderPath))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.EnumerateFiles(folderPath, "*.json").Where(name => !string.IsNullOrWhiteSpace(name))!;
        }
    }
}

public interface IPrepareDataManager
{
    IEnumerable<string> DataFiles { get; }

    // IEnumerable<string> NrtDataFiles { get; }

    Task SaveAsync(DataPrepareContext prepareContext);

    Task<DataPrepareContext?> LoadAsync(string fileName);
}

public class PrepareDataManager : IPrepareDataManager
{
    private static readonly string SubFolder = "prepare-data";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

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

    public async Task SaveAsync(DataPrepareContext prepareContext)
    {
        var targetFolder = Path.Combine(Environment.CurrentDirectory, SubFolder, ModelType.Relation.AsString());
        Directory.CreateDirectory(targetFolder); // Ensure that the directory exists.
        await using var fileStream = File.Create(Path.Combine(targetFolder, prepareContext.Name + ".json"));
        await JsonSerializer.SerializeAsync(fileStream, prepareContext, SerializerOptions);
    }

    public async Task<DataPrepareContext?> LoadAsync(string fileName)
    {
        var targetFilePath = Path.Combine(Environment.CurrentDirectory, SubFolder, ModelType.Relation.AsString(), fileName);
        await using var fileStream = File.OpenRead(targetFilePath);
        return await JsonSerializer.DeserializeAsync<DataPrepareContext>(fileStream, SerializerOptions);
    }
}