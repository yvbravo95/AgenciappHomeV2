using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models
{
    public class ListFilterResponse<T> where T: class
    {
        public int Count { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}
