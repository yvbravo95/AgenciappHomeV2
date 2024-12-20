using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public partial class PaymentTicket
    {
        public PaymentTicket()
        {

        }
        [Key]
        public Guid PagoId { get; set; }
        public Guid TicketId { get; set; }
        public Guid ClientId { get; set; }
        public string Number { get; set; }
        public string tipoPago { get; set; }
        public decimal ValorPagado { get; set; }

        public Ticket Ticket { get; set; }
        public Client Client { get; set; }
        public DateTime Date { get; set; }
        public Agency Agency { get; set; }
        public Office Office { get; set; }
        public User User { get; set; }
    }
}
