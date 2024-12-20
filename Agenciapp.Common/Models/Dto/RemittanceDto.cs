using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Dto
{
    public class RemittanceDto
    {
        public Guid RemittanceId { get; set; }
        public string Agency { get; set; }
        public Guid AgencyTransferidaId { get; set; }
        public string AgencyTransferida { get; set; }
        public string Date { get; set; }
        public string FechaEntrega { get; set; }
        public string Number { get; set; }
        public string Municipio { get; set; }
        public string Ciudad { get; set; }
        public string Contact { get; set; }
        public string Client { get; set; }
        public string Status { get; set; }
        public string ValorPagado { get; set; }
        public string MoneyType { get; set; }
        public string AEntregar { get; set; }
        public string Amount { get; set; }
        public string Balance { get; set; }
        public string DespachadaA { get; set; }
        public string User { get; set; }
    }
}
