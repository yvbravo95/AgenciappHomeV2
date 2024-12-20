using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IAirShippingServices.Models
{
    public class CreateManualOrder
    {
        public Guid ContactId { get; set; }
        public Guid RetailId { get; set; }
        public Guid? PrincipalDistributorId { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public decimal CantLb { get; set; }
    }
}
