using AgenciappHome.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    public partial class Product
    {
        public Product()
        {
            PackageItem = new HashSet<PackageItem>();
            ShippingItem = new HashSet<ShippingItem>();
        }

        public Guid ProductId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? ProductoBodegaIdProducto { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Tipo { get; set; }
        public string Color { get; set; }
        public string TallaMarca { get; set; }

        public Agency Agency { get; set; }
        public ICollection<PackageItem> PackageItem { get; set; }
        public ICollection<ShippingItem> ShippingItem { get; set; }
        public BagItem BagItem { get; set; }

        public Wholesaler Wholesaler { get; set; } //Para cuando el producto sea un combo 
        [ForeignKey("ProductoBodegaIdProducto")]
        public ProductoBodega ProductoBodega { get; set; } //Para cuando es combo
        public bool esDespachado { get; set; } //Para cuando es combo
    }
}
