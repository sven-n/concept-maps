namespace ConceptMaps.UI.Services;

using System.Text;
using System.Text.Json;
using System.Threading;
using ConceptMaps.UI.Data;

/// <summary>
/// A service which creates triples out of a text in a remote service.
/// </summary>
public class RemoteTripleService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly string RemoteServiceUrl = @"http://localhost:5001/get-triples"; // TODO: Make configurable

    /// <summary>
    /// Generates the triples for the specified text.
    /// </summary>
    /// <param name="text">The specified text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated triple.</returns>
    public async Task<IList<Triple>> GenerateTriplesAsync(string? text, CancellationToken cancellationToken = default)
    {
        var result = new List<Triple>();
        result.Add(new("Bob", "SIBLINGS", "Alice"));
        result.Add(new("Bob", "SIBLINGS", "Tom"));
        result.Add(new("Alice", "SIBLINGS", "Tom"));

        // Jeffs children:
        result.Add(new("Bob", "CHILDREN", "Jeff"));
        result.Add(new("Alice", "CHILDREN", "Jeff"));

        // Marys children:
        result.Add(new("Bob", "CHILDREN", "Mary"));
        result.Add(new("Alice", "CHILDREN", "Mary"));

        // Jeff and Mary are married
        result.Add(new("Mary", "SPOUSE", "Jeff"));

        // Tom is child of Alex and Mary (they're not married):
        result.Add(new("Tom", "CHILDREN", "Alex"));
        result.Add(new("Tom", "CHILDREN", "Mary"));

        // Alice husband:
        result.Add(new("Jim", "SPOUSE", "Alice"));

        // Unrelated:
        result.Add(new ("Kate", "SIBLINGS", "Bruce"));
        return result;
        /*  
        using var client = new HttpClient();
        var response = await client.PostAsync(
            RemoteServiceUrl,
            new StringContent($"\"{text}\"", Encoding.UTF8, mediaType: "application/json"),
            cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<List<Triple>>(
            JsonSerializerOptions,
            cancellationToken);
        return result ?? new List<Triple>();*/
    }
}