using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Agenciapp.Service.ReyEnviosStore.WooCommerce.Models
{
    public class ProductItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Permalink { get; set; }
        public DateTime Date_created { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public bool Featured { get; set; }
        private string _catalog_visibility { get; set; }
        public string Catalog_visibility { get { return _catalog_visibility; } set { _catalog_visibility = CleanHtml(value); } }
        private string _description { get; set; }
        public string Description { get { return _description; } set { _description = CleanHtml(value); } }
        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }
        public string Sku { get; set; }

        [JsonIgnore]
        public decimal? FarePrice { get; set; }
        public decimal? Price { get { return FarePrice.HasValue ? Math.Round(FarePrice.Value + (FarePrice.Value * FeeAgencia / 100), 2) : 0; } set { FarePrice = value;  } }

        [JsonProperty("stock_quantity")]
        public int StockQuantity { get; set; }
        public ImageItem[] Images { get; set; }

        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal ShippingCost { get; set; }

        [JsonProperty("meta_data")]
        public List<MetaDataItem> MetaData { get; set; }

        private decimal FeeAgencia { get; set; }
        public void SetFeeAgencia(decimal fee)
        {
            FeeAgencia = fee;
        }

        public decimal Cost { get { return FarePrice.HasValue ? Math.Round(FarePrice.Value * FeeWholesaler / 100, 2) : 0; } }
        private decimal FeeWholesaler { get; set; } = 100;
        public void SetFeeWholesaler(decimal fee)
        {
            FeeWholesaler = fee;
        }

        private string CleanHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode.InnerText.Trim();
        }

        public class ImageItem
        {
            public string Id { get; set; }
            public string Src { get; set; }
            public string Name { get; set; }
        }

        public class MetaDataItem
        {
            public string Id { get; set; }
            public string Key { get; set; }
            public dynamic Value { get; set; }
        }
    }

}
