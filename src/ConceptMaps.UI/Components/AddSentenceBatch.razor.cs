﻿namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ConceptMaps.Crawler;
using ConceptMaps.UI.Data;
using Microsoft.AspNetCore.Components;

public partial class AddSentenceBatch
{
    private static readonly Regex SentencelistEntryRegex = new(@"^(\d+. )(.*)$");

    public List<Relationship> Relationships { get; set; } = new();

    [Parameter]
    [Required]
    public DataPrepareContext PrepareContext { get; set; } = null!;

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public EventCallback OnAdded { get; set; }

    public string FreeText { get; set; } = string.Empty;

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
            if (SentencelistEntryRegex.Match(sentence) is { Success: true } match)
            {
                finalSentence = match.Groups[2].Value;
            }

            this.PrepareContext.Sentences.Add(new SentenceContext(finalSentence));
        }

        this.FreeText = null;
        if (this.OnAdded is { } onAdded)
        {
            await onAdded.InvokeAsync();
        }
    }

    private async Task OnCancelClickAsync()
    {
        if (this.OnCancel is { } onCancel)
        {
            await onCancel.InvokeAsync();
        }
    }

    private void OnAddRelationshipClick()
    {
        this.Relationships.Add(new Relationship());
    }
}