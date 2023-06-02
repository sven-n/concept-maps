namespace ConceptMaps.Crawler;

/// <summary>
/// Loader for <see cref="WebsiteSettings"/> from a simple text file.
/// Empty lines, and lines starting with '#' are filtered out.
/// The file is separated in segments.
/// The first segments contains one or more <see cref="WebsiteSettings.EntryUris"/>.
/// The next segment contains the <see cref="WebsiteSettings.BaseUri"/>. All pages which should be crawled should have this address as base.
/// After that, a dynamic amount of <see cref="WebsiteSettings.BlockUris"/> which should not be crawled, can be defined, one per line.
/// </summary>
public class SimpleWebsiteSettingsLoader : IWebsiteSettingsLoader
{
    /// <inheritdoc />
    public IEnumerable<string> AvailableSettings => Directory.EnumerateFiles(Path.GetDirectoryName(this.GetType().Assembly.Location), "*.cfg", SearchOption.TopDirectoryOnly);

    /// <inheritdoc />
    public WebsiteSettings LoadSettings(string settingsFilePath)
    {
        var lines = File.ReadLines(settingsFilePath)
            .Where(line => !line.StartsWith("#"))
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (!lines.Any())
        {
            throw new ArgumentException($"The file '{settingsFilePath}' doesn't contain any settings.");
        }

        return new WebsiteSettings(
            entryUris: lines.SkipWhile(l => l != "[EntryPoints]").Skip(1).TakeWhile(l => l != "[Base]"), 
            baseUri: lines.SkipWhile(l => l != "[Base]").Skip(1).FirstOrDefault() ?? string.Empty,
            blockUris: new HashSet<string>(lines.SkipWhile(l => l != "[BlockUris]").Distinct()));
    }
}