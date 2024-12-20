using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Dto
{
    public class TicketDto
    {
        public Guid ClientId { get; set; }
        public string Agency { get; set; }
        public string AgencyTransferida { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? AgencyTransferidaId { get; set; }
        public string ClientName { get; set; }
        public string ClientSurname { get; set; }
        public Guid TicketId { get; set; }
        public string NumReserva { get; set; }
        public DateTime DateFlight { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Date { get; set; }
        public string CreatedDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Debit { get; set; }
        public string Phone { get; set; }
        public string NumVuelo { get; set; }
        public string DateOut { get; set; }
        public string HoraSalida { get; set; }
        public string DateIn { get; set; }
        public string HoraRegreso { get; set; }
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
