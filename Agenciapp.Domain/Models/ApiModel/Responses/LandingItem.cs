using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RapidMultiservice.Models.Responses
{
    public class LandingItem
    {
        public LandingItem()
        {
            CatalogItems = new List<CatalogItem>();
        }
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public List<CatalogItem> CatalogItems { get; set; }
        public Agency Agency { get; set; }
        public string CustomNavigation { get; set; }
    }
}