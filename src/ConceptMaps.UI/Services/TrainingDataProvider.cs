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