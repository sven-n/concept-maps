namespace ConceptMaps.UI.Data;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ConceptMaps.Crawler;

/// <summary>
/// View Model for the <see cref="IWebsiteSettings"/>.
/// </summary>
public sealed class WebsiteSettingsViewModel: INotifyPropertyChanged, IDisposable
{
    private string _id;
    private string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebsiteSettingsViewModel"/> class.
    /// </summary>
    public WebsiteSettingsViewModel()
    {
        this._id = Random.Shared.Next().ToString("X");
        this._name = "New Website Name";
        this.BaseUri = new UriViewModel(new Uri("https://localhost"));
        this.BlockUris = new();
        this.EntryUris = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebsiteSettingsViewModel"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="websiteSettings">The website settings.</param>
    public WebsiteSettingsViewModel(string id, IWebsiteSettings websiteSettings)
    {
        this._id = id;
        this._name = websiteSettings.Name;
        this.EntryUris = websiteSettings.EntryUris.Select(uri => new UriViewModel(uri)).ToList();
        this.BaseUri = new UriViewModel(websiteSettings.BaseUri);
        this.BlockUris = websiteSettings.BlockUris.Select(uri => new UriViewModel(uri)).ToList();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Id
    {
        get => _id;
        set
        {
            if (Equals(value, this._id))
            {
                return;
            }

            this._id = value;
            this.RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (Equals(value, this._name))
            {
                return;
            }

            this._name = value;
            this.RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the block uris.
    /// </summary>
    public List<UriViewModel> BlockUris { get; set; }

    /// <summary>
    /// Gets or sets the base URI.
    /// </summary>
    public UriViewModel BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the entry uris.
    /// </summary>
    public List<UriViewModel> EntryUris { get; set; }

    /// <inheritdoc />
    public void Dispose()
    {
        this.PropertyChanged = null;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.Name} ({this.BaseUri})";
    }

    /// <summary>
    /// Raises the property changed event.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}