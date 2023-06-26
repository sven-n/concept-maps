namespace ConceptMaps.Crawler;

/// <summary>
/// Interface for a provider of crawled data of a website.
/// </summary>
public interface ICrawledDataProvider
{
    IEnumerable<string> AvailableRelationalData { get; }

    IEnumerable<SentenceRelationships> GetRelationships(string filePath);
}