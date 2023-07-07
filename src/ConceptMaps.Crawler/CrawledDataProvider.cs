namespace ConceptMaps.Crawler;

using System.Text.Json;
using ConceptMaps.DataModel;

public class CrawledDataProvider : ICrawledDataProvider
{
    private const string FileNameExtension = "json";

    private static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static string SubFolder => "crawl-results";

    public string FolderPath { get; } = Path.Combine(Path.GetDirectoryName(typeof(JsonWebsiteSettingsProvider).Assembly.Location)!, SubFolder);
    

    /// <inheritdoc />
    public IEnumerable<string> AvailableRelationalData
    {
        get
        {
            if (!Directory.Exists(FolderPath))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.EnumerateFiles(FolderPath, "*_SentenceRelationships." + FileNameExtension, SearchOption.TopDirectoryOnly);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SentenceRelationships>> GetRelationshipsAsync(string filePath)
    {
        await using var fileStream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<SentenceRelationships[]>(fileStream, SerializerOptions)
               ?? Enumerable.Empty<SentenceRelationships>();
    }
}