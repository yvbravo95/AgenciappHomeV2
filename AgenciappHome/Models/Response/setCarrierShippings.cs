using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class SetCarrierShippings
    {
        public SetCarrierShippings()
        {
            Shippings = new List<Guid>();
        }
        public List<Guid> Shippings { get; set; }
        public Guid CarrierId { get; set; }
        public string NoVuelo { get; set; }
        public DateTime FechaLlegada { get; set; }
        public decimal CostoPasaje { get; set; }
        public decimal GastoCuba { get; set; }
        public decimal GastoUsa { get; set; }
        public string NotaEnvio { get; set; }
        public Guid? DistributorId { get; set; }
    }
}
