using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RapidMultiservice.Models.Responses
{
    public class CatalogItem
    {
        public CatalogItem()
        {
            productoBodegaCatalogItems = new List<ProductoBodegaCatalogItem>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int order { get; set; }
        public Guid? RefrenceId { get; set; }
        public Guid LandingItemId { get; set; }
        [ForeignKey("LandingItemId")]
        public LandingItem LandingItem { get; set; }
        public Guid? AgencyId { get; set; }
        public Agency Agency { get; set; }
        public List<ProductoBodegaCatalogItem> productoBodegaCatalogItems { get; set; }
    }
}