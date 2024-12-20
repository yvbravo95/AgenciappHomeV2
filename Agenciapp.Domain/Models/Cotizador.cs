using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace AgenciappHome.Models
{
    public class Cotizador
    {
        [Key]
        public Guid Id { get; set; }
        public string Json { get; set; }

        public Agency Agency { get; set; }
        public Guid AgencyId { get; set; }

        public User User { get; set; }
        public Guid UserId { get; set; }

        public DateTime CreateAt { get; set; }

        public string Tipo { get; set; }
        public string Number { get; set; }

        public Client Client { get; set; }
        public Guid ClientId { get; set; }
        public List<ServiciosxPagar> ServicesByPay { get; set; }
    }
}
