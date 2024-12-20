using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.ModelsEnvioCaribe
{
    public class AuxEnvioCaribe
    {
        public string numero { get; set; }
        public string modalidadEnvio { get; set; }
        public string servicio { get; set; }
        public string user { get; set; }
        public AuxECClient client { get; set; }
        public AuxECContact contact { get; set; }
        public List<AuxECPaquete> paquetes { get; set; }
    }
}
