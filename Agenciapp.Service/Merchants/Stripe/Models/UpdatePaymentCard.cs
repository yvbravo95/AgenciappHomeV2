using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Merchants.Stripe.Models
{
    public class UpdatePaymentCard
    {
        public string CardId { get; }
        public string CustomerId { get; }
        public int ExpireMonth { get; }
        public int ExpireYear { get; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
    }
}
