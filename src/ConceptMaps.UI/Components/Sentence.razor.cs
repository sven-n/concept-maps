namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using ConceptMaps.UI.Data;
using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;

public sealed partial class Sentence : IDisposable
{
    [Parameter]
    [Required]
    public SentenceContext? Context { get; set; }

    [Parameter]
    public EventCallback OnStateChange { get; set; }

    [Parameter]
    public EventCallback<SentenceContext> OnDelete { get; set; }

    [Inject]
    private SentenceAnalyzer SentenceAnalyzer { get; set; } = null!;

    public string AlertClass => this.Context?.State switch
    {
        SentenceState.Initial => "alert-secondary",
        SentenceState.Processing => "alert-info",
        SentenceState.Processed => "alert-primary",
        SentenceState.Reviewed => "alert-success",
        SentenceState.Hidden => "alert-warning",
        _ => "alert-secondary"
    };

    public void Dispose()
    {
        this.SentenceAnalyzer.IsAnalyzingChanged -= OnAnalyzingChanged;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.SentenceAnalyzer.IsAnalyzingChanged += OnAnalyzingChanged;
    }

    private void OnAnalyzingChanged(object? sender, EventArgs e)
    {
        this.InvokeAsync(this.StateHasChanged);
    }

    private void StartResolveSentence()
    {
        var progress = new Progress<int>(i => this.InvokeAsync(this.StateHasChanged));
        this.SentenceAnalyzer.StartAnalyzeSingle(this.Context!, 0, progress);
    }

    private async Task OnDeleteClickAsync()
    {
        this.Context!.State = SentenceState.Deleted;
        if (this.OnDelete.HasDelegate)
        {
            await this.OnDelete.InvokeAsync(this.Context);
        }
    }

    private async Task OnHideClickAsync()
    {
        this.Context!.State = SentenceState.Hidden;
        await this.RaiseChangeEventAsync();
    }

    private async Task OnAcceptClickAsync()
    {
        foreach (var relationship in this.Context!.Relationships)
        {
            relationship.KnownRelationshipType = relationship.RelationshipTypeInSentence;
        }

        this.Context!.State = SentenceState.Reviewed;
        await this.RaiseChangeEventAsync();
    }

    private async Task OnEditClickAsync()
    {
        this.Context!.State = SentenceState.Initial;
        await this.RaiseChangeEventAsync();
    }

    private async Task RaiseChangeEventAsync()
    {
        if (this.OnStateChange.HasDelegate)
        {
            await this.OnStateChange.InvokeAsync();
        }
    }
}