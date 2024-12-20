using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    
    public partial class Pago
    {
        public Pago()
        {
        }

        public Guid PagoId { get; set; }
        public Guid OrderId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid ClientId { get; set; }
        public string Number { get; set; }
        public Guid TipoPagoId { get; set; }
        public Guid UserId { get; set; }
        public decimal ValorPagado { get; set; }
        public string nota { get; set; } //Usado para cuando el pago es con Zell, Cheque, Transferencia Bancaria
        public Agency Agency { get; set; }
        public Office Office { get; set; }
        public Order Order { get; set; }
        public Client Client { get; set; }
        public TipoPago TipoPago { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; }
    }
}
