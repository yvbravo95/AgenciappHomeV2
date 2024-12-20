using Agenciapp.Common.Models.Dto;
namespace Agenciapp.Common.Models.AirShippingModule
{
    public class OrderListDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? AgencyTransferidaId { get; set; }
        public Guid AgencyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? DeliverDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? DistributedDate { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public string NoOrden { get; set; }
        public string DispatchNumberDistributor { get; set; }
        public string AgencyTransferredName { get; set; }
        public string AgencyName { get; set; }
        public string AgencyPhoneNumber { get; set; }
        public string ClientFullData { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string ContactAddressProvince { get; set; }
        public string ContactAddressMunicipality { get; set; }
        public string ClientPhoneNumber { get; set; }
        public string ContactFullData { get; set; }
        public string Status { get; set; }
        public string PrincipalDistributorFullName { get; set; }
        public string DistributorFullName { get; set; }
        public string DeliveryFullName { get; set; }
        public string UserFirstName { get; set; }
        public bool Express { get; set; }
        public bool LackSend { get; set; }
        public bool LackReview { get; set; }
        public bool WholesalerComodin { get; set; }
        public bool OrderReviewIsIncomplete { get; set; }
        public bool Problem { get { return Bags.Any(x => !x.IsComplete); } }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal WholesalerCost { get; set; }
        public List<BagDto> Bags { get; set; }
        public List<ShippingListDto> Shippings { get; set; }

        public class ShippingListDto
        {
            public Guid Id { get; set; }
            public string Number { get; set; }
        }
    }
}
