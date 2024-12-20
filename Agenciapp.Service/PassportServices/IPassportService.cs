using Agenciapp.Common.Class;
using Agenciapp.Common.Services.INotificationServices;
using Agenciapp.Domain.Models;
using Agenciapp.Service.IBuildEmailServices;
using Agenciapp.Service.PassportServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.PassportServices
{
    public interface IPassportService
    {
        Task<Result> AddAttachments(AddAttachmentsModel model);
        Task<Result<Passport>> Create(CreatePassportModel model);
        Task SendEmailReviewDCuba();
    }

    public class PassportService : IPassportService
    {
        private readonly IWebHostEnvironment _env;
        private readonly databaseContext _context;
        private readonly IBuildEmailService _buildEmailService;
        private readonly INotificationService _notificationService;
        private readonly Guid AgencyDCubaId = Guid.Parse("4752B08A-7684-42B3-930D-FF86F496DF2F");
        private readonly Guid AgencyCubabienTravelId = Guid.Parse("91C9F482-B875-4D9D-8C6F-4A4021690479");
        public PassportService(IWebHostEnvironment env, INotificationService notificationService, databaseContext context, IBuildEmailService buildEmailService)
        {
            _env = env;
            _context = context;
            _notificationService = notificationService;
            _buildEmailService = buildEmailService;
        }

        public async Task<Result> AddAttachments(AddAttachmentsModel model)
        {
            var passport = await _context.Passport.FindAsync(model.PassportId);
            if (passport == null) return Result.Failure("El pasaporte no existe");
            string sWebRootFolder = _env.WebRootPath;
            var date = DateTime.Now.ToString("yMMddHHmmssff");
            int count = 1;
            foreach (var file in model.Files)
            {
                var auxName = file.FileName;
                var arrName = auxName.Split('.');
                string filename = date + count + '.' + arrName[arrName.Length - 1];
                string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "Upload" + Path.DirectorySeparatorChar + "Passport";
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                string FullPath = Path.Combine(filePath, filename);
                using (var fileStream = new FileStream(FullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                count++;

                var newFile = new AttachmentPassport
                {
                    CreatedAt = DateTime.UtcNow,
                    FileType = arrName[arrName.Length - 1],
                    Name = filename,
                    OriginalName = auxName,
                    Passport = passport,
                    Path = filePath,
                    Description = model.Description,
                };
                _context.Attach(newFile);
            }
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task SendEmailReviewDCuba()
        {
            try
            {
                var agency = await _context.Agency.FindAsync(Guid.Parse("4752B08A-7684-42B3-930D-FF86F496DF2F"));
                var passports = _context.Passport.Include(x => x.Client).ThenInclude(x => x.Phone)
                .Where(x => x.AgencyId == agency.AgencyId &&  x.DateStatusEnviada.AddDays(4).Date == DateTime.Now.Date && x.MinoristaId == null)
                .Select(x => new { x.Client, x.OrderNumber });
                Serilog.Log.Information($"Envios de emails review de pasaportes DCuba. Cantidad de pasaportes {passports.Count()}");

                var sendEmail = await _context.EmailBodies.FirstOrDefaultAsync(x => x.EmailTemplate.AgencyId == agency.AgencyId && x.Number == "_05_");
                if (sendEmail != null)
                {

                    foreach (var passport in passports)
                    {
                        string msg = $"Notificacion pasaporte #{passport.OrderNumber} enviado. ";
                        if (passport.Client != null)
                        {
                            Dictionary<string, string> values = new Dictionary<string, string>();
                            values["passport_number"] = passport.OrderNumber;
                            values["client_name"] = passport.Client?.FullData;
                            var response = await _buildEmailService.SendBuildEmail(sendEmail.Id, passport.Client.ClientId, values);
                            if (response.IsSuccess)
                            {
                                msg += "Se ha enviado el email";
                            }
                            else
                            {
                                msg += response.Error;
                            }
                           if(passport.Client.Phone != null){
                                await _notificationService.sendSms(
                                $"Hola {passport.Client.Name}, su trámite ha sido entregado, puede darnos su comentario pinchando el siguiente link https://g.page/districtcuba/review?gm. Gracias nuevamente por confiar en District Cuba.",
                                passport.Client.Phone?.Number.Replace(")", "").Replace("(", "").Replace("-", ""));
                            }
                        }
                        else
                        {
                            msg += "El cliente no existe";
                        }
                        Serilog.Log.Information(msg);
                    }
                }
                else
                {
                    Serilog.Log.Information("El email _05_ no existe");
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
            }
        }

        public async Task<Result<Passport>> Create(CreatePassportModel model)
        {
            try
            {
                var user = _context.User.Find(model.EmployeeId);
                if (user == null)
                {
                    Serilog.Log.Information("No se ha podido crear el pasaporte. El empleado no existe");
                    return Result.Failure<Passport>("No se ha podido crear el pasaporte");
                }
                var client = _context.Client.Include(x => x.Phone).FirstOrDefault(x => x.ClientId == model.ClientId);
                if (client == null)
                {
                    Serilog.Log.Information("No se ha podido crear el pasaporte. El cliente no existe");
                    return Result.Failure<Passport>("No se ha podido crear el pasaporte");
                }
                if (client.AgencyId != user.AgencyId)
                {
                    Serilog.Log.Information("No se ha podido crear el pasaporte. El empleado no pertenece a la misma agencia del cliente");
                    return Result.Failure<Passport>("No se ha podido crear el pasaporte");
                }
                var agency = _context.Agency.Find(user.AgencyId);
                if (agency == null)
                {
                    Serilog.Log.Information("No se ha podido crear el pasaporte. La agencia no existe");
                    return Result.Failure<Passport>("No se ha podido crear el pasaporte");
                }
                var office = _context.Office.FirstOrDefault(x => x.Agency == agency);

                Passport passport = new Passport()
                {
                    OrderNumber = "PS" + DateTime.Now.ToString("yMMddHHmmssff"),
                    User = user,
                    Agency = agency,
                    Client = client,
                    Telefono = model.Telefono?.Replace("(", "").Replace(")", "").Replace("-", ""),
                    TelefonoTrabajo = model.TelefonoTrabajo?.Replace("(", "").Replace(")", "").Replace("-", ""),
                    FechaSolicitud = DateTime.Now,
                    MismaDireccion = true,
                    RegistroPagos = new List<RegistroPago>(),
                    AuthorizationCard = new AuthorizationCard(),
                    ServicioConsular = model.ServicioConsular,
                    Type = model.Type,
                    Tramite = model.Tramite,
                    TipoSolicitud = model.TipoSolicitud,
                    CarnetIdentidad = model.CarnetIdentidad,
                    PassaportNumber = model.PassaportNumber,
                    CaracteEspesciales = model.CaracteEspesciales,
                    PrimerApellidos = model.PrimerApellidos,
                    SegundoApellidos = model.SegundoApellidos,
                    PrimerNombre = model.PrimerNombre,
                    SegundoNombre = model.SegundoNombre,
                    Padre = model.Padre,
                    Madre = model.Madre,
                    Estatura = model.Estatura,
                    EstadoCivil = model.EstadoCivil,
                    Sex = model.Sex,
                    ColorOjos = model.ColorOjos,
                    ColorPiel = model.ColorPiel,
                    ColorCabello = model.ColorCabello,
                    PaisResidencia = model.PaisResidencia,
                    Estado = model.Estado,
                    ClasificacionMigratoria = model.ClasificacionMigratoria,
                    FechaSalida = model.FechaSalida,
                    Nationality = model.Nationality,
                    PaisNacimiento = model.PaisNacimiento,
                    MunicipioNacimiento = model.MunicipioNacimiento,
                    ProvinciaNacimiento = model.ProvinciaNacimiento,
                    DateBirth = model.DateBirth,
                    DireccionActual = model.DireccionActual,
                    ProvinciaActual = model.ProvinciaActual,
                    EstadoActual = model.EstadoActual,
                    CodigoPostalActual = model.CodigoPostalActual,
                    PaisActual = model.PaisActual,
                    Email = model.Email,
                    DatosLaborales = model.DatosLaborales,
                    Profesion = model.Profesion,
                    Ocupacion = model.Ocupacion,
                    CategoriaProfesion = model.CategoriaProfesion,
                    DireccionTrabajo = model.DireccionTrabajo,
                    CodigoPostalTrabajo = model.CodigoPostalTrabajo,
                    ProvinciaTrabajo = model.ProvinciaTrabajo,
                    CodigoProvTrabajo = model.CodigoProvTrabajo,
                    EstadoTrabajo = model.EstadoTrabajo,
                    PaisTrabajo = model.PaisTrabajo,
                    EmailTrabajo = model.EmailTrabajo,
                    NivelCultural = model.NivelCultural,
                    ApellidosReferencia = model.ApellidosReferencia,
                    ApellidosReferencia2 = model.ApellidosReferencia2,
                    DireccionReferencia = model.DireccionReferencia,
                    ProvinciaReferencia = model.ProvinciaReferencia,
                    MunicipioReferencia = model.MunicipioReferencia,
                    RelacionFamiliar = model.RelacionFamiliar,
                    TelefonoReferencia = model.TelefonoReferencia,
                    AgencyPassport = model.AgencyPassport,
                    DireccionCuba1 = model.DireccionCuba1,
                    DireccionCuba2 = model.DireccionCuba2,
                    CiudadCuba1 = model.CiudadCuba1,
                    CiudadCuba2 = model.CiudadCuba2,
                    ProvinciaCuba1 = model.ProvinciaCuba1,
                    ProvinciaCuba2 = model.ProvinciaCuba2,
                    Desde1 = model.Desde1,
                    Dasde2 = model.Desde2 == null? DateTime.MinValue: (DateTime)model.Desde2,
                    Hasta1 = model.Hasta1,
                    Hasta2 = model.Hasta2 == null ? DateTime.MinValue : (DateTime)model.Hasta2,
                    Number2 = model.Number2,
                    FechaExpedicion = model.FechaExpedicion,
                    CantidadProrrogas = model.CantidadProrrogas,
                    RazonNoDisponibilidad = model.RazonNoDisponibilidad,
                    Lugar = model.Lugar,
                    Tomo = model.Tomo,
                    Folio = model.Folio,
                    RegistroCivil = model.RegistroCivil,
                    InscripcionConsularNo = model.InscripcionConsularNo,
                    InscipcionConsularDate = model.InscipcionConsularDate,
                    NacidoUSA = model.NacidoUSA,
                    NacidoOtroPais = model.NacidoOtroPais,
                    Nota = model.Nota,
                    OFAC = model.OFAC,
                    NumPassport1 = model.NumPassport1,
                    NumPassport2 = model.NumPassport2,
                    CountryPassport1 = model.CountryPassport1,
                    CountryPassport2 = model.CountryPassport2,
                    ExpirePassport1 = model.ExpirePassport1,
                    ExpirePassport2 = model.ExpirePassport2,
                    ExpedicionCertNaci = model.ExpedicionCertNaci,
                    LugarExpedCertNaci = model.LugarExpedCertNaci,
                    NombreReferencia = model.NombreReferencia,
                    NombreReferencia2 = model.NombreReferencia2,
                    Total = model.Total,
                    OtrosCostos = model.CostService,
                    Precio = model.Price,
                    WholesalerId = model.WholesalerId,
                    costo = model.Cost,
                    PaisEntrega = model.PaisActual,
                    EstadoEntrega = model.Estado,
                    CiudadEntrega = model.ProvinciaActual,
                    DireccionEntrega = model.DireccionActual,
                    CodPostalEntrega = model.CodigoPostalActual
                };
                _context.Passport.Add(passport);
                //Validations

                if (passport.ServicioConsular != ServicioConsular.PrimerVez && passport.ServicioConsular != ServicioConsular.PrimerVez2 && string.IsNullOrEmpty(passport.PassaportNumber))
                    return Result.Failure<Passport>("El número de pasaporte es requerido");

                if (string.IsNullOrEmpty(passport.PrimerNombre))
                    return Result.Failure<Passport>("El campo Primer Nombre es requerido");

                if (string.IsNullOrEmpty(passport.PrimerApellidos))
                    return Result.Failure<Passport>("El campo Primer Apellido es requerido");

                if (string.IsNullOrEmpty(passport.SegundoApellidos))
                    return Result.Failure<Passport>("El campo Segundo Apellido es requerido");

                if (string.IsNullOrEmpty(passport.Padre))
                    return Result.Failure<Passport>("El campo Padre es requerido");

                if (string.IsNullOrEmpty(passport.Madre))
                    return Result.Failure<Passport>("El campo Madre es requerido");

                if (passport.Estatura <= 0)
                    return Result.Failure<Passport>("El campo Estatura debe ser mayor que 0");

                if (passport.Sex == Sex.None)
                    return Result.Failure<Passport>("El campo Sexo es requerido");

                if (passport.ColorOjos == ColorOjos.None)
                    return Result.Failure<Passport>("El campo Color de Ojos es requerido");

                if (passport.ColorPiel == ColorPiel.None)
                    return Result.Failure<Passport>("El campo Color de Piel es requerido");

                if (passport.ColorCabello == ColorCabello.None)
                    return Result.Failure<Passport>("El campo Color de Cabello es requerido");

                if (passport.ClasificacionMigratoria == ClasificacionMigratoria.None)
                    return Result.Failure<Passport>("El campo Clasificacion Migratoria es requerido");

                if (passport.FechaSalida == DateTime.MinValue)
                    return Result.Failure<Passport>("El campo Fecha de Salida es requerido");

                if (string.IsNullOrEmpty(passport.ProvinciaNacimiento))
                    return Result.Failure<Passport>("El campo Provincia de Nacimiento es requerido");

                if (string.IsNullOrEmpty(passport.MunicipioNacimiento))
                    return Result.Failure<Passport>("El campo Municipio de Nacimiento es requerido");

                if (string.IsNullOrEmpty(passport.DireccionActual))
                    return Result.Failure<Passport>("El campo Dirección Actual es requerido");

                if (string.IsNullOrEmpty(passport.PaisActual))
                    return Result.Failure<Passport>("El campo País Actual es requerido");

                if (string.IsNullOrEmpty(passport.ProvinciaActual))
                    return Result.Failure<Passport>("El campo Ciudad Actual es requerido");

                if (string.IsNullOrEmpty(passport.CodigoPostalActual))
                    return Result.Failure<Passport>("El campo Código Postal Actual es requerido");

                if (string.IsNullOrEmpty(passport.Telefono))
                    return Result.Failure<Passport>("El campo Teléfono es requerido");

                if (string.IsNullOrEmpty(passport.DatosLaborales))
                    return Result.Failure<Passport>("El campo Nombre del Centro de Trabajo es requerido");

                if (string.IsNullOrEmpty(passport.Profesion))
                    return Result.Failure<Passport>("El campo Profesión es requerido");

                if (string.IsNullOrEmpty(passport.Ocupacion))
                    return Result.Failure<Passport>("El campo Ocupación es requerido");

                if (passport.CategoriaProfesion == CategoriaProfesion.None)
                    return Result.Failure<Passport>("El campo Categoría de Profesión es requerido");

                if (passport.NivelCultural == NivelEscolar.None)
                    return Result.Failure<Passport>("El campo Nivel Escolar es requerido");

                if (string.IsNullOrEmpty(passport.NombreReferencia))
                    return Result.Failure<Passport>("El campo Primer Nombre de Referencia en Cuba es requerido");

                if (string.IsNullOrEmpty(passport.ApellidosReferencia))
                    return Result.Failure<Passport>("El campo Primer Apellido de Referencia en Cuba es requerido");

                if (string.IsNullOrEmpty(passport.ApellidosReferencia2))
                    return Result.Failure<Passport>("El campo Segundo Apellido de Referencia en Cuba es requerido");

                if (string.IsNullOrEmpty(passport.DireccionReferencia))
                    return Result.Failure<Passport>("El campo Dirección de Referencia en Cuba es requerido");

                if (string.IsNullOrEmpty(passport.ProvinciaReferencia))
                    return Result.Failure<Passport>("El campo Provincia de Referencia en Cuba es requerido");

                if (string.IsNullOrEmpty(passport.MunicipioReferencia))
                    return Result.Failure<Passport>("El campo Municipio de Referencia en Cuba es requerido");

                if (passport.ServicioConsular == ServicioConsular.PrimerVez || passport.ServicioConsular == ServicioConsular.PrimerVez2)
                {
                    if (string.IsNullOrEmpty(passport.DireccionCuba1))
                        return Result.Failure<Passport>("El campo Dirección 1 en Cuba es requerido");

                    if (string.IsNullOrEmpty(passport.ProvinciaCuba1))
                        return Result.Failure<Passport>("El campo Provincia 1 en Cuba es requerido");

                    if (string.IsNullOrEmpty(passport.CiudadCuba1))
                        return Result.Failure<Passport>("El campo Municipio 1 en Cuba es requerido");

                    if (passport.Desde1 == DateTime.MinValue)
                        return Result.Failure<Passport>("El campo Desde 1 en Cuba es requerido");

                    if (passport.Hasta1 == DateTime.MinValue)
                        return Result.Failure<Passport>("El campo Hasta 1 en Cuba es requerido");

                }

                if (passport.ServicioConsular == ServicioConsular.Prorroga1 || passport.ServicioConsular == ServicioConsular.Prorroga2)
                {
                    if (passport.FechaExpedicion == DateTime.MinValue)
                        return Result.Failure<Passport>("El campo Fecha de Expedición es requerido");
                }

                if (passport.ServicioConsular == ServicioConsular.Renovacion)
                {
                    if (passport.RazonNoDisponibilidad == RazonNoDisponibilidad.None)
                        return Result.Failure<Passport>("El campo Razón de No Disponibilidad es requerido");
                }                

                foreach (var item in model.Pays)
                {
                    var pay = new RegistroPago
                    {
                        RegistroPagoId = Guid.NewGuid(),
                        Agency = agency,
                        User = user,
                        Office = office,
                        date = DateTime.UtcNow,
                        number = "PAY" + DateTime.Now.ToString("yMMddHHmmssff"),
                        tipoPagoId = item.PaymentTypeId,
                        valorPagado = item.Amount,
                    };
                    passport.RegistroPagos.Add(pay);

                }

                passport.Pagado = passport.RegistroPagos.Sum(x => x.valorPagado);

                if (passport.Credito > 0)
                {
                    if (passport.Credito >= passport.Total)
                    {
                        passport.Credito = passport.Total;
                    }

                    //Creo un pago para credito
                    RegistroPago pagocredito = new RegistroPago
                    {
                        AgencyId = agency.AgencyId,
                        date = DateTime.UtcNow,
                        number = "PAY" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        OfficeId = agency.AgencyId,
                        Passport = passport,
                        tipoPagoId = _context.TipoPago.FirstOrDefault(x => x.Type == "Crédito de Consumo").TipoPagoId,
                        UserId = user.UserId,
                        valorPagado = passport.Credito,
                        RegistroPagoId = Guid.NewGuid(),
                        ClientId = client.ClientId,
                        nota = "",
                    };

                    _context.Logs.Add(new Log
                    {
                        Date = DateTime.Now,
                        Event = LogEvent.Eliminar,
                        Type = LogType.Credito,
                        LogId = Guid.NewGuid(),
                        Message = pagocredito.valorPagado.ToString(),
                        User = user,
                        Client = client,
                        AgencyId = client.AgencyId,
                    });
                    _context.RegistroPagos.Add(pagocredito);

                    var credito = passport.Credito;
                    foreach (var item in client.Creditos)
                    {
                        if (item.value > credito)
                        {
                            item.value -= credito;
                            _context.Credito.Update(item);
                            break;
                        }
                        else
                        {
                            credito -= item.value;
                            _context.Credito.Remove(item);
                        }
                    }
                }

                passport.Balance = passport.Total - passport.Pagado - passport.Credito;
                if (passport.Balance != 0)
                {
                    passport.Status = Passport.STATUS_PENDIENTE;
                    var sxc = new servicioxCobrar
                    {
                        date = DateTime.UtcNow,
                        Passport = passport,
                        ServicioId = passport.PassportId,
                        tramite = "Pasaporte",
                        NoServicio = passport.OrderNumber,
                        cliente = client,
                        No_servicioxCobrar = "PAYT" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        cobrado = 0,
                        remitente = client,
                        valorTramite = passport.Balance,
                        importeACobrar = passport.Balance,
                        mayorista = agency
                    };
                    _context.servicioxCobrar.Add(sxc);
                }
                else
                {
                    passport.Status = Passport.STATUS_INICIADA;
                }

                if (agency.AgencyId == AgencyDCubaId)
                {
                    passport.Status = Passport.STATUS_PENDIENTE;
                }

                passport.RegistroEstados = new List<RegistroEstado>();
                passport.RegistroEstados.Add(new RegistroEstado
                {
                    Date = DateTime.Now,
                    Estado = passport.Status,
                    User = user,
                    Id = Guid.NewGuid()
                });

                bool isbytransferencia = false;
                //Servicio por pagar
                if (passport.WholesalerId != null && passport.WholesalerId != Guid.Empty)
                {
                    var wholesaler = _context.Wholesalers.Find(passport.WholesalerId);
                    if (!wholesaler.Comodin)
                    {
                        if (wholesaler.byTransferencia)
                        {
                            var agenciaMayorista = _context.Agency.Where(x => x.Name == wholesaler.name && x.Type == "Mayorista").FirstOrDefault();
                            if (agenciaMayorista != null)
                            {
                                passport.AgencyTransferidaId = agenciaMayorista.AgencyId;
                                isbytransferencia = true;

                                #region A pagar por mayorista a su proveedor
                                //Rapid crea el servicio por pagar cuando despacha los combos
                                if (agenciaMayorista.AgencyId != Guid.Parse("680B03D4-A92D-44F5-8B34-FD70E0D9847C"))
                                {
                                    var proveedorMayorista = _context.Wholesalers.Include(x => x.ServConsularMayoristas)
                                    .FirstOrDefault(x => x.EsVisible && x.AgencyId == passport.AgencyTransferidaId && x.Category.category == "Pasaporte");

                                    if (proveedorMayorista != null)
                                    {
                                        ServConsularMayorista aux = proveedorMayorista.ServConsularMayoristas.FirstOrDefault(x => x.servicio == passport.ServicioConsular);
                                        if (aux != null)
                                        {
                                            SettingPassportExpress spe = _context.SettingPassportExpresses.FirstOrDefault(x => x.AgencyId == AgencyDCubaId && x.ServicioConsular == passport.ServicioConsular.ToString());
                                            passport.costoProveedor = aux.costo + (passport.Express ? (decimal)spe?.Costo : 0);

                                            var porPagarMayorista = new ServiciosxPagar
                                            {
                                                Date = DateTime.Now,
                                                ImporteAPagar = passport.costoProveedor,
                                                Mayorista = proveedorMayorista,
                                                Agency = agenciaMayorista,
                                                SId = passport.PassportId,
                                                Passport = passport,
                                                NoServicio = passport.OrderNumber,
                                                Tipo = STipo.Passport,
                                                SubTipo = passport.ServicioConsular.GetDescription()
                                            };
                                            _context.ServiciosxPagar.Add(porPagarMayorista);
                                        }
                                    }
                                }
                                #endregion

                                var sxc = new servicioxCobrar
                                {
                                    servicioxCobrarId = Guid.NewGuid(),
                                    Passport = passport,
                                    date = DateTime.UtcNow,
                                    ServicioId = passport.PassportId,
                                    tramite = "Pasaporte",
                                    NoServicio = passport.OrderNumber,
                                    mayorista = agenciaMayorista,
                                    minorista = agency,
                                    No_servicioxCobrar = "PAYT" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                                    cobrado = 0,
                                    remitente = _context.Client.Find(passport.ClientId),
                                    valorTramite = passport.Total,
                                    importeACobrar = passport.costo + passport.OtrosCostos + (passport.Express ? (decimal)passport.PassportExpress?.Costo : 0),
                                };
                                _context.servicioxCobrar.Add(sxc);

                                var porPagar = new ServiciosxPagar
                                {
                                    Date = DateTime.Now,
                                    ImporteAPagar = passport.costo + passport.OtrosCostos + (passport.Express ? (decimal)passport.PassportExpress?.Costo : 0),
                                    Mayorista = wholesaler,
                                    Agency = agency,
                                    SId = passport.PassportId,
                                    Passport = passport,
                                    NoServicio = passport.OrderNumber,
                                    Tipo = STipo.Passport,
                                    SubTipo = passport.ServicioConsular.GetDescription()
                                };
                                _context.ServiciosxPagar.Add(porPagar);
                            }
                            else
                            {
                                return Result.Failure<Passport>("No existe el mayorista por transferencia.");
                            }
                        }
                        else
                        {
                            // el servicio por pagar se crea cuando se realiza el despacho para Rapid Multiservice
                            var porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = passport.costo + (passport.Express ? (decimal)passport.PassportExpress?.Costo : 0),
                                Mayorista = wholesaler,
                                Agency = agency,
                                SId = passport.PassportId,
                                Passport = passport,
                                NoServicio = passport.OrderNumber,
                                Tipo = STipo.Passport,
                                SubTipo = passport.ServicioConsular.GetDescription()
                            };
                            _context.ServiciosxPagar.Add(porPagar);
                        }
                    }
                    else if (AgencyDCubaId == agency.AgencyId)
                    {
                        var porPagar = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = passport.costo + (passport.Express ? (decimal)passport.PassportExpress?.Costo : 0),
                            Mayorista = wholesaler,
                            Agency = agency,
                            SId = passport.PassportId,
                            Passport = passport,
                            NoServicio = passport.OrderNumber,
                            Tipo = STipo.Passport,
                            SubTipo = passport.ServicioConsular.GetDescription()
                        };
                        _context.ServiciosxPagar.Add(porPagar);
                    }
                }
                //Servicio por pagar de agencia
                if(passport.OtrosCostos > 0)
                {
                    if (isbytransferencia)
                    {
                        var wholesaler = _context.Wholesalers.Find(passport.WholesalerId);
                        var agenciaMayorista = _context.Agency.Where(x => x.Name == wholesaler.name && x.Type == "Mayorista").FirstOrDefault();

                        var s = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = passport.OtrosCostos,
                            Mayorista = null,
                            Agency = agenciaMayorista,
                            SId = passport.PassportId,
                            Passport = passport,
                            NoServicio = passport.OrderNumber,
                            Tipo = STipo.Passport,
                            SubTipo = passport.ServicioConsular.GetDescription()
                        };
                        _context.ServiciosxPagar.Add(s);
                    }
                    else
                    {
                        var s = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = passport.OtrosCostos,
                            Mayorista = null,
                            Agency = agency,
                            SId = passport.PassportId,
                            Passport = passport,
                            NoServicio = passport.OrderNumber,
                            Tipo = STipo.Passport,
                            SubTipo = passport.ServicioConsular.GetDescription()
                        };
                        _context.ServiciosxPagar.Add(s);
                    }
                }

                //Guardo que empleado realizo el tramite
                TramiteEmpleado tramite = new TramiteEmpleado();
                tramite.fecha = DateTime.UtcNow;
                tramite.Id = Guid.NewGuid();
                tramite.IdEmpleado = user.UserId;
                tramite.IdTramite = passport.PassportId;
                tramite.tipoTramite = TramiteEmpleado.tipo_PASSPORT;
                tramite.IdAgency = agency.AgencyId;
                _context.TramiteEmpleado.Add(tramite);


                _context.Logs.Add(new Log
                {
                    Date = DateTime.Now,
                    Event = LogEvent.Crear,
                    Type = LogType.Pasaporte,
                    LogId = Guid.NewGuid(),
                    Message = passport.OrderNumber,
                    User = user,
                    Client = client,
                    Precio = passport.Total.ToString(),
                    Pagado = passport.RegistroPagos.Sum(x => x.valorPagado).ToString(),
                    AgencyId = passport.AgencyId,
                    Passport = passport
                });

                if (agency.AgencyId == AgencyDCubaId)
                {
                    var n = await _context.Passport.CountAsync(x => x.AgencyId == agency.AgencyId && !x.CreatedByCode);
                    passport.OrderNumber = "DC" + DateTime.Now.ToString("yyMM") + n.ToString("D4");
                }

                await _context.SaveChangesAsync();

                var sendEmail = await _context.EmailBodies.FirstOrDefaultAsync(x => x.EmailTemplate.AgencyId == agency.AgencyId && x.Number == "_01_");
                if (sendEmail != null)
                {
                    if (AgencyDCubaId == agency.AgencyId)
                    {
                        //Send Sms
                        string msg = $"Hola {passport.Client?.Name}, Gracias por confiar en District Cuba! \n" +
                        "Su documentación ha sido recibida en nuestra oficina y se encuentra en proceso de revision";
                        var response = _notificationService.sendSms(msg, passport.Client?.Phone?.Number.Replace(")", "").Replace("(", "").Replace("-", ""));
                    }

                    Dictionary<string, string> values = new Dictionary<string, string>();
                    values["passport_number"] = passport.OrderNumber;
                    await _buildEmailService.SendBuildEmail(sendEmail.Id, passport.ClientId, values);
                }

                if (AgencyCubabienTravelId == agency.AgencyId)
                {
                    //Send Sms
                    string msg = $"Hola {passport.Client?.Name}, Gracias por confiar en {agency.LegalName}! \n" +
                    "Su documentación ha sido recibida en nuestra oficina y se encuentra en proceso de revision";
                    var response = _notificationService.sendSms(msg, passport.Client?.Phone?.Number.Replace(")", "").Replace("(", "").Replace("-", ""));
                }

                return Result.Success(passport);
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Result.Failure<Passport>("No se ha podido crear el pasaporte");
            }
        }
    }
}
