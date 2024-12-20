using Agenciapp.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace AgenciappHome.Models
{

    public partial class Passport: OrderBase
    {
        public Passport()
        {
            RegistroEstados = new List<RegistroEstado>();
            Attachments = new List<AttachmentPassport>();
        }

        public static string STATUS_REVIEW = "Revision";
        public static string STATUS_PENDIENTE = "Pendiente";
        public static string STATUS_INICIADA = "Iniciada";
        public static string STATUS_PROCESADA = "Procesada";
        public static string STATUS_IMPORTADA = "Importada";
        public static string STATUS_DESPACHADA = "Despachada";
        public static string STATUS_RECIBIDA = "Recibida";
        public static string STATUS_ENTREGADA = "Entregada";
        public static string STATUS_CANCELADA = "Cancelada";
        public static string STATUS_MANIFIESTO = "Manifiesto";
        public static string STATUS_ENVIADA = "Enviada";

        public bool AsignarOtorgamiento()
        {
            if((ServicioConsular == ServicioConsular.PrimerVez || ServicioConsular == ServicioConsular.PrimerVez2 || ServicioConsular == ServicioConsular.HE11) && Status == STATUS_MANIFIESTO)
            {
                Otorgamiento = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        [Key]
        public Guid PassportId { get; set; }
        public string Status { get; set; }
        public Client Client { get; set; }
        [Display(Name = "Cliente")]
        public Guid ClientId { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public Agency Agency { get; set; }
        public Guid AgencyId { get; set; }
        public string fotoPasaporte { get; set; }
        public string NumDespacho { get; set; }
        public string NumDespachoImportada { get; set; }
        public string TransactionId { get; set; }
        public string ProrrogaNumber { get; set; }
        public DateTime FechaDespacho { get; set; }
        public DateTime FechaDespachoImportada { get; set; }
        public DateTime DateStatusEnviada { get; set; }
        public DateTime? EmissionDate { get; set; }
        public bool Otorgamiento { get; private set; }
        public decimal Precio { get; set; }
        [Display(Name = "Comisión")]
        public decimal Comision { get; set; }
        public decimal Descuento { get; set; }
        public decimal Credito { get; set; }

        [Display(Name = "Costo por Serv.")]
        public decimal OtrosCostos { get; set; }
        public decimal costo { get; set; }
        public decimal costoProveedor { get; set; }
        public decimal Total { get; set; }
        public decimal Pagado { get; set; }
        public decimal Balance { get; set; }
        public List<RegistroPago> RegistroPagos { get; set; }
        public List<RegistroEstado> RegistroEstados { get; set; }

        public AuthorizationCard AuthorizationCard { get; set; }


        [Display(Name = "Tipo de Pasaporte")]
        public PassportType Type { get; set; }

        [Display(Name = "Tramite")]
        public TramiteType Tramite { get; set; }
        
        [Display(Name = "Tipo de Solicitud")]
        public TypeSolicitud TipoSolicitud { get; set; }

        [Display(Name="Servicio Consular")]
        public ServicioConsular ServicioConsular { get; set; }

        [Display(Name = "Tipo de Prorroga")]
        public Prorroga1Type ProrrogaType { get; set; }


        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; }

        [Display(Name = "Carnet de Identidad")]
        public string CarnetIdentidad { get; set; }

        [Display(Name = "Número de Pasaporte")]
        public string PassaportNumber { get; set; }

        [Display(Name = "Caracteristicas Especiales")]
        public string CaracteEspesciales { get; set; }
        
        [Display(Name = "Primer Apellido")]
        public string PrimerApellidos { get; set; }
        [Display(Name = "Segundo Apellido")]
        public string SegundoApellidos { get; set; }
        [Display(Name = "Primer Nombre")]
        public string PrimerNombre { get; set; }
        [Display(Name = "Segundo Nombre")]
        public string SegundoNombre { get; set; }
        public string Padre { get; set; }
        public string Madre { get; set; }
        [Display(Name = "Estatura (ft) EJ: 5.05")]
        public decimal Estatura { get; set; }
        [Display(Name = "Estado Civil")]
        public string EstadoCivil { get; set; }
        [Display(Name = "Sexo")]
        public Sex Sex { get; set; }
        
        [Display(Name = "Color de Ojos")]
        public ColorOjos ColorOjos { get; set; }
        
        [Display(Name = "Color de Piel")]
        public ColorPiel ColorPiel { get; set; }
        
        [Display(Name = "Color de Cabello")]
        public ColorCabello ColorCabello { get; set; }
        
        [Display(Name = "País de residencia")]
        public string PaisResidencia { get; set; }
        
        public string Estado { get; set; }
        
        [Display(Name = "Clasificación Migratoria")]
        public ClasificacionMigratoria ClasificacionMigratoria { get; set; }
        
        [Display(Name = "Fecha de Salida")]
        public DateTime FechaSalida { get; set; }

        [Display(Name = "Nacionalidad")]
        public string Nationality { get; set; }
        
        [Display(Name = "Pais de Nacimiento")]

        public string PaisNacimiento { get; set; }
        
        [Display(Name = "Municipio de Nacimiento")]
        public string MunicipioNacimiento { get; set; }
        
        [Display(Name = "Provincia de Nacimiento")]
        public string ProvinciaNacimiento { get; set; }
        
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime DateBirth { get; set; }
        
        [Display(Name = "Dirección")]
        public string DireccionActual { get; set; }
        
        [Display(Name = "Ciudad")]
        public string ProvinciaActual { get; set; }
        [Display(Name = "Estado")]
        public string EstadoActual { get; set; }

        [Display(Name = "Código Postal")]
        public string CodigoPostalActual { get; set; }
        
        [Display(Name = "País")]
        public string PaisActual { get; set; }
        
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }
        public string Email { get; set; }
        
        [Display(Name = "Nombre del centro de trabajo")]
        public string DatosLaborales { get; set; }
        
        [Display(Name = "Profesión")]
        public string Profesion { get; set; }
        
        [Display(Name = "Ocupación")]
        public string Ocupacion { get; set; }

        [Display(Name = "Categoría de Prefosión")]
        public CategoriaProfesion CategoriaProfesion { get; set; }

        [Display(Name = "Dirección")]
        public string DireccionTrabajo { get; set; }
        [Display(Name = "Código Postal")]
        public string CodigoPostalTrabajo { get; set; }
        [Display(Name = "Ciudad")]
        public string ProvinciaTrabajo { get; set; }
        [Display(Name = "Código")]
        public string CodigoProvTrabajo { get; set; }
        [Display(Name = "Estado")]
        public string EstadoTrabajo { get; set; }
        [Display(Name = "Pais")]
        public string PaisTrabajo { get; set; }


        [Display(Name = "Teléfono")]
        public string TelefonoTrabajo { get; set; }
        
        [Display(Name = "Email")]
        public string EmailTrabajo { get; set; }

        [Display(Name = "Nivel Escolar")]
        public NivelEscolar NivelCultural { get; set; }
        [Display(Name = "Primer Apellido")]
        public string ApellidosReferencia { get; set; }
        [Display(Name = "Segundo Apellido")]
        public string ApellidosReferencia2 { get; set; }
        [Display(Name = "Primer Nombre")]
        public string NombreReferencia { get; set; }
        [Display(Name = "Segundo Nombre")]
        public string NombreReferencia2 { get; set; }
        [Display(Name = "Dirección")]
        public string DireccionReferencia { get; set; }
        [Display(Name = "Provincia")]
        public string ProvinciaReferencia { get; set; }
        [Display(Name = "Municipio")]
        public string MunicipioReferencia { get; set; }
        [Display(Name = "Relación Familiar")]
        public string RelacionFamiliar { get; set; }
        [Display(Name = "Teléfono")]
        public string TelefonoReferencia { get; set; }
        [Display(Name="Agencia")]
        public string AgencyPassport { get; set; }
        [Display(Name = "Dirección 1")]
        public string DireccionCuba1 { get; set; }
        [Display(Name = "Provincia")]
        public string ProvinciaCuba1 { get; set; }
        [Display(Name = "Municipio")]
        public string CiudadCuba1 { get; set; }
        [Display(Name = "Dirección 2")]
        public string DireccionCuba2 { get; set; }
        [Display(Name = "Provincia")]
        public string ProvinciaCuba2 { get; set; }
        [Display(Name = "Municipios")]
        public string CiudadCuba2 { get; set; }
        [Display(Name = "Desde")]
        public DateTime Desde1 { get; set; }
        [Display(Name = "Desde")]
        public DateTime Dasde2 { get; set; }
        [Display(Name = "Hasta")]
        public DateTime Hasta1 { get; set; }
        [Display(Name = "Hasta")]
        public DateTime Hasta2 { get; set; }
        [Display(Name = "Número")]
        public string Number2 { get; set; }
        [Display(Name = "Fecha de Emisión")]
        public DateTime FechaExpedicion { get; set; }
        [Display(Name = "Cantidad de Prorrogas")]
        public int CantidadProrrogas { get; set; }
        [Display(Name = "Razón de no disponibilidad")]
        public RazonNoDisponibilidad RazonNoDisponibilidad { get; set; }
        public string Lugar { get; set; }
        public string Tomo { get; set; }
        public string Folio { get; set; }
        [Display(Name = "Registro Civil")]
        public string RegistroCivil { get; set; }
        [Display(Name = "Número")]
        public string InscripcionConsularNo { get; set; }
        [Display(Name = "de fecha")]
        public DateTime InscipcionConsularDate { get; set; }
        public bool NacidoUSA { get; set; }
        public bool NacidoOtroPais { get; set; }

        public OrderRevisada FirmaCliente { get; set; }
        public string OrderNumber { get; set; }
        public string Nota { get; set; }
        public string OFAC { get; set; }
        public string NumPassport1 { get; set; }
        public string NumPassport2 { get; set; }
        public string CountryPassport1 { get; set; }
        public string CountryPassport2 { get; set; }
        public DateTime ExpirePassport1 { get; set; }
        public DateTime ExpirePassport2 { get; set; }
        public Guid? WholesalerId { get; set; }
        public Wholesaler Wholesaler { get; set; }

        public Guid? WholesalerDespachoId { get; set; }
        [ForeignKey("WholesalerDespachoId")]
        public Wholesaler WholeslerDespacho { get; set; }

        public Guid? AgencyTransferidaId { get; set; }
        [ForeignKey("AgencyTransferidaId")]
        public Agency AgencyTransferida { get; set; }

        public bool PrecioFoto { get; set; }
        public decimal ValuePrecioFoto { get; set; }
        [Display(Name = "Fecha de Expedición")]
        public DateTime ExpedicionCertNaci { get; set; }
        [Display(Name = "Lugar de Expedición")]
        public string LugarExpedCertNaci { get; set; }
        public bool Express { get; set; }
        public Guid? PassportExpressId { get; set; }
        [ForeignKey("PassportExpressId")]
        public PassportExpress PassportExpress { get; set; }
        public bool CreatedByCode { get; set; }
        public MinoristaPasaporte Minorista { get; set; }
        public Guid? MinoristaId { get; set; }
        public DateTime FechaImportacion { get; set; }

        public Guid? ManifiestoPasaporteId { get; set; }
        [ForeignKey("ManifiestoPasaporteId")]
        public ManifiestoPasaporte ManifiestoPasaporte { get; set; }

        public string NoPasaporteDc { get; set; }


        public bool MismaDireccion { get; set; }
        
        [Display(Name = "Dirección")]
        public string DireccionEntrega { get; set; }
        [Display(Name = "País")]
        public string PaisEntrega { get; set; }
        [Display(Name = "Ciudad")]
        public string CiudadEntrega { get; set; }
        [Display(Name = "Estado")]
        public string EstadoEntrega { get; set; }
        [Display(Name = "Código Postal")]
        public string CodPostalEntrega { get; set; }

        public Guid? ChequeOtorgamientoId { get; set; }
        public ChequeOtorgamiento ChequeOtorgamiento { get; set; }
        public List<AttachmentPassport> Attachments { get; set; }
        public string LinkFedex { get; set; }
        public DateTime FechaOtorgamiento { get; set; }
        public bool AppMovil { get; set; }
        public BigInteger getNumberToValue(){
            BigInteger number;
            if(BigInteger.TryParse(this.OrderNumber.Replace("DC","").Replace("PS", ""), out number)){
                return number;
            }
            else
                return -1;
        }
    }

    public class PassportExpress
    {
        [Key]
        public Guid PassportExpressId { get; set; }
        public string servicioConsular { get; set; }
        public decimal Costo { get; set; }
        public decimal Precio { get; set; }

    }
       
    public enum PassportType
    {
        Corriente, 
        Servicio,
        Diplomatico,
        Marino,
        Oficial,
        [Description("")]
        None
    }
    
    public enum TramiteType
    {
        [Description("")]
        None,
        [Description("Confección")]
        Confeccion,
        Prorroga
    }

    public enum ServicioConsular
    {
        [Description("")]
        None,
        [Description("PASAPORTE 1 VEZ")]
        PrimerVez,
        [Description("PRORROGA 1")]
        Prorroga1,
        [Description("PRORROGAR 2")]
        Prorroga2,
        [Description("RENOVAR")]
        Renovacion,       
        [Description("HE-11")]
        HE11,
        [Description("RENOVAR MENOR")]
        Renovacion2,
        [Description("PASAPORTE 1 VEZ MENOR")]
        PrimerVez2,
        [Description("DVT")]
        DVT,
    }

    public enum Prorroga1Type
    {
        [Description("")]
        None,
        [Description("Primera")]
        Primera,
        [Description("Segunda")]
        Segunda
    }

    public enum TypeSolicitud
    {
        [Description("")]
        None,
        Regular,
        Inmediata
    }

    public enum CategoriaProfesion {
        [Description("")]
        None, 
        [Description("Cuenta Propista")]
        CuentaPropista,
        Deporte,
        [Description("Educación")]
        Educacion,
        Salud,
        Turismo,
        Otra
    }

    public enum NivelEscolar
    {
        [Description("")]
        None,
        Analfabeto,
        [Description("Pre-Universitario")]
        PreUniversitario,
        Primario,
        Secundario,
        [Description("Tec. Medio")]
        TecMedio,
        Universitario

    }

    public enum RazonNoDisponibilidad
    {
        [Description("")]
        None,
        [Description("DAÑADO")]
        Danado,
        [Description("DESACTIVADO")]
        Desactivado,
        [Description("ERRONEO")]
        Erroneo,
        [Description("HOJAS AGOTADAS")]
        HojasAgotadas,
        [Description("PERDIDA")]
        Perdida,
        [Description("PROXIMO VENCIMIENTO")]
        ProximoVenc,
        [Description("ROBADO")]
        Robado,
        [Description("VENCIDO")]
        Vencido,
        [Description("VISA FALSA")]
        VisaFalsa
    }

    public enum Sex
    {
        [Description("")]
        None,
        Femenino, 
        Masculino,
    }

    public enum ColorOjos
    {
        [Description("")]
        None,
        Claros,
        Negros,
        Pardos,
        Azules,
        Verdes
    }
    
    public enum ColorPiel
    {
        [Description("")]
        None,
        Blanca,
        Mulata,
        Negra,
        Albina,
        Amarilla
        //Mestiza
    }
    
    public enum ColorCabello
    {
        [Description("")]
        None,
        Canoso,
        Rojo,
        Negro,
        Castaño,
        Rubio,
        Otros
    }
    
    public enum ClasificacionMigratoria
    {
        [Description("")]
        None,
        [Description("Asunto Oficial")]
        AstOficial,

        [Description("Permiso Emigracion")]
        PermisoEmigraion,
        
        [Description("PRE")]
        Pre,
        
        [Description("PSI")]
        Psi,

        [Description("PVE")]
        Pve,
        
        [Description("PVT")]
        Pvt,
        [Description("PSD")]
        Psd,
        
        [Description("Salida Ilegal")]
        Ilegal
    }

    public enum EstadoCivil
    {
        [Description("")]
        NONE,
        CASADO,
        SOLTERO,
        DIVORCIADO,
        VIUDO,
        SEPARADO
    }
}
