using System;

namespace Agenciapp.Common.Models.PassportModule
{
    public class PassportListQuery
    {
        const int _maxSize = 1000;
        private int _size = 10;

        public int Page { get; set; } = 1;
        public int Size
        {
            get { return _size; }
            set { _size = Math.Min(_maxSize, value); }
        }

        public string SortBy { get; set; }

        private string _sortOrder = "asc";
        public string SortOrder
        {
            get { return _sortOrder; }
            set
            {
                if (value == "asc" || value == "desc")
                {
                    _sortOrder = value;
                }
            }
        }
        public string Search { get; set; }
        public string ClientName { get; set; }
        public string ClientSurname { get; set; }
        public string ClientFullData { get; set; }
        public string OrderNumber { get; set; }
        public string FechaSolicitud { get; set; }
        public string FechaDespacho { get; set; }
        public string FechaRecibido { get; set; }
        public string NumDespacho { get; set; }
        public string WholeslerDespacho { get; set; }
        public string FechaImportacion { get; set; }
        public string ServicioConsular { get; set; }
        public string Status { get; set; }
        public decimal Pagado { get; set; }
        public decimal Debe { get; set; }
        public string ManifiestoPasaporte { get; set; }
    }
}
