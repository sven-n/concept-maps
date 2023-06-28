namespace ConceptMaps.UI.Services;

public class TrainingDataProvider : ITrainingDataProvider
{
    public IEnumerable<string> RelationsDataFiles
    {
        get
        {
            var folderPath = Path.Combine("training-data", ModelType.Relation.AsString());
            return Directory.EnumerateFiles(folderPath, "*.json").Where(name => !string.IsNullOrWhiteSpace(name))!;
        }
    }

    public IEnumerable<string> NrtDataFiles
    {
        get
        {
            var folderPath = Path.Combine("training-data", ModelType.Nrt.AsString());
            return Directory.EnumerateFiles(folderPath, "*.json").Where(name => !string.IsNullOrWhiteSpace(name))!;
        }
    }