namespace ConceptMaps.UI.Components;

using System.Text.Json;
using System.Threading;
using ConceptMaps.Crawler;
using ConceptMaps.DataModel;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

public partial class StartTraining : IDisposable
{
    private List<string> _selectedFiles = new();

    private string? _selectedSourceModel;

    private CancellationTokenSource? _startCts;

    private bool? _startSuccess;

    private bool _isStarting;

    [Parameter]
    public ModelType ModelType { get; set; }

    [Inject]
    private ITrainingDataManager TrainingDataManager { get; set; } = null!;

    [Inject]
    private RemoteTrainingService TrainingService { get; set; } = null!;

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
        else if (this.ModelType == ModelType.Ner)
        {
            this._startSuccess = await this.StartTrainingAsync<SentenceEntities>(this._startCts.Token);
        }
    }

    private async Task<bool> StartTrainingAsync<T>(CancellationToken cancellationToken)
    {
        this._isStarting = true;
        await this.InvokeAsync(this.StateHasChanged);
        try
        {
            var sentences = await CollectSelectedDataAsync<T>(cancellationToken);
            return await this.TrainingService.StartTrainingAsync(this.ModelType, sentences, this._selectedSourceModel, "todo", cancellationToken);
        }
        finally
        {
            this._isStarting = false;
        }
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

        return sentences;
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

    private async Task LoadFilesAsync(InputFileChangeEventArgs e)
    {
        var targetFolder = TrainingDataManager.GetFolderPath(ModelType.Relation);
        foreach (var file in e.GetMultipleFiles(100))
        {
            var targetPath = Path.Combine(targetFolder, file.Name);
            await using var inputStream = file.OpenReadStream();
            await using var writeStream = File.Create(targetPath);
            await inputStream.CopyToAsync(writeStream);
        }
    }

    private void DeleteFile(string filePath)
    {
        File.Delete(filePath);
    }
}
