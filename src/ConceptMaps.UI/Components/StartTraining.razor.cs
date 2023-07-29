namespace ConceptMaps.UI.Components;

using System.Text.Json;
using System.Threading;
using ConceptMaps.Crawler;
using ConceptMaps.DataModel;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

/// <summary>
/// The component which allows to start a training.
/// It shows a selection of available training files and the button to start the training.
/// </summary>
public partial class StartTraining : IDisposable
{
    /// <summary>
    /// The list of selected training files.
    /// </summary>
    private List<string> _selectedFiles = new();

    /// <summary>
    /// The name of the selected source model. Currently unused.
    /// </summary>
    private string? _selectedSourceModel;

    /// <summary>
    /// The <see cref="CancellationTokenSource"/> which is used to start the training.
    /// TODO: A button to cancel it, is missing.
    /// </summary>
    private CancellationTokenSource? _startCts;

    /// <summary>
    /// The start success flag.
    /// </summary>
    private bool? _startSuccess;

    /// <summary>
    /// The flag which holds if the training is currently starting.
    /// </summary>
    private bool _isStarting;

    /// <summary>
    /// Gets or sets the type of the model.
    /// In our case, it's always <see cref="ModelType.Relation"/>.
    /// </summary>
    [Parameter]
    public ModelType ModelType { get; set; }

    /// <summary>
    /// Gets or sets the training data manager.
    /// </summary>
    [Inject]
    private ITrainingDataManager TrainingDataManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets the training service.
    /// </summary>
    [Inject]
    private RemoteTrainingService TrainingService { get; set; } = null!;

    /// <inheritdoc />
    public void Dispose()
    {
        this._startCts?.Dispose();
        this._startCts = null;
    }

    /// <summary>
    /// Called when the start button is clicked.
    /// Starts the training.
    /// </summary>
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

    /// <summary>
    /// Starts the training by first collecting and combining the selected training data files,
    /// and then sending a training request to the <see cref="TrainingService"/>.
    /// </summary>
    /// <typeparam name="T">The type of training data.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A flag indicating, if the training has been successfully started.</returns>
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

    /// <summary>
    /// Collects and combines the selected training data.
    /// </summary>
    /// <typeparam name="T">The type of the training data.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The combined list of training data.</returns>
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

    /// <summary>
    /// Called when the cancel button is clicked.
    /// TODO: This button is currently missing.
    /// </summary>
    private void OnCancelButtonClick()
    {
        this._startCts?.Cancel();
        this._startCts?.Dispose();
        this._startCts = null!;
    }

    /// <summary>
    /// Handles the selection of files from the local file system, saving them
    /// into the training data folder.
    /// </summary>
    /// <param name="e">The <see cref="InputFileChangeEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Deletes the training data file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    private void DeleteFile(string filePath)
    {
        File.Delete(filePath);
    }
}
