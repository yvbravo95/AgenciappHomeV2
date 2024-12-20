using AgenciappHome.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.PassportServices.Models
{
    public class CreatePassportModel
    {

        public Guid? WholesalerId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid ClientId { get; set; }

        public decimal CreditValue { get; set; }
        public decimal CostService { get; set; }
        public decimal Price { get; set; }
        public decimal Commission { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public decimal Cost { get; set; }

        public bool IsExpress { get; set; }

        public AuthorizationCard AuthorizationCard { get; set; }
        public ServicioConsular ConsularService { get; set; }

        public List<CreatePayItem> Pays { get; set; }

        // Data Passport 
        public PassportType Type { get; set; }
        public TramiteType Tramite { get; set; }
        public TypeSolicitud TipoSolicitud { get; set; }
        public ServicioConsular ServicioConsular { get; set; }
        public string CarnetIdentidad { get; set; }
        public string PassaportNumber { get; set; }
        public string CaracteEspesciales { get; set; }
        public string PrimerApellidos { get; set; }
        public string SegundoApellidos { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; }
        public string Padre { get; set; }
        public string Madre { get; set; }
        public decimal Estatura { get; set; }
        public string EstadoCivil { get; set; }
        public Sex Sex { get; set; }
        public ColorOjos ColorOjos { get; set; }
        public ColorPiel ColorPiel { get; set; }
        public ColorCabello ColorCabello { get; set; }
        public string PaisResidencia { get; set; }
        public string Estado { get; set; }
        public ClasificacionMigratoria ClasificacionMigratoria { get; set; }
        public DateTime FechaSalida { get; set; }
        public string Nationality { get; set; }
        public string PaisNacimiento { get; set; }
        public string MunicipioNacimiento { get; set; }
        public string ProvinciaNacimiento { get; set; }
        public DateTime DateBirth { get; set; }
        public string DireccionActual { get; set; }
        public string ProvinciaActual { get; set; }
        public string EstadoActual { get; set; }
        public string CodigoPostalActual { get; set; }
        public string PaisActual { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string DatosLaborales { get; set; }
        public string Profesion { get; set; }
        public string Ocupacion { get; set; }
        public CategoriaProfesion CategoriaProfesion { get; set; }
        public string DireccionTrabajo { get; set; }
        public string CodigoPostalTrabajo { get; set; }
        public string ProvinciaTrabajo { get; set; }
        public string CodigoProvTrabajo { get; set; }
        public string EstadoTrabajo { get; set; }
        public string PaisTrabajo { get; set; }
        public string TelefonoTrabajo { get; set; }
        public string EmailTrabajo { get; set; }
        public NivelEscolar NivelCultural { get; set; }
        public string ApellidosReferencia { get; set; }
        public string ApellidosReferencia2 { get; set; }
        public string NombreReferencia { get; set; }
        public string NombreReferencia2 { get; set; }
        public string DireccionReferencia { get; set; }
        public string ProvinciaReferencia { get; set; }
        public string MunicipioReferencia { get; set; }
        public string RelacionFamiliar { get; set; }
        public string TelefonoReferencia { get; set; }
        public string AgencyPassport { get; set; }
        public string DireccionCuba1 { get; set; }
        public string ProvinciaCuba1 { get; set; }
        public string CiudadCuba1 { get; set; }
        public string DireccionCuba2 { get; set; }
        public string ProvinciaCuba2 { get; set; }
        public string CiudadCuba2 { get; set; }
        public DateTime Desde1 { get; set; }
        public DateTime? Desde2 { get; set; }
        public DateTime Hasta1 { get; set; }
        public DateTime? Hasta2 { get; set; }
        public string Number2 { get; set; }
        public DateTime FechaExpedicion { get; set; }
        public int CantidadProrrogas { get; set; }
        public RazonNoDisponibilidad RazonNoDisponibilidad { get; set; }
        public string Lugar { get; set; }
        public string Tomo { get; set; }
        public string Folio { get; set; }
        public string RegistroCivil { get; set; }
        public string InscripcionConsularNo { get; set; }
        public DateTime InscipcionConsularDate { get; set; }
        public bool NacidoUSA { get; set; }
        public bool NacidoOtroPais { get; set; }
        public string Nota { get; set; }
        public string OFAC { get; set; }
        public string NumPassport1 { get; set; }
        public string NumPassport2 { get; set; }
        public string CountryPassport1 { get; set; }
        public string CountryPassport2 { get; set; }
        public DateTime ExpirePassport1 { get; set; }
        public DateTime ExpirePassport2 { get; set; }
        public DateTime ExpedicionCertNaci { get; set; }
        public string LugarExpedCertNaci { get; set; }
    }
}
