using System.Collections.Generic;

namespace StaffPortal.Common.Models
{
    public class PagedList<T>
    {
        public int TotalItems { get; set; }
        public int PageIndex { get; set; }
        public IList<T> Items { get; set; }

        public PagedList()
        {
            this.Items = new List<T>();
        }

        public PagedList(int pageIndex, int totalItems)
            : this()
        {
            this.PageIndex = pageIndex;
            this.TotalItems = totalItems;
        }

        public PagedList(int pageIndex, int totalItems, List<T> items)
            : this(pageIndex, totalItems)
        {
            this.Items = items;
        }
    }
}