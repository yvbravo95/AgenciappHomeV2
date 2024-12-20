using System.Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Domain.Models;

namespace AgenciappHome.Models
{

    public class Servicio: OrderBase
    {
        public static string EstadoPendiente = "Pendiente";
        public static string EstadoConsulado = "Consulado"; //Para DCuba
        public static string EstadoCompletado = "Completado";
        public static string EstadoDespachado = "Despachado";
        public static string EstadoRecibido = "Recibido";
        public static string EstadoEnviado = "Enviado"; //Para DCuba
        public static string EstadoEntregado = "Entregado";
        public static string EstadoCancelado = "Cancelado";

        public Servicio()
        {
            Products = new List<ProductosVendidos>();
        }

        [Key]
        public Guid ServicioId { get; set; }
        public Guid? TipoServicioId { get; set; }
        public Guid? SubServicioId { get; set; }
        public Guid clienteClientId { get; set; }
        public Guid? mayoristaIdWholesaler { get; set; }
        public Guid? AgencyId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? PaqueteTuristicoId { get; set; }
        public Guid? MinoristaId { get; set; }
        public Guid? MinorAuthorizationId { get; set; }

        public DateTime fecha { get; set; }
        public DateTime fecharecibido { get; set; }
        public DateTime fechaentregado { get; set; }
        public DateTime fechadespacho { get; set; }

        public string numero { get; set; }
        public string Nota { get; set; }
        public string estado { get; set; }
        public string LinkFedex { get; set; }
        public string numerodespacho { get; set; }
        public string Data { get; set; }

        public decimal costo { get; set; }
        public decimal importe { get; set; }
        public decimal importeTotal { get; set; }
        public decimal importePagado { get; set; }
        public decimal Credito { get; set; }
        public decimal costoMayorista { get; set; }
        public decimal cantidadOrdenes_Tienda { get; set; } //Servicio Tienda
        public decimal costoDespacho { get; set; }
        public decimal debe { get; set; }
        public decimal CostoXServicio { get; set; }

        public bool UsedCredito { get; set; }
        
        public Wholesaler despachadaA { get; set; }
        public TipoServicio tipoServicio { get; set; }
        public SubServicio SubServicio { get; set; }
        public Client cliente { get; set; }
        public Wholesaler mayorista { get; set; }
        public Agency agency { get; set; }
        public Office office { get; set; }
        public TipoPago tipoPago { get; set; }
        public AuthorizationCard AuthorizationCard { get; set; }
        public OrderRevisada FirmaCliente { get; set; }
        public PaqueteTuristico PaqueteTuristico { get; set; }
        public MinoristaOtrosServ Minorista { get; set; }

        [ForeignKey("MinorAuthorizationId")] 
        public MinorAuthorization MinorAuthorization { get; set; }

        [ForeignKey("UserId")] 
        public User User { get; set; }

        public virtual List<RegistroPago> RegistroPagos { get; set; }
        public List<RegistroEstado> RegistroEstados { get; set; }
        public List<AttachmentServicio> Attachments { get; set; }
        public List<ProductosVendidos> Products { get; set; }
        public BigInteger getNumberToValue()
        {
            BigInteger number;
            if (BigInteger.TryParse(this.numero.Replace("OS", ""), out number))
            {
                return number;
            }
            else
                return -1;
        }
    }

    public class TipoServicio
    {
        [Key]
        public Guid TipoServicioId { get; set; }
        public bool Visibility { get; set; }
        public string Nombre { get; set; }
        public decimal Cost { get; set; }
        public decimal WholesaleCosto { get; set; }
        public decimal Price { get; set; }
        public Agency agency { get; set; }
        public bool Package { get; set; }

        public List<SubServicio> SubServicios { get; set; }
    }
    public class SubServicio
    {
        [Key]
        public Guid SubServicioId { get; set; }
        public string Nombre { get; set; }

        public Guid TipoServicioId { get; set; }
        public TipoServicio TipoServicio { get; set; }
        public bool Visibility { get; set; }

        public decimal Cost { get; set; }
        public decimal WholesaleCosto { get; set; }
        public decimal Price { get; set; }
    }
}
