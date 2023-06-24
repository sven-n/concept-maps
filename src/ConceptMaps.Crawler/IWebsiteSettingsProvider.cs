namespace ConceptMaps.Crawler;

/// <summary>
/// Interface for a provider of crawling settings of a website.
/// </summary>
public interface IWebsiteSettingsProvider
{
    /// <summary>
    /// Gets the identifiers of available settings.
    /// </summary>
    IEnumerable<string> AvailableSettings { get; }

    /// <summary>
    /// Loads the settings for a website.
    /// </summary>
    /// <param name="settingsId">The settings identifier.</param>
    /// <returns>The loaded settings.</returns>
    IWebsiteSettings LoadSettings(string settingsId);

    /// <summary>
    /// Loads the settings for a website.
    /// </summary>
    /// <param name="settings">The settings which should be saved.</param>
    /// <param name="settingsId">The settings identifier.</param>
    void SaveSettings(IWebsiteSettings settings, string settingsId);

    /// <summary>
    /// Removes the settings.
    /// </summary>
    /// <param name="settingsId">The settings identifier.</param>
    void RemoveSettings(string settingsId);
}