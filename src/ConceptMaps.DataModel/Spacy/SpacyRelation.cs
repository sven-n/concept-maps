namespace ConceptMaps.DataModel.Spacy;

using System.Text.Json.Serialization;

/// <summary>
/// Relation between two <see cref="SpacySentence.Tokens"/> in the serializable json object for spacy.
/// </summary>
public class SpacyRelation
{
    /// <summary>
    /// Gets or sets the 'child' of the relation.
    /// </summary>
    [JsonPropertyName("child")]
    public string Child { get; set; }

    /// <summary>
    /// Gets or sets the 'head' of the relation.
    /// </summary>
    [JsonPropertyName("head")]
    public string Head { get; set; }

    /// <summary>
    /// Gets or sets the label of the relation.
    /// </summary>
    /// <remarks>Usually one of these: 'undefined', 'SPOUSE', 'SIBLINGS', 'CHILDREN'.</remarks>
    [JsonPropertyName("relationLabel")]
    public string RelationLabel { get; set; }
}