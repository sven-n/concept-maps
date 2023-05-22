namespace ConceptMaps.Crawler;

using AngleSharp.Dom;

/// <summary>
/// Interface for an extractor which takes a document and extracts personal relationships
/// based on the content of the document of the page.
/// </summary>
public interface IRelationshipExtractor
{
    /// <summary>
    /// Gets the base URI for the page on which the extractor should be used.
    /// </summary>
    Uri BaseUri { get; }

    /// <summary>
    /// Extracts the relationships from the content of the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>The extracted relationships.</returns>
    IEnumerable<(string CurrentPerson, string RelationType, string RelativeName)> ExtractRelationships(IDocument document);
}