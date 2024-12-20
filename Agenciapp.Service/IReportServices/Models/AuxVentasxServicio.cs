using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.IReportServices.Models
{
    public class AuxVentasxServicio
    {
        public List<ParAux<string,decimal>> ventasHoy { get; set; }
        public decimal totalVentasHoy { get; set; }
        public List<ParAux<string, decimal>> ventasAyer { get; set; }
        public decimal totalVentasAyer { get; set; }

        public List<ParAux<string, decimal>> UtilidadHoy { get; set; }
        public decimal totalUtilidadHoy { get; set; }
        public List<ParAux<string, decimal>> UtilidadAyer { get; set; }
        public decimal totalUtilidadAyer { get; set; }

        public List<ParAux<ParAux<Guid, string>, decimal>> liquidacionHoy { get; set; }
        public decimal totalLiquidacionHoy { get; set; }
        public decimal totalLiquidacionAyer { get; set; }
        public List<ParAux<ParAux<Guid, string>, decimal>> liquidacionAyer { get; set; }

    }
}
