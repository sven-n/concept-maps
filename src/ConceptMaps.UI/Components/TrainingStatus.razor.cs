namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;

public sealed partial class TrainingStatus
{
    [Parameter]
    [Required]
    public ModelType ModelType { get; set; }

    [Parameter]
    [Required]
    public Services.TrainingStatus Status { get; set; } = null!;

    [Inject]
    private Services.RemoteTrainingService TrainingService { get; set; } = null!;

    private bool _isStopping;

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

