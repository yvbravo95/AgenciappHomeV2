using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Agenciapp.Common.Models.Dto
{
    public class ShippingDto
    {
        public Guid PackingId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateSend { get; set; }
        public DateTime? ShippingDate { get; set; }
        public string Status { get; set; }
        public string CarrierName { get; set; }
        public string CarrierSurname { get; set; }
        public string NumeroVuelo { get; set; }
        public string PrincipalDistributorFullName { get; set; }
        public string AgencyName { get; set; }
        public bool isReview { get; set; }
        public List<BagDto> Bags { get; set; }
        public List<string> PackageNumbers { get; set; }
        public List<ShippingItemDto> Items { get; set; }

    }
}

