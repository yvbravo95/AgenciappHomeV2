using Agenciapp.Service.IWholesalerServices.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.ReyEnviosStore.Product.Models
{
    public class ProductStore
    {
        public Guid? Id { get; set; }
        public string Code { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Shipping { get; set; }
        public string Category { get; set; }
        public bool IsAvailable { get; set; }
        public int Availability { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Province> Provinces { get; set; }
        public Supplier Supplier { get; set; }
    }

    public class Province
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ProductStoreValidartor : AbstractValidator<ProductStore>
    {
        public ProductStoreValidartor()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
