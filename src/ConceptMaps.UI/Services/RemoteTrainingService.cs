namespace ConceptMaps.UI.Services;

using System.Text;
using System.Text.Json;

/// <summary>
/// The service to start and stop the training of a model remotely on the python service.
/// </summary>
public class RemoteTrainingService
{
    /// <summary>
    /// The json serializer options which should be used to serialize the input and response data.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets the name of the start action in the remote url.
    /// </summary>
    private static string ActionStart => "start";

    /// <summary>
    /// Gets the name of the cancel action in the remote url.
    /// </summary>
    private static string ActionCancel => "cancel";

    /// <summary>
    /// Gets the name of the status request action in the remote url.
    /// </summary>
    private static string ActionStatus => "status";

    /// <summary>
    /// Tries to start the training of the nlp model at the remote service.
    /// </summary>
    /// <typeparam name="T">The data type of the sentences. This can vary between relation data and nrt data.</typeparam>
    /// <param name="modelType">Type of the model.</param>
    /// <param name="sentences">The sentences.</param>
    /// <param name="sourceModel">The source model.</param>
    /// <param name="targetModel">The target model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A flag, if the starting was successful.</returns>
    public async Task<bool> StartTrainingAsync<T>(ModelType modelType, IList<T> sentences, string? sourceModel, string targetModel, CancellationToken cancellationToken = default)
    {
        try
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
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to stop the training of the nlp model at the remote service.
    /// </summary>
    /// <param name="modelType">Type of the model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A flag, if stopping was successful.</returns>
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

    /// <summary>
    /// Gets the current training status of the remote service.
    /// </summary>
    /// <param name="modelType">Type of the model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The training status, if available.</returns>
    public async Task<TrainingStatus?> GetTrainingStatus(ModelType modelType, CancellationToken cancellationToken = default)
    {
        var uri = GetUri(modelType, ActionStatus);
        using var client = new HttpClient();
        try
        {
            return await client.GetFromJsonAsync<TrainingStatus>(
                uri,
                JsonSerializerOptions,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return new TrainingStatus
            {
                IsActive = false,
                State = "no connection",
                Error = ex.Message,
            };
        }
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