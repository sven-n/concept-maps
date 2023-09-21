namespace ConceptMaps.UI.Services;

/// <summary>
/// Defines the type of the nlp model.
/// </summary>
public enum ModelType
{
    /// <summary>
    /// The relation model which is used to find relationships between entities.
    /// </summary>
    Relation,

    /// <summary>
    /// The named entity recognition model which is used to find entities in a text.
    /// </summary>
    Ner,
}