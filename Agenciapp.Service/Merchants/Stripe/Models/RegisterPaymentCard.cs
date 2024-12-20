using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Merchants.Stripe.Models
{
    public class RegisterPaymentCard
    {

        public string CustomerId { get; set; }
        public string CardNumber { get; set; }
        public int ExpireMonth { get; set; }
        public int ExpireYear { get; set; }
        public string Cvc { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
    }
}
