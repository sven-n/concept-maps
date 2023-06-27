﻿using System.Text.Json;

namespace ConceptMaps.Crawler;

public class CrawledDataProvider : ICrawledDataProvider
{
    private const string FileNameExtension = "json";

    private static string ConfigFolder { get; } = Path.Combine(Path.GetDirectoryName(typeof(JsonWebsiteSettingsProvider).Assembly.Location)!, "crawl-results");
    private static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <inheritdoc />
    public IEnumerable<string> AvailableRelationalData => Directory.EnumerateFiles(ConfigFolder, "*_SentenceRelationships." + FileNameExtension, SearchOption.TopDirectoryOnly);

    /// <inheritdoc />
    public IEnumerable<SentenceRelationships> GetRelationships(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize<SentenceRelationships[]>(fileStream, SerializerOptions);
    }
}