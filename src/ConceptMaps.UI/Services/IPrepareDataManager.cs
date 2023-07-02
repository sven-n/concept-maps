using ConceptMaps.UI.Data;

namespace ConceptMaps.UI.Services;

public interface IPrepareDataManager
{
    IEnumerable<string> DataFiles { get; }

    Task SaveAsync(DataPrepareContext prepareContext);

    Task<DataPrepareContext?> LoadAsync(string fileName);
}