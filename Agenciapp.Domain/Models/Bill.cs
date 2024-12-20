using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Bill
    {
        public Bill()
        {
            ServiciosxPagar = new List<ServiciosxPagar>();
            RegistroPagos = new List<RegistroPago>();
        }
        [Key]
        public Guid BillId { get; set; }
        public Guid AgencyId { get; set; }
        public string NoBill { get; set; }
        public decimal valorTotal { get { return ServiciosxPagar.Sum(x => x.ImporteAPagar); } }
        public decimal valorAPagar { get { return valorTotal - refunds + otrospagos; } }
        public decimal valorPagado { get; set; }
        public decimal refunds { get; set; }
        public string refundsdetalles { get; set; }
        public decimal otrospagos { get; set; }
        public string otrospagosdetalles { get; set; }
        public string estado { get; set; }
        public string tipopago { get; set; }
        public bool IsSendPdf { get; set; }
        public bool IsPaymentProductShipping { get; set; }
        public DateTime atransferir { get; set; }
        public DateTime fecha { get; set; }
        public List<ServiciosxPagar> ServiciosxPagar { get; set; }
        public List<RegistroPago> RegistroPagos { get; set; }

    }
}
