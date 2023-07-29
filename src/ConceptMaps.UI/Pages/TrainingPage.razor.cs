namespace ConceptMaps.UI.Pages;

using Microsoft.AspNetCore.Components;
using ConceptMaps.UI.Services;

/// <summary>
/// Webpage which allows to start and monitor the training of the spaCy NLP model.
/// It retrieves the training status periodically and updates the UI accordingly.
/// </summary>
public partial class TrainingPage : IDisposable
{
    /// <summary>
    /// Gets or sets the injected training service.
    /// </summary>
    [Inject]
    private RemoteTrainingService TrainingService { get; set; } = null!;

    /// <summary>
    /// The task which updates the training status periodically.
    /// </summary>
    private Task? _refreshStatusTask;

    /// <summary>
    /// The <see cref="CancellationTokenSource"/> which stops the <see cref="_refreshStatusTask"/>
    /// when this page is disposed (e.g. by leaving it).
    /// </summary>
    private CancellationTokenSource? _disposeCts;

    /// <summary>
    /// The current training status of the relation model.
    /// </summary>
    private TrainingStatus _relationTrainingStatus = new();

    /// <inheritdoc />
    protected override Task OnInitializedAsync()
    {
        this._refreshStatusTask = Task.Run(this.RefreshStatusLoopAsync);
        return base.OnInitializedAsync();
    }

    /// <summary>
    /// Refreshes the <see cref="_relationTrainingStatus"/> periodically.
    /// </summary>
    private async Task RefreshStatusLoopAsync()
    {
        var cancellationToken = (this._disposeCts ??= new()).Token;
        while (true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await this.TrainingService.GetTrainingStatus(ModelType.Relation, cancellationToken).ConfigureAwait(false) is { } newStatus)
                {
                    this._relationTrainingStatus = newStatus;
                }

                await this.InvokeAsync(this.StateHasChanged);
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // The exception may also occur, when the http client runs into a timeout.
                // In that case, we don't want to stop the loop.
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
            catch
            {
                // try again next time ...
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposeCts?.Cancel();
        _disposeCts?.Dispose();
        _disposeCts = null;
    }
}
