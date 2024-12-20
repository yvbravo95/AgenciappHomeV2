using Agenciapp.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AgenciappHome.Models
{
    public class Factura
    {
        [Key]
        public Guid FacturaId { get; set; }
        public Guid agencyId { get; set; }
        public Guid? PreDespachoId { get; set; }
        public string NoFactura { get; set; }
        public decimal valorTotal { get; set; }
        public decimal valorPagado { get; set; }
        public decimal pagado { get; set; }
        public decimal refunds { get; set; }
        public string refundsdetalles { get; set; }
        public decimal otrospagos { get; set; }
        public bool IsSendPdf { get; set; }
        public string otrospagosdetalles { get; set; }
        public string conceptoafacturar { get; set; }
        public string estado { get; set; }
        public string tipopago { get; set; }
        public string Retail { get; set; }
        public DateTime atransferir { get; set; }
        public DateTime fecha { get; set; }
        public virtual ICollection<servicioxCobrar> valoresTramites { get; set; }
        public virtual ICollection<RegistroPago> RegistroPagos { get; set; }

        [ForeignKey("PreDespachoId")]
        public virtual PreDespachoCubiq PreDespacho { get; set; }
    }
}
