using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    //Cuando un minorista cree un nuevo tramite por transferencia se genera un tramitexPagar
    public class servicioxCobrar
    {
        [Key]
        public Guid servicioxCobrarId { get; set; }
        public Guid ServicioId { get; set; }
        public string NoServicio { get; set; }
        public DateTime date { get; set; }
        public string No_servicioxCobrar { get; set; }
        public string tramite { get; set; }
        public decimal valorTramite { get; set; }// valor del tramite
        public decimal importeACobrar { get; set; }//valor aplicado por costo del mayorista
        public decimal cobrado { get; set; }//valor cobrado
        public Agency minorista { get; set; }
        public Minorista MinoristaTramite { get; set; }
        public Agency mayorista { get; set; }
        public Client remitente { get; set; }
        public Contact destinatario { get; set; }
        public Factura factura { get; set; }
        public Client cliente { get; set; }
        public Passport Passport { get; set; }
        public OrderCubiq OrderCubiq {get; set;}
        public PaqueteTuristico PaqueteTuristico { get; set; }
        public Order Order { get; set; }
    }
}
