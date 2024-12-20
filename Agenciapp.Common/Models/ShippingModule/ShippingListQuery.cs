using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Common.Models.ShippingModule
{
    public class ShippingListQuery
    {
        [Required] public int Page { get; set; }
        [Required] public int Size { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string Search { get; set; }

        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateSend { get; set; }
        public DateTime? ShippingDate { get; set; }
        public string Status { get; set; }
        public string CarrierName { get; set; }
        public string CarrierSurname { get; set; }
        public string NumeroVuelo { get; set; }
        public string Bags { get; set; }
        public string PackageNumbers { get; set; }
        public string Product { get; set; }
        public string PrincipalDistributorFullName { get; set; }
    }
}
