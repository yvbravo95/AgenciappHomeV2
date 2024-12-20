using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxModelTienda
    {
        public Guid OrderId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid AgencyTransferidaId { get; set; }
        public string AgencyName { get; set; }
        public string AgencyTransferidaName { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string ClientFullName { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public decimal costoMayorista { get; set; }
        public string NoOrden { get; set; } // Para tipo envio tienda
        public Wholesaler wholesaler { get; set; }
        public Guid ClientId { get; set; }

    }
}
