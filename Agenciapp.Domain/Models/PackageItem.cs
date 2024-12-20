using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class PackageItem
    {
        public Guid PackageItemId { get; set; }
        public Guid PackageId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Qty { get; set; }
        public string Description { get; set; }
        public int Supplier { get; set; }

        public Package Package { get; set; }
        public Product Product { get; set; }
    }
}
