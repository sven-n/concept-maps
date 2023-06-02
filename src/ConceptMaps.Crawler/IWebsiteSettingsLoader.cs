namespace ConceptMaps.Crawler;

/// <summary>
/// Interface for crawling settings of a website.
/// </summary>
public interface IWebsiteSettingsLoader
{
    /// <summary>
    /// Loads the settings for a website.
    /// </summary>
    /// <param name="settingsFilePath">The settings file path.</param>
    /// <returns>The loaded settings.</returns>
    WebsiteSettings LoadSettings(string settingsFilePath);

    /// <summary>
    /// Gets the available settings.
    /// </summary>
    IEnumerable<string> AvailableSettings { get; }
}