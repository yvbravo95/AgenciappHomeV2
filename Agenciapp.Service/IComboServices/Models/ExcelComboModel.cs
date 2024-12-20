using System;
namespace Agenciapp.Service.IComboServices.Models
{
    public class ExcelComboModel
    {
        public string Agencia { get; set; }
        public DateTime Fecha { get; set; }
        public string Producto { get; set; }
        public string Codigo { get; set; }
        public int Cantidad { get; set; }
        public string Remitente { get; set; }
        public string NotasEnvio { get; set; }
        public string Destinatario { get; set; }
        public string CI { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Provincia { get; set; }
        public string Municipio { get; set; }
    }
}