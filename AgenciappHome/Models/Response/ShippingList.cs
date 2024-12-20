using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class ShippingList
    {
        public Guid PackingId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateSend { get; set; }
        public string ShippingDate { get; set; }
        public string Status { get; set; }
        public string CarrierName { get; set; }
        public string NumeroVuelo { get; set; }
        public bool isReview { get; set; }
        public List<string> Bags { get; set; }
        public List<string> PackageNumbers { get; set; }
    }
}
