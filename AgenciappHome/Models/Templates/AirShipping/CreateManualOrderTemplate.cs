using System;

namespace AgenciappHome.Models.Templates.AirShipping
{
    public class CreateManualOrderTemplate
    {
        public Guid ContactId { get; set; }
        public Guid RetailId { get; set; }
        public Guid? PrincipalDistributorId { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public decimal CantLb { get; set; }
    }
}
