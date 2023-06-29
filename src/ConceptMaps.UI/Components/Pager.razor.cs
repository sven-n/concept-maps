using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConceptMaps.UI.Components;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;

public partial class Pager<T>
{
    [Parameter]
    [Required]
    public PaginationState<T> PaginationState { get; set; }
}

public class PaginationState<T> : INotifyPropertyChanged
{
    private int _currentPageIndex;

    private int _pageSize = 10;

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

    public bool IsNextPageAvailable => this.TotalPageCount > this.CurrentPageIndex + 1;

    public bool IsPreviousPageAvailable => this._currentPageIndex > 0;

    public bool IsCurrentPageAvailable => this._currentPageIndex < this.TotalPageCount;

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

            return pageCount;
        }
    }

    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}