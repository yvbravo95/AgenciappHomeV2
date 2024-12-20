using Agenciapp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Common.Models
{
    public class ListQueryBase
    {
        [Required] public int Page { get; set; }
        [Required] public int Size { get; set; }
        public int PageSkip { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string Search { get; set; }
        public PageType PageType { get; set; }
    }
    public enum PageType
    {
        Quantity,
        Length
    }
}
