using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class BagReviewShipping
    {
        public Guid ShippingId { get; set; }
        public Guid BagId { get; set; }
        public Guid PackageId { get; set; }
        public string BagNumber { get; set; }
        public bool IsComplete { get; set; }
        public string Comment { get; set; }
    }
}
