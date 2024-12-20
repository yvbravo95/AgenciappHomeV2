namespace Agenciapp.Common.Models.AirShippingModule
{
    public class ComboListDto
    {
        public ComboListDto()
        {
            Products = new List<Product>();
        }
        public Guid Id { get; set; }
        public Guid? InvoiceId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? AgencyTransferidaId { get; set; }
        public Guid? MinoristaId { get; set; }
        public Guid ClientId { get; set; }

        public string Status { get; set; }
        public string Number { get; set; }
        public string NoOrden { get; set; }
        public string Date { get; set; }
        public string InvoiceNumber { get; set; }
        public string AgencyName { get; set; }
        public string AgencyTransferidaName { get; set; }
        public string RetailerName { get; set; }
        public string ClientFullData { get; set; }
        public string ClientPhoneNumber { get; set; }
        public string ContactFullData { get; set; }
        public string ContactAddressProvince { get; set; }

        public decimal Balance { get; set; }
        public decimal Amount { get; set; }
        public decimal WholesalerCost { get; set; }
        public decimal OtherCost { get; set; }

        public bool IsCreatedMovileApp { get; set; }
        public bool Express { get; set; }

        public string DispatchDate { get; set; }
        public string DeliverDate { get; set; }

        public List<Product> Products { get; set; }

        public class Product
        {
            public Guid Id { get; set; }
            public Guid? WholesalerId { get; set; }
            public string Name { get; set; }
            public string WholesalerName { get; set; }
            public int Quantity { get; set; }
        }
    }
}
