using System;
using System.Collections.Generic;

namespace RapidMultiservice.Models.Responses
{
    public class OrdersDetails
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        public ContactDetails Contact { get; set; }

        public decimal TotalToPay { get; set; }
        public decimal Charges { get; set; }

        public List<ProductItem> ProductItems { get; set; }
    }

    public class ProductItem
    {
        public decimal Amount { get; set; }
        public ProductDetails Product { get; set; }

    }
}