using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AgenciappHome.Models.ApiModel;

namespace AgenciappHome.Models
{
    public class ZelleItem
    {
        [Key]
        public Guid ZellItemId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Code { get; set; }
        public string Client { get; set; }
        public string OrderNumber { get; set; }
        public string Nota { get; set; }
        public STipo Type { get; set; }
        public Order Order { get; set; }
        public Remittance Remittance { get; set; }
        public Rechargue Rechargue { get; set; }
        public EnvioMaritimo EnvioMaritimo { get; set; }
        public OrderCubiq OrderCubic { get; set; }
        public EnvioCaribe EnvioCaribe { get; set; }
        public Ticket Reserva { get; set; }
        public Passport Passport { get; set; }
        public PaqueteTuristico PaqueteTuristico { get; set; }
        public Servicio Servicio { get; set; }
        public Invoice Invoice { get; set; }

        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
    }
}
