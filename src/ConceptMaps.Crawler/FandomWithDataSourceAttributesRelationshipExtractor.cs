namespace ConceptMaps.Crawler;

using AngleSharp.Dom;

/// <summary>
/// <see cref="IRelationshipExtractor"/> for the Fandom wikis which specify the
/// relationship between persons in elements which are marked with the corresponding
/// relationship type in a "data-source" attribute.
/// </summary>
public class FandomWithDataSourceAttributesRelationshipExtractor : IRelationshipExtractor
{
    /// <summary>
    /// The relevant values for the "data-source" attribute of "pi-data"-DIVs.
    /// </summary>
    private static readonly HashSet<string?> RelevantDataSourceNames = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Father", "Mother", "Spouse", "Siblings", "Children"
    };

    /// <inheritdoc />
    public IEnumerable<Uri> BaseUris { get; } = new List<Uri>
        {
            new("https://gameofthrones.fandom.com/wiki/"),
            new("https://lotr.fandom.com/wiki/")
        };

    /// <inheritdoc />
    public IEnumerable<(string CurrentPerson, string RelationType, string RelativeName)> ExtractRelationships(IDocument document)
    {
        var familyLinks = document.GetElementsByTagName("div")
            .Where(div => div.ClassList.Contains("pi-data"))
            .Where(div => RelevantDataSourceNames.Contains(div.GetAttribute("data-source")))
            .SelectMany(div => div.GetElementsByClassName("pi-data-value"))
            .SelectMany(value => value.GetElementsByTagName("a"));

        var pageTitle = document.GetPageTitle();
        var currentPerson = pageTitle.Split('|').FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(currentPerson))
        {
            yield break;
        }

        foreach (var familyLink in familyLinks)
        {
            var relationType = familyLink.ParentElement.ParentElement.GetAttribute("data-source");
            var relativeName = familyLink.GetAttribute("title");
            if (!string.IsNullOrWhiteSpace(relationType)
                && familyLink.Descendents().OfType<IElement>().FirstOrDefault(e => e.TagName == "small")?.TextContent is { } relationTag)
            {
                relationType += $"|{relationTag}";
            }

            if (!string.IsNullOrWhiteSpace(relativeName) && !string.IsNullOrWhiteSpace(relationType))
            {
                yield return (currentPerson, relationType, relativeName);
            }
        }
    }
}