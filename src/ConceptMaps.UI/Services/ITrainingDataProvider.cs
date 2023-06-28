namespace ConceptMaps.UI.Services;

public interface ITrainingDataProvider
{
    IEnumerable<string> RelationsDataFiles { get; }

    IEnumerable<string> NrtDataFiles { get; }
}

public static class TrainingDataProviderExtensions
{
    public static IEnumerable<string> GetFiles(this ITrainingDataProvider dataProvider, ModelType modelType)
    {
        if (modelType is ModelType.Nrt)
        {
            return dataProvider.NrtDataFiles;
        }
        
        if (modelType is ModelType.Relation)
        {
            return dataProvider.RelationsDataFiles;
        }

        throw new ArgumentOutOfRangeException(nameof(modelType));
    }
}