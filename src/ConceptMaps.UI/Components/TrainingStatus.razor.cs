namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Component to show the current training status and its console output.
/// </summary>
public sealed partial class TrainingStatus
{
    /// <summary>
    /// Flag, if the training is currently being stopped by the user.
    /// </summary>
    private bool _isStopping;

    /// <summary>
    /// Gets or sets the type of the model.
    /// </summary>
    [Parameter]
    [Required]
    public ModelType ModelType { get; set; }

    /// <summary>
    /// Gets or sets the current status of the training.
    /// </summary>
    [Parameter]
    [Required]
    public Services.TrainingStatus Status { get; set; } = null!;

    /// <summary>
    /// Gets or sets the training service.
    /// </summary>
    [Inject]
    private RemoteTrainingService TrainingService { get; set; } = null!;

    /// <summary>
    /// Called when the stop button is clicked.
    /// Requests the <see cref="TrainingService"/> to stop the training.
    /// </summary>
    private async Task OnStopClickAsync()
    {
        this._isStopping = true;
        this.StateHasChanged();
        try
        {
            await this.TrainingService.StopTrainingAsync(this.ModelType);
        }
        finally
        {
            this._isStopping = false;
        }
    }
}
