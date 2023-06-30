namespace ConceptMaps.UI.Pages;

using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Services;

/// <summary>
/// Webpage for the <see cref="ICrawler"/>.
/// </summary>
public partial class ModelsPage : IDisposable
{
    /// <summary>
    /// Gets or sets the injected <see cref="IModelProvider"/>.
    /// </summary>
    [Inject]
    private IModelProvider ModelProvider { get; set; } = null!;

    [Inject]
    private RemoteTrainingService TrainingService { get; set; } = null!;
    
    private Task _refreshStatusTask;

    private CancellationTokenSource? _disposeCts;
    
    private TrainingStatus _relationTrainingStatus = new();

    protected override Task OnInitializedAsync()
    {
        this._refreshStatusTask = Task.Run(this.RefreshStatusLoopAsync);
        return base.OnInitializedAsync();
    }

    private async Task RefreshStatusLoopAsync()
    {
        var cancellationToken = (this._disposeCts ??= new()).Token;
        while (true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                this._relationTrainingStatus = await this.TrainingService.GetTrainingStatus(ModelType.Relation, cancellationToken).ConfigureAwait(false);
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

    public void Dispose()
    {
        _disposeCts?.Cancel();
        _disposeCts?.Dispose();
        _disposeCts = null;
    }
}

