using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IAirShippingServices.Models
{
    public class CreateAirShipping
    {
        public CreateAirShipping()
        {
            Bags = new List<CreateBag>();
            Pays = new List<Pay>();
        }

        public DateTime? ReceivedDate { get; set; }

        public Guid UserId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid ClientId { get; set; }
        public Guid ContactId { get; set; }
        public Guid? RetaiId { get; set; }
        public Guid? WholesalerId { get; set; }
        public Guid? PrincipalDistributorId { get; set; }

        public string Type { get; set; }
        public string Number { get; set; }
        public string NoOrden { get; set; }
        public string Note { get; set; }
        public string Attachment { get; set; }
        public string Status { get; set; }

        public bool Express { get; set; }

        public decimal Delivery { get; set; }
        public decimal AddCosto { get; set; }
        public decimal AddPrecio { get; set; }
        public decimal AditionalCharge { get; set; }
        public decimal PriceLbMedicina { get; set; }
        public decimal CantLbMedicina { get; set; }
        public decimal ProductsShipping { get; set; }
        public decimal CantLb { get; set; }
        public decimal PriceLb { get; set; }
        public decimal OtherCosts { get; set; }
        public decimal CustomsTax { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal Credit { get; set; }
        public decimal CostoMayorista { get; set; }

        public CreateAuthorizationCard AuthorizationCard { get; set; }

        public List<CreateBag> Bags { get; set; }
        public List<Pay> Pays { get; set; }

        public class CreateBag
        {
            public List<CreateProduct> Products { get; set; }
        }

        public class CreateProduct
        {
            public Guid? IdWineryProduct { get; set; }
            public int Quantity { get; set; }
            public string Type { get; set; }
            public string Color { get; set; }
            public string Size { get; set; }
            public string Brand { get; set; }
            public string Description { get; set; }
        }

        public class Pay
        {
            public decimal Paid { get; set; }
            public string Type { get; set; }
            public string Note { get; set; }
        }

        public class CreateAuthorizationCard
        {
            public string AddressOfSend { get; set; }
            public string CardCreditEnding { get; set; }
            public string CCV { get; set; }
            public string ConvCharge { get; set; }
            public string Email { get; set; }
            public DateTime? ExpDate { get; set; }
            public string OwnerAddressDiferent { get; set; }
            public string Phone { get; set; }
            public string SaleAmount { get; set; }
            public string TotalCharge { get; set; }
            public string TypeCard { get; set; }
        }
    }
}
