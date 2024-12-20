namespace Agenciapp.Common.Models.AirShippingModule
{
    public class OrderListQuery: ListQueryBase
    {
        public DateTime? CreatedAt { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? DistributedDate { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientLastName { get; set; }
        public string ClientPhone { get; set; }
        public string ContactName { get; set; }
        public string ContactLastName { get; set; }
        public string BagNumber { get; set; }
        public string ShippingNumber { get; set; }
        public string Status { get; set; }
        public string Amount { get; set; }

        //Filtros Personalizados
        public string SearchClientData { get; set; }
        public string SearchContactData { get; set; }
        public string SearchProductType { get; set; }
        public string NumberOrAgencyTransferida { get; set; }
        public string NumberOrDispatchNumberDistributor { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string SearchContactAddress { get; set; }
        public string DistributorFullName { get; set; }
        public string DeliveryFullName { get; set; }
        public string AgencyName { get; set; }
        public bool OnlyPrincipalDistributor { get; set; }
        public bool OnlyDistributor { get; set; }
        public bool OnlyDistributor2 { get; set; }
    }
}
