using Agenciapp.Service.IClientServices.Models;
using Agenciapp.Service.IContactServices.Models;
using Agenciapp.Service.ReyEnviosStore.Order.Enums;
using System.Collections.Generic;

namespace Agenciapp.Service.ReyEnviosStore.Order.Models
{
    public class CreateOrderModel
    {
        public PaymentType PaymentType { get; set; }
        public string OrderNumber { get; set; }
        public string Nota { get; set; }
        public decimal? CreditCardFee { get; set; }
        public List<ProductModel> Products { get; set; }
        public CreateClientModel Client { get; set; }
        public CreateContactModel Contact { get; set; }
    
    }
}
