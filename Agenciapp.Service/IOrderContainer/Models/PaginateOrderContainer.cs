using Agenciapp.Domain.Models;
using System.Collections.Generic;

namespace Agenciapp.Service.IOrderContainer.Models
{
    public class PaginateOrderContainer
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public bool HasPreviousPage
        {
            get
            {
                return Page > 1;
            }
        }
        public bool HasNextPage
        {
            get
            {
                return Page < TotalPages;
            }
        }

        public int TotalPages
        {
            get
            {
                int total = Total / PageSize;
                return Total % PageSize == 0 ? total : total + 1;
            }
        }
        public string Search { get; set; }
        public IEnumerable<OrderContainer> Data { get; set; }
    }
}
