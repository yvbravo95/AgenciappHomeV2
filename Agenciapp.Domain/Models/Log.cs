using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AgenciappHome.Models.ApiModel;

namespace AgenciappHome.Models
{
    public class Log
    {
        [Key]
        public Guid LogId { get; set; }

        public Guid AgencyId { get; set; }
        
        public Agency Agency { get; set; }

        public User User { get; set; }

        public Client Client { get; set; }

        public LogEvent Event { get; set; }

        public LogType Type { get; set; }

        public string Message { get; set; }

        public DateTime Date { get; set; }
        public string Precio { get;  set; }
        public string Pagado { get;  set; }
        public Order Order { get; set; }
        public Remittance Remittance { get; set; }
        public Rechargue Rechargue { get; set; }
        public EnvioMaritimo EnvioMaritimo { get; set; }
        public OrderCubiq OrderCubic { get; set; }
        public EnvioCaribe EnvioCaribe { get; set; }
        public Ticket Reserva { get; set; }
        public Passport Passport { get; set; }
        public Servicio Servicio { get; set; }
        public Invoice Invoice { get; set; }
        public PaqueteTuristico PaqueteTuristico { get; set; }
        public Mercado Mercado { get; set; }
    }

    public enum LogEvent
    {
        Todas,
        Crear,
        Editar,
        Eliminar,
        Cancelar,
        Consumir
    }

    public enum LogType
    {
        Todos,
        Credito,
        Reserva,
        Orden,
        Combo,
        Pasaporte,
        EnvioMaritimo,
        Recarga,
        Cubiq,
        Servicio,
        Remesa,
        EnvioCaribe,
        PaqueteTuristico,
        Client,
        Mercado
    }
}
