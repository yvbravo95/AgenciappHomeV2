using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Multienvio.Models
{
    internal class RemittanceResult
    {
        public List<CashDelivery> CreateCashDeliveries { get; set; }
    }

    internal class CashDelivery
    {
        public string _id { get; set; }
    }
}
