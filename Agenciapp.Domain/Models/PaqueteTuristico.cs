using Agenciapp.Domain.Models;
using AgenciappHome.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{

    public partial class PaqueteTuristico: OrderBase
    {
        public static String STATUS_PENDIENTE = "Pendiente";
        public static String STATUS_COMPLETADA = "Completada";
        public static String STATUS_CANCELADA = "Cancelada";
        public static String STATUS_INCOMPLETA = "Incompleta";

        [Key]
        public Guid PaqueteId { get; set; }

        public Agency Agency { get; set; }
        public Guid AgencyId { get; set; }

        public User User { get; set; }
        public Guid UserId { get; set; }

        public Client Client { get; set; }
        public Guid? ClientId { get; set; }
        
        public string Number { get; set; }
        
        public DateTime Date { get; set; }
        
        public string Status { get; set; }
        
        public decimal Amount { get; set; }
        public decimal Costo { get; set; }
        public decimal Precio { get; set; }
        public decimal ValorPagado { get; set; }
        public decimal Balance { get { return Amount - ValorPagado; } }
        public decimal OtrosCostos { get; set; }
        public string Nota { get; set; }

        public decimal credito { get; set; }

        public List<RegistroEstado> RegistroEstados { get; set; }
        public List<RegistroPago> Pagos { get; set; }

        public List<Ticket> Tickets { get; set; }
        public List<Servicio> Servicios { get; set; }
        public OrderRevisada FirmaCliente { get; set; }

    }
}
