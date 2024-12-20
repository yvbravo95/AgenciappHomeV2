using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public class TrackProduct
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public string TallaMarca { get; set; }
        public decimal Qty { get; set; }

        public TrackProduct(string Code, string Description, string Type, string Color, string TallaMarca, decimal Qty)
        {
            this.Code = Code;
            this.Description = Description;
            this.Type = Type;
            this.Color = Color;
            this.TallaMarca = TallaMarca;
            this.Qty = Qty;
        }
    }
}
