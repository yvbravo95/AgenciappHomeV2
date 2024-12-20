using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class OrderList
    {
        public Guid OrderId { get; set; }
        public Guid ClientId { get; set; }
        public string Date { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string AgencyName { get; set; }
        public Guid? AgencyTransferidaId { get; set; }
        public string AgencyTransferidaName { get; set; }
        public string TimeDelivery { get; set; }
        public bool Express { get; set; }
        public string ClientName { get; set; }
        public string ContactName { get; set; }
        public string WholesalerName { get; set; }
        public string NoOrderTienda { get; set; }
        public string TimeSend { get; set; }
        public bool LackSend { get; set; }
        public bool LackReview { get; set; }
        public string Amount { get; set; }
        public string ShippingDate { get; set; }
        public List<string> Bags { get; set; }
        public List<ShippingOrder> Shippings { get; set; }
        public List<Product> Products { get; set; }
        public OrderRevisada orderRevisada { get; set; }


    }

    
}
