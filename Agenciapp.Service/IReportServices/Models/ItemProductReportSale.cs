using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IReportServices.Models
{
    public class ItemProductReportSale
    {
        public Guid AgencyId { get; set; }
        public Guid? ProductId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Bodega { get; set; }
        public int QtySale { get; set; }
        public int Availability { get; set; }
        public decimal Cost { get; set; }
        public decimal PriceRef { get; set; }
        public decimal SumSale { get; set; }
    }
}
