using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Templates.Print
{
    public class CubiqHAWBModel
    {
        public GuiaAerea Guia { get; set; }
        public AgenciappHome.Models.Settings Settings { get; set; }
    }

    public class CubiqOrderHAWBModel
    {
        public OrderCubiq Order { get; set; }
        public AgenciappHome.Models.Settings Settings { get; set; }
    }
}
