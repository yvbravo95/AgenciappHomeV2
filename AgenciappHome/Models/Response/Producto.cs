using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class Producto
    {
        public Guid ProductId { get; set; }
        public Guid ProductoBodegaId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Quantity2 { get; set; }
        public Guid? WholesalerId { get; set; }
        public string WholesalerName { get; set; }
        public Guid? WholesalerTransferencia { get; set; }
        public string BarCode { get; set; }
        public decimal Shipping { get; set; }
    }
}
