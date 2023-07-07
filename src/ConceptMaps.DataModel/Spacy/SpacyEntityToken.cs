namespace ConceptMaps.DataModel.Spacy;

using System.Text.Json.Serialization;

/// <summary>
/// Entity token of a <see cref="SpacySentence"/> in the serializable json object for spacy.
/// </summary>
public class SpacyEntityToken
{
    public static string DefaultEntityLabel = "PERSON";

    /// <summary>
    /// Gets or sets the text of the entity.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start index of the <see cref="Text"/> within the <see cref="SpacySentence.Sentence"/>.
    /// </summary>
    [JsonPropertyName("start")]
    public int Start { get; set; }

    /// <summary>
    /// Gets or sets the end index of the <see cref="Text"/> within the <see cref="SpacySentence.Sentence"/>.
    /// </summary>
    [JsonPropertyName("end")]
    public int End { get; set; }

    /// <summary>
    /// Gets or sets the index of the start token of the entity within the tokenized <see cref="SpacySentence.Sentence"/>.
    /// </summary>
    [JsonPropertyName("token_start")]
    public int TokenStart { get; set; }

    /// <summary>
    /// Gets or sets the index of the end token of the entity within the tokenized <see cref="SpacySentence.Sentence"/>.
    /// </summary>
    [JsonPropertyName("token_end")]
    public int TokenEnd { get; set; }

    /// <summary>
    /// Gets or sets the entity label.
    /// </summary>
    /// <remarks>
    /// In our use case, it's always 'PERSON'.
    /// </remarks>
    [JsonPropertyName("entityLabel")]
    public string EntityLabel { get; set; } = DefaultEntityLabel;
}