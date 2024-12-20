using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    
    public partial class PagoEnvioMaritimo
    {
        public PagoEnvioMaritimo()
        {
        }
        [Key]
        public Guid PagoId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public string Number { get; set; }
        public Guid TipoPagoId { get; set; }
        public Guid UserId { get; set; }
        public decimal ValorPagado { get; set; }

        public Agency Agency { get; set; }
        public Office Office { get; set; }
        public EnvioMaritimo envio { get; set; }
        public Client Client { get; set; }
        public TipoPago TipoPago { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; }
    }
}
