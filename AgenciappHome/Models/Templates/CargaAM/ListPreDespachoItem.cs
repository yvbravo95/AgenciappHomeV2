using System;

namespace AgenciappHome.Models.Templates.CargaAM
{
    public class ListPreDespachoItem
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public string AgencyName { get; set; }
        public DateTime Date { get; set; }
        public int CountVerificado { get; set; }
        public int CountTotal { get; set; }
    }
}
