using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.TicketModule
{
    public class RemittanceListQuery : ListQueryBase
    {
        public string Date { get; set; }
        public string FechaEntrega { get; set; }
        public string Number { get; set; }
        public string Municipio { get; set; }
        public string Ciudad { get; set; }
        public string Contact { get; set; }
        public string Client { get; set; }
        public string Status { get; set; }
    }
}
