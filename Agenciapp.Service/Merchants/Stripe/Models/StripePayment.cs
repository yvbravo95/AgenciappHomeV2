using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Merchants.Stripe.Models
{
    public class StripePayment
    {
        public long Amount { get; set; }
        public string ReceiptEmail { get; set; }
        public string Description { get; set; }
        public string TokenPaymentCard { get; set; }
        public string CustomerId { get; set; }
    }
}
