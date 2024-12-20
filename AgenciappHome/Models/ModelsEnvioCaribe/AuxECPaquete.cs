using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.ModelsEnvioCaribe
{
    public class AuxECPaquete
    {
        public string numero { get; set; }
        public string tipo_producto { get; set; }
        public decimal peso { get; set; }
        public string descripcion { get; set; }
        public decimal tarifa { get; set; }

    }
}
