using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Dto
{
    public class PackageDto
    {
        public PackageDto()
        {
            PackageItem = new List<PackageItemDto>();
            ShippingItem = new List<ShippingItemDto>();
        }
        public Guid Id { get; set; }
        public List<PackageItemDto> PackageItem { get; set; }
        public List<ShippingItemDto> ShippingItem { get; set; }
    }
}
