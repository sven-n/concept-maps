namespace ConceptMaps.UI.Services;

/// <summary>
/// Extensions for <see cref="ModelType"/>.
/// </summary>
public static class ModelTypeExtensions
{
    /// <summary>
    /// Gets the name of the relation model type.
    /// </summary>
    public static string RelationModelType => "relations";

    /// <summary>
    /// Gets the name of the ner model type.
    /// </summary>
    public static string NerModelType => "ner";

    /// <summary>
    /// Returns the <see cref="ModelType"/> as a string.
    /// </summary>
    /// <param name="modelType">Type of the model.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentOutOfRangeException"></exception>
    public static string AsString(this ModelType modelType)
    {
        switch (modelType)
        {
            case ModelType.Ner:
                return NerModelType;
            case ModelType.Relation:
                return RelationModelType;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
