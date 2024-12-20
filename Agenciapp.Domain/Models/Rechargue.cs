using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Agenciapp.Domain.Models;

namespace AgenciappHome.Models
{
    public partial class Rechargue: OrderBase
    {
        public static String STATUS_PENDIENTE = "Pendiente";
        public static String STATUS_PAGADO = "Pagado";
        public static String STATUS_CANCELADA = "Cancelada";

        public Rechargue()
        {
            
        }
        
        public Guid RechargueId { get; set; }
        public Guid ClientId { get; set; }
        public Guid AgencyId { get; set; }
        public string NumberPhone { get; set; }
        public decimal Count { get; set; }
        public decimal Import { get; set; } //Total a pagar
        public decimal Balance { get; set; }
        public string Number { get; set; }
        public decimal costoMayorista { get; set; }
        public decimal precioventa { get; set; } //Precio de venta
        public decimal valorPagado { get; set; } //Valor pagado
        public string estado { get; set; } // Estado de la recarga
        public DateTime date { get; set; }
        public Client Client { get; set; }
        public Guid idTipoPago { get; set; }
        public TipoPago tipoPago { get; set; }
        public Agency Agency { get; set; }
        public Wholesaler wholesaler { get; set; }
        public TipoRecarga tipoRecarga { get; set; }

        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public List<RegistroPago> RegistroPagos { get; set; }
        public List<RegistroEstado> RegistroEstados { get; set; }
        public Guid? AuthorizationCardId { get; set; }
        public AuthorizationCard AuthorizationCard { get; set; }
        public decimal Credito { get; set; }
        public OrderRevisada FirmaCliente { get; set; }
        public string Nota { get; set; }
    }

    public class TipoRecarga
    {
        public Guid TipoRecargaId { get; set; }
        public String tipo { get; set; }
    }
}
