namespace ConceptMaps.Crawler;

using AngleSharp.Dom;

/// <summary>
/// Extension methods for <see cref="IDocument"/>.
/// </summary>
internal static class DocumentExtensions
{
    /// <summary>
    /// Gets the title of the webpage.
    /// </summary>
    /// <param name="document">The document of the webpage.</param>
    /// <returns></returns>
    public static string GetPageTitle(this IDocument document)
    {
        return document.GetElementsByClassName("page-header__title").FirstOrDefault()?.TextContent
               ?? document.GetElementsByClassName("mw-page-title-main")?.FirstOrDefault()?.TextContent
               ?? document.GetElementsByTagName("h1").FirstOrDefault()?.TextContent
               ?? document.GetElementsByTagName("title").FirstOrDefault()?.TextContent
               ?? document.Title;
    }

    /// <summary>
    /// Determines whether the node has a table as parent.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>
    ///   <c>true</c> if the specified element has a table as parent; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasTableAsParent(this INode element)
    {
        var parent = element.ParentElement;
        if (parent is null)
        {
            return false;
        }

        if (parent.TagName == "TABLE")
        {
            return true;
        }

        return parent.HasTableAsParent();
    }
}