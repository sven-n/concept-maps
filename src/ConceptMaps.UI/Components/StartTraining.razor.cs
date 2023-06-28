namespace ConceptMaps.UI.Components;

using System.Text.Json;
using System.Threading;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;

public partial class StartTraining : IDisposable
{
    private List<string> _selectedFiles = new();

    private string? _selectedSourceModel;

    private CancellationTokenSource? _startCts;

    private bool? _startSuccess;

    [Parameter]
    public ModelType ModelType { get; set; }

    [Inject]
    private ITrainingDataProvider TrainingDataProvider { get; set; } = null!;

    [Inject]
    private RemoteTrainingService TrainingService { get; set; } = null!;

    [Inject]
    private IModelProvider ModelProvider { get; set; } = null!;

    public async Task OnStartButtonClick()
    {
        if (this._selectedFiles.Count == 0)
        {
            return;
        }

        this._startCts = new CancellationTokenSource();

        if (this.ModelType == ModelType.Relation)
        {
            this._startSuccess = await this.StartTrainingAsync<SentenceRelationships>(this._startCts.Token);
        }
        else if (this.ModelType == ModelType.Nrt)
        {
            this._startSuccess = await this.StartTrainingAsync<SentenceEntities>(this._startCts.Token);
        }
    }

    private async Task<bool> StartTrainingAsync<T>(CancellationToken cancellationToken)
    {
        var sentences = await CollectSelectedDataAsync<T>(cancellationToken);
        await this.TrainingService.StartTrainingAsync(this.ModelType, sentences, this._selectedSourceModel, cancellationToken);
    }

    private async Task<List<T>> CollectSelectedDataAsync<T>(CancellationToken cancellationToken)
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var sentences = new List<T>();
        foreach (var filePath in this._selectedFiles)
        {
            await using var file = File.OpenRead(filePath);
            if (await JsonSerializer.DeserializeAsync<T[]>(file, serializerOptions, cancellationToken)
                is { } relationships)
            {
                sentences.AddRange(relationships);
            }
        }
    }

    private void OnCancelButtonClick()
    {
        this._startCts?.Cancel();
        this._startCts?.Dispose();
        this._startCts = null!;
    }

    public void Dispose()
    {
        this._startCts?.Dispose();
        this._startCts = null;
    }
}
