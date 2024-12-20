using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class BagDetails
    {
        public Guid OrderId { get; set; }
        public Guid BagId { get; set; }
        public string BagNumber { get; set; }
        public List<BagItem> BagItems { get; set; }
    }

    public class BagItem
    {
        public Guid ProductId { get; set; }
        public decimal Qty { get; set; }
        public string Tipo { get; set; }
        public string Color { get; set; }
        public string TallaMarca { get; set; }
        public Guid? ProductoBodegaId { get; set; }
    }
}
