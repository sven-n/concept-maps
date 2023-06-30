using ConceptMaps.UI.Data;

namespace ConceptMaps.UI.Services;

public interface ITrainingDataManager
{
    IEnumerable<string> RelationsDataFiles { get; }

    IEnumerable<string> NrtDataFiles { get; }

    string SubFolder { get; }

    string GetFolderPath(ModelType modelType);

    Task SaveRelationsAsync(DataPrepareContext prepareContext);
}

public static class TrainingDataProviderExtensions
{
    public static IEnumerable<string> GetFiles(this ITrainingDataManager dataManager, ModelType modelType)
    {
        if (modelType is ModelType.Nrt)
        {
            return dataManager.NrtDataFiles;
        }
        
        if (modelType is ModelType.Relation)
        {
            return dataManager.RelationsDataFiles;
        }

        throw new ArgumentOutOfRangeException(nameof(modelType));
    }
}