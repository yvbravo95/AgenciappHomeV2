using Agenciapp.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Reclamo
    {
        public static string Abierto = "Abierto";
        public static string Cerrado = "Cerrado";
        [Key]
        public Guid ReclamoId { get; set; }
        public string Number { get; set; }

        public Guid ClientId  { get; set; }
        public Client Client { get; set; }
        public Agency Agency { get; set; }
        public Guid AgencyId { get; set; }
        public User Asignado { get; set; }
        
        [Display(Name = "Asignado a")]
        public Guid? AsignadoId { get; set; }

        [Display(Name = "Número de Orden")]
        public string OrderNumber { get; set; }

        public string Estado { get; set; }   
        
        [Display(Name = "Tipo")]
        public ReclamoType Type { get; set; }
        public List<ReclamoTicket> ReclamoTickets { get; set; }
        public List<AttachmentReclamo> Attachments { get; set; }
    }
    public class ReclamoTicket
    {
        public Guid ReclamoTicketId { get; set; }
        public Guid ReclamoId { get; set; }
        public Reclamo Reclamo { get; set; }
        public User CreateBy { get; set; }
        public DateTime CreateAt { get; set; }
        public string Nota { get; set; }
    }
    public enum ReclamoType
    {
        [Description("")]
        None,
        Combo,
        Paquete,
        Tienda,
        Reserva,
        Pasaporte,
        Remesa,
        Recarga,
        [Description("Envío Marítimo")]
        EnvioMaritimo,
        [Description("Envío Caribe")]
        EnvioCaribe,
        [Description("Ordenes Cubiq")]
        OrderCubiq,
        Mercado
    }
}
