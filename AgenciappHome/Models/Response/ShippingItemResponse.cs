using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class ShippingItemByOrder
    {
        public Guid PackageId { get; set; }
        public string PackageNumber { get; set; }
        public decimal PesoLb { get; set; }
        public decimal Amount { get; set; }
        public List<ShippingItemDetail> ShippingItems { get; set; }
    }

    public class ShippingItemDetail
    {
        public decimal Qty { get; set; }
        public Guid ProductId { get; set; }
        public string ProductType { get; set; }
        public Guid PackingId { get; set; }
        public string PackingNumber { get; set; }
        public Guid BagId { get; set; }
        public string BagNumber { get; set; }
    }

    public class ShippingItemResponse
    {
        public decimal Qty { get; set; }
        public Guid ProductId { get; set; }
        public string ProductType { get; set; }
        public Guid PackageId { get; set; }
        public string PackageNumber { get; set; }
        public Guid PackingId { get; set; }
        public string PackingNumber { get; set; }
        public Guid BagId { get; set; }
        public string BagNumber { get; set; }
    }
}
