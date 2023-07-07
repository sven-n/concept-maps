namespace ConceptMaps.DataModel.Spacy;

using System.Text.Json.Serialization;

/// <summary>
/// Sentence in the serializable json object for spacy.
/// </summary>
public class SpacySentence
{
    /// <summary>
    /// Gets or sets the sentence as text.
    /// </summary>
    [JsonPropertyName("sentence")]
    public string Sentence { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tokens of the text.
    /// </summary>
    [JsonPropertyName("tokens")]
    public List<SpacyEntityToken> Tokens { get; set; } = new();

    /// <summary>
    /// Gets or sets the relations which are described in the text.
    /// </summary>
    [JsonPropertyName("relations")]
    public List<SpacyRelation> Relations { get; set; } = new();
}