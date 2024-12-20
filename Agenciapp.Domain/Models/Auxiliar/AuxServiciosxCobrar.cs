using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxServiciosxCobrar
    {
        public Agency agencia { get; set; }
        public Client cliente { get; set; }
        public string tramite { get; set; }
        public int cantTramite { get; set; }
        public decimal valor { get; set; }
        public DateTime desde { get; set; }
        public Minorista Minorista { get; set; }
        public MinoristaPasaporte MinoristaPasaporte {get; set; }
    }
}
