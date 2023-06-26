namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using ConceptMaps.UI.Data;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;

public partial class Sentence
{
    [Parameter]
    [Required]
    public SentenceContext? Context { get; set; }

    [Inject]
    private SentenceAnalyzer SentenceAnalyzer { get; set; } = null!;

    public string AlertClass => this.Context?.State switch
    {
        SentenceState.Initial => "alert-secondary",
        SentenceState.Processing => "alert-info",
        SentenceState.Processed => "alert-primary",
        SentenceState.Reviewed => "alert-success",
        SentenceState.Removed => "alert-warning",
        _ => "alert-secondary"
    };

    private void StartResolveSentence()
    {
        var progress = new Progress<int>(i => this.InvokeAsync(this.StateHasChanged));
        this.SentenceAnalyzer.StartAnalyzeSingle(this.Context!, 0, progress);
    }
}