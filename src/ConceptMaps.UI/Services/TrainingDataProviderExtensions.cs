namespace ConceptMaps.UI.Services;

/// <summary>
/// Extensions for <see cref="ITrainingDataManager"/>.
/// </summary>
public static class TrainingDataProviderExtensions
{
    /// <summary>
    /// Gets the available files of the specified model type.
    /// </summary>
    /// <param name="dataManager">The data manager.</param>
    /// <param name="modelType">Type of the model.</param>
    /// <returns>The available files of the specified model type.</returns>
    public static IEnumerable<string> GetFiles(this ITrainingDataManager dataManager, ModelType modelType)
    {
        if (modelType is ModelType.Ner)
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