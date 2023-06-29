namespace ConceptMaps.UI.Pages;

using Microsoft.AspNetCore.Components;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Services;

/// <summary>
/// Webpage for the <see cref="ICrawler"/>.
/// </summary>
public partial class ModelsPage
{
    /// <summary>
    /// Gets or sets the injected <see cref="IModelProvider"/>.
    /// </summary>
    [Inject]
    private IModelProvider ModelProvider { get; set; } = null!;

    [Inject]
    private RemoteTrainingService TrainingService { get; set; } = null!;
    
    private Task _refreshStatusTask;

    private CancellationTokenSource _disposeCts = new();

    private TrainingStatus _nrtTrainingStatus = new();

    private TrainingStatus _relationTrainingStatus = new();

    protected override Task OnInitializedAsync()
    {
        this._refreshStatusTask = Task.Run(this.RefreshStatusLoopAsync);
        return base.OnInitializedAsync();
    }

    private async Task RefreshStatusLoopAsync()
    {
        var cancellationToken = this._disposeCts.Token;
        while (true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                this._nrtTrainingStatus = await this.TrainingService.GetTrainingStatus(ModelType.Nrt, cancellationToken).ConfigureAwait(false);
                this._relationTrainingStatus = await this.TrainingService.GetTrainingStatus(ModelType.Relation, cancellationToken).ConfigureAwait(false);
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // try again next time ...
            }
        }
    }
}

