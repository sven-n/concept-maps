namespace ConceptMaps.Crawler;

/// <summary>
/// Website specific settings for the crawler.
/// </summary>
public class WebsiteSettings : IWebsiteSettings
{
    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public Uri BaseUri { get; set; } = new Uri("http://localhost");

    /// <inheritdoc />
    public List<Uri> EntryUris { get; set; } = new ();

    /// <inheritdoc />
    public ISet<Uri> BlockUris { get; set; } = new HashSet<Uri>();

    public void Assign(IWebsiteSettings settings)
    {
        this.Name = settings.Name;
        this.BaseUri = settings.BaseUri;
        this.BlockUris = new HashSet<Uri>(settings.BlockUris);
        this.EntryUris = new List<Uri>(settings.EntryUris);

        if (string.IsNullOrWhiteSpace(this.Name))
        {
            this.Name = this.BaseUri.ToString();
        }
    }
}