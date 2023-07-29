namespace ConceptMaps.UI.Components;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// The state of a pagination.
/// </summary>
/// <typeparam name="T">The type of paged objects.</typeparam>
public class PaginationState<T> : INotifyPropertyChanged
{
    /// <summary>
    /// The page size. We only support 10 items on a page for now.
    /// </summary>
    private readonly int _pageSize = 10;

    /// <summary>
    /// The current page index.
    /// </summary>
    private int _currentPageIndex;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the index of the current page.
    /// </summary>
    public int CurrentPageIndex
    {
        get => Math.Max(Math.Min(this._currentPageIndex, this.TotalPageCount - 1), 0);
        set
        {
            var newValue = Math.Max(Math.Min(value, this.TotalPageCount - 1), 0);
            if (newValue != this._currentPageIndex)
            {
                this._currentPageIndex = newValue;
                this.RaisePropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether a next page is available.
    /// </summary>
    public bool IsNextPageAvailable => this.TotalPageCount > this.CurrentPageIndex + 1;

    /// <summary>
    /// Gets a value indicating whether a previous page is available.
    /// </summary>
    public bool IsPreviousPageAvailable => this._currentPageIndex > 0;

    /// <summary>
    /// Gets a value indicating whether the current page is available.
    /// </summary>
    public bool IsCurrentPageAvailable => this._currentPageIndex < this.TotalPageCount;

    /// <summary>
    /// Gets the total page count based on the number of items and the page size.
    /// </summary>
    public int TotalPageCount
    {
        get
        {
            var sentenceCount = this.Items.Count();
            var pageCount = sentenceCount / this._pageSize;
            if (sentenceCount % this._pageSize > 0)
            {
                pageCount++;
            }

            return Math.Max(pageCount, 1);
        }
    }

    /// <summary>
    /// Gets or sets the items which should be paged.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Gets the items of the current page.
    /// </summary>
    public IEnumerable<T> ItemsOfPage
    {
        get
        {
            if (!this.IsCurrentPageAvailable)
            {
                this._currentPageIndex = this.TotalPageCount - 1;
            }

            return this.Items
                .Skip(this._currentPageIndex * this._pageSize)
                .Take(this._pageSize);
        }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}