using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgenciappHome.Models;

namespace AgenciappHome.Controllers.Class
{
    public class TrackShipping
    {
        public string NumberShipping;
        public string FullnameCarrier = "Sin Definir";
        public decimal Qty;
        public List<TrackProduct> ProductsShipping = new List<TrackProduct>();
    }
}
