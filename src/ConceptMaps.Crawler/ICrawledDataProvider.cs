namespace ConceptMaps.Crawler;

using ConceptMaps.DataModel;

/// <summary>
/// Interface for a provider of crawled data of a website.
/// </summary>
public interface ICrawledDataProvider
{
    string FolderPath { get; }

    IEnumerable<string> AvailableRelationalData { get; }

    Task<IEnumerable<SentenceRelationships>> GetRelationshipsAsync(string filePath);
}