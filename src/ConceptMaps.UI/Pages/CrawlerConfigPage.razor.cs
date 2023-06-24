namespace ConceptMaps.UI.Pages;

using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Data;

/// <summary>
/// Webpage for the <see cref="ICrawler"/>.
/// </summary>
public partial class CrawlerConfigPage
{
    private bool _isSaved;

    private SortedList<string, IWebsiteSettings> _websiteSettings = new();

    private string? _selectedId;

    private WebsiteSettingsViewModel? _selectedSettings;

    /// <summary>
    /// THe injected <see cref="IWebsiteSettingsProvider"/>.
    /// </summary>
    [Inject]
    private IWebsiteSettingsProvider SettingsProvider { get; set; } = null!;

    [Parameter] public string SettingsId { get; set; } = null!;

    private bool IsNewAndUnsaved => !this._isSaved && this._selectedId is null;

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

    protected override void OnInitialized()
    {
        this._websiteSettings = new SortedList<string, IWebsiteSettings>(this.SettingsProvider.AvailableSettings.Select(path => (Path.GetFileName(path), this.SettingsProvider.LoadSettings(path)))
            .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2));

        this.SelectedId = this._websiteSettings.Keys.FirstOrDefault(path => path == this.SettingsId)
                            ?? this._websiteSettings.Keys.FirstOrDefault(); ;

        base.OnInitialized();
    }

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

    private void OnCreateNew()
    {
        this._selectedId = null;
        this._selectedSettings = new WebsiteSettingsViewModel();
    }

    private void OnCancelNew()
    {
        this.SelectedId = this._websiteSettings.Keys.FirstOrDefault();
    }

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
