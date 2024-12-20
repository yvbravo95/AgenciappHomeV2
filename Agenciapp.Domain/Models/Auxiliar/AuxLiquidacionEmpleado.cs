using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxLiquidacionEmpleado
    {

        public string tramite { get; set; }
        public int cantidad { get; set; }
        public decimal total { get; set; }
        public decimal pagado { get; set; }
        public decimal balance { get; set; }
    }
}
