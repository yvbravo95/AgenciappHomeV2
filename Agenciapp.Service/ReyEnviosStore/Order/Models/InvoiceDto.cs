using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.ReyEnviosStore.Order.Models
{
    public class InvoiceDto
    {
        public string Number { get; set; }
        public List<OrderDto> Orders { get; set; }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public List<ProductItemDto> Products { get; set; }
    }

    public class ProductItemDto
    {
        public int Qty { get; set; }
        public ProductDto Product { get; set; }
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
