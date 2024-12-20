using System;
using System.Collections.Generic;

namespace RapidMultiservice.Models.Responses
{
    public class ProductListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Rating { get; set; }
        public decimal Prices { get; set; }
        public bool IsAvailable { get; set; }
    }
}