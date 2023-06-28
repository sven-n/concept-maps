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
    
}

