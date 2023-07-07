using System.Text.Json.Serialization;

namespace ConceptMaps.DataModel;

public record class Relationship
{
    public string FirstEntity { get; set; } = string.Empty;

    public string SecondEntity { get; set; } = string.Empty;

    public string RelationshipType { get; set; } = string.Empty;

    [JsonIgnore]
    public double Score { get; set; } = double.NaN;

    [JsonIgnore]
    public string FirstEntityForeName => this.FirstEntity.Split(' ').First();

    [JsonIgnore]
    public string SecondEntityForeName => SecondEntity.Split(' ').First();
}
