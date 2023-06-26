namespace ConceptMaps.UI.Services;

using System.Text;
using System.Text.Json;
using ConceptMaps.Crawler;

public class RemoteTrainingDataConversionService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly string RemoteServiceUrl = @"http://localhost:5001/convert_rel"; // TODO: Make configurable

    public async Task<string> ConvertToJsonTrainingData(IList<SentenceRelationships> sentences, CancellationToken cancellationToken = default)
    {
        if (sentences.Count == 0)
        {
            return "[]"; // Empty json array
        }

        var serializedInput = JsonSerializer.Serialize(sentences.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var client = new HttpClient();
        var response = await client.PostAsync(
            RemoteServiceUrl,
            new StringContent(serializedInput, Encoding.UTF8, mediaType: "application/json"),
            cancellationToken);
        var result = await response.Content.ReadAsStringAsync(cancellationToken);
        return result;
    }
}