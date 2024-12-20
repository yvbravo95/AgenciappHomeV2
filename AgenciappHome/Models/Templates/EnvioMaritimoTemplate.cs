using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Templates
{

    public class EnvioMaritimoTemplate
    {
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid IdAuthorizationCard { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public decimal PriceLb { get; set; }
        public decimal CantLb { get; set; }
        public Guid TipoPagoId { get; set; }
        public Guid ContactId { get; set; }
        public Guid ClientId { get; set; }
        public Guid UserId { get; set; }
        public decimal ValorPagado { get; set; }
        public decimal Balance { get; set; }
        public decimal OtrosCostos { get; set; }
        public decimal ValorAduanal { get; set; }
        public string numeroDespacho { get; set; }
        public DateTime fechadespacho { get; set; }
        public decimal costoDespacho { get; set; } 
        public string Nota { get; set; }
        public decimal cargoAdicional { get; set; }
        public string referCargoAdicional { get; set; }

        public Agency Agency { get; set; }
        public Client Client { get; set; }
        public Contact Contact { get; set; }
        public Office Office { get; set; }
        public TipoPago TipoPago { get; set; }
        public User User { get; set; }
        public ICollection<ValorAduanalItemEnvMaritimo> valorAduanalItems { get; set; }
        public AuthorizationCard authorizationCard { get; set; }
        public ICollection<ProductEM> products { get; set; }
        public Wholesaler wholesaler { get; set; }
        public Wholesaler despachadaA { get; set; } //Mayorista a quien fue despachada

        public Agency agencyTransferida { get; set; } // Para transferir el tramite a un mayorista
        public decimal costoMayorista { get; set; } //Si el mayorista no es por transferencia define un costo

        public OrderRevisada firmaCliente { get; set; }

        public List<RegistroEstado> RegistroEstados { get; set; }
        public List<RegistroPago> RegistroPagos { get; set; }
    }
}
