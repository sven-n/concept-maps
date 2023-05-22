namespace ConceptMaps.Crawler;

/// <summary>
/// Loader for <see cref="WebsiteSettings"/> from a simple text file.
/// Empty lines, and lines starting with '#' are filtered out.
/// The first real line is the <see cref="WebsiteSettings.EntryUri"/>.
/// The next line is the <see cref="WebsiteSettings.BaseUri"/>. All pages which should be crawled should have this address as base.
/// After that, a dynamic amount of <see cref="WebsiteSettings.BlockUris"/> which should not be crawled, can be defined, one per line.
/// </summary>
public class SimpleWebsiteSettingsLoader : IWebsiteSettingsLoader
{
    /// <inheritdoc />
    public WebsiteSettings LoadSettings(string settingsFilePath)
    {
        var lines = File.ReadLines(settingsFilePath)
            .Where(line => !line.StartsWith("#"))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (!lines.Any())
        {
            throw new ArgumentException($"The file '{settingsFilePath}' doesn't contain any settings.");
        }

        return new WebsiteSettings(
            entryUri: lines[0], 
            baseUri: lines.Count > 1 ? lines[1] : string.Empty,
            blockUris: new HashSet<string>(lines.Skip(2).Distinct()));
    }
}