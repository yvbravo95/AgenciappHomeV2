using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class Package
    {
        public Package()
        {
            PackageItem = new HashSet<PackageItem>();
            ShippingItem = new HashSet<ShippingItem>();
        }

        public Guid PackageId { get; set; }

        public Order PackageNavigation { get; set; }
        
        public ICollection<PackageItem> PackageItem { get; set; }
        public ICollection<ShippingItem> ShippingItem { get; set; }
    }
}
