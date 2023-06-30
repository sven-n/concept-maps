namespace ConceptMaps.UI.Services;

using System.Text.Json.Serialization;

public class TrainingStatus
{
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("output")]
    public string? Output { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
