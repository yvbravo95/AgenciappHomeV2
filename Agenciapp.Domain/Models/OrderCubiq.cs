using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using Agenciapp.Domain.Models.ValueObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace AgenciappHome.Models
{

    public partial class OrderCubiq : OrderBase
    {
        public const String STATUS_PENDIENTE = "Pendiente";
        public const String STATUS_INICIADA = "Iniciada";
        public const String STATUS_PREDESPACHADA = "PreDespachada";
        public const String STATUS_ALMACENUSA = "AlmacenUsa";
        public const String STATUS_TRANSITOUSACUBA = "Transito USA-Cuba";
        public const String STATUS_RECIBIDOCUBA = "Recibido Cuba";
        public const String STATUS_ENTREGADA = "Entregada";
        public const String STATUS_CANCELADA = "Cancelada";

        #region Status Cuba
        public const String STATUS_NACIONALIZANDO = "Nacionalizando";
        public const String STATUS_TRANSITOCUBA = "TransitoCuba";
        public const String STATUS_RECIBIDOALMACEN = "RecibidoAlmacen";
        public const String STATUS_DISTRIBUIDO = "Distribuido";

        #endregion

        public OrderCubiq()
        {
            Status = STATUS_INICIADA;
            RegistroEstados = new List<RegistroEstado>();
            CargaAMSeguroItems = new List<CargaAMSeguroItem>();
            ProductsCargaAm = new List<ProductCargaAm>();
        }


        public void UpdateStatus(string status, Guid userId, bool force = false)
        {
            UpdateStatus(status, userId, DateTime.Now, force);
        }

        public void UpdateStatus(string status, Guid userId, DateTime date, bool force = false)
        {
            Dictionary<int, string> statusOrder = new Dictionary<int, string>
            {
                { 1, STATUS_INICIADA },
                { 2, STATUS_PREDESPACHADA },
                { 3, STATUS_ALMACENUSA },
                { 4, STATUS_TRANSITOUSACUBA },
                { 5, STATUS_RECIBIDOCUBA },
                { 7, STATUS_NACIONALIZANDO },
                { 8, STATUS_TRANSITOCUBA },
                { 9, STATUS_RECIBIDOALMACEN },
                { 10, STATUS_DISTRIBUIDO },
                { 13, STATUS_ENTREGADA },
                { 14, STATUS_CANCELADA }
            };

            int key = statusOrder.FirstOrDefault(x => x.Value == status).Key;
            int actuallyKey = statusOrder.FirstOrDefault(x => x.Value == this.Status).Key;
            // el estado solo se asigna si es mayor al actual
            if(key > actuallyKey || force == true)
            {
                Status = status;
                RegistroEstados.Add(new RegistroEstado
                {
                    Id = Guid.NewGuid(),
                    OrderCubiq = this,
                    Estado = Status,
                    Date = date,
                    UserId = userId
                });
            }
        }

        [Key]
        public Guid OrderCubiqId { get; set; }
        public Guid AgencyId { get; set; }
        public Agency Agency { get; set; }

        public Guid OfficeId { get; set; }
        public Office Office { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public Guid ContactId { get; set; }
        public Contact Contact { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public PuntoEntrega PuntoEntrega { get; set; }
        public Zona Zona { get; set; }

        public Guid? AuthorizationCardId { get; set; }
        public AuthorizationCard AuthorizationCard { get; set; }

        public string AWB { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public string Transitaria { get; set; }
        public string NoManifiesto { get; set; }
        public string EnaPassport { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal Costo { get; set; }
        public decimal CostoProductosBodega { get; set; }
        public decimal OtrosCostos { get; set; }
        public decimal ValorAduanal { get; set; }
        public decimal Amount { get; set; }
        public decimal ValorPagado { get; set; }
        public decimal Balance { get; set; }
        public decimal Descuento { get; set; }
        public decimal InsuranceValue { get; set; }
        public decimal CargoAdicional { get; set; }
        public string CargoAdicionalDescription { get; set; }
        public string Status { get; private set; }
        public bool express { get; set; } //Para marcar el envio como express
        public bool NoAduana { get; set; }
        public bool RecogidaAlmacen { get; set; }
        public string Nota { get; set; }

        public decimal costoMayorista { get; set; } //Si el mayorista no es por transferencia define un costo

        public OrderRevisada orderRevisada { get; set; } // Para revisar una orden
        public OrderRevisada orderEntregada { get; set; } // Para entregar order

        public Agency agencyTransferida { get; set; } // Para transferir el tramite a un mayorista
        public Guid? agencyTransferidaId { get; set; } // Para transferir el tramite a un mayorista

        public List<ValorAduanalItem> ValorAduanalItem { get; set; }
        public List<CargaAMSeguroItem> CargaAMSeguroItems { get; set; }

        public Wholesaler wholesaler { get; set; }

        public decimal credito { get; set; }
        public List<RegistroEstado> RegistroEstados { get; set; }
        public List<RegistroPago> RegistroPagos { get; set; }
        public List<Paquete> Paquetes { get; set; }
        public List<ProductCargaAm> ProductsCargaAm { get; set; }

        public bool IsAv { get; set; }

        public HandlingAndTransportation HandlingAndTransportation { get; set; }
        public string Stamp { get; set; }

    }

}
