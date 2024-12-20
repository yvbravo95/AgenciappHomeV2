using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.ReyEnviosStore.WooCommerce.Models
{
    public class CategoryItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Parent { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
    }
}
