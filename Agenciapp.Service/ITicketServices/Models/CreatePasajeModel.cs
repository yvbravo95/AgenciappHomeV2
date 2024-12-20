using Agenciapp.Service.IComboServices.Models;
using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Service.ITicketServices.Models
{
    public class CreatePasajeModel
    {
        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid ClientId { get; set; }
        public Guid UserId { get; set; }
        public string Pnr { get; set; }
        public string Category { get; set; }
        public string ReservationNumber { get; set; }
        public List<Pasajero> Pasajeros { get; set; }
        public AuthorizationCard authorizationCard { get; set; }
        public DateTime? CreatedDate { get; set; } //Fecha de creado el pasaje
        [Description("No. Reserva")]
        public string DepartureNumberFlight { get; set; }
        public string ReturnNumberFlight { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal Charges { get; set; } //C.servicio
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public decimal Fee { get; set; }
        [Description("Descripción")]
        public string Description { get; set; }
        public Guid? IdWholesaler { get; set; }
        public AuthorizationCard AuthorizationCard { get; set; }
        public List<Pay> Pays { get; set; }
        public bool ClientIsCarrier { get; set; }
        public string NombreAerolinea { get; set; }
        public string NombreCharter { get; set; }
        public bool IsCharter { get; set; }
        public decimal Credito { get; set; }

        public string ProvinciaReferencia { get; set; }
        public string MunicipioReferencia { get; set; }
        public string TelefonoReferencia { get; set; }
        public string PromoCode { get; set; }

        //Api Reservation Data
        public int Adults { get; set; }
        public int Infants { get; set; }
        public int Childs { get; set; }
        public string DepartureTravelClass { get; set; }
        public string ReturnTravelClass { get; set; }
        public string DepartureCity { get; set; }
        public string DestinationCity { get; set; }
        public string ReturnFromCity { get; set; }
        public string ReturnToCity { get; set; }
        public string MerchantTransactionId { get; set; }
        public bool IsMovileApp { get; set; }
        public string ReferenceContactName { get; set; }
        public string ReferenceContactPhone { get; set; }
    }
}
