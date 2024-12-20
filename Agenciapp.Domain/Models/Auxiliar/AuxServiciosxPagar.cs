using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxServiciosxPagar
    {
        public Wholesaler agencia { get; set; }
        public STipo tramite { get; set; }
        public int cantTramite { get; set; }
        public decimal valor { get; set; }
        public DateTime desde { get; set; }
        public decimal costo { get; set; }
        public Order order { get; set; }
        //public Guid FacturaId { get; set; }
        public decimal Pagado { get; set; }
        public bool IsPaymentProductShipping { get; set; }
    }
}
