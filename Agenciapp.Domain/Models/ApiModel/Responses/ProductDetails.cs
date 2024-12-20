using System;
using System.Collections.Generic;

namespace RapidMultiservice.Models.Responses
{
    public class ProductDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ImageUrl { get; set; }
        public decimal Rating { get; set; }
        public decimal Prices { get; set; }
        public decimal costoServicio { get; set; }
        public decimal fee { get; set; }
        public bool IsAvailable { get; set; }
        public string Description { get; set; }
        public string Terms { get; set; }
       
        public List<ProductListItem> RelatedProducts { get; set; }
        public List<ProductListItem> MostChooseProducts { get; set; }
        public List<PrivinceAvailability> PrivinceAvailabilities { get; set; }
    }

    public class PrivinceAvailability
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}