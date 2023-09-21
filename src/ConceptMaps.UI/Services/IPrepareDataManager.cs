namespace ConceptMaps.UI.Services;

using ConceptMaps.UI.Data;

/// <summary>
/// Interface for the data manager which is used to save and load data preparation data (sessions).
/// </summary>
public interface IPrepareDataManager
{
    /// <summary>
    /// Gets the available data files.
    /// </summary>
    IEnumerable<string> DataFiles { get; }

    /// <summary>
    /// Gets the name of the sub folder where the data files can be found.
    /// </summary>
    string SubFolder { get; }

    /// <summary>
    /// Gets the folder path for the specified model type.
    /// </summary>
    /// <param name="modelType">Type of the model.</param>
    /// <returns>The folder path for the specified model type.</returns>
    string GetFolderPath(ModelType modelType);

    /// <summary>
    /// Saves the prepare context (session).
    /// </summary>
    /// <param name="prepareContext">The prepare context.</param>
    /// <returns></returns>
    Task SaveAsync(DataPrepareContext prepareContext);

    /// <summary>
    /// Loads the prepare context data (session) with the specified name.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>The loaded data, if available.</returns>
    Task<DataPrepareContext?> LoadAsync(string fileName);
}