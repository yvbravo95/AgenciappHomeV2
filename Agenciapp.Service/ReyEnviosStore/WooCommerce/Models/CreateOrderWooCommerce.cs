using System;
using System.Collections.Generic;

namespace Agenciapp.Service
{
    public class CreateOrderWooCommerce
    {
        public Guid ClientId { get; set; }
        public Guid ContactId { get; set; }
        public decimal Price { get; set; }
        public decimal Shipping { get; set; }
        public decimal ServiceCost { get; set; }
        public decimal Total { get; set; }
        public List<Product> Products { get; set; }
        public class Product
        {
            public string Id { get; set; }
            public string Sku { get; set; }
            public int Quantity { get; set; }
        }
    }
}
