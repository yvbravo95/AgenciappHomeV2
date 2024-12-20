using Agenciapp.Service.Multienvio.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Multienvio.Models
{
    public class CreateRemittanceModel
    {
        public string municipalityName { get; set; }
        public string provinceName { get; set; }
        public string agencyNote { get; set; }
        public string neighborhood { get; set; }
        public CurrencyType currency { get; set; }
        public decimal amount { get; set; }
        public Receiver receiver { get; set; }
        public DeliveryServerData deliveryServerData { get; set; }
    }
}
