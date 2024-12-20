using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    public partial class Shipping
    {
        public const string STATUS_INICIADA = "Iniciado";
        public const string STATUS_ENVIADO = "Enviado";
        public const string STATUS_RECIBIDO = "Recibido";
        public const string STATUS_CONFIRMADO = "Confirmado";
        public const string STATUS_CANCELADA = "Cancelado";
        public Shipping()
        {
            ShippingItem = new HashSet<ShippingItem>();
            status = STATUS_INICIADA;
        }

        public string Type { get; set; }
        public string Number { get; set; }
        public Guid PackingId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid? CarrierId { get; set; }
        public DateTime date { get; set; }
        public DateTime? ShippingDate { get; set; } //Fecha de enviado el paquete
        public string status { get; set; }
        public Agency Agency { get; set; }
        public Carrier Carrier { get; set; }
        public Office Office { get; set; }
        public decimal pesoActual { get; set; }    
        public DateTime fechaEnvio { get; set; }
        public string numeroVuelo { get; set; } 
        public ICollection<ShippingItem> ShippingItem { get; set; }
        public string Nota { get; set; }
        public string NotaEnvio { get; set; }
        public string NoDespachoDistributor { get; set; }
        public decimal CostoPasaje { get; set; }
        public decimal GastoCuba { get; set; }
        public decimal GastoUsa { get; set; }
        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public Guid? PrincipalDistributorId { get; set; }
        [ForeignKey("PrincipalDistributorId")]
        public User PrincipalDistributor { get; set; }
        public bool isReview { get; set; }
    }
}
