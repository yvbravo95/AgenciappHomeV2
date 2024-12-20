using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.BarcodeLookupServices.Models
{
    public class Store
    {
        public String name { get; set; }
        public String price { get; set; }
        public String link { get; set; }
        public String currency { get; set; }
        public String currency_symbol { get; set; }
    }
}
