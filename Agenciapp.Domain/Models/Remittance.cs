using Agenciapp.Domain.Enums;
using AgenciappHome.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agenciapp.Domain.Models;

namespace AgenciappHome.Models
{
    public class Remittance: OrderBase
    {
        public const String STATUS_ENPROGESO = "En Progreso";
        public const String STATUS_PROCESADA = "Procesada";
        public const String STATUS_PENDIENTE = "Pendiente";
        public const String STATUS_INICIADA = "Iniciada";
        public const String STATUS_COMPLETADA = "Completada";
        public const String STATUS_ENVIADA = "Enviada";
        public const String STATUS_REVISADA = "Revisada";
        public const String STATUS_ENTREGADA = "Entregada";
        public const String STATUS_Despachada = "Despachada";
        public const String STATUS_CANCELADA = "Cancelada";
        public const String STATUS_NOENTREGADA = "No Entregada";

        public Remittance()
        {
            RegistroEstados = new List<RegistroEstado>();
            Pagos = new List<RegistroPago>();
            UpdatedDate = DateTime.Now;
        }

        public void UpdateStatus(string status, Guid userId)
        {
            string oldStatus = Status;
            switch (status)
            {
                case STATUS_INICIADA:
                    Status = STATUS_INICIADA;
                    break;
                case STATUS_ENTREGADA:
                    Status = STATUS_ENTREGADA;
                    break;
                case STATUS_CANCELADA:
                    Status = STATUS_CANCELADA;
                    break;
                case STATUS_NOENTREGADA:
                    Status = STATUS_NOENTREGADA;
                    break;
                case STATUS_Despachada:
                    Status = STATUS_Despachada;
                    break;
                default:
                    throw new Exception("Status not found");               
            }

            if (oldStatus!= status)
            {
                RegistroEstados.Add(new RegistroEstado
                {
                    Date = DateTime.Now,
                    UserId = userId,
                    Remittance = this,
                    Estado = Status
                });
            }
        }

        public Guid RemittanceId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? AgencyTransferidaId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid ClientId { get; set; }
        public Guid? IdAuthorizationCard { get; set; }
        public Guid ContactId { get; set; }
        public Guid UserId { get; set; }
        public string MultienvioTransactionId { get; set; }
        public string Number { get; set; }
        public string Status { get; private set; }
        public string Nota { get; set; }
        public string tarifa { get; set; } // (Tarifa fijo o porciento)
        public string numerodespacho { get; set; }
        public string NoTarjeta { get; set; }
        public string NotaTarjeta { get; set; }
        public string NumerosSerie { get; set; }
        public string CardNumber { get; set; }
        public DateTime Date { get; set; }
        public DateTime UpdatedDate { get; set; }
        public decimal Amount { get; set; }
        public decimal OtrosCostos { get; set; }
        public decimal ValorPagado { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Balance { get; set; }
        public decimal feeCobro { get; set; }
        public decimal costoMayorista { get; set; } //Si el mayorista no es por transferencia define un costo
        public decimal costoporDespacho { get; set; } //Cuando es despachada se guarda el costo aplicado por el mayorista
        public decimal credito { get; set; }
        public decimal ExchangeRate { get; set; } // Tasa de cambio CUP
        public decimal AddPrecio { get; set; }
        public bool isUsd { get; set; }
        public MoneyType MoneyType { get; set; }
        public DateTime fechaEntrega { get; set; }
        public OrderRevisada orderRevisada { get; set; } // para firma cliente
        public OrderRevisada orderEntregada { get; set; } // para entregar remesa
        [ForeignKey("AgencyTransferidaId")]
        public Agency agencyTransferida { get; set; } // Para transferir el tramite a un mayorista
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
        public Client Client { get; set; }
        public Contact Contact { get; set; }
        public Office Office { get; set; }
        public User User { get; set; }
        public AuthorizationCard authorizationCard { get; set; }
        public Wholesaler wholesaler { get; set; }
        public Wholesaler despachadaA { get; set; } //Para cuando una agencia mayorista envia el tramite a otra agencia mayorista
        public DateTime fechadespacho { get; set; }
        public List<RegistroEstado> RegistroEstados { get; set; }
        public List<RegistroPago> Pagos { get; set; }

        public Guid? CardRemittanceId { get; set; }
        [ForeignKey("CardRemittanceId")]
        public CardRemittance CardRemittance { get; set; }
        [NotMapped]
        public string AEntregar
        {
            get
            {
                switch (MoneyType)
                {
                    case MoneyType.CUP:
                        return (ValorPagado * ExchangeRate).ToString("0.00") + " CUP";
                    case MoneyType.USD:
                        return ValorPagado.ToString("0.00") + " USD";
                    case MoneyType.USD_TARJETA:
                        return ValorPagado.ToString("0.00") + " USD";
                    default:
                    return "0.00";
                }
            }
        }

        [NotMapped]
        public decimal AEntregarValue
        {
            get
            {
                switch (MoneyType)
                {
                    case MoneyType.CUP:
                        return ValorPagado * ExchangeRate;
                    case MoneyType.USD:
                        return ValorPagado;
                    case MoneyType.USD_TARJETA:
                        return ValorPagado;
                    default:
                        return 0;
                }
            }
        }
    }

    public class CardRemittance
    {
        [Key]
        public Guid CardRemittanceId { get; set; }
        public string SenderName { get; set; }
        public string SenderSurName { get; set; }
        public string SenderSecondSurName { get; set; }
        public string SenderDocumentType { get; set; }
        public string SenderNumberIdentityDocument { get; set; }
        public DateTime SenderExpirationDateDocument { get; set; }
        public string SenderCountryBirth { get; set; }
        public DateTime SenderDateBirth { get; set; }
        public decimal SenderAmountRechargueCard { get; set; }
        public string SenderAddressEEUU { get; set; }
        public string SenderPhoneEEUU { get; set; }
        public string RecipientName { get; set; }
        public string RecipientSurname { get; set; }
        public string RecipientSecondSurname { get; set; }
        public string RecipientNumberCI { get; set; }
        public string RecipientCardType { get; set; }
        public string RecipientCardNumber { get; set; }
        public string RecipientAddressCountry { get; set; }
        public string RecipientCity { get; set; }
        public string RecipientProvince { get; set; }
        public string RecipientPhone { get; set; }
        public string NotaTarjeta { get; set; }
    }
}
