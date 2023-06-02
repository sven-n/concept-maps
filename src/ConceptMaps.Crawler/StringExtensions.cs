namespace ConceptMaps.Crawler;

/// <summary>
/// Extension methods for strings.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Gets the clean name of an entity. It's stripping any additional info like
    /// '(son of XY)' from the name.
    /// </summary>
    /// <param name="entityName">Name of the entity.</param>
    /// <returns>The clean name of an entity.</returns>
    public static string GetCleanEntityName(this string entityName)
    {
        var bracketIndex = entityName.IndexOf('(');
        if (bracketIndex > 0)
        {
            return entityName.Substring(0, bracketIndex).Trim();
        }

        return entityName.Trim();
    }
}

