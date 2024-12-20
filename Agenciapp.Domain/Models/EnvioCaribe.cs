using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Domain.Models;

namespace AgenciappHome.Models
{

    public class EnvioCaribe: OrderBase
    {
        public static String STATUS_PENDIENTE = "Pendiente";
        public static String STATUS_INICIADA = "Iniciada";
        public static String STATUS_DESPACHADA = "Despachada";
        public static String STATUS_CANCELADA = "Cancelada";
        public static String STATUS_CONFECCION = "Confeccion";

        public EnvioCaribe()
        {
            EnvioCaribeValorAduanals = new HashSet<EnvioCaribeValorAduanal>();
            paquetes = new List<PaqueteEnvCaribe>();
            Number = "EC" + DateTime.UtcNow.ToString("yMMddHHmmssff");
            EnvioCaribeId = Guid.NewGuid();
            RegistroPagos = new List<RegistroPago>();
            RegistroEstados = new List<RegistroEstado>();
        }
        [Key]
        public Guid EnvioCaribeId { get; set; }

        [Required]
        public Guid OfficeId { get; set; }
        [Required]
        public Guid ContactId { get; set; }
        [Required]
        public Guid ClientId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Number { get; set; }
        [Required]
        public string numero { get; set; } //JSON
        [Required]
        public string modalidadEnvio { get; set; } //JSON
        [Required]
        public string servicio { get; set; }//JSON
        [Required]
        public string Status { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string Nota { get; set; }
        public string NoDespacho { get; set; }
        public string NotaDespacho { get; set; }

        [Required]
        public decimal ValorPagado { get; set; }
        [Required]
        public decimal OtrosCostos { get; set; }
        public decimal Precio { get; set; }
        public decimal addPrecio { get; set; }
        public bool isComboAseoAlim { get; set; }

        public decimal costo { get; set; }
        public decimal PesoTotal { get { return paquetes.Sum(x => x.peso); } }
        public decimal Balance { get { return Amount - ValorPagado - Credito; } }

        public decimal Credito { get; set; }

        public DateTime Date { get; set; }
        [Required]
        public Guid AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
        public Guid? AgencyTransferidaId { get; set; }
        [ForeignKey("AgencyTransferidaId")]
        public Agency AgencyTransferida { get; set; } // Para transferir el tramite a un mayorista
        public Client Client { get; set; }
        public Contact Contact { get; set; }
        public Office Office { get; set; }
        public User User { get; set; }
        public OrderRevisada firmaCliente { get; set; }
        public Guid? AuthorizationCardId { get; set; }
        public AuthorizationCard AuthorizationCard { get; set; }
        public Guid? WholesalerId { get; set; }
        [ForeignKey("WholesalerId")]
        public Wholesaler Wholesaler { get; set; }

        public ICollection<EnvioCaribeValorAduanal> EnvioCaribeValorAduanals { get; set; }
        public List<RegistroEstado> RegistroEstados { get; set; }
        public List<PaqueteEnvCaribe> paquetes { get; set; }
        public List<RegistroPago> RegistroPagos { get; set; }


    }

    public class PaqueteEnvCaribe
    {
        [Key]
        public Guid PaqueteEnvCaribeId { get; set; }
        public Guid EnvioCaribeId { get; set; }
        public string numero { get; set; }
        public string tipo_producto { get; set; }
        public decimal peso { get; set; }
        public string descripcion { get; set; }
        public decimal tarifa { get; set; }
        public EnvioCaribe EnvioCaribe { get; set; }
    }

}
