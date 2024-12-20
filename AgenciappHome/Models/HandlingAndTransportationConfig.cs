using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class GeneralHandlingAndTransportationConfig
    {
        public HandlingAndTransportationConfig Minoristas { get; set; }
        public HandlingAndTransportationConfig Cubiq { get; set; }
    }
    public class HandlingAndTransportationConfig
    {
        public HandlingAndTransportationConfig_Prices Cost { get; set; }
        public HandlingAndTransportationConfig_Prices Sale { get; set; }
    }

    public class HandlingAndTransportationConfig_Prices
    {
        public List<HandlingAndTransportationConfig_Prices_Item> Hav { get; set; }
        public List<HandlingAndTransportationConfig_Prices_Item> Others { get; set; }
    }

    public class HandlingAndTransportationConfig_Prices_Item
    {
        public decimal Value { get; set; }
        public decimal InitKg { get; set; }
        public decimal EndKg { get; set; }
    }

}
