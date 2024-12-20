using Agenciapp.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class RegistroEstado
    {
        public RegistroEstado()
        {
            Date = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }
        public string Estado { get; set; }

        public DateTime Date { get; set; }

        public User User { get; set; }
        public Guid UserId { get; set; }
        public Guid? ServicioId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? BagEMId { get; set; }
        public Guid? PaqueteId { get; set; }
        public Remittance Remittance { get; set; }
        public Order Order { get; set; }
        public Rechargue Rechargue { get; set; }
        public Servicio Servicio { get; set; }
        public OrderCubiq OrderCubiq { get; set; }
        public PaqueteTuristico PaqueteTuristico { get; set; }
        public EnvioCaribe EnvioCaribe { get; set; }
        public EnvioMaritimo EnvioMaritimo { get; set; }
        public Ticket Ticket { get; set; }
        public OrderContainer OrderContainer { get; set; }

        [ForeignKey("BagEMId")]
        public BagEM BagEM { get; set; }

        [ForeignKey("PaqueteId")]
        public Paquete Paquete { get; set; }
        public string GetStatusTracking()
        {
            if(OrderId != null)
            {
                string status = this.Estado;
                string publicStatus = statusOrder[this.Estado];
                if (!string.IsNullOrEmpty(publicStatus))
                    status = $"{status}: {publicStatus}";

                return status;
            }

            return this.Estado;
        }

        private Dictionary<string, string> statusOrder = new Dictionary<string, string>()
        {
            [Order.STATUS_INICIADA] = "Recibida en Agencia",
            [Order.STATUS_COMPLETADA] = "Revisada y Empaquetada",
            [Order.STATUS_ENVIADA] = "Enviada",
            [Order.STATUS_DESPACHADA] = "Arribo a Cuba",
            [Order.STATUS_RECIBIDA] = "En Almacen de Cuba",
            ["Distribuida"] = "En tránsito a cliente final",
            [Order.STATUS_ENTREGADA] = "Entregada a cliente final"
        };
    }
}
