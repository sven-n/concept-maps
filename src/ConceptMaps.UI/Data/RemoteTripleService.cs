namespace ConceptMaps.UI.Data;

using System.Text;
using System.Text.Json;

/// <summary>
/// A service which creates triples out of a text in a remote service.
/// </summary>
public class RemoteTripleService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly string RemoteServiceUrl = @"http://localhost:5000/get-triples"; // TODO: Make configurable

    /// <summary>
    /// Generates the triples for the specified text.
    /// </summary>
    /// <param name="text">The specified text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated triple.</returns>
    public async Task<IList<Triple>> GenerateTriplesAsync(string? text, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        var response = await client.PostAsync(
            RemoteServiceUrl, 
            new StringContent($"\"{text}\"", Encoding.UTF8, mediaType: "application/json"),
            cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<List<Triple>>(
            JsonSerializerOptions,
            cancellationToken);
        return result ?? new List<Triple>();
    }
}
