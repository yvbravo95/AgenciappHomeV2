using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Templates
{
    public class PassportPaysTemplate
    {
        public Guid PassportId { get; set; }
        public List<RegistroPago> RegistroPagos { get; set; }
        public decimal Total { get; set; }
        public decimal Debit { get; set; }
        public DateTime Date { get; set; }
        public AuthorizationCard AuthorizationCard { get; set; }
    }
}
