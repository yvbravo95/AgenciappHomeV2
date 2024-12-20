using RapidMultiservice.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class ProductoBodegaCatalogItem
    {
        public Guid ProductoBodegaId { get; set; }
        public ProductoBodega ProductoBodega { get; set; }
        public Guid CatalogItemId { get; set; }
        public CatalogItem CatalogItem { get; set; }
    }
}
