using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Domain.Models;

namespace AgenciappHome.Models
{
    public class Mercado: OrderBase
    {

        public static String STATUS_PENDIENTE = "Pendiente";
        public static String STATUS_INICIADA = "Iniciada";
        public static String STATUS_CANCELADA = "Cancelada";

        [Key]
        public Guid MercadoId { get; set; }

        public Guid AgencyId { get; set; }
        public Agency Agency { get; set; }

        public Guid OfficeId { get; set; }
        public Office Office { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public List<ProductosVendidos> Productos { get; set; }

        public Guid? AuthorizationCardId { get; set; }
        public AuthorizationCard AuthorizationCard { get; set; }

        public string Number { get; set; }
        public DateTime Date { get; set; }

        public decimal ServiceCost { get; set; }
        public decimal Precio { get; set; }
        public decimal Descuento { get; set; }
        public decimal Cargos { get; set; }
        public decimal Amount { get; set; }
        public decimal ValorPagado { get; set; }
        public decimal Balance { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; }
        //public decimal Cost { get; set; }
        public decimal Credito { get; set; }
        public string Nota { get; set; }
        public List<RegistroEstado> RegistroEstados { get; set; }
        public List<RegistroPago> RegistroPagos { get; set; }
        public OrderRevisada OrderRevisada { get; set; }
    }
}
