namespace ConceptMaps.UI.Services;

using ConceptMaps.UI.Data;

/// <summary>
/// Interface for the data manager which is used to save and load data training data.
/// </summary>
public interface ITrainingDataManager
{
    /// <summary>
    /// Gets the relations data files.
    /// </summary>
    IEnumerable<string> RelationsDataFiles { get; }

    /// <summary>
    /// Gets the NRT data files.
    /// </summary>
    /// <remarks>Currently, not in use.</remarks>
    IEnumerable<string> NrtDataFiles { get; }

    /// <summary>
    /// /// Gets the name of the sub folder where the data files can be found.
    /// </summary>
    string SubFolder { get; }

    /// <summary>
    /// Gets the folder path for the specified model type.
    /// </summary>
    /// <param name="modelType">Type of the model.</param>
    /// <returns>The folder path for the specified model type.</returns>
    string GetFolderPath(ModelType modelType);

    /// <summary>
    /// Saves the reviewed sentences and their prepared relations.
    /// </summary>
    /// <param name="prepareContext">The prepare context.</param>
    Task SaveRelationsAsync(DataPrepareContext prepareContext);
}