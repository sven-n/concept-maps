namespace ConceptMaps.Crawler;

/// <summary>
/// Factory to create site-specific <see cref="IRelationshipExtractor"/>s.
/// </summary>
public class RelationshipExtractorFactory
{
    private readonly List<IRelationshipExtractor> _registeredExtractors = new();

    /// <summary>
    /// Gets the extractor for the specified URI.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <returns>The suitable <see cref="IRelationshipExtractor"/>.</returns>
    public IRelationshipExtractor GetExtractor(Uri uri)
    {
        return _registeredExtractors.FirstOrDefault(extractor => extractor.BaseUris.Any(baseUri => baseUri.IsBaseOf(uri)))
               ?? _registeredExtractors.First();
    }

    /// <summary>
    /// Registers the specified extractor.
    /// </summary>
    /// <param name="extractor">The extractor.</param>
    public void Register(IRelationshipExtractor extractor)
    {
        this._registeredExtractors.Add(extractor);
    }
}