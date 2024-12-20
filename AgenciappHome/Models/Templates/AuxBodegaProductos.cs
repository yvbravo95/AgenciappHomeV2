using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Templates
{
    public class AuxBodegaProductos
    {
        public ProductoBodega producto { get; set; }
        public decimal cantidad { get; set; }
        public Wholesaler wholesaler { get; set; }
        public decimal cantidad2 { get; set; }
    }
}
