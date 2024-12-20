using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Merchants.Stripe.Models
{
    public class RegisterCustomerModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
    }
}
