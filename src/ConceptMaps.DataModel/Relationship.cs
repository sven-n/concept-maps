namespace ConceptMaps.DataModel;

using System.Text.Json.Serialization;

/// <summary>
/// Defines a relationship between two entities.
/// </summary>
public record class Relationship
{
    /// <summary>
    /// Gets or sets the first entity.
    /// </summary>
    public string FirstEntity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the second entity.
    /// </summary>
    public string SecondEntity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the relationship.
    /// </summary>
    public string RelationshipType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the score (0.0 to 1.0) for the prediction of the relationship type.
    /// </summary>
    [JsonIgnore]
    public double Score { get; set; } = double.NaN;

    /// <summary>
    /// Gets the fore name of the first entity.
    /// </summary>
    [JsonIgnore]
    public string FirstEntityForeName => this.FirstEntity.Split(' ').First();

    /// <summary>
    /// Gets the fore name of the second entity.
    /// </summary>
    [JsonIgnore]
    public string SecondEntityForeName => SecondEntity.Split(' ').First();
}
