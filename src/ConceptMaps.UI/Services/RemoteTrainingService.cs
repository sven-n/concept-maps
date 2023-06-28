namespace ConceptMaps.UI.Services;

using System.Text;
using System.Text.Json;

public class RemoteTrainingService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private static string ActionStart => "start";

    private static string ActionCancel => "cancel";

    private static string ActionStatus => "status";

    public async Task<bool> StartTrainingAsync<T>(ModelType modelType, IList<T> sentences, string? sourceModel, string targetModel, CancellationToken cancellationToken = default)
    {
        var uri = GetStartUri(modelType, sourceModel);
        var serializedInput = JsonSerializer.Serialize(sentences.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var client = new HttpClient();
        var response = await client.PostAsync(
            uri,
            new StringContent(serializedInput, Encoding.UTF8, mediaType: "application/json"),
            cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> StopTrainingAsync(ModelType modelType, CancellationToken cancellationToken = default)
    {
        var uri = GetUri(modelType, ActionCancel);
        using var client = new HttpClient();
        var response = await client.PostAsync(
            uri,
            new StringContent(string.Empty, Encoding.UTF8, mediaType: "application/json"),
            cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<TrainingStatus?> GetTrainingStatus(ModelType modelType, CancellationToken cancellationToken = default)
    {
        var uri = GetUri(modelType, ActionStatus);
        using var client = new HttpClient();
        return await client.GetFromJsonAsync<TrainingStatus>(
            uri,
            JsonSerializerOptions,
            cancellationToken);
    }

    private static string GetStartUri(ModelType modelType, string? sourceModel)
    {
        var uri = GetUri(modelType, ActionStart);
        if (!string.IsNullOrWhiteSpace(sourceModel))
        {
            uri += "/" + Uri.EscapeDataString(sourceModel);
        }

        return uri;
    }

    private static string GetUri(ModelType modelType, string action)
    {
        // TODO: Make host configurable
        return $@"http://localhost:5001/training/{modelType.AsString()}/{action}";
    }
}