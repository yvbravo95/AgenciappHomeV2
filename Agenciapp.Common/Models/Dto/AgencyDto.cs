using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Dto
{
    public class AgencyDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string LegalName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LogoName { get; set; }
        public string AgencyInfo { get; set; }
        public decimal CreditCardFee { get; set; }
        public decimal CreditCardFeeCombos { get; set; }
        public decimal CreditCardFeePasaje { get; set; }
        public decimal RemesaEntregaCuba { get; set; }
        public string BagCount { get; set; }
        public string AgencyNumber { get; set; }
        public decimal PrecioFoto { get; set; }
        public string Url { get; set; }
        public string UrlCalbackApiReyenvios { get; set; }
        public PhoneDto Phone { get; set; }
    }
}
