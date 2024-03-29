namespace ConceptMaps.UI.Services;

using System.Text;
using System.Text.Json;
using ConceptMaps.UI.Data;

public class TrainingDataManager : ITrainingDataManager
{
    internal static readonly string SubFolder = "training-data";

    public string GetFolderPath(ModelType modelType) => Path.Combine(Environment.CurrentDirectory, SubFolder, modelType.AsString());

    string ITrainingDataManager.SubFolder => SubFolder;

    public IEnumerable<string> RelationsDataFiles
    {
        get
        {
            var folderPath = GetFolderPath(ModelType.Relation);
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
            var folderPath = Path.Combine(Environment.CurrentDirectory, SubFolder, ModelType.Ner.AsString());
            if (!Directory.Exists(folderPath))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.EnumerateFiles(folderPath, "*.json").Where(name => !string.IsNullOrWhiteSpace(name))!;
        }
    }

    public async Task SaveRelationsAsync(DataPrepareContext prepareContext)
    {
        var reviewedData = prepareContext.GetReviewedData();
        var serializedData = JsonSerializer.Serialize(reviewedData.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var targetFolderPath = this.GetFolderPath(ModelType.Relation);
        Directory.CreateDirectory(targetFolderPath);
        
        // todo: replace invalid characters
        var fileName = prepareContext.Name + "_Sentences.json";
        var targetPath = Path.Combine(targetFolderPath, fileName);
        await File.WriteAllTextAsync(targetPath, serializedData, Encoding.UTF8);
    }
}