namespace ConceptMaps.UI.Services;

public interface ITrainingDataProvider
{
    IEnumerable<string> RelationsDataFiles { get; }

    IEnumerable<string> NrtDataFiles { get; }
}