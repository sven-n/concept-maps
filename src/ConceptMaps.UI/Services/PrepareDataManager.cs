namespace ConceptMaps.UI.Services;

using System.Text.Json;
using ConceptMaps.UI.Data;

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
        foreach (var s in prepareContext.Sentences.Where(s => s.State == SentenceState.Reviewed).SelectMany(s => s.Relationships))
        {
            s.KnownRelationshipType = s.RelationshipTypeInSentence;
        }

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