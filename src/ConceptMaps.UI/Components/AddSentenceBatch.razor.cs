namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ConceptMaps.DataModel;
using ConceptMaps.UI.Data;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Component which allows to add multiple sentences with the same relationships
/// in one step.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
public partial class AddSentenceBatch
{
    /// <summary>
    /// A regex which matches on a numbered list of sentences, e.g.:
    ///   1. Bob is the sister of Alice.
    ///   2. Alice is the mother of Tom.
    /// The first group is the number, the second group is the sentence.
    /// It's used to process answers of ChatGPT.
    /// </summary>
    private static readonly Regex SentenceListEntryRegex = new(@"^(\d+. )(.*)$");

    /// <summary>
    /// Gets or sets the relationships which should be applied to the sentences.
    /// </summary>
    public List<Relationship> Relationships { get; set; } = new();

    /// <summary>
    /// Gets or sets the <see cref="DataPrepareContext"/> on which the sentences
    /// should be added.
    /// </summary>
    [Parameter]
    [Required]
    public DataPrepareContext PrepareContext { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="EventCallback"/> which should be called when
    /// the cancel button has been clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="EventCallback"/> which should be called after
    /// the sentences has been added.
    /// </summary>
    [Parameter]
    public EventCallback OnAdded { get; set; }

    /// <summary>
    /// Gets or sets the free input text which may contain multiple sentences.
    /// It's bound to the input field.
    /// </summary>
    public string FreeText { get; set; } = string.Empty;

    /// <summary>
    /// Called when the Add-Button is clicked. It splits the sentences and adds
    /// them with the specified relationships to the <see cref="PrepareContext"/>.
    /// </summary>
    private async Task OnAddBatchClickAsync()
    {
        if (string.IsNullOrWhiteSpace(this.FreeText))
        {
            return;
        }

        var sentences = this.FreeText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var sentence in sentences)
        {
            var finalSentence = sentence;
            if (SentenceListEntryRegex.Match(sentence) is { Success: true } match)
            {
                finalSentence = match.Groups[2].Value;
            }

            var sentenceContext = new SentenceContext(finalSentence);
            sentenceContext.Relationships.AddRange(this.Relationships.Select(r => r with { }));
            if (sentenceContext.Relationships.Count > 0)
            {
                sentenceContext.State = SentenceState.Reviewed;
            }

            this.PrepareContext.Sentences.Add(sentenceContext);
        }

        this.FreeText = string.Empty;
        if (this.OnAdded is { HasDelegate: true } onAdded)
        {
            await onAdded.InvokeAsync();
        }
    }

    /// <summary>
    /// Called when the cancel button is clicked and calls the <see cref="OnCancel"/>
    /// callback, so that the outer component can handle that - usually by removing
    /// this component.
    /// </summary>
    private async Task OnCancelClickAsync()
    {
        if (this.OnCancel is { HasDelegate: true } onCancel)
        {
            await onCancel.InvokeAsync();
        }
    }
}