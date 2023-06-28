namespace ConceptMaps.UI.Components;

using ConceptMaps.UI.Services;
using Microsoft.AspNetCore.Components;

public partial class Model
{
    /// <summary>
    /// Gets or sets the injected <see cref="IModelProvider"/>.
    /// </summary>
    [Inject]
    private IModelProvider ModelProvider { get; set; } = null!;

    [Parameter]
    public string ModelName { get; set; }

    [Parameter]
    public ModelType ModelType { get; set; }

    private bool IsActive => this.ModelProvider.IsActiveModel(this.ModelType, this.ModelName);
}
