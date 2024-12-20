using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models
{
    public class PaginatedList<T> : List<T>
    {
        public PaginatedList(int page, int pageSize, int total, List<T> list)
        {
            Page = page;
            PageSize = pageSize;
            Total = total;
            List = list;
        }

        public int Page { get; }
        public int PageSize { get; }
        public List<T> List { get; }
        public int Total { get; }

        public static PaginatedList<T> Empty()
        {
            return new PaginatedList<T>(1, 1, 0, new List<T>());
        }
    }
}
