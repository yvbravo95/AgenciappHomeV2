using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Request.Shipping
{
    public class CreateShippingVM
    {
        public Guid? CarrierId { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public decimal? Peso { get; set; }
        public string Nota { get; set; }
        public List<ShippingProductItem> Products { get; set; }
        public List<string> OrdersComplete { get; set; }
    }

    public class ShippingProductItem
    {
        public string OrderNumber { get; set; }
        public int Qty { get; set; }
        public Guid ProductId { get; set; }
    }
}
