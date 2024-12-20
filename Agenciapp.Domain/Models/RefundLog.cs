using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class RefundLog
    {
        public static string STATUS_PENDIENTE = "Pendiente";
        public static string STATUS_PAGADO = "Pagado";

        [Key]
        public Guid RefundLogId { get; set; }
        public Guid Agencyd { get; set; }
        public Guid UserId { get; set; }
        public Guid ClientId { get; set; }
        public Guid? TicketId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? PassportId { get; set; }
        public Guid? RecharueId { get; set; }
        public Guid? ServicioId { get; set; }
        public Guid? EnvioCaribeId { get; set; }
        public Guid? OrderCubiqId { get; set; }
        public Guid? RemesaId { get; set; }
        public Guid? EnvioMaritimoId { get; set; }
        public Guid? PaqueteTuristicoId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string NumberRefound { get; set; }
        public string NumberOrder { get; set; }
        public string Reference { get; set; }
        [DefaultValue("Pendiente")]
        public string Status { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; }
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        [ForeignKey("PassportId")]
        public Passport Passport { get; set; }
        [ForeignKey("RechargueId")]
        public Rechargue Rechargue { get; set; }
        [ForeignKey("ServicioId")]
        public Servicio Servicio { get; set; }
        [ForeignKey("EnvioCaribeId")]
        public EnvioCaribe EnvioCaribe { get; set; }
        [ForeignKey("OrderCubiqId")]
        public OrderCubiq OrderCubiq { get; set; }
        [ForeignKey("RemesaId")]
        public Remittance Remesa { get; set; }
        [ForeignKey("EnvioMaritimoId")]
        public EnvioMaritimo EnvioMaritimo { get; set; }
        [ForeignKey("PaqueteTuristicoId")]
        public PaqueteTuristico PaqueteTuristico { get; set; }

    }
}
