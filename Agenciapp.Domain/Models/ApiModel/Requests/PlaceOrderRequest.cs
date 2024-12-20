using System;
using System.Collections.Generic;
using AgenciappHome.Models.ApiModel;
using RapidMultiservice.Models.Responses;

namespace RapidMultiservice.Models.Requests
{
    public class PlaceOrderRequest
    {
        public Guid ContactsId { get; set; }
        public Guid paymentCardId { get; set; }
        public string PromoCode { get; set; }
        public string ZelleName { get; set; }
        public PaymentType PaymentType { get; set; }
        public List<PlaceOrderProduct> Products { get; set; }
    }

    public class PlaceOrderProduct
    {
        public Guid ProductId { get; set; }
        public int Amount { get; set; }

    }

}