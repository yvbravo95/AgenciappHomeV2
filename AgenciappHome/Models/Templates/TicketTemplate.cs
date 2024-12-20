using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class TicketTemplate
    {
        [Required(ErrorMessage = "El campo de Cliente es requerido.")]
        public Guid ClientId { get; set; }

        public string typeReservation { get; set; }
        public string descriptionPassage { get; set; }
        public string descriptionAuto { get; set; }
        public string descriptionHotel { get; set; }
        public string HoraRecogida { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "El campo de Estado es requerido.")]
        [Display(Name = "Estado")]
        public string State { get; set; }

        public string ReservationNumber { get; set; }


        [Display(Name = "DateOut")]
        public DateTime DateOutPassage { get; set; }

        [Display(Name = "DateOut")]
        public DateTime DateOutAuto { get; set; }

        [Display(Name = "DateOut")]
        public DateTime DateOutHotel { get; set; }

        [Display(Name = "DateIn")]
        public DateTime DateInPassage { get; set; }

        [Display(Name = "DateIn")]
        public DateTime DateInAuto { get; set; }

        [Display(Name = "DateIn")]
        public DateTime DateInHotel { get; set; }

        [Display(Name = "Category")]
        public string Passagecategory { get; set; }
        
        [Display(Name = "Mayorista")]
        //[Required(ErrorMessage = "El campo mayorista es obligatorio")]
        public string MayoristaPassage { get; set; }

        [Display(Name = "Mayorista")]
        public string MayoristaAuto { get; set; }

        [Display(Name = "Mayorista")]
        public string MayoristaHotel { get; set; }

        [Display(Name = "Cantidad de pasajeros")]
        public int AmountPassengers { get; set; }
        
        [Display(Name = "CityOutIn")]
        public string CityOutIn { get; set; }

        [Display(Name = "Numero de Vuelo")]
        public string Flight { get; set; }

        [Required(ErrorMessage = "El campo de Puerta es requerido.")]
        [Display(Name = "Door")]
        public decimal Door { get; set; }
        
        [Required(ErrorMessage = "El campo de Boleto es requerido.")]
        [Display(Name = "Ticket")]
        public string TicketReservation { get; set; }

        [Required(ErrorMessage = "El campo de Boleto es requerido.")]
        [Display(Name = "Ticket")]
        public string TicketAuto { get; set; }

        [Required(ErrorMessage = "El campo de Boleto es requerido.")]
        [Display(Name = "Ticket")]
        public string TicketHotel { get; set; }

        [Required(ErrorMessage = "El campo de Boleto es requerido.")]
        [Display(Name = "Ticket")]
        public string TicketAislamiento { get; set; }

        [Required(ErrorMessage = "El campo de Cargos es requerido.")]
        [Display(Name = "Charges")]
        public string Charges { get; set; }
        
        [Required(ErrorMessage = "El campo de Precio es requerido.")]
        [Display(Name = "Price")]
        public string Price { get; set; }
        
        [Required(ErrorMessage = "El campo de Costo es requerido.")]
        [Display(Name = "Cost")]
        public string Cost { get; set; }

        public string CostMayorista { get; set; }

        [Required(ErrorMessage = "El campo de Impuesto es requerido.")]
        [Display(Name = "Tax")]
        public decimal Tax { get; set; }
        
        [Required(ErrorMessage = "El campo de Descuento es requerido.")]
        [Display(Name = "Discount")]
        public string Discount { get; set; }
        
        [Required(ErrorMessage = "El campo de Comisión es requerido.")]
        [Display(Name = "Commission")]
        public decimal Commission { get; set; }
        
        [Required(ErrorMessage = "El campo de Tipo de Pago es requerido.")]
        [Display(Name = "TypePayment")]
        public string TypePayment { get; set; }
        
        [Required(ErrorMessage = "El campo de Total es requerido.")]
        [Display(Name = "Total")]
        public string Total { get; set; }
        
        [Required(ErrorMessage = "El campo de Pagado es requerido.")]
        [Display(Name = "Payment")]
        public string Payment { get; set; }
        
        [Required(ErrorMessage = "El campo de Débito es requerido.")]
        [Display(Name = "Debit")]
        public string Debit { get; set; }

        public List<RegistroPago> RegistroPagos { get; set; }

        public string AuthTypeCard { get; set; }
        public string AuthCardCreditEnding { get; set; }
        public DateTime AuthExpDate { get; set; }
        public string AuthCCV { get; set; }
        public string AuthaddressOfSend { get; set; }
        public string AuthOwnerAddressDiferent { get; set; }
        public string Authemail { get; set; }
        public string Authphone { get; set; }
        public string AuthConvCharge { get; set; }
        public string TotalCharge { get; set; }
        public string AuthSaleAmount { get; set; }
        public string TextoIDImg { get; set; }
        public DateTime FechaPasaporteImg { get; set; }
        

        public string NoVueloRegreso { get; set; }
        public DateTime HoraSalida { get; set; }
        public DateTime HoraRegreso { get; set; }

        public List<Pasajero> Pasajeros { get; set; }
        public string Credito { get; set; }
        public bool CheckCredito { get; set; }
        public bool IsCarrier { get; set; }
        public Guid TicketId { get; set; }

        public string ProvinciaReferencia { get; set; }
        public string MunicipioReferencia { get; set; }
        public string TelefonoReferencia { get; set; }

        public string ModeloAuto { get; set; }
        public string TransmisionAuto { get; set; }
        public string CategoriaAuto { get; set; }
            
        public int CantidadAdultos { get; set; }
        public int CantidadaMenores { get; set; }
        public List<Habitaciones> Habitaciones { get; set; }
        public int CantidadHabitaciones { get; set; }

        public Hotel Hotel { get; set; }
        public Guid HotelId { get; set; }
        public int CantidadPersonas { get; set; }
        public DateTime DateHotel { get; set; }
        public Guid MayoristaAislamiento { get; set; }
        public string DescriptionAis { get; set; }
        public Guid PaqueteId { get; set; }
        public Guid? RentadoraId { get; set; }
        public TemporadaAuto TemporadaAuto { get; internal set; }
        public Guid? AffiliateId { get; set; }
    }

    public class TicketViewTemplate
    {
        public Guid ClientId { get; set; }
        public Agency Agency { get; set; }
        public Agency AgencyTransferida { get; set; }
        public string Fullname { get; set; }
        //public string Airline { get; set; }
        public string CityOutIn { get; set; }
        public Guid TicketId { get; set; }
        public string NumReserva { get; set; }
        public DateTime DateFlight { get; set; }
        public string Status { get; set; }
        public string type { get; set; }
        public string fecha { get; set; }
        public string CreatedDate { get; set; }
        public decimal monto { get; set; }
        public decimal debit { get; set; }
        public string phone { get; set; }
        
        public string NumVuelo { get; set; }
        
        public string DateOut { get; set; }
        public string HoraSalida { get; set; }
        
        public string DateIn { get; set; } //Fecha Regreso
        public string HoraRegreso { get; set; }


        public string NombreAerolinea { get; set; }

        public string Flight { get; set; } // NoVueloSalida
        public string NoVueloRegreso { get; set; }

        public string FromSalida { get; set; }
        public string ToSalida { get; set; }

        public string UserAirline { get; set; }

        public bool ClientIsCarrier { get; set; }

        public string TicketBy { get; set; }

        public bool IsMovileApp { get; set; }
        public Guid? PaqueteId { get; internal set; }
        public string WholesalerName { get; set; }
        public string NumeroConfirmacion { get; set; }

    }
}
