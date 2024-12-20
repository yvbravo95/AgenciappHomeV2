using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Common.Models.Dto
{
    public class PassportDto
    {
        public Guid PassportId { get; set; }
        public Guid AgencyId { get; set; }
        public string Agency { get; set; }
        public Guid AgencyTransferidaId { get; set; }
        public string AgencyTransferida { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSurname { get; set; }
        public string ClientFullData { get; set; }
        public string OrderNumber { get; set; }
        public string FechaSolicitud { get; set; }
        public string FechaDespacho { get; set; }
        public string FechaDespachoImportada { get; set; }
        public string FechaRecibido { get; set; }
        public string NumDespacho { get; set; }
        public string NumDespachoImportada { get; set; }
        public string WholeslerDespacho { get; set; }
        public string FechaImportacion { get; set; }
        public string ServicioConsular { get; set; }
        public string Status { get; set; }
        public string ProrrogaNumber { get; set; }
        public decimal Pagado { get; set; }
        public decimal Debe { get; set; }
        public bool AppMovil { get; set; }
        public bool Express { get; set; }
        public string AgencyPassport { get; set; }
        public bool Otorgamiento { get; set; }
        public string FechaOtorgamiento { get; set; }
        public Guid? ChequeOtorgamientoId { get; set; }
        public Guid? GuiaPasaporteGuiaId { get; set; }
        public Guid? ManifiestoPasaporteId { get; set; }
        public string ManifiestoPasaporte { get; set; }
        public bool isValid { get; set; }
    }
}
