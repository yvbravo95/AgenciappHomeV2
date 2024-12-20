using Agenciapp.Domain.Models.AirTimeModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class RegistroPago
    {
        // Registro de pagos para un tramitexPagar
        [Key]
        public Guid RegistroPagoId { get; set; }
        public decimal valorPagado { get; set; }
        public DateTime date { get; set; }
        public Guid tipoPagoId { get; set; }
        public TipoPago tipoPago { get; set; }
        public decimal fee { get; set; }
        public string number { get; set; }
        public string referecia { get; set; }
        public string recibe { get; set; }
        public string nota { get; set; }
        public bool wasCanceled { get; set; }

        public Guid? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        public Guid? CuentaBancariaId { get; set; }
        [ForeignKey("CuentaBancariaId")]
        public CuentaBancaria CuentaBancaria { get; set; }

        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid UserId { get; set; }

        public Agency Agency { get; set; }
        public Office Office { get; set; }
        public User User { get; set; }

        public Guid? FacturaId { get; set; }
        [ForeignKey("FacturaId")]
        public Factura Factura { get; set; }

        public Guid? BillId { get; set; }
        [ForeignKey("BillId")]
        public Bill Bill { get; set; }

        public Guid? RechargueId { get; set; }
        [ForeignKey("RechargueId")]
        public Rechargue Rechargue { get; set; }

        public Guid? ServicioId { get; set; }
        [ForeignKey("ServicioId")]
        public Servicio Servicio { get; set; }

        public Guid? OrderCubiqId { get; set; }
        [ForeignKey("OrderCubiqId")]
        public OrderCubiq OrderCubiq { get; set; }

        public Guid? PassportId { get; set; }
        [ForeignKey("PassportId")]
        public Passport Passport { get; set; }

        public Guid? EnvioCaribeId { get; set; }
        [ForeignKey("EnvioCaribeId")]
        public EnvioCaribe EnvioCaribe { get; set; }

        public Guid? MercadoId { get; set; }
        [ForeignKey("MercadoId")]
        public Mercado Mercado { get; set; }

        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public Guid? EnvioMaritimoId { get; set; }
        [ForeignKey("EnvioMaritimoId")]
        public EnvioMaritimo EnvioMaritimo { get; set; }

        public Guid? TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }

        public Guid? RemittanceId { get; set; }
        [ForeignKey("RemittanceId")]
        public Remittance Remittance { get; set; }

        public Guid? PaqueteTuristicoId { get; set; }
        public PaqueteTuristico PaqueteTuristico { get; set; }

        public Guid? AuthorizationCardId { get; set; }

        public AuthorizationCard AuthorizationCard { get; set; }

        public Guid? ZelleItemId { get; set; }
        [ForeignKey("ZelleItemId")]
        public ZelleItem ZelleItem { get; set; }
    }


    [NotMapped]
    public class RegistroPagosToday
    {
        // Registro de pagos para un tramitexPagar
        [Key]
        public Guid RegistroPagoId { get; set; }
        public decimal valorPagado { get; set; }
        public DateTime date { get; set; }
        public Guid tipoPagoId { get; set; }
        public TipoPago tipoPago { get; set; }
        public decimal fee { get; set; }
        public string number { get; set; }
        public string referecia { get; set; }
        public string recibe { get; set; }
        public string nota { get; set; }
        public bool wasCanceled { get; set; }
        public Guid? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        public Guid? CuentaBancariaId { get; set; }
        [ForeignKey("CuentaBancariaId")]
        public CuentaBancaria CuentaBancaria { get; set; }

        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid UserId { get; set; }

        public Agency Agency { get; set; }
        public Office Office { get; set; }
        public User User { get; set; }

        public Guid? FacturaId { get; set; }
        [ForeignKey("FacturaId")]
        public Factura Factura { get; set; }

        public Guid? BillId { get; set; }
        [ForeignKey("BillId")]
        public Bill Bill { get; set; }

        public Guid? RechargueId { get; set; }
        [ForeignKey("RechargueId")]
        public Rechargue Rechargue { get; set; }

        public Guid? ServicioId { get; set; }
        [ForeignKey("ServicioId")]
        public Servicio Servicio { get; set; }

        public Guid? OrderCubiqId { get; set; }
        [ForeignKey("OrderCubiqId")]
        public OrderCubiq OrderCubiq { get; set; }

        public Guid? PassportId { get; set; }
        [ForeignKey("PassportId")]
        public Passport Passport { get; set; }

        public Guid? EnvioCaribeId { get; set; }
        [ForeignKey("EnvioCaribeId")]
        public EnvioCaribe EnvioCaribe { get; set; }

        public Guid? MercadoId { get; set; }
        [ForeignKey("MercadoId")]
        public Mercado Mercado { get; set; }

        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public Guid? EnvioMaritimoId { get; set; }
        [ForeignKey("EnvioMaritimoId")]
        public EnvioMaritimo EnvioMaritimo { get; set; }

        public Guid? TicketId { get; set; }
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }

        public Guid? RemittanceId { get; set; }
        [ForeignKey("RemittanceId")]
        public Remittance Remittance { get; set; }

        public Guid? PaqueteTuristicoId { get; set; }
        public PaqueteTuristico PaqueteTuristico { get; set; }

        public Guid? AuthorizationCardId { get; set; }

        public AuthorizationCard AuthorizationCard { get; set; }

        public Guid? ZelleItemId { get; set; }
        [ForeignKey("ZelleItemId")]
        public ZelleItem ZelleItem { get; set; }
    }
}
