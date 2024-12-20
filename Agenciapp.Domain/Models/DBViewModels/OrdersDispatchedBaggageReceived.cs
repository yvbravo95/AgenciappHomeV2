using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.DBViewModels
{
    public class OrdersDispatchedBaggageReceived
    {
        [Key]
        public string OrderNumber { get; set; }
        public string ShippingNumber { get; set; }
        public bool IsComplete { get; set; }
        public int Qty { get; set; }
        public int QtyReceived { get; set; }
    }
}
