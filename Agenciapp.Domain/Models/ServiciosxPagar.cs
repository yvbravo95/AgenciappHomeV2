using AgenciappHome.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class ServiciosxPagar
    {
        [Key]
        public Guid ServiciosxPagarId { get; set; }
        public Guid SId { get; set; }
        public Order Order { get; set; }
        public Remittance Remittance { get; set; }
        public Rechargue Rechargue { get; set; }
        public EnvioMaritimo EnvioMaritimo { get; set; }
        public OrderCubiq OrderCubic { get; set; }
        public EnvioCaribe EnvioCaribe { get; set; }
        public Ticket Reserva { get; set; }
        public Passport Passport { get; set; }
        public Servicio Servicio { get; set; }
        //public Mercado Mercado { get; set; }
        public Cotizador Cotizador { get; set; }
        public Invoice Invoice { get; set; }
        public string NoServicio { get; set; }
        public DateTime Date { get; set; }
        public STipo Tipo { get; set; }
        public string SubTipo { get; set; }
        public decimal ImporteAPagar { get; set; }
        public Agency Agency { get; set; }
        public Wholesaler Mayorista { get; set; }
        public Bill Bill { get;  set; }
        public bool Express { get; set; } //Utilizada para marcar como express un trámite
        public bool NotaCredito {get; set;}
        public bool IsPaymentProductShipping { get; set; }
    }

    public class ServicesByPayNotBilled
    {
        [Key]
        public Guid ServiciosxPagarId { get; set; }
        public DateTime Date { get; set; }
        public Guid AgencyId { get; set; }
        public string Tipo { get; set; }
        public bool IsPaymentProductShipping { get; set; }
        public decimal ImporteAPagar { get; set; }
        public string WholesalerName { get; set; }
        public Guid? IdWholesaler { get; set; }
    }

    public enum STipo{
        Recarga,
        Remesa,
        Paquete,
        Maritimo,
        Tienda,
        [Description("Carga AM")] Cubiq,
        [Description("Envios Caribe")] EnvioCaribe,
        Reserva,
        Servicio,
        [Description("Pasaporte")] Passport,
        AppMovil,
        Invoice,
        None,
        Combo,
        [Description("Paquete Turístico")] PTuristico,
        Cotizador,
        Cargo,
        Mercado
    }
}
