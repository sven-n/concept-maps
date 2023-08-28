namespace ConceptMaps.UI.Components;

using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Component to display and activate a trained NLP model.
/// </summary>
public partial class Model
{
    /// <summary>
    /// Gets or sets the injected <see cref="IModelProvider"/>.
    /// </summary>
    [Inject]
    private IModelProvider ModelProvider { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the model.
    /// </summary>
    [Parameter]
    public string ModelName { get; set; }

    /// <summary>
    /// Gets or sets the type of the model.
    /// </summary>
    [Parameter]
    public ModelType ModelType { get; set; }

    /// <summary>
    /// Gets a value indicating whether this model is active.
    /// </summary>
    private bool IsActive => this.ModelProvider.IsActiveModel(this.ModelType, this.ModelName);
}
