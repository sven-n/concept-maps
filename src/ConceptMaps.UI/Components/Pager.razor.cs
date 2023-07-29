namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;

/// <summary>
/// A pager component which offers buttons to navigate trough the data set.
/// </summary>
/// <typeparam name="T">The type of paged objects.</typeparam>
public partial class Pager<T>
{
    /// <summary>
    /// Gets or sets the state of the pagination.
    /// </summary>
    [Parameter]
    [Required]
    public PaginationState<T> PaginationState { get; set; } = null!;
}