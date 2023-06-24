namespace ConceptMaps.UI.Data;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// View Model for an <see cref="Uri"/>.
/// </summary>
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
public sealed class UriViewModel : INotifyPropertyChanged
{
    private Uri _uri;

    /// <summary>
    /// Initializes a new instance of the <see cref="UriViewModel"/> class.
    /// </summary>
    /// <param name="uri">The URI.</param>
    public UriViewModel(Uri uri)
    {
        this._uri = uri;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the URI.
    /// </summary>
    public Uri Uri
    {
        get => _uri;
        set
        {
            if (Equals(value, _uri))
            {
                return;
            }

            this._uri = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(UriString));
        }
    }

    /// <summary>
    /// Gets or sets the URI as string.
    /// </summary>
    public string UriString
    {
        get => Uri.OriginalString;
        set => this.Uri = new Uri(value);
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}