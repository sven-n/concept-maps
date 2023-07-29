namespace ConceptMaps.UI.Pages;

using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Data;

/// <summary>
/// Webpage for the <see cref="ICrawler"/> configuration.
/// </summary>
public partial class CrawlerConfigPage
{
    /// <summary>
    /// Flag, if the configuration was saved.
    /// </summary>
    private bool _isSaved;

    /// <summary>
    /// The list of available website settings.
    /// </summary>
    private SortedList<string, IWebsiteSettings> _websiteSettings = new();

    /// <summary>
    /// The identifier of the selected <see cref="IWebsiteSettings"/>.
    /// Backing-Field for <see cref="SelectedId"/>.
    /// </summary>
    private string? _selectedId;

    /// <summary>
    /// The view model of the selected <see cref="IWebsiteSettings"/>.
    /// </summary>
    private WebsiteSettingsViewModel? _selectedSettings;

    /// <summary>
    /// The injected <see cref="IWebsiteSettingsProvider"/>.
    /// </summary>
    [Inject]
    private IWebsiteSettingsProvider SettingsProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the parameter of the initially selected <see cref="IWebsiteSettings"/>.
    /// </summary>
    [Parameter]
    public string SettingsId { get; set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the current <see cref="_selectedSettings"/> is new and unsaved.
    /// </summary>
    private bool IsNewAndUnsaved => !this._isSaved && this._selectedId is null;

    /// <summary>
    /// Gets or sets the identifier of the selected <see cref="IWebsiteSettings"/>.
    /// </summary>
    private string? SelectedId
    {
        get => _selectedId;
        set
        {
            if (value is null || !this._websiteSettings.TryGetValue(value, out var settings))
            {
                return;
            }

            this._selectedId = value;
            this._selectedSettings?.Dispose();

            this._selectedSettings = new WebsiteSettingsViewModel(value, settings);
            this._selectedSettings.PropertyChanged += (_, _) => this._isSaved = false;
            this._isSaved = false;
        }
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        this._websiteSettings = new SortedList<string, IWebsiteSettings>(this.SettingsProvider.AvailableSettings.Select(path => (Path.GetFileName(path), this.SettingsProvider.LoadSettings(path)))
            .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2));

        this.SelectedId = this._websiteSettings.Keys.FirstOrDefault(path => path == this.SettingsId)
                            ?? this._websiteSettings.Keys.FirstOrDefault(); ;

        base.OnInitialized();
    }

    /// <summary>
    /// Saves the settings of the currently selected settings.
    /// </summary>
    private void SaveSettings()
    {
        if (this._selectedSettings is null)
        {
            return;
        }

        var settings = new WebsiteSettings
        {
            Name = this._selectedSettings.Name,
            BaseUri = this._selectedSettings.BaseUri.Uri,
            BlockUris = new HashSet<Uri>(this._selectedSettings.BlockUris.Select(uri => uri.Uri).Distinct())
        };
        settings.EntryUris.AddRange(this._selectedSettings.EntryUris.Select(uri => uri.Uri));

        this.SettingsProvider.SaveSettings(settings, this._selectedSettings.Id);
        
        this._websiteSettings.Remove(this._selectedSettings.Id);
        this._websiteSettings.Add(this._selectedSettings.Id, settings);

        this.SelectedId = this._selectedSettings.Id;
        
        this._isSaved = true;
    }

    /// <summary>
    /// Called when the create new button is clicked. Sets a new view model as selected.
    /// </summary>
    private void OnCreateNew()
    {
        this._selectedId = null;
        this._selectedSettings = new WebsiteSettingsViewModel();
    }

    /// <summary>
    /// Called when the cancel button is clicked for a new view model.
    /// It resets the <see cref="SelectedId"/> back to the first of the available
    /// settings.
    /// </summary>
    private void OnCancelNew()
    {
        this.SelectedId = this._websiteSettings.Keys.FirstOrDefault();
    }

    /// <summary>
    /// Called when the button to delete the selected configuration is clicked.
    /// </summary>
    private void OnDeleteSelectedConfiguration()
    {
        if (this._selectedId is null)
        {
            return;
        }

        this.SettingsProvider.RemoveSettings(this._selectedId);
        this._websiteSettings.Remove(this._selectedId);
        this.SelectedId = this._websiteSettings.Keys.FirstOrDefault();
    }
}
