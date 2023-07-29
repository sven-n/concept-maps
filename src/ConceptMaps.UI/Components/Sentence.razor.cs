namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using ConceptMaps.UI.Data;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Component to display a sentence and its relationships, which are contained
/// in a <see cref="SentenceContext"/>.
/// </summary>
public sealed partial class Sentence : IDisposable
{
    /// <summary>
    /// Gets or sets the sentence context.
    /// </summary>
    [Parameter]
    [Required]
    public SentenceContext? Context { get; set; }

    /// <summary>
    /// Gets or sets the callback which signals a state change.
    /// </summary>
    [Parameter]
    public EventCallback OnStateChange { get; set; }

    /// <summary>
    /// Gets or sets the callback which is called on a delete.
    /// </summary>
    [Parameter]
    public EventCallback<SentenceContext> OnDelete { get; set; }

    /// <summary>
    /// Gets or sets the injected sentence analyzer for the Analyze-function.
    /// </summary>
    [Inject]
    private SentenceAnalyzer SentenceAnalyzer { get; set; } = null!;

    /// <summary>
    /// Gets the alert class for the corresponding <see cref="SentenceState"/>
    /// of the sentence.
    /// </summary>
    private string AlertClass => this.Context?.State switch
    {
        SentenceState.Initial => "alert-secondary",
        SentenceState.Processing => "alert-info",
        SentenceState.Processed => "alert-primary",
        SentenceState.Reviewed => "alert-success",
        SentenceState.Hidden => "alert-warning",
        _ => "alert-secondary"
    };

    /// <inheritdoc />
    public void Dispose()
    {
        this.SentenceAnalyzer.IsAnalyzingChanged -= OnAnalyzingChanged;
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.SentenceAnalyzer.IsAnalyzingChanged += OnAnalyzingChanged;
    }

    /// <summary>
    /// Called when the analyzer starts or ends the analysis of a sentence.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void OnAnalyzingChanged(object? sender, EventArgs e)
    {
        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Starts to analyze the sentence of the <see cref="Context"/>.
    /// </summary>
    private void StartAnalyzeSentence()
    {
        var progress = new Progress<int>(i => this.InvokeAsync(this.StateHasChanged));
        this.SentenceAnalyzer.StartAnalyzeSingle(this.Context!, 0, progress);
    }

    /// <summary>
    /// Called when delete button is clicked.
    /// </summary>
    private async Task OnDeleteClickAsync()
    {
        this.Context!.State = SentenceState.Deleted;
        if (this.OnDelete.HasDelegate)
        {
            await this.OnDelete.InvokeAsync(this.Context);
        }
    }

    /// <summary>
    /// Called when the hide button is clicked.
    /// Changes the state of the sentence to <see cref="SentenceState.Hidden"/>.
    /// </summary>
    private async Task OnHideClickAsync()
    {
        this.Context!.State = SentenceState.Hidden;
        await this.RaiseChangeEventAsync();
    }

    /// <summary>
    /// Called when the accept button is clicked.
    /// Changes the state of the sentence to <see cref="SentenceState.Reviewed"/>.
    /// </summary>
    private async Task OnAcceptClickAsync()
    {
        this.Context!.State = SentenceState.Reviewed;
        await this.RaiseChangeEventAsync();
    }

    /// <summary>
    /// Called when the edit button is clicked.
    /// Changes the state of the sentence to <see cref="SentenceState.Initial"/>,
    /// so that it's not read-only anymore.
    /// </summary>
    private async Task OnEditClickAsync()
    {
        this.Context!.State = SentenceState.Initial;
        await this.RaiseChangeEventAsync();
    }

    /// <summary>
    /// Raises the <see cref="OnStateChange"/> event callback.
    /// </summary>
    private async Task RaiseChangeEventAsync()
    {
        if (this.OnStateChange.HasDelegate)
        {
            await this.OnStateChange.InvokeAsync();
        }
    }
}