using System;

namespace Agenciapp.Common.Models.Dto
{
    public class PackageItemDto
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Qty { get; set; }
        public string Description { get; set; }
        public int Supplier { get; set; }

        //public PackageDto Package { get; set; }
        public ProductDto Product { get; set; }
    }
}