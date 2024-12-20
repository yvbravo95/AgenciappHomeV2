using System.Collections.Generic;
using System;

namespace AgenciappHome.Models.Templates.Mercado
{
    public class EditPaymentTemplate
    {
        public Guid MercadoId { get; set; }
        public List<RegistroPago> Pagos { get; set; }
        public decimal Total { get; set; }
        public decimal Debit { get; set; }
        public DateTime Date { get; set; }
        public AuthorizationCard AuthCard { get; set; }
        public STipo Tipo { get; set; }
        public string ReturnUrl { get; set; }
    }
}
