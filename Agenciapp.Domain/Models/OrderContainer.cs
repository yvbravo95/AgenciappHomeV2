using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agenciapp.Domain.Models
{
    /// <summary>
    /// Modelo de Base de Datos Orden Conenedor para modulo de importacion HM Paquetes
    /// </summary>
    public class OrderContainer
    {
        public static readonly string Status_Recibida = "Recibida";
        public static readonly string Status_Distribuida = "Distribuida";
        public static readonly string Status_Entregada = "Entregada";
        public static readonly string Status_Cancelada = "Cancelada";

        public OrderContainer() { 
            StateRegistry = new List<RegistroEstado>();
        }
        public OrderContainer(User user, Guid afiliadoId, string billNumber, string containerNumber, string agencyRef, int totalHbl, string hbl, string contactName, string contactAddress, string contactPhone, string contactProvince, string contactMunicipality, decimal weight)
        {
            AgencyId = user.AgencyId;
            Status = Status_Recibida;
            CreatedAt = DateTime.UtcNow;
            BillNumber = billNumber ?? throw new ArgumentNullException(nameof(billNumber));
            ContainerNumber = containerNumber ?? throw new ArgumentNullException(nameof(containerNumber));
            AgencyRef = agencyRef ?? throw new ArgumentNullException(nameof(agencyRef));
            TotalHbl = totalHbl;
            Hbl = hbl ?? throw new ArgumentNullException(nameof(hbl));
            ContactName = contactName ?? throw new ArgumentNullException(nameof(contactName));
            ContactAddress = contactAddress ?? throw new ArgumentNullException(nameof(contactAddress));
            ContactPhone = contactPhone ?? throw new ArgumentNullException(nameof(contactPhone));
            ContactProvince = contactProvince ?? throw new ArgumentNullException(nameof(contactProvince));
            ContactMunicipality = contactMunicipality ?? throw new ArgumentNullException(nameof(contactMunicipality));
            Weight = weight;
            AfiliadoId = afiliadoId;
            StateRegistry = new List<RegistroEstado>
            {
                new RegistroEstado
                {
                    Date = DateTime.Now,
                    Estado = Status,
                    UserId = user.UserId,
                    OrderContainer = this
                }
            };
        }

        public void ChangeStatus(User user, string status)
        {
            Status = status;
            var registry = new RegistroEstado
            {
                Date = DateTime.Now,
                Estado = status,
                UserId = user.UserId,
                OrderContainer = this
            };
            StateRegistry.Add(registry);
        }

        public void SetDistributor(User user, Guid distributorId)
        {
            DistributorId = distributorId;
            Status = Status_Distribuida;
            StateRegistry.Add(new RegistroEstado
            {
                Date = DateTime.Now,
                Estado = Status_Distribuida,
                UserId = user.UserId,
                OrderContainer = this
            });
        }

        public void SetRepartidor(User user, Guid repartidorId)
        {
            RepartidorId = repartidorId;
            Status = Status_Distribuida;
            StateRegistry.Add(new RegistroEstado
            {
                Date = DateTime.Now,
                Estado = Status_Distribuida,
                UserId = user.UserId,
                OrderContainer = this
            });
        }

        [Key]
        public Guid Id { get; private set; }
        public Guid AgencyId { get; private set; }
        public Guid? AfiliadoId { get; private set; }
        public Guid? DistributorId { get; private set; }
        public Guid? RepartidorId { get; private set; }
        public string Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string BillNumber { get; set; }
        public string ContainerNumber { get; private set; }
        public string AgencyRef { get; private set; }
        public int TotalHbl { get; private set; }
        public string Hbl { get; private set; }
        public string ContactName { get; private set; }
        public string ContactAddress { get; private set; }
        public string ContactPhone { get; private set; }
        public string ContactProvince { get; private set; }
        public string ContactMunicipality { get; private set; }
        public decimal Weight { get; private set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; private set; }

        [ForeignKey(nameof(AfiliadoId))]
        public Minorista Afiliado { get; private set; }

        [ForeignKey(nameof(DistributorId))]
        public User Distributor { get; private set; }

        [ForeignKey(nameof(RepartidorId))]
        public User Repartidor { get; private set; }

        public List<RegistroEstado> StateRegistry { get; set; }
    }
}
