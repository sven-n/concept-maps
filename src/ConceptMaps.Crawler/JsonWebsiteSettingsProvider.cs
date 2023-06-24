namespace ConceptMaps.Crawler;

using System.Text.Json;

/// <summary>
/// Loader for <see cref="WebsiteSettings"/> from a json file.
/// </summary>
public class JsonWebsiteSettingsProvider : IWebsiteSettingsProvider
{
    private const string FileNameExtension = "json";

    private static string ConfigFolder { get; } = Path.Combine(Path.GetDirectoryName(typeof(JsonWebsiteSettingsProvider).Assembly.Location)!, "crawler-settings");
    private static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <inheritdoc />
    public IEnumerable<string> AvailableSettings => Directory.EnumerateFiles(ConfigFolder, "*." + FileNameExtension, SearchOption.TopDirectoryOnly)
        .Select(Path.GetFileNameWithoutExtension)!;

    /// <inheritdoc />
    public IWebsiteSettings LoadSettings(string settingsId)
    {
        var targetPath = this.GetPathForId(settingsId);
        using var fileStream = File.OpenRead(targetPath);
        return JsonSerializer.Deserialize<WebsiteSettings>(fileStream, SerializerOptions);
    }

    /// <inheritdoc />
    public void RemoveSettings(string settingsId)
    {
        var targetPath = this.GetPathForId(settingsId);
        File.Delete(targetPath);
    }

    /// <inheritdoc />
    public void SaveSettings(IWebsiteSettings settings, string settingsId)
    {
        var targetPath = this.GetPathForId(settingsId);
        if (!Directory.Exists(ConfigFolder))
        {
            Directory.CreateDirectory(ConfigFolder);
        }

        using var fileStream = File.Create(targetPath);
        var saveSettings = new WebsiteSettings();
        saveSettings.Assign(settings);
        JsonSerializer.Serialize(fileStream, saveSettings, SerializerOptions);
    }

    private string GetPathForId(string settingsId)
    {
        return Path.Combine(
            ConfigFolder,
            settingsId + "." + FileNameExtension);
    }
}