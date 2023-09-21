namespace ConceptMaps.UI.Services;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the data structure of the training status which is received from
/// the remote python service.
/// </summary>
public class TrainingStatus
{
    /// <summary>
    /// A flag, if the training is currently active.
    /// </summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the state of the training, if available.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the console output (stdout), if available.
    /// </summary>
    [JsonPropertyName("output")]
    public string? Output { get; set; }

    /// <summary>
    /// Gets or sets the error output (errout), if available.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
