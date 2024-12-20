using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class GuiaAereaDto
    {
        public Guid GuiaAereaId { get; set; }
        public Agency Agency { get; set; }
        public DateTime FechaVuelo { get; set; }
        public string NoVuelo { get; set; }
        public string NoGuia { get; set; }
        public int Bultos { get; set; }
        public decimal PesoKg { get; set; }
        public decimal PriceAv1 { get; set; }
        public decimal PriceAv2 { get; set; }
        public decimal CostAv1 { get; set; }
        public decimal CostAv2 { get; set; }
        public string Status { get; set; }
        public string Agente { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public bool EnableHandlingAndTransportation { get; set; }

    }
}
