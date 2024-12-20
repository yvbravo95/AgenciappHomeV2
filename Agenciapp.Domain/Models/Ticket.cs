using Agenciapp.Domain.Models;
using AgenciappHome.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AgenciappHome.Models
{
    public partial class Ticket : OrderBase
    {
        public static String STATUS_PENDIENTE = "Pendiente";
        public static String STATUS_COMPLETADA = "Completada";
        public static String STATUS_CANCELADA = "Cancelada";
        public static String STATUS_DESPACHADA = "Despachada";
        public static String STATUS_CONFIRMADA = "Confirmada";

        public Ticket()
        {
            RegistroPagos = new List<RegistroPago>();
            RegistroEstados = new List<RegistroEstado>();
            DocumentAttachments = new List<DocumentAttachment>();
            ImageAttachments = new List<ImageAttachment>();
        }

        public void SetStatus(string status, User user)
        {
            if (State != status)
                RegistroEstados.Add(new RegistroEstado
                {
                    Date = DateTime.Now,
                    Estado = status,
                    User = user
                });
            this.State = status;
        }

        public Guid TicketId { get; set; }
        public Agency Agency { get; set; }

        [ForeignKey(nameof(AgencyTransferidaId))]
        public Agency AgencyTransferida { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? AgencyTransferidaId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid ClientId { get; set; }
        public Client Client { get; set; }
        public string State { get; private set; }

        public string Category { get; set; }
        //public Guid AirlineId { get; set; }
        public int AmountPassengers { get; set; }
        public string Checked { get; set; }
        public string Door { get; set; }
        public string NoDespacho { get; set; }
        public DateTime RegisterDate { get; set; } //Fecha de registro en agenciapp
        public DateTime CreatedDate { get; set; } //Fecha de creado el pasaje
        [Description("No. Reserva")]
        public string TicketBy { get; set; }
        public string RefZelleNumber { get; set; } //Para cuando se compra por la app de rapid
        public PaymentType RefPaymentType { get; set; } //Referencia del tipo de pago en la app de rapid
        public decimal Charges { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal CostoMayorista { get; set; } //Si el mayorista no es por transferencia define un costo
        public decimal Tax { get; set; }
        public decimal Commission { get; set; }
        public decimal Fee { get; set; }
        [Description("Descuento")]
        public decimal Discount { get; set; }
        public string TypePayment { get; set; }
        public decimal Total { get; set; }
        [Description("Valor PAgado")]
        public decimal Payment { get; set; }
        [Description("Balance")]
        public decimal Debit { get; set; }
        public Guid AirlineFlightsId { get; set; }
        [Description("Descripción")]
        public string Description { get; set; }
        public string type { get; set; }
        public string ReservationNumber { get; set; }
        public Guid? IdWholesaler { get; set; }
        [ForeignKey("IdWholesaler")]
        public Wholesaler Wholesaler { get; set; }
        public List<AttachmentTicket> Attachments { get; set; }
        public List<ImageAttachment> ImageAttachments { get; set; }
        public List<DocumentAttachment> DocumentAttachments { get; set; }
        public List<RegistroPago> RegistroPagos { get; set; }
        public AuthorizationCard authorizationCard { get; set; }
        public Guid IdAuthorizationCard { get; set; }
        public List<RegistroEstado> RegistroEstados { get; set; }
        public OrderRevisada FirmaCliente { get; set; }

        public bool ClientIsCarrier { get; set; }

        public string NombreAerolinea { get; set; }
        public string Tripulada { get; set; }

        [Description("Vuelo Salida")]
        public string Flight { get; set; } // NoVueloSalida
        [Description("Fecha Salida")]
        public DateTime DateOut { get; set; } //Fecha Salida

        [Description("Hora Salida")]
        public DateTime HoraSalida { get; set; }
        public string FromSalida { get; set; }
        public string ToSalida { get; set; }
        public string PuertaSalida { get; set; }

        [Description("Vuelo Regreso")]
        public string NoVueloRegreso { get; set; }
        [Description("Fecha Regreso")]
        public DateTime DateIn { get; set; } //Fecha Regreso
        [Description("Hora Regreso")]
        public DateTime HoraRegreso { get; set; }
        public string HoraRecogida { get; set; }
        public string FromRegreso { get; set; }
        public string ToRegreso { get; set; }
        public string PuertaSalidaRegreso { get; set; }
        public string NoConfirmacion { get; set; }
        public bool CreatedByCode { get; set; }
        public List<Pasajero> Pasajeros { get; set; }

        public string UserAirline { get; set; }

        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public decimal Credito { get; set; }

        public string ProvinciaReferencia { get; set; }
        public string MunicipioReferencia { get; set; }
        public string TelefonoReferencia { get; set; }

        public string ModeloAuto { get; set; }
        public string TransmisionAuto { get; set; }
        public string CategoriaAuto { get; set; }

        public Guid? RentadoraId { get; set; }
        public Rentadora Rentadora { get; set; }

        public int CantidadAdultos { get; set; }
        public int CantidadaMenores { get; set; }
        public int CantidadInfantes { get; set; }
        public List<Habitaciones> Habitaciones { get; set; }
        public int CantidadHabitaciones { get; set; }

        public Discount DiscountCode { get; set; }

        //Api Reservation Data
        public string DepartureTravelClass { get; set; }
        public string ReturnTRavelClass { get; set; }
        public string DepartureCity { get; set; }
        public string DestinationCity { get; set; }
        public string ReturnFromCity { get; set; }
        public string ReturnToCity { get; set; }
        public string MerchantTransactionId { get; set; }
        public string NombreCharter { get; set; }
        public bool IsCharter { get; set; }
        public bool IsMovileApp { get; set; }
        public string ReferenceContactName { get; set; }
        public string ReferenceContactPhone { get; set; }
        public List<Log> Logs { get; set; }

        public Hotel Hotel { get; set; }
        public Guid? HotelId { get; set; }

        public Guid? PaqueteTuristicoId { get; set; }
        public PaqueteTuristico PaqueteTuristico { get; set; }
        public Minorista Minorista { get; set; }

        public RealValues GetRealValues
        {
            get
            {
                var res = new RealValues
                {
                    Total = Total,
                    Debit = 0,
                    Payment = 0
                };

                foreach (var pay in RegistroPagos.OrderByDescending(x => x.date))
                {
                    if(pay.tipoPago.Type == "Crédito o Débito")
                    {
                        decimal amount = pay.valorPagado;
                        decimal fee = Agency.creditCardFee;
                        decimal amountReal =  amount / (1 + fee / 100);
                        decimal feeApply = amount - amountReal;

                        res.Total -= feeApply;
                        res.Payment += amountReal;
                    }
                    else
                    {
                        res.Payment += pay.valorPagado;
                    }
                }
                res.Total = RedondearPorExceso(res.Total, 2);
                res.Payment = RedondearPorExceso(res.Payment, 2);
                res.Debit = res.Total - res.Payment;
                return res;
            }
        }

        private decimal RedondearPorExceso(decimal valor, int decimales)
        {
            decimal factor = (decimal)Math.Pow(10, decimales);
            return Math.Ceiling(valor * factor) / factor;
        }

        public class RealValues
        {
            public decimal Total { get; set; }
            public decimal Debit { get; set; }
            public decimal Payment { get; set; }
        }
    }
}
