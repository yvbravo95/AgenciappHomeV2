using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.TicketModule
{
    public class TicketListQuery: ListQueryBase
    {
        public string ClientName { get; set; }
        public string ClientSurname { get; set; }
        public string NumReserva { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public string CreatedDate { get; set; }
        public string Amount { get; set; }
        public string Debit { get; set; }
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
        public string UserId { get; set; }
        public string UserType { get; set; }
    }
}
