using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Templates
{
    public class OrderTemplate
    {
        public Guid OrderId { get; set; }
        public List<RegistroPago> Pagos { get; set; }
        public decimal Total { get; set; }
        public decimal Debit { get; set; }
        public DateTime Date { get; set; }
        public AuthorizationCard AuthCard { get; set; }
        public STipo Tipo { get; set; }
        public string ReturnUrl { get; set; }
    }
}
