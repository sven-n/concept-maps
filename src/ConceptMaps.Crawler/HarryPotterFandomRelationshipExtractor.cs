namespace ConceptMaps.Crawler;

using AngleSharp.Dom;

/// <summary>
/// <see cref="IRelationshipExtractor"/> for the Fandom wikis which don't specify the
/// relationship between persons in elements which are marked with the corresponding
/// relationship type in a "data-source" attribute.
/// </summary>
public class HarryPotterFandomRelationshipExtractor : IRelationshipExtractor
{
    /// <summary>
    /// The relevant values for the "data-source" attribute of "pi-data"-DIVs.
    /// </summary>
    private static readonly List<(string Keyword, string Type)> RelationshipKeywords = new()
    {
        ("(father)", "father"),
        ("(mother)", "mother"),
        ("(wife)", "spouse"),
        ("(husband)", "spouse"),
        ("(brother)", "siblings"),
        ("(sister)", "siblings"),
        ("(son)", "children"),
        ("(daughter)", "children"),
    };

    /// <inheritdoc />
    public IEnumerable<Uri> BaseUris { get; } = new List<Uri>
        {
            new("https://harrypotter.fandom.com/wiki/"),
        };

    /// <inheritdoc />
    public IEnumerable<(string CurrentPerson, string RelationType, string RelativeName)> ExtractRelationships(IDocument document)
    {
        var pageTitle = document.GetPageTitle();
        var currentPerson = pageTitle.Split('|').FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(currentPerson))
        {
            yield break;
        }

        /* <div class="pi-item pi-data pi-item-spacing pi-border-color" data-source="family">
         *  <div class="pi-data-value pi-font">
         *   <ul><li>
         *     <a href="/wiki/James_Potter_I" title="James Potter I">James Potter I</a>
         *     <sup id="cite_ref-PS_40-0" class="reference"><a href="#cite_note-PS-40">[40]</a>
         *     </sup> (father) †</li>
         */
        var familyLinks = document.GetElementsByTagName("div")
            .Where(div => div.ClassList.Contains("pi-data"))
            .Where(div => div.GetAttribute("data-source") == "family")
            .SelectMany(div => div.GetElementsByClassName("pi-data-value"))
            .SelectMany(value => value.GetElementsByTagName("a").Where(aelement => !aelement.GetAttribute("href").StartsWith("#")));

        foreach (var familyLink in familyLinks)
        {
            var relationType = RelationshipKeywords
                .FirstOrDefault(tuple => familyLink.ParentElement.InnerHtml.Contains(tuple.Keyword, StringComparison.InvariantCultureIgnoreCase)).Type;
            if (string.IsNullOrWhiteSpace(relationType))
            {
                continue;
            }

            var relativeName = familyLink.GetAttribute("title");
            if (!string.IsNullOrWhiteSpace(relativeName) && !string.IsNullOrWhiteSpace(relationType)
                && !relativeName.EndsWith("family"))
            {
                yield return (currentPerson, relationType, relativeName);
            }
        }
    }
}