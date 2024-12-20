using Agenciapp.Common.Contrains;
using Agenciapp.Common.Services.INotificationServices;
using Agenciapp.Domain.Models.DBViewModels;
using Agenciapp.Service.IReportServices;
using Agenciapp.Service.IReportServices.Models;
using Agenciapp.Service.IReportServices.Reports;
using AgenciappHome.Controllers.Class;
using AgenciappHome.Models;
using AgenciappHome.Models.Auxiliar;
using GraphQL.Client.Abstractions.Utilities;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AgenciappHome.Controllers.Class.AgenciaAuthorize;
using static iTextSharp.text.pdf.AcroFields;
using Document = iTextSharp.text.Document;

namespace AgenciappHome.Controllers
{
    public class HomeController : Base
    {
        private readonly IReportService _reportService;
        private readonly INotificationService _notificationService;

        public Task<object> ReporteVentas { get; private set; }

        public HomeController(databaseContext context, IReportService reportService, IWebHostEnvironment env,
            Microsoft.Extensions.Options.IOptions<Settings> settings, INotificationService notification) : base(context, env, settings)
        {
            _reportService = reportService;
            _notificationService = notification;
        }

        [Authorize]
        [CustomAuthorizeAttribute("ADMIN")]
        public async Task<IActionResult> Index()
        {
            //await UpdateAgencyPhone();
            //await Reporte.UpdateCancelaciones(_context);
            // InitMunicipios();
            //fix();
            //await AuxTransferirPagos();
            //await ServxPagarCaribe();
            //await UpdateSxpTicket();
            //await SetDateRegister();
            //await createSxP();

            var role = AgenciaAuthorize.getRole(User, _context);
            if (role == AgenciaAuthorize.TypeAutorize.None)
            {
                return RedirectToAction("Index", "Landing");
            }
            if (role != AgenciaAuthorize.TypeAutorize.Agencia)
            {
                return RedirectToAction("indexempleado", "Home");
            }
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();

            ViewBag.agency = aAgency;
            ViewBag.user = aUser.FirstOrDefault();

            if (aAgency.AgencyId == AgencyName.CubiqLLC || aAgency.AgencyId == AgencyName.BehiqueExpress)
            {
                var guiaactual = _context.GuiaAerea.Include(x => x.Paquetes).Where(x => x.Agency.AgencyId == AgencyName.CubiqLLC && x.Status == "Nueva");
                ViewBag.cantPaquetes = guiaactual.Sum(x => x.Paquetes.Count());
                ViewBag.cantKg = guiaactual.Sum(x => x.Paquetes.Sum(y => y.PesoKg));

                var guias = await _context.GuiaAerea.Where(x => x.Agency.AgencyId == AgencyName.CubiqLLC && x.Status == "Nueva").ToListAsync();
                ViewBag.guiasAbiertas = guias;
            }
            var Cookie = DataCookie.getCookie("AgenciappOfficeId", Request);

            var date = DateTime.Now;
            var datePrincipioMes = Convert.ToDateTime(date.Month + "/01/" + date.Year);
            if (aAgency.AgencyId != AgencyName.DCubaWashington && aAgency.AgencyId != AgencyName.DCubaHouston)
            {
                var dateMesAnterior = Convert.ToDateTime(DateTime.Now.AddMonths(-1).Month + "/01/" + date.Year);
                var dateMesAnterior2 = DateTime.Now.AddMonths(-1);
                int TotalOrder = _context.Order.Where(x => x.Type != "Remesas" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId)).Count();
                int TotalOrderEsteMes = _context.Order.Where(x => x.Type != "Remesas" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && (x.Date >= datePrincipioMes && x.Date <= date)).Count();
                int TotalOrderMesAnterior = _context.Order.Where(x => x.Type != "Remesas" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && (x.Date >= dateMesAnterior && x.Date <= dateMesAnterior2)).Count();

                int ticketTotal = _context.Ticket.Join(
                        _context.Client,
                        data => data.ClientId,
                        client => client.ClientId,
                        (data, client) => new { data, client }
                    )
                    .Where(x => x.client.AgencyId == aAgency.AgencyId && x.data.State != "Cancelada").Count();

                int ticketTotalEsteMes = _context.Ticket.Join(
                        _context.Client,
                        data => data.ClientId,
                        client => client.ClientId,
                        (data, client) => new { data, client }
                    )
                    .Where(x => x.client.AgencyId == aAgency.AgencyId && x.data.State != "Cancelada" && (x.data.RegisterDate <= date && x.data.RegisterDate >= datePrincipioMes)).Count();
                int ticketTotalMesAnterior = _context.Ticket.Join(
                        _context.Client,
                        data => data.ClientId,
                        client => client.ClientId,
                        (data, client) => new { data, client }
                    )
                    .Where(x => x.client.AgencyId == aAgency.AgencyId && x.data.State != "Cancelada" && (x.data.RegisterDate >= dateMesAnterior && x.data.RegisterDate <= dateMesAnterior)).Count();

                int clientsTotal = _context.Client.Where(x => x.AgencyId == aAgency.AgencyId).Count();
                int TotalClientsEsteMes = _context.Client.Where(x => x.AgencyId == aAgency.AgencyId && x.CreatedAt >= datePrincipioMes && x.CreatedAt <= date).Count();
                int TotalEmpleados = _context.User.Where(x => x.AgencyId == aAgency.AgencyId && (x.Type.Equals("Empleado") || x.Type.Equals("Agencia"))).Count();
                ViewData["TotalOrder"] = TotalOrderEsteMes;
                ViewData["TotalOrderRespectoMes"] = TotalOrderEsteMes - TotalOrderMesAnterior;
                ViewData["TotalTicket"] = ticketTotal;
                ViewData["TotalTicketRespectoMes"] = ticketTotalEsteMes - ticketTotalMesAnterior;
                ViewData["TotalClients"] = clientsTotal;
                ViewData["TotalClientsEsteMes"] = TotalClientsEsteMes;
                ViewData["Empleados"] = TotalEmpleados;
            }

            try //Primera vez que entra y no hay cookie
            {
                var o = _context.Office.Where(x => x.OfficeId == Guid.Parse(Cookie)).FirstOrDefault().Name;
                ViewData["NombreOficina"] = _context.Office.Where(x => x.OfficeId == Guid.Parse(Cookie)).FirstOrDefault().Name;
            }
            catch
            {
                var office = _context.Office.Where(x => x.AgencyId == aAgency.AgencyId);
                if (office.Count() == 1)
                {
                    DataCookie.setCookie("AgenciappOfficeId", office.FirstOrDefault().OfficeId.ToString(), Response);
                    ViewData["NombreOficina"] = office.FirstOrDefault().Name;

                }
            }
            ViewData["TotalEmpleados"] = _context.User.Where(x => x.Type == "Empleado" && x.AgencyId == aAgency.AgencyId).Count();

            return ViewAutorize(new string[] { }, null);
        }

        [Authorize]
        public async Task<IActionResult> AgencyAccountReport()
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            var accounts = _context.ServiciosxPagar.Include(x => x.Bill)
                .Include(x => x.EnvioCaribe).ThenInclude(x => x.Client)
                .Include(x => x.EnvioMaritimo).ThenInclude(x => x.Client)
                .Include(x => x.Order).ThenInclude(x => x.Client)
                .Include(x => x.OrderCubic).ThenInclude(x => x.Client)
                .Include(x => x.Passport).ThenInclude(x => x.Client)
                .Include(x => x.Rechargue).ThenInclude(x => x.Client)
                .Include(x => x.Remittance).ThenInclude(x => x.Client)
                .Include(x => x.Reserva).ThenInclude(x => x.Client)
                .Where(x => x.Mayorista == null && x.Agency.AgencyId == user.AgencyId && x.Date.Date == DateTime.Now.Date).OrderByDescending(x => x.Date);
            return ViewAutorize(new string[] { }, await accounts.ToListAsync());
        }

        public IActionResult IndexEmpleado()
        {
            var role = AgenciaAuthorize.getRole(User, _context);
            if (role == AgenciaAuthorize.TypeAutorize.None)
            {
                return RedirectToAction("Index", "Landing");
            }
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();
            ViewBag.User = aUser.FirstOrDefault();
            //Ventas por servicio.
            var datehoy = DateTime.Now.Date;
            var dateayer = datehoy.AddDays(-1);

            decimal importeAuto1 = 0;
            decimal importePasaje1 = 0;
            decimal importeHotel1 = 0;
            var ticket = _context.Ticket.Where(x => x.AgencyId == aAgency.AgencyId && x.State != "Cancelada").Join(_context.TramiteEmpleado.Where(t => t.IdEmpleado == aUser.FirstOrDefault().UserId), x => x.TicketId, y => y.IdTramite, (t, tr) => new { t.Payment, t.RegisterDate, t.type });

            var ticketaux = ticket.Where(x => x.RegisterDate >= datehoy);
            foreach (var item in ticketaux)
            {
                if (item.type == "pasaje")
                {
                    importePasaje1 += item.Payment;
                }
                if (item.type == "auto")
                {
                    importeAuto1 += item.Payment;
                }
                if (item.type == "hotel")
                {
                    importeHotel1 += item.Payment;
                }
            }

            decimal importeEnvios1 = _context.Order.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Type != "Remesas" && x.Date >= datehoy && x.UserId == aUser.FirstOrDefault().UserId).Sum(x => x.ValorPagado);
            decimal importeRemesas1 = _context.Remittance.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date >= datehoy && x.UserId == aUser.FirstOrDefault().UserId).Sum(x => x.Amount);
            var auxpasaporte = _context.Passport.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.UserId == aUser.FirstOrDefault().UserId);
            decimal importepasaporte = auxpasaporte.Where(x => x.FechaSolicitud.Date >= datehoy.Date).Sum(x => x.Pagado);
            decimal importeRecarga1 = _context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.date >= datehoy)
                .Join(_context.TramiteEmpleado.Where(t => t.IdEmpleado == aUser.FirstOrDefault().UserId), x => x.RechargueId, y => y.IdTramite, (t, tr) => new { t.Import })
                .Sum(x => x.Import);

            decimal importeEnvioMaritimo1 = _context.EnvioMaritimo.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date >= datehoy && x.UserId == aUser.FirstOrDefault().UserId).Sum(x => x.ValorPagado);

            AuxVentasPorServicio ventasxservicio1 = new AuxVentasPorServicio();
            if (importeAuto1 > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto1));
            if (importePasaje1 > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje1));
            if (importeHotel1 > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel1));
            if (importeEnvios1 > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios1));
            if (importeRemesas1 > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas1));
            if (importepasaporte > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Pasaportes", importepasaporte));
            if (importeRecarga1 > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga1));
            if (importeEnvioMaritimo1 > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo1));
            if (aAgency.Name == "Rapid Multiservice")
            {
                var auxenviocaribe = _context.EnvioCaribes.Include(x => x.Contact).ThenInclude(x => x.Address).Where(x => x.User.UserId == aUser.FirstOrDefault().UserId && x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date >= datehoy);
                decimal importeenviocaribe1 = auxenviocaribe.Sum(x => x.Amount);
                if (importeenviocaribe1 > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe1));

            }
            var servicios = _context.Servicios.Include(x => x.tipoServicio).Where(x => x.estado != "Cancelado" && x.agency.AgencyId == aAgency.AgencyId && x.fecha >= datehoy).Join(_context.TramiteEmpleado.Where(t => t.IdEmpleado == aUser.FirstOrDefault().UserId), x => x.ServicioId, y => y.IdTramite, (t, tr) => t);
            var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == aAgency.AgencyId);
            foreach (var item in tiposervicios)
            {
                decimal sum = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importePagado);
                if (sum > 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>(item.Nombre, sum));
            }

            ViewBag.ventasxservicio1 = ventasxservicio1.servicios.OrderByDescending(x => x.elem2);
            ViewBag.total1 = ventasxservicio1.getTotal();
            decimal importeAuto2 = 0;
            decimal importePasaje2 = 0;
            decimal importeHotel2 = 0;
            ticketaux = ticket.Where(x => x.RegisterDate >= dateayer && x.RegisterDate < datehoy);
            foreach (var item in ticketaux)
            {
                if (item.type == "pasaje")
                {
                    importePasaje2 += item.Payment;
                }
                if (item.type == "auto")
                {
                    importeAuto2 += item.Payment;
                }
                if (item.type == "hotel")
                {
                    importeHotel2 += item.Payment;
                }
            }

            decimal importeEnvios2 = _context.Order.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Type != "Remesas" && x.Date < datehoy && x.Date >= dateayer && x.UserId == aUser.First().UserId).Sum(x => x.ValorPagado);
            decimal importeRemesas2 = _context.Remittance.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date < datehoy && x.Date >= dateayer && x.UserId == aUser.First().UserId).Sum(x => x.Amount);
            decimal importeRecarga2 = _context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.date < datehoy && x.date >= dateayer && x.UserId == aUser.FirstOrDefault().UserId).Sum(x => x.Import);
            decimal importeEnvioMaritimo = _context.EnvioMaritimo.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date < datehoy && x.Date >= dateayer && x.UserId == aUser.First().UserId).Sum(x => x.ValorPagado);
            var auxpasaporte2 = _context.Passport.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.UserId == aUser.FirstOrDefault().UserId);
            decimal importepasaporte2 = auxpasaporte2.Where(x => x.FechaSolicitud < datehoy && x.FechaSolicitud >= dateayer).Sum(x => x.Total);

            AuxVentasPorServicio ventasxservicio2 = new AuxVentasPorServicio();
            if (importeAuto2 > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto2));
            if (importePasaje2 > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje2));
            if (importeHotel2 > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel2));
            if (importeEnvios2 > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios2));
            if (importeRemesas2 > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas2));
            if (importepasaporte2 > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Pasaportes", importepasaporte2));
            if (importeRecarga2 > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga2));
            if (importeEnvioMaritimo > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo));
            if (aAgency.Name == "Rapid Multiservice")
            {
                var auxenviocaribe = _context.EnvioCaribes.Include(x => x.Contact).ThenInclude(x => x.Address).Where(x => x.User.UserId == aUser.FirstOrDefault().UserId && x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date < datehoy && x.Date >= dateayer);
                decimal importeenviocaribe1 = auxenviocaribe.Sum(x => x.Amount);
                if (importeenviocaribe1 > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe1));

            }
            servicios = _context.Servicios.Include(x => x.tipoServicio).Where(x => x.estado != "Cancelada" && x.agency.AgencyId == aAgency.AgencyId && x.fecha < datehoy && x.fecha >= dateayer).Join(_context.TramiteEmpleado.Where(t => t.IdEmpleado == aUser.FirstOrDefault().UserId), x => x.ServicioId, y => y.IdTramite, (t, tr) => t); ;
            foreach (var item in tiposervicios)
            {
                decimal sum = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importePagado);
                if (sum > 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>(item.Nombre, sum));
            }
            ViewBag.ventasxservicio2 = ventasxservicio2.servicios.OrderByDescending(x => x.elem2);
            ViewBag.total2 = ventasxservicio2.getTotal();

            var user = aUser.FirstOrDefault();
            if (user.Type.Equals(TypeAutorize.PrincipalDistributor.ToString()))
            {
                var agencies = _context.UserAgencyTransferreds
                    .Include(x => x.Agency)
                    .Where(x => x.UserId == user.UserId).Select(x => x.Agency).ToList();
                ViewBag.agenciesDistributor = agencies;

                ViewBag.minoristasPaquete = _context.Minoristas.Where(x => x.Agency.AgencyId == user.AgencyId && x.Type == STipo.Paquete);
            }



            return ViewAutorize(new string[] { }, null);
        }

        [Authorize]
        public async Task<AuxVentasxServicio> getVentasxServicioRapid()
        {
            try
            {
                var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
                return await Reporte.GetVentasxServicioRapid(user, _context, _reportService);
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, "Server Error");
                return null;
            }

        }

        [Authorize]
        public async Task<AuxVentasxServicio> getVentasxServicio()
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Find(user.AgencyId);

            AuxVentasxServicio response = new AuxVentasxServicio();

            //**** Ventas por servicio. ****
            var datehoy = DateTime.Now.Date;
            DateTime dateayer = datehoy.AddDays(-1);
            DateTime dateayer2 = datehoy.AddDays((datehoy.Day - 1) * -1);


            //Utilidad por servicio
            decimal importeAuto1Utilidad = 0;
            decimal importePasaje1Utilidad = 0;
            decimal importeHotel1Utilidad = 0;
            decimal importeAislamiento1Utilidad = 0;
            //Venta por servicio
            decimal importeAuto1 = 0;
            decimal importePasaje1 = 0;
            decimal importeHotel1 = 0;
            decimal importeAislamiento1 = 0;
            var ticket = _context.Ticket
                .Where(x => x.PaqueteTuristicoId == null && x.AgencyId == aAgency.AgencyId && x.State != "Cancelada")
                ;
            var ticketaux = ticket.Where(x => x.RegisterDate.Date >= datehoy);
            foreach (var item in ticketaux)
            {
                if (item.type == "pasaje")
                {
                    importePasaje1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importePasaje1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
                if (item.type == "auto")
                {
                    importeAuto1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAuto1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
                if (item.type == "hotel")
                {
                    importeHotel1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeHotel1Utilidad += item.Total - item.Cost - item.Charges;
                    }


                }
                if (item.type == "aislamiento")
                {
                    importeAislamiento1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAislamiento1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
            }

            var auxorder = _context.Order.Include(x => x.agencyTransferida).Include(x => x.TipoPago).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type != "Remesas" && x.Type != "Combo" && x.Date.Date >= datehoy);
            decimal importeEnvios1 = auxorder.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxorder.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista);
            decimal importeEnvios1Utilidad = auxorder.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costoMayorista) + auxorder.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId && x.credito == 0).Sum(x => x.costoMayorista - x.costoDeProveedor - x.OtrosCostos);

            var auxcombos = _context.Order.Include(x => x.agencyTransferida).Include(x => x.Minorista).Include(x => x.TipoPago).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type == "Combo" && x.Date.Date >= datehoy);
            decimal importeCombos1 = auxcombos.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount) + auxcombos.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeCombos1Utilidad = auxcombos.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount - x.costoMayorista - x.OtrosCostos) + auxcombos.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista - x.costoDeProveedor);

            var auxRemesas = _context.Remittance.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= datehoy);
            decimal importeRemesas1 = auxRemesas.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxRemesas.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeRemesas1Utilidad = auxRemesas.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - (x.costoMayorista + x.costoporDespacho + aAgency.remesa_entregaCuba)) + auxRemesas.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - x.ValorPagado - aAgency.remesa_entregaCuba);

            var auxPaqueteTuristico = _context.PaquetesTuristicos.Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= datehoy);
            decimal importePaqueteTuristico1 = auxPaqueteTuristico.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount);
            decimal importePaqueteTuristico1Utilidad = auxPaqueteTuristico.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.Costo - x.OtrosCostos);

            var auxrecarga = _context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.date.Date >= datehoy);
            decimal importeRecarga1 = auxrecarga.Sum(x => x.Import);
            decimal importeRecarga1Utilidad = auxrecarga.Sum(x => x.Import - x.costoMayorista);

            var auxenviomatitimo = _context.EnvioMaritimo.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date.Date >= datehoy);
            decimal importeEnvioMaritimo1 = auxenviomatitimo.Sum(x => x.Amount);
            decimal importeEnvioMaritimo1Utilidad = auxenviomatitimo.Sum(x => x.Amount - x.costoMayorista);

            var auxpasaporte = _context.Passport.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.FechaSolicitud.Date >= datehoy.Date);
            decimal importepasaporte = auxpasaporte.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Total) + auxpasaporte.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
            decimal importePasaporteUtilidad = (await _reportService.UtilityByService(aAgency.AgencyId, STipo.Passport, datehoy, datehoy)).Value.Sum(x => x.Utility);

            var auxcubiq = _context.OrderCubiqs.Include(x => x.Paquetes).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= datehoy);
            decimal importecubiq = 0;
            decimal importecubiqUtilidad = 0;
            foreach (var item in auxcubiq)
            {
                if (item.AgencyId == aAgency.AgencyId)
                {
                    importecubiq += item.Amount;
                    if (aAgency.Name == "Cubiq LLC")
                    {
                        importecubiqUtilidad += item.Amount - item.Costo - item.HandlingAndTransportation.Cost - item.OtrosCostos;
                    }
                    else
                    {
                        importecubiqUtilidad += item.Amount - item.costoMayorista - item.HandlingAndTransportation.Cost - item.OtrosCostos;
                    }
                }
                else if (item.agencyTransferida.AgencyId == aAgency.AgencyId)
                {
                    importecubiq += item.costoMayorista + item.HandlingAndTransportation.Cost + item.OtrosCostos;
                    importecubiqUtilidad += item.costoMayorista - item.Paquetes.Sum(y => y.CostoCubiq);
                }
            }

            AuxVentasPorServicio ventasxservicio1 = new AuxVentasPorServicio();
            if (importeAuto1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto1)); };
            if (importePasaje1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje1)); };
            if (importeHotel1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel1)); };
            if (importeAislamiento1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento1)); };
            if (importeEnvios1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios1)); };
            if (importeCombos1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos1)); };
            if (importeRemesas1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas1)); };
            if (importePaqueteTuristico1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico1)); };
            if (importeRecarga1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga1)); };
            if (importeEnvioMaritimo1 != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo1));
            if (importepasaporte != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporte));
            if (importecubiq != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiq));

            AuxVentasPorServicio ventasxservicio1Utilidad = new AuxVentasPorServicio();
            if (importeAuto1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto1Utilidad));
            if (importePasaje1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje1Utilidad));
            if (importeHotel1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel1Utilidad));
            if (importeAislamiento1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento1Utilidad));
            if (importeEnvios1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios1Utilidad));
            if (importeCombos1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos1Utilidad));
            if (importeRemesas1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas1Utilidad));
            if (importePaqueteTuristico1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico1Utilidad));
            if (importeRecarga1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga1Utilidad));
            if (importeEnvioMaritimo1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo1Utilidad));
            if (importePasaporteUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaporte", importePasaporteUtilidad));
            if (importecubiqUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiqUtilidad));
            if (aAgency.Name == "Rapid Multiservice")
            {
                var auxenviocaribe = _context.EnvioCaribes.Include(x => x.Contact).ThenInclude(x => x.Address).Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.Date.Date >= datehoy);
                decimal importeenviocaribe1 = auxenviocaribe.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxenviocaribe.Where(x => x.AgencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
                decimal importeenviocaribe1Utilidad = auxenviocaribe.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costo - x.OtrosCostos) + getCostoCaribe(auxenviocaribe.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).ToList());
                if (importeenviocaribe1 != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe1));
                if (importeenviocaribe1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe1Utilidad));

            }

            var servicios = _context.Servicios.Include(x => x.tipoServicio).Where(x => x.PaqueteTuristicoId == null && x.estado != "Cancelado" && x.agency.AgencyId == aAgency.AgencyId && x.fecha.ToLocalTime().Date >= datehoy);
            var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == aAgency.AgencyId);
            foreach (var item in tiposervicios)
            {
                //Utilidad por servicio
                decimal sumUtilidad = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal - x.costoMayorista - x.CostoXServicio);
                //Ventas por servicio
                decimal sum = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal);

                if (sum != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>(item.Nombre, sum));
                if (sumUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>(item.Nombre, sumUtilidad));
            }

            response.ventasHoy = ventasxservicio1.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalVentasHoy = ventasxservicio1.getTotal();

            response.UtilidadHoy = ventasxservicio1Utilidad.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalUtilidadHoy = ventasxservicio1Utilidad.getTotal();

            decimal importeAuto2 = 0;
            decimal importeAuto2Utilidad = 0;

            decimal importePasaje2 = 0;
            decimal importePasaje2Utilidad = 0;

            decimal importeHotel2 = 0;
            decimal importeHotel2Utilidad = 0;

            decimal importeAislamiento2 = 0;
            decimal importeAislamiento2Utilidad = 0;
            ticketaux = ticket.Where(x => x.PaqueteTuristicoId == null && x.RegisterDate.Date.Date >= dateayer2);
            foreach (var item in ticketaux)
            {
                if (item.type == "pasaje")
                {
                    importePasaje2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importePasaje2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "auto")
                {
                    importeAuto2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAuto2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "hotel")
                {
                    importeHotel2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeHotel2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "aislamiento")
                {
                    importeAislamiento2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAislamiento2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
            }

            var auxenvios2 = _context.Order.Include(x => x.agencyTransferida).Include(x => x.TipoPago).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type != "Remesas" && x.Type != "Combo" && x.Date.Date >= dateayer2);
            decimal importeEnvios2 = auxenvios2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxenvios2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista);
            decimal importeEnvios2Utilidad = auxenvios2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costoMayorista) + auxenvios2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - x.costoDeProveedor - x.OtrosCostos);

            var auxcombos2 = _context.Order.Include(x => x.agencyTransferida).Include(x => x.Minorista).Include(x => x.TipoPago).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type == "Combo" && x.Date.Date.Date >= dateayer2);
            decimal importeCombos2 = auxcombos2.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount) + auxcombos2.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeCombos2Utilidad = auxcombos2.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount - x.costoMayorista - x.OtrosCostos) + auxcombos2.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista - x.costoDeProveedor);


            var auxremesa2 = _context.Remittance.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
            decimal importeRemesas2 = auxremesa2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxremesa2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeRemesas2Utilidad = auxremesa2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - (x.costoMayorista + x.costoporDespacho + aAgency.remesa_entregaCuba)) + auxremesa2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - aAgency.remesa_entregaCuba);

            var auxPaqueteTuristico2 = _context.PaquetesTuristicos.Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= dateayer2);
            decimal importePaqueteTuristico2 = auxPaqueteTuristico2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount);
            decimal importePaqueteTuristico2Utilidad = auxPaqueteTuristico2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.Costo - x.OtrosCostos);

            var auxrecarga2 = _context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.date.Date >= dateayer2);
            decimal importeRecarga2 = auxrecarga2.Sum(x => x.Import);
            decimal importeRecarga2Utilidad = auxrecarga2.Sum(x => x.Import - x.costoMayorista);

            var auxenviomaritimo2 = _context.EnvioMaritimo.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date.Date >= dateayer2);
            decimal importeEnvioMaritimo = auxenviomaritimo2.Sum(x => x.Amount);
            decimal importeEnvioMaritimo2Utilidad = auxenviomaritimo2.Sum(x => x.Amount - x.costoMayorista);

            var auxpasaporte2 = _context.Passport.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.FechaSolicitud.Date.Date >= dateayer2);
            decimal importepasaporte2 = auxpasaporte2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Total) + auxpasaporte2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
            decimal importepasaporteUtilidad2 = (await _reportService.UtilityByService(aAgency.AgencyId, STipo.Passport, dateayer2, datehoy)).Value.Sum(x => x.Utility);

            var auxcubiq2 = _context.OrderCubiqs.Include(x => x.Paquetes).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
            decimal importecubiq2 = 0;
            decimal importecubiqUtilidad2 = 0;
            foreach (var item in auxcubiq2)
            {
                if (item.AgencyId == aAgency.AgencyId)
                {
                    importecubiq2 += item.Amount;
                    if (aAgency.Name == "Cubiq LLC")
                    {
                        importecubiqUtilidad2 += item.Amount - item.Costo - item.HandlingAndTransportation.Cost - item.OtrosCostos;
                    }
                    else
                    {
                        importecubiqUtilidad2 += item.Amount - item.costoMayorista - item.HandlingAndTransportation.Cost - item.OtrosCostos;
                    }
                }
                else if (item.agencyTransferida.AgencyId == aAgency.AgencyId)
                {
                    importecubiq2 += item.costoMayorista + item.HandlingAndTransportation.Cost + item.OtrosCostos;
                    importecubiqUtilidad2 += item.costoMayorista - item.Paquetes.Sum(y => y.CostoCubiq);
                }

            }

            AuxVentasPorServicio ventasxservicio2 = new AuxVentasPorServicio();
            if (importeAuto2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto2));
            if (importePasaje2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje2));
            if (importeHotel2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel2));
            if (importeAislamiento2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento2));
            if (importeEnvios2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios2));
            if (importeCombos2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos2));
            if (importeRemesas2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas2));
            if (importePaqueteTuristico2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico2));
            if (importeRecarga2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga2));
            if (importeEnvioMaritimo != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo));
            if (importepasaporte2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporte2));
            if (importecubiq2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiq2));

            AuxVentasPorServicio ventasxservicio2Utilidad = new AuxVentasPorServicio();
            if (importeAuto2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto2Utilidad));
            if (importePasaje2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje2Utilidad));
            if (importeHotel2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel2Utilidad));
            if (importeAislamiento2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento2Utilidad));
            if (importeEnvios2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios2Utilidad));
            if (importeCombos2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos2Utilidad));
            if (importeRemesas2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas2Utilidad));
            if (importePaqueteTuristico2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico2Utilidad));
            if (importeRecarga2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga2Utilidad));
            if (importeEnvioMaritimo2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo2Utilidad));
            if (importepasaporteUtilidad2 != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporteUtilidad2));
            if (importecubiqUtilidad2 != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiqUtilidad2));
            if (aAgency.Name == "Rapid Multiservice")
            {
                var auxenviocaribe2 = _context.EnvioCaribes.Include(x => x.Contact).ThenInclude(x => x.Address).Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
                decimal importeenviocaribe = auxenviocaribe2.Where(x => x.AgencyId == user.AgencyId).Sum(x => x.Amount) + auxenviocaribe2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
                decimal importeenviocaribe2Utilidad = auxenviocaribe2.Where(x => x.AgencyId == user.AgencyId).Sum(x => x.Amount - x.costo - x.OtrosCostos) + getCostoCaribe(auxenviocaribe2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).ToList());
                if (importeenviocaribe != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe));
                if (importeenviocaribe2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe2Utilidad));
            }

            servicios = _context.Servicios.Include(x => x.tipoServicio).Where(x => x.PaqueteTuristicoId == null && x.estado != "Cancelado" && x.agency.AgencyId == aAgency.AgencyId && x.fecha.ToLocalTime().Date >= dateayer2);
            foreach (var item in tiposervicios)
            {
                decimal sum = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal);
                decimal sumUtilidad = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal - x.costoMayorista - x.CostoXServicio);
                if (sum != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>(item.Nombre, sum));
                if (sumUtilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>(item.Nombre, sumUtilidad));
            }
            response.ventasAyer = ventasxservicio2.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalVentasAyer = ventasxservicio2.getTotal();
            response.UtilidadAyer = ventasxservicio2Utilidad.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalUtilidadAyer = ventasxservicio2Utilidad.getTotal();

            //Ventas por empleado Hoy
            AuxVentasPorEmpleado ventasxempleado = new AuxVentasPorEmpleado();
            var ventasEmpleado = _context.RegistroPagos
                .Include(x => x.User)
                .Include(x => x.tipoPago)
                .Include(x => x.EnvioCaribe)
                .Include(x => x.EnvioMaritimo)
                .Include(x => x.Order).ThenInclude(x => x.Minorista)
                .Include(x => x.OrderCubiq)
                .Include(x => x.Passport)
                .Include(x => x.Rechargue)
                .Include(x => x.Servicio)
                .Include(x => x.Remittance)
                .Include(x => x.PaqueteTuristico)
                .Include(x => x.Ticket)
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date.ToLocalTime().Date >= datehoy && x.tipoPago.Type != "Crédito de Consumo" && x.tipoPago.Type != "Cubiq" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.Remittance.Status != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada")
                .Where(x => x.Order.Minorista == null && x.Ticket.PaqueteTuristicoId == null && x.Servicio.PaqueteTuristicoId == null)
                .Select(x => new { x.User, x.valorPagado })
                .GroupBy(x => x.User);

            foreach (var t in ventasEmpleado)
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionHoy = ventasxempleado.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionHoy = ventasxempleado.getTotal();

            //Ventas por empleado Ayer
            AuxVentasPorEmpleado ventasxempleado2 = new AuxVentasPorEmpleado();
            var ventasEmpleado2 = _context.RegistroPagos
                .Include(x => x.User)
                .Include(x => x.tipoPago)
                .Include(x => x.EnvioCaribe)
                .Include(x => x.EnvioMaritimo)
                .Include(x => x.Order).ThenInclude(x => x.Minorista)
                .Include(x => x.OrderCubiq)
                .Include(x => x.Passport)
                .Include(x => x.Rechargue)
                .Include(x => x.Remittance)
                .Include(x => x.PaqueteTuristico)
                .Include(x => x.Servicio)
                .Include(x => x.Ticket)
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date.ToLocalTime().Date < datehoy && x.date.ToLocalTime().Date >= dateayer && x.tipoPago.Type != "Crédito de Consumo" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada")
                .Where(x => x.Order.Minorista == null)
                .Select(x => new { x.User, x.valorPagado })
                .GroupBy(x => x.User);

            foreach (var t in ventasEmpleado2)
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado2.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionAyer = ventasxempleado2.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionAyer = ventasxempleado2.getTotal();

            return response;
        }

        [Authorize]
        [HttpGet]
        public async Task<AuxVentasxServicio> getLiquidacionByDateRapid(DateTime date)
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Find(user.AgencyId);
            AuxVentasxServicio response = new AuxVentasxServicio();
            //Ventas por empleado Hoy
            AuxVentasPorEmpleado ventasxempleado = new AuxVentasPorEmpleado();
            var ventasEmpleado = await _context.RegistroPagos
                .Include(x => x.User)
                .Include(x => x.tipoPago)
                .Include(x => x.EnvioCaribe)
                .Include(x => x.EnvioMaritimo)
                .Include(x => x.Order)
                .Include(x => x.Order)
                .Include(x => x.OrderCubiq)
                .Include(x => x.Passport)
                .Include(x => x.Rechargue)
                .Include(x => x.Remittance)
                .Include(x => x.PaqueteTuristico)
                .Include(x => x.Servicio)
                .Include(x => x.Ticket)
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date.ToLocalTime().Date == date && x.tipoPago.Type != "Crédito de Consumo" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                //.Where(x => x.EnvioCaribe.Status != "Cancelada" && x.Remittance.Status != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada")
                .Where(x => x.Order.Minorista == null).ToListAsync()
                //.Select(x => new { x.User, x.valorPagado })
                //.GroupBy(x => x.User)
                ;

            List<RegistroPago> aux = new List<RegistroPago>();
            foreach (var item in ventasEmpleado)
            {
                if (item.EnvioCaribe != null && (item.EnvioCaribe.Status != EnvioCaribe.STATUS_CANCELADA || ((DateTime)item.EnvioCaribe.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.EnvioMaritimo != null && (item.EnvioMaritimo.Status != EnvioMaritimo.STATUS_CANCELADA || ((DateTime)item.EnvioMaritimo.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Order != null && (item.Order.Status != Order.STATUS_CANCELADA || ((DateTime)item.Order.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.OrderCubiq != null && (item.OrderCubiq.Status != OrderCubiq.STATUS_CANCELADA || ((DateTime)item.OrderCubiq.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.PaqueteTuristico != null && (item.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA || ((DateTime)item.PaqueteTuristico.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Passport != null && (item.Passport.Status != Passport.STATUS_CANCELADA || ((DateTime)item.Passport.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Rechargue != null && (item.Rechargue.estado != Rechargue.STATUS_CANCELADA || ((DateTime)item.Rechargue.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Remittance != null && (item.Remittance.Status != Remittance.STATUS_CANCELADA || ((DateTime)item.Remittance.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Servicio != null && (item.Servicio.estado != Servicio.EstadoCancelado || ((DateTime)item.Servicio.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Ticket != null && (item.Ticket.State != Ticket.STATUS_CANCELADA || ((DateTime)item.Ticket.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
            }

            foreach (var t in aux.Select(x => new { x.User, x.valorPagado }).GroupBy(x => x.User))
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionHoy = ventasxempleado.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionHoy = ventasxempleado.getTotal();

            return response;
        }
        [Authorize]
        [HttpGet]
        public async Task<AuxVentasxServicio> getLiquidacionByDate(DateTime date)
        {
            var dateIni = date.Date.ToUniversalTime();
            var dateEnd = date.Date.AddDays(1);
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Find(user.AgencyId);
            AuxVentasxServicio response = new AuxVentasxServicio();
            AuxVentasPorEmpleado ventasxempleado = new AuxVentasPorEmpleado();
            var ventasEmpleado = await _context.RegistroPagos
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date >= dateIni && x.date < dateEnd && x.tipoPago.Type != "Crédito de Consumo" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.Remittance.Status != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada")
                .Where(x => x.Order.Minorista == null)
                .Select(x => new { x.User, x.valorPagado })
                .ToListAsync();

            foreach (var t in ventasEmpleado.GroupBy(x => x.User))
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionHoy = ventasxempleado.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionHoy = ventasxempleado.getTotal();

            return response;
        }

        [Authorize]
        public IActionResult Settings()
        {
            ViewData["EmailServer"] = _context.Config.First().Email_Server;
            ViewData["EmailPort"] = _context.Config.First().Email_Port;
            ViewData["EmailUser"] = _context.Config.First().Email_User;
            ViewData["EmailPass"] = _context.Config.First().Email_Pass;

            return ViewAutorize(new string[] { }, null);
        }

        [Authorize]
        [HttpPost]
        public JsonResult SaveSetting([FromBody] string[] list)
        {
            Config config = _context.Config.First();
            config.Email_Server = list[0];
            config.Email_Port = Int32.Parse(list[1]);
            config.Email_User = list[2];
            config.Email_Pass = list[3];

            _context.SaveChanges();

            return Json(new { });

        }

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return ViewAutorize(new string[] { }, null);
        }

        [Authorize]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return ViewAutorize(new string[] { }, null);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return ViewAutorize(new string[] { }, null);
        }

        [Authorize]
        public IActionResult ImportClient()
        {
            ViewData["Message"] = "Your application description page.";

            return ViewAutorize(new string[] { }, null);
        }

        [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return ViewAutorize(new string[] { }, new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static Office getOfficeActiveAuth(databaseContext _context, HttpRequest Request, HttpResponse Response)
        {
            var Cookie = DataCookie.getCookie("AgenciappOfficeId", Request);
            return _context.Office.Where(x => x.OfficeId == Guid.Parse(Cookie)).FirstOrDefault();
        }

        [Authorize]
        public async Task<object> ExportVentasRapid(string strdate)
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            return await Reporte.GetReporteVentas(strdate, _context, user, _env);
        }

        public void AddValueToTable(PdfPTable table, string value, Font font, int border = 0, BaseColor backgroundColor = null)
        {
            PdfPCell cell = new PdfPCell
            {
                Border = border,
            };
            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }
            cell.AddElement(new Phrase(value, font));
            table.AddCell(cell);
        }

        public void AddValueToTable(PdfPTable table, List<(string value, Font font)> values, int border = 0, BaseColor backgroundColor = null)
        {
            PdfPCell cell = new PdfPCell
            {
                Border = border,
            };
            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }
            foreach (var item in values)
            {
                cell.AddElement(new Phrase(item.value, item.font));
            }
            table.AddCell(cell);
        }

        [Authorize]
        public async Task<object> ExportVentas(string strdate)
        {
            Serilog.Log.Information("Reporte de ventas");

            var role = AgenciaAuthorize.getRole(User, _context);
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId);

            if (aUser.FirstOrDefault().Username == "7863517647" || aUser.FirstOrDefault().Username == "7865698868")
            {
                return await Reporte.GetReporteVentasAdrianMyScooter(strdate, _context, aUser.FirstOrDefault(), _env);
            }

            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {
                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font fonttransferida = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.ITALIC, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    BaseColor colorcell = WebColors.GetRGBColor("#fffdc4");

                    Agency agency = aAgency.FirstOrDefault();
                    Models.Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        if (System.IO.File.Exists(filePathQR))
                        {
                            iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                            imagelogo.ScaleAbsolute(75, 75);
                            celllogo.AddElement(imagelogo);
                        }
                    }

                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    var auxDate = strdate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateIni = DateTime.Parse(auxDate[0], culture);
                    var dateFin = DateTime.Parse(auxDate[1], culture);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);

                    string texto = dateIni.Date.ToShortDateString() + " a " + dateFin.Date.ToShortDateString();
                    if (dateIni.Date == dateFin.Date) texto = dateIni.Date.ToShortDateString();

                    Paragraph parPaq = new Paragraph("Ventas por servicio del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);

                    Dictionary<string, decimal> ventasTipoPago = new Dictionary<string, decimal>();
                    Dictionary<string, int> cantTipoPago = new Dictionary<string, int>();

                    Action<string, string, decimal> addVentaTipoPago = (tipo, reference, valor) =>
                    {
                        if (AgencyName.MiIslaServices == agency.AgencyId && tipo == "Zelle")
                            tipo = $"{tipo} {reference}";
                        if (ventasTipoPago.ContainsKey(tipo))
                            ventasTipoPago[tipo] += valor;
                        else
                            ventasTipoPago[tipo] = valor;

                        if (cantTipoPago.ContainsKey(tipo))
                            cantTipoPago[tipo]++;
                        else
                            cantTipoPago[tipo] = 1;
                    };

                    decimal grantotal = 0;
                    decimal deudatotal = 0;
                    decimal totalpagado = 0;

                    List<(int, ProductoBodega, Guid, decimal)> productosBodega = new List<(int, ProductoBodega, Guid, decimal)>(); // Cantidad, ProductoBodega, AgencyId

                    #region // REMESAS
                    List<Remittance> remesas = await _context.Remittance
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.Agency).ThenInclude(x => x.Phone)
                        .Include(x => x.agencyTransferida)
                        .Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();

                    float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    PdfPTable tblData = new PdfPTable(columnWidths)
                    {
                        WidthPercentage = 100
                    };

                    decimal refTotalRemesas = 0;
                    decimal refPagadpRemesas = 0;
                    decimal refDeudaRemesas = 0;
                    if (remesas.Count != 0)
                    {
                        doc.Add(new Phrase("Remesas", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                        .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (Remittance remesa in remesas)
                        {
                            decimal reftotalEQ = remesa.Amount;
                            decimal refpagadoEQ = 0;
                            decimal refdeudaEQ = 0;
                            bool isbytransferencia = false;
                            if (remesa.agencyTransferida != null)
                            {
                                if (remesa.agencyTransferida.AgencyId == agency.AgencyId)
                                {
                                    isbytransferencia = true;
                                    reftotalEQ = remesa.costoMayorista + remesa.OtrosCostos;
                                    refpagadoEQ = 0;
                                    refdeudaEQ = reftotalEQ;
                                }
                            }
                            List<string> pagos = new List<string>();
                            if (!isbytransferencia)
                            {
                                foreach (var item in remesa.Pagos)
                                {
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);

                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagadoEQ += item.valorPagado;

                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                }
                                refdeudaEQ = reftotalEQ - refpagadoEQ;
                            }

                            grantotal += reftotalEQ;
                            totalpagado += refpagadoEQ;
                            deudatotal += refdeudaEQ;

                            refTotalRemesas += reftotalEQ;
                            refPagadpRemesas += refpagadoEQ;
                            refDeudaRemesas += refdeudaEQ;

                            var index = remesas.IndexOf(remesa);

                            AddValueToTable(tblData, isbytransferencia ? "T." + remesa.Number : remesa.Number, isbytransferencia ? fonttransferida : normalFont, index == 0 ? 1 : 0);

                            var valueCell = new List<(string, Font)>() { (remesa.Client.Name + " " + remesa.Client.LastName, normalFont),
                            (remesa.Client.Phone?.Number ?? string.Empty, normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                            AddValueToTable(tblData, remesa.User != null ? remesa.User.Name + " " + remesa.User.LastName : string.Empty, normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refpagadoEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, reftotalEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refdeudaEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                        }

                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, refPagadpRemesas.ToString(), headFont, 0);
                        AddValueToTable(tblData, refTotalRemesas.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaRemesas.ToString(), headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }

                    #endregion

                    #region // Paquete Turistico
                    List<PaqueteTuristico> paquetesTuristicos = await _context.PaquetesTuristicos
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();
                    tblData = new PdfPTable(columnWidths);
                    tblData.WidthPercentage = 100;

                    decimal refTotalPaqueteTuristico = 0;
                    decimal refPagadoPaqueteTuristico = 0;
                    decimal refDeudaPaqueteTuristico = 0;
                    if (paquetesTuristicos.Count != 0)
                    {
                        doc.Add(new Phrase("Paquete Turístico", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                        .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (PaqueteTuristico paqueteTuristico in paquetesTuristicos)
                        {
                            decimal reftotalEQ = paqueteTuristico.Amount;
                            decimal refpagadoEQ = 0;
                            decimal refdeudaEQ = 0;
                            bool isbytransferencia = false;

                            List<string> pagos = new List<string>();
                            if (!isbytransferencia)
                            {
                                foreach (var item in paqueteTuristico.Pagos)
                                {
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);

                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagadoEQ += item.valorPagado;
                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                }
                                refdeudaEQ = reftotalEQ - refpagadoEQ;
                            }

                            grantotal += reftotalEQ;
                            totalpagado += refpagadoEQ;
                            deudatotal += refdeudaEQ;

                            refTotalPaqueteTuristico += reftotalEQ;
                            refPagadoPaqueteTuristico += refpagadoEQ;
                            refDeudaPaqueteTuristico += refdeudaEQ;

                            var index = paquetesTuristicos.IndexOf(paqueteTuristico);

                            AddValueToTable(tblData, isbytransferencia ? "T." + paqueteTuristico.Number : paqueteTuristico.Number, isbytransferencia ? fonttransferida : normalFont, index == 0 ? 1 : 0);

                            var valueCell = new List<(string, Font)>() { (paqueteTuristico.Client.Name + " " + paqueteTuristico.Client.LastName, normalFont),
                            (paqueteTuristico.Client?.Phone?.Number ?? string.Empty, normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                            AddValueToTable(tblData, paqueteTuristico.User != null ? paqueteTuristico.User.Name + " " + paqueteTuristico.User.LastName : string.Empty, normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refpagadoEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, reftotalEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refdeudaEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                        }

                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, refPagadoPaqueteTuristico.ToString(), headFont, 0);
                        AddValueToTable(tblData, refTotalPaqueteTuristico.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaPaqueteTuristico.ToString(), headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }

                    #endregion

                    #region // ENVIOS MARITIMOS

                    float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsmaritimo);
                    tblData.WidthPercentage = 100;

                    List<EnvioMaritimo> enviosmaritimos = await _context.EnvioMaritimo
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();

                    decimal refTotalMaritimo = 0;
                    decimal refDeudaMaritimo = 0;
                    decimal refPagadoMaritimo = 0;
                    if (enviosmaritimos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Marítimos", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                       .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (EnvioMaritimo enviomaritimo in enviosmaritimos)
                        {
                            decimal reftotal = enviomaritimo.Amount;
                            decimal refpagado = 0;
                            decimal refdeuda = 0;

                            List<string> pagos = new List<string>();
                            foreach (var item in enviomaritimo.RegistroPagos)
                            {
                                if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                    pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                else
                                    pagos.Add(item.tipoPago.Type);

                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refpagado += item.valorPagado;
                                addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                            }

                            refdeuda = reftotal - refpagado;

                            grantotal += reftotal;
                            totalpagado += refpagado;
                            deudatotal += refdeuda;

                            refTotalMaritimo += reftotal;
                            refPagadoMaritimo += refpagado;
                            refDeudaMaritimo += refdeuda;

                            var index = enviosmaritimos.IndexOf(enviomaritimo);

                            AddValueToTable(tblData, enviomaritimo.Number, normalFont, index == 0 ? 1 : 0);

                            var valueCell = new List<(string, Font)>() { (enviomaritimo.Client.Name + " " + enviomaritimo.Client.LastName, normalFont),
                            (enviomaritimo.Client?.Phone?.Number ?? string.Empty, normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                            AddValueToTable(tblData, enviomaritimo.User != null ? $"{enviomaritimo.User.Name} {enviomaritimo.User.LastName}" : string.Empty, normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refpagado.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, reftotal.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refdeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                        }

                        AddValueToTable(tblData, "Totales", normalFont, 0);
                        AddValueToTable(tblData, string.Empty, normalFont, 0);
                        AddValueToTable(tblData, string.Empty, normalFont, 0);
                        AddValueToTable(tblData, refPagadoMaritimo.ToString(), normalFont, 0);
                        AddValueToTable(tblData, refTotalMaritimo.ToString(), normalFont, 0);
                        AddValueToTable(tblData, refDeudaMaritimo.ToString(), normalFont, 0);
                        AddValueToTable(tblData, string.Empty, normalFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // ENVIOS CARIBE

                    float[] columnWidthscaribe = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsmaritimo);
                    tblData.WidthPercentage = 100;

                    List<EnvioCaribe> envioscaribe = await _context.EnvioCaribes
                        .Include(x => x.AgencyTransferida)
                        .Include(x => x.User)
                        .Include(x => x.RegistroPagos)
                        .ThenInclude(x => x.tipoPago)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.AgencyTransferidaId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();

                    decimal refTotalEnvioCaribe = 0;
                    decimal refPagadoEnvioCaribe = 0;
                    decimal refDeudaEnvioCaribe = 0;
                    if (envioscaribe.Count != 0)
                    {

                        doc.Add(new Phrase("Envíos Caribe", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                       .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (EnvioCaribe enviocaribe in envioscaribe)
                        {
                            decimal reftotalEC = enviocaribe.Amount;
                            decimal refpagadoEC = 0;
                            decimal refdeudaEC = 0;
                            bool isbytransferencia = false;
                            if (enviocaribe.AgencyTransferidaId != null)
                            {
                                if (enviocaribe.AgencyTransferidaId == agency.AgencyId)
                                {
                                    isbytransferencia = true;
                                    reftotalEC = enviocaribe.costo + enviocaribe.OtrosCostos;
                                    refpagadoEC = 0;
                                    refdeudaEC = reftotalEC;
                                }
                            }

                            List<string> pagos = new List<string>();
                            if (!isbytransferencia)
                            {
                                foreach (var item in enviocaribe.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagadoEC += item.valorPagado;
                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);

                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }
                                refdeudaEC = reftotalEC - refpagadoEC;

                            }

                            refTotalEnvioCaribe += reftotalEC;
                            refPagadoEnvioCaribe += refpagadoEC;
                            refDeudaEnvioCaribe += refdeudaEC;

                            grantotal += reftotalEC;
                            totalpagado += refpagadoEC;
                            deudatotal += refdeudaEC;

                            if (!pagos.Any()) { pagos.Add("-"); }

                            var index = envioscaribe.IndexOf(enviocaribe);

                            AddValueToTable(tblData, isbytransferencia ? "T." + enviocaribe.Number : enviocaribe.Number, isbytransferencia ? fonttransferida : normalFont, index == 0 ? 1 : 0);

                            var valueCell = new List<(string, Font)>() { (enviocaribe.Client.Name + " " + enviocaribe.Client.LastName, normalFont),
                            (enviocaribe.Client?.Phone?.Number ?? string.Empty, normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                            AddValueToTable(tblData, enviocaribe.User != null ? $"{enviocaribe.User.Name} {enviocaribe.User.LastName}" : string.Empty, normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refpagadoEC.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, reftotalEC.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refdeudaEC.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                        }

                        // Añado el total
                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, refPagadoEnvioCaribe.ToString(), headFont, 0);
                        AddValueToTable(tblData, refTotalEnvioCaribe.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaEnvioCaribe.ToString(), headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS CUBIQ

                    float[] columnWidthsCubiq = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsCubiq);
                    tblData.WidthPercentage = 100;

                    List<OrderCubiq> envioscubiq = await _context.OrderCubiqs
                        .Include(x => x.agencyTransferida)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.User)
                        .Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferidaId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();
                    decimal refTotalCubiq = 0;
                    decimal refPagadoCubiq = 0;
                    decimal refDeudaCubiq = 0;
                    if (envioscubiq.Count != 0)
                    {

                        doc.Add(new Phrase("Envíos Carga AM", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                      .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (OrderCubiq enviocubiq in envioscubiq)
                        {
                            decimal reftotal = enviocubiq.Amount;
                            decimal refpagado = 0;
                            decimal refdeuda = 0;
                            bool isbytransferencia = false;
                            if (enviocubiq.agencyTransferida != null)
                            {
                                if (enviocubiq.agencyTransferidaId == agency.AgencyId)
                                {
                                    isbytransferencia = true;
                                    reftotal = enviocubiq.costoMayorista + enviocubiq.HandlingAndTransportation.Cost + enviocubiq.OtrosCostos + enviocubiq.InsuranceValue + enviocubiq.ValorAduanal; ;
                                    refpagado = 0;
                                    refdeuda = reftotal;
                                }
                            }

                            List<string> pagos = new List<string>();
                            if (!isbytransferencia)
                            {
                                foreach (var item in enviocubiq.RegistroPagos)
                                {
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);

                                    if (item.tipoPago.Type != "Crédito de Consumo" && item.tipoPago.Type != "Cubiq")
                                        refpagado += item.valorPagado;
                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);

                                }
                                refdeuda = reftotal - refpagado;
                            }

                            if (!pagos.Any()) { pagos.Add("-"); }

                            refTotalCubiq += reftotal;
                            refPagadoCubiq += refpagado;
                            refDeudaCubiq += refdeuda;

                            grantotal += reftotal;
                            totalpagado += refpagado;
                            deudatotal += refdeuda;

                            var index = envioscubiq.IndexOf(enviocubiq);

                            AddValueToTable(tblData, isbytransferencia ? "T." + enviocubiq.Number : enviocubiq.Number, isbytransferencia ? fonttransferida : normalFont, index == 0 ? 1 : 0);

                            var valueCell = new List<(string, Font)>() { (enviocubiq.Client.Name + " " + enviocubiq.Client.LastName, normalFont),
                            (enviocubiq.Client != null ? enviocubiq.Client.Phone.Number : string.Empty, normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                            AddValueToTable(tblData, enviocubiq.User != null ? $"{enviocubiq.User.Name} {enviocubiq.User.LastName}" : string.Empty, normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refpagado.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, reftotal.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refdeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                        }

                        // Añado el total
                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, refPagadoCubiq.ToString(), headFont, 0);
                        AddValueToTable(tblData, refTotalCubiq.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaCubiq.ToString(), headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }

                    #endregion

                    #region // PASAPORTES

                    float[] columnWidthspasaportes = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthspasaportes);
                    tblData.WidthPercentage = 100;

                    List<Passport> pasaportes = await _context.Passport
                        .Include(x => x.Agency)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.AgencyTransferida)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Client)
                        .Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId) && x.FechaSolicitud.Date >= dateIni.Date && x.FechaSolicitud.Date <= dateFin.Date)
                        .Where(x => !x.AppMovil || (x.AppMovil && x.Status != Passport.STATUS_REVIEW))
                        .ToListAsync();

                    if (AgencyName.IsDistrictCuba(agency.AgencyId))
                    {
                        pasaportes = pasaportes.OrderBy(x => x.OrderNumber).ToList();
                    }
                    decimal refTotalPasaporte = 0;
                    decimal refPagadoPasaporte = 0;
                    decimal refDeudaPasaporte = 0;
                    if (pasaportes.Count != 0)
                    {

                        doc.Add(new Phrase("Pasaportes", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                      .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (Passport pasaporte in pasaportes)
                        {
                            decimal reftotal = pasaporte.Total;
                            decimal refpagado = 0;
                            decimal refdeuda = 0;
                            bool bytransferencia = false;
                            if (pasaporte.AgencyTransferidaId == agency.AgencyId)
                            {
                                bytransferencia = true;
                                reftotal = pasaporte.costo + pasaporte.OtrosCostos;
                                refpagado = 0;
                                refdeuda = reftotal;
                            }

                            List<string> pagos = new List<string>();

                            if (!bytransferencia)
                            {
                                foreach (var item in pasaporte.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagado += item.valorPagado;
                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);

                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }
                                refdeuda = reftotal - refpagado;
                            }

                            if (!pagos.Any()) { pagos.Add("-"); }

                            refTotalPasaporte += reftotal;
                            refDeudaPasaporte += refdeuda;
                            refPagadoPasaporte += refpagado;

                            grantotal += reftotal;
                            totalpagado += refpagado;
                            deudatotal += refdeuda;

                            var index = pasaportes.IndexOf(pasaporte);

                            var valueCell = new List<(string, Font)>() { (pasaporte.OrderNumber, normalFont) };
                            if (pasaporte.AgencyTransferida != null)
                            {
                                if (pasaporte.AgencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    valueCell.Add(("T. " + pasaporte.Agency.Name, fonttransferida));
                                }
                            }
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                            valueCell = new List<(string, Font)>() { (pasaporte.Client.Name + " " + pasaporte.Client.LastName, normalFont),
                            (pasaporte.Client.Phone != null ? pasaporte.Client.Phone.Number : "", normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                            AddValueToTable(tblData, pasaporte.User != null ? $"{pasaporte.User.Name} {pasaporte.User.LastName}" : "", normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refpagado.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, reftotal.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refdeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, bytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                        }

                        // Añado el total
                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, refPagadoPasaporte.ToString(), headFont, 0);
                        AddValueToTable(tblData, refTotalPasaporte.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaPasaporte.ToString(), headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS AEREOS

                    float[] columnWidthsaereo = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsaereo);
                    tblData.WidthPercentage = 100;

                    List<Order> enviosaereos = await _context.Order
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Agency)
                        .Include(x => x.User)
                        .Include(x => x.Agency).ThenInclude(x => x.Phone)
                        .Include(x => x.agencyTransferida)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date && x.Type != "Remesas" && x.Type != "Combo").ToListAsync();
                    decimal refTotalEnvioAereo = 0;
                    decimal refDeudaEnvioAereo = 0;
                    decimal refPagadoEnvioAereo = 0;
                    decimal refCreditoEnvioAereo = 0;
                    if (enviosaereos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Aéreos", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                      .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (Order envioaereo in enviosaereos)
                        {
                            decimal refTotal = envioaereo.Amount;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;
                            decimal creditoConsumo = 0;

                            bool isbytransferencia = false;

                            if (envioaereo.agencyTransferida != null)
                            {
                                if (envioaereo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    isbytransferencia = true;
                                    refTotal = envioaereo.costoMayorista;
                                    refPagado = 0;
                                    refDeuda = refTotal;
                                }
                            }

                            List<string> pagos = new List<string>();
                            if (!isbytransferencia)
                            {
                                foreach (var item in envioaereo.Pagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;

                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);

                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }

                                refDeuda = refTotal - (refPagado + creditoConsumo);
                                refCreditoEnvioAereo += creditoConsumo;

                                refTotalEnvioAereo += refTotal;
                                refPagadoEnvioAereo += refPagado;
                                refDeudaEnvioAereo += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;
                            }
                            else
                            {
                                refTotalEnvioAereo += refTotal;
                                refPagadoEnvioAereo += refPagado;
                                refDeudaEnvioAereo += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;
                            }
                            if (!pagos.Any()) { pagos.Add("-"); }

                            var index = enviosaereos.IndexOf(envioaereo);

                            List<(string, Font)> valueCell = new List<(string, Font)>() { (envioaereo.Number, normalFont) };
                            if (envioaereo.agencyTransferida != null)
                            {
                                if (envioaereo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    valueCell.Add(("T. " + envioaereo.Agency.Name, fonttransferida));
                                }
                            }
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);

                            valueCell = new List<(string, Font)>() { (envioaereo.Client.Name + " " + envioaereo.Client.LastName, normalFont),
                                (isbytransferencia ? envioaereo.Agency.Phone?.Number ?? "" : envioaereo.Client.Phone?.Number ?? "", normalFont) };

                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                            AddValueToTable(tblData, envioaereo.User != null ? $"{envioaereo.User.Name} {envioaereo.User.LastName}" : "", normalFont, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);

                            valueCell = new List<(string, Font)>() { (refPagado.ToString(), normalFont) };
                            if (creditoConsumo > 0 && !isbytransferencia)
                                valueCell.Add((creditoConsumo.ToString(), normalRedFont));

                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                            AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                            AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                            AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                        }

                        // Añado el total
                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, refPagadoEnvioAereo.ToString(), headFont, 0);

                        if (refCreditoEnvioAereo > 0)
                            AddValueToTable(tblData, refCreditoEnvioAereo.ToString(), headRedFont, 0);
                        else
                            AddValueToTable(tblData, "", headFont, 0);

                        AddValueToTable(tblData, refDeudaEnvioAereo.ToString(), headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // ENVIOS COMBOS

                    tblData = new PdfPTable(columnWidthsaereo);
                    tblData.WidthPercentage = 100;

                    List<Order> envioscombos = await _context.Order
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Agency)
                        .Include(x => x.User)
                        .Include(x => x.Minorista)
                        .Include(x => x.agencyTransferida)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date && x.Type == "Combo").ToListAsync();
                    decimal refTotalEnvioCombo = 0;
                    decimal refDeudaEnvioCombo = 0;
                    decimal refPagadoEnvioCombo = 0;
                    decimal refCreditoEnvioCombo = 0;
                    if (envioscombos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Combos", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                            .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (Order enviocombo in envioscombos)
                        {
                            decimal refTotal = enviocombo.Amount;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;
                            decimal creditoConsumo = 0;

                            bool isbytransferencia = false;

                            if (enviocombo.agencyTransferida != null)
                            {
                                if (enviocombo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    isbytransferencia = true;
                                    refTotal = enviocombo.costoMayorista + enviocombo.OtrosCostos;
                                    refPagado = 0;
                                    refDeuda = refTotal;
                                }
                            }
                            else if (enviocombo.Minorista != null)
                            {
                                isbytransferencia = true;
                                refTotal = enviocombo.costoMayorista + enviocombo.OtrosCostos;
                                refPagado = 0;
                                refDeuda = refTotal;
                            }

                            List<string> pagos = new List<string>();
                            if (!isbytransferencia)
                            {
                                foreach (var item in enviocombo.Pagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;

                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }

                                refDeuda = refTotal - (refPagado + creditoConsumo);
                                refCreditoEnvioCombo += creditoConsumo;

                                refTotalEnvioCombo += refTotal;
                                refPagadoEnvioCombo += refPagado;
                                refDeudaEnvioCombo += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;
                            }
                            else
                            {
                                refTotalEnvioCombo += refTotal;
                                refPagadoEnvioCombo += refPagado;
                                refDeudaEnvioCombo += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;
                            }
                            if (!pagos.Any()) { pagos.Add("-"); }

                            var index = envioscombos.IndexOf(enviocombo);

                            List<(string, Font)> valueCell = new List<(string, Font)>() { (enviocombo.Number, normalFont) };
                            if (enviocombo.agencyTransferida != null)
                            {
                                if (enviocombo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    valueCell.Add(("T. " + enviocombo.Agency.Name, fonttransferida));
                                }
                            }
                            else if (enviocombo.Minorista != null)
                            {
                                valueCell.Add(("T. " + enviocombo.Minorista.Name, fonttransferida));
                            }

                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);

                            valueCell = new List<(string, Font)>() { (enviocombo.Client.Name + " " + enviocombo.Client.LastName, normalFont),
                            (enviocombo.Client.Phone != null ? enviocombo.Client.Phone.Number : "", normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);

                            AddValueToTable(tblData, enviocombo.User != null ? $"{enviocombo.User.Name} {enviocombo.User.LastName}" : "", normalFont, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);

                            valueCell = new List<(string, Font)>() { (refPagado.ToString(), normalFont) };
                            if (creditoConsumo > 0 && !isbytransferencia)
                                valueCell.Add((creditoConsumo.ToString(), normalRedFont));
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);

                            AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);
                            AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);
                            AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);
                        }

                        // Añado el total
                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, refPagadoEnvioCombo.ToString(), headFont, 0);
                        if (refCreditoEnvioCombo > 0)
                            AddValueToTable(tblData, refCreditoEnvioCombo.ToString(), headFont, 0);
                        else
                            AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, refDeudaEnvioCombo.ToString(), headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // BOLETOS  
                    float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, (float)1, (float)1, (float)1, (float)1 };
                    tblData = new PdfPTable(columnWidthboletos);
                    tblData.WidthPercentage = 100;

                    List<Ticket> boletos = _context.Ticket
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => !x.ClientIsCarrier && x.PaqueteTuristicoId == null && x.State != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date).ToList();
                    decimal refTotalBoletos = 0;
                    decimal refDeudaBoletos = 0;
                    decimal refPagadoBoletos = 0;
                    decimal refCreditoBoletos = 0;
                    if (boletos.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos", headFont));
                        new List<string>() { "No.", "Cliente", "Empleado", "Tipo", "Pagado", "Total", "Debe", "Tipo de Pago" }
                            .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (Ticket boleto in boletos)
                        {
                            decimal refTotal = boleto.Total;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;
                            decimal creditoConsumo = 0;
                            List<string> pagos = new List<string>();

                            foreach (var item in boleto.RegistroPagos)
                            {
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refPagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;

                                addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                    pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                else
                                    pagos.Add(item.tipoPago.Type);
                            }
                            refDeuda = refTotal - (refPagado + creditoConsumo);
                            refCreditoBoletos += creditoConsumo;
                            grantotal += refTotal;
                            totalpagado += refPagado;
                            deudatotal += refDeuda;

                            refTotalBoletos += refTotal;
                            refPagadoBoletos += refPagado;
                            refDeudaBoletos += refDeuda;

                            var index = boletos.IndexOf(boleto);

                            AddValueToTable(tblData, boleto.ReservationNumber, normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                            var valueCell = new List<(string, Font)>() { (boleto.Client.Name + " " + boleto.Client.LastName, normalFont),
                            (boleto.Client.Phone != null ? boleto.Client.Phone.Number : "", normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                            AddValueToTable(tblData, boleto.User != null ? $"{boleto.User.Name} {boleto.User.LastName}" : "", normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                            AddValueToTable(tblData, boleto.type, normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                            valueCell = new List<(string, Font)>() { (refPagado.ToString(), normalFont) };
                            if (creditoConsumo > 0)
                            {
                                valueCell.Add((creditoConsumo.ToString(), normalRedFont));
                            }
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                            AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                            AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                            AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                        }

                        // Añado el total
                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);

                        var valueCell1 = new List<(string, Font)>() { (refPagadoBoletos.ToString(), headFont) };
                        if (refCreditoBoletos > 0)
                            valueCell1.Add((refCreditoBoletos.ToString(), headRedFont));

                        AddValueToTable(tblData, valueCell1, 0);
                        AddValueToTable(tblData, refTotalBoletos.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaBoletos.ToString(), headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }

                    #endregion

                    #region // BOLETOS CARRIER
                    tblData = new PdfPTable(columnWidthboletos);
                    tblData.WidthPercentage = 100;

                    List<Ticket> boletosCarrier = _context.Ticket
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.ClientIsCarrier && x.PaqueteTuristicoId == null && x.State != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date).ToList();
                    decimal refTotalBoletosCarrier = 0;
                    decimal refDeudaBoletosCarrier = 0;
                    decimal refPagadoBoletosCarrier = 0;
                    decimal refCreditoBoletosCarrier = 0;
                    if (boletosCarrier.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos Carrier", headFont));

                        doc.Add(new Phrase("Boletos", headFont));
                        new List<string>() { "No.", "Cliente", "Empleado", "Tipo", "Pagado", "Total", "Debe", "Tipo de Pago" }
                            .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (Ticket boleto in boletosCarrier)
                        {
                            decimal refTotal = boleto.Total;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;
                            decimal creditoConsumo = 0;
                            List<string> pagos = new List<string>();

                            foreach (var item in boleto.RegistroPagos)
                            {
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refPagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;

                                addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                    pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                else
                                    pagos.Add(item.tipoPago.Type);
                            }
                            refDeuda = refTotal - (refPagado + creditoConsumo);
                            refCreditoBoletosCarrier += creditoConsumo;
                            grantotal += refTotal;
                            totalpagado += refPagado;
                            deudatotal += refDeuda;

                            refTotalBoletosCarrier += refTotal;
                            refPagadoBoletosCarrier += refPagado;
                            refDeudaBoletosCarrier += refDeuda;

                            var index = boletosCarrier.IndexOf(boleto);

                            AddValueToTable(tblData, boleto.ReservationNumber, normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                            var valueCell = new List<(string, Font)>() { (boleto.Client.Name + " " + boleto.Client.LastName, normalFont),
                            (boleto.Client.Phone != null ? boleto.Client.Phone.Number : "", normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                            AddValueToTable(tblData, boleto.User != null ? $"{boleto.User.Name} {boleto.User.LastName}" : "", normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                            AddValueToTable(tblData, boleto.type, normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                            valueCell = new List<(string, Font)>() { (refPagado.ToString(), normalFont) };
                            if (creditoConsumo > 0)
                            {
                                valueCell.Add((creditoConsumo.ToString(), normalRedFont));
                            }
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                            AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                            AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                            AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                        }

                        // Añado el total

                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);

                        var valueCell1 = new List<(string, Font)>() { (refPagadoBoletosCarrier.ToString(), headFont) };
                        if (refCreditoBoletosCarrier > 0)
                            valueCell1.Add((refCreditoBoletosCarrier.ToString(), headRedFont));
                        AddValueToTable(tblData, valueCell1, 0);

                        AddValueToTable(tblData, refTotalBoletosCarrier.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaBoletosCarrier.ToString(), headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // RECARGAS

                    float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsaereo);
                    tblData.WidthPercentage = 100;

                    List<Rechargue> recargas = await _context.Rechargue
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.User)
                        .Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.date.Date >= dateIni.Date && x.date.Date <= dateFin.Date).ToListAsync();
                    decimal refTotalRecarga = 0;
                    decimal refPagadoRecarga = 0;
                    decimal refDeudaRecarga = 0;
                    if (recargas.Count != 0)
                    {
                        doc.Add(new Phrase("RECARGAS", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo de Pago" }
                            .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (Rechargue recarga in recargas)
                        {
                            decimal refTotal = recarga.Import;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;

                            List<string> pagos = new List<string>();
                            foreach (var item in recarga.RegistroPagos)
                            {
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refPagado += item.valorPagado;
                                addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                    pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                else
                                    pagos.Add(item.tipoPago.Type);
                            }
                            refDeuda = refTotal - refPagado;

                            refTotalRecarga += refTotal;
                            refPagadoRecarga += refPagado;
                            refDeudaRecarga += refDeuda;

                            grantotal += refTotal;
                            totalpagado += refPagado;
                            deudatotal += refDeuda;

                            var index = recargas.IndexOf(recarga);

                            AddValueToTable(tblData, recarga.Number, normalFont, index == 0 ? 1 : 0);

                            var valueCell = new List<(string, Font)>() { (recarga.Client.Name + " " + recarga.Client.LastName, normalFont),
                            (recarga.Client.Phone != null ? recarga.Client.Phone.Number : "", normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                            AddValueToTable(tblData, recarga.User != null ? $"{recarga.User.Name} {recarga.User.LastName}" : "", normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refPagado.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                        }

                        // Añado el total

                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);
                        AddValueToTable(tblData, refPagadoRecarga.ToString(), headFont, 0);
                        AddValueToTable(tblData, refTotalRecarga.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaRecarga.ToString(), headFont, 0);
                        AddValueToTable(tblData, "", headFont, 0);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region OTROS SERVICIOS
                    float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1 };

                    DateTime initUtc = dateIni.Date.ToUniversalTime();
                    DateTime endUtc = dateFin.Date.AddDays(1).ToUniversalTime();
                    List<Servicio> servicios = await _context.Servicios
                        .Include(x => x.tipoServicio)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.cliente).ThenInclude(x => x.Phone)
                        .Include(x => x.User)
                        .Include(x => x.Products).ThenInclude(x => x.Product).ThenInclude(x => x.Categoria)
                        .Where(x => x.estado != "Cancelado" && x.PaqueteTuristicoId == null && x.agency.AgencyId == aAgency.FirstOrDefault().AgencyId && x.fecha >= initUtc && x.fecha < endUtc).ToListAsync();
                    //Hago una tabla para cada servicio
                    var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == agency.AgencyId);

                    decimal refTotalServicios = 0;
                    decimal refPagadoServicios = 0;
                    decimal refDeudaServicios = 0;
                    decimal refGastos = 0;
                    foreach (var tipo in tiposervicios)
                    {
                        var auxservicios = servicios.Where(x => x.tipoServicio == tipo).ToList();
                        if (tipo.Nombre.Equals("Gastos"))
                        {
                            refGastos += auxservicios.Sum(x => x.importeTotal);
                        }
                        else if (auxservicios.Count() != 0)
                        {
                            tblData = new PdfPTable(columnWidthsservicios);
                            tblData.WidthPercentage = 100;

                            doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo de Pago" }
                                .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            decimal refTotalServiciosTbl = 0;
                            decimal refPagadoServiciosTbl = 0;
                            decimal refDeudaServiciosTbl = 0;
                            foreach (Servicio servicio in auxservicios)
                            {
                                if (servicio.Products != null && servicio.Products.Count > 0)
                                {
                                    productosBodega.AddRange(servicio.Products.Where(x => x.Product != null).Select(x => (x.Cantidad, x.Product, (Guid)servicio.AgencyId, x.Price)));
                                }

                                decimal refTotal = servicio.importeTotal;
                                decimal refPagado = 0;
                                decimal refDeuda = 0;

                                List<string> pagos = new List<string>();
                                foreach (var item in servicio.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    addVentaTipoPago(item.tipoPago.Type, item.referecia, item.valorPagado);
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.referecia}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }
                                refDeuda = refTotal - refPagado;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;

                                refTotalServicios += refTotal;
                                refDeudaServicios += refDeuda;
                                refPagadoServicios += refPagado;

                                refTotalServiciosTbl += refTotal;
                                refDeudaServiciosTbl += refDeuda;
                                refPagadoServiciosTbl += refPagado;

                                var index = auxservicios.IndexOf(servicio);

                                AddValueToTable(tblData, servicio.numero, normalFont, index == 0 ? 1 : 0);

                                var valueCell = new List<(string, Font)>() { (servicio.cliente.Name + " " + servicio.cliente.LastName, normalFont),
                                    (servicio.cliente.Phone != null ? servicio.cliente.Phone.Number : "", normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                AddValueToTable(tblData, servicio.User != null ? $"{servicio.User.Name} {servicio.User.LastName}" : "", normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refPagado.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            // Añado el total

                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, refPagadoServiciosTbl.ToString(), headFont, 0);
                            AddValueToTable(tblData, refTotalServiciosTbl.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaServiciosTbl.ToString(), headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region MERCADO
                    var mercados = await _context.Mercado
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.Agency).ThenInclude(x => x.Phone)
                        .Include(x => x.Productos).ThenInclude(x => x.Product).ThenInclude(x => x.Categoria)
                        .Where(x => x.Status != Mercado.STATUS_CANCELADA && x.AgencyId == aAgency.FirstOrDefault().AgencyId
                        && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date)
                        .ToListAsync();

                    float[] columnWidthsmercado = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsmercado)
                    {
                        WidthPercentage = 100
                    };

                    decimal refTotalMercado = 0;
                    decimal refPagadoMercado = 0;
                    decimal refDeudaMercado = 0;

                    if (mercados.Count != 0)
                    {
                        doc.Add(new Phrase("MERCADO", headFont));

                        new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo de Pago" }
                            .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                        foreach (Mercado mercado in mercados)
                        {
                            productosBodega.AddRange(mercado.Productos.Where(x => x.Product != null).Select(x => (x.Cantidad, x.Product, mercado.AgencyId, x.Price)));

                            decimal refTotal = mercado.Amount;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;

                            List<string> pagos = new List<string>();
                            foreach (var item in mercado.RegistroPagos)
                            {
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refPagado += item.valorPagado;
                                addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                    pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                else
                                    pagos.Add(item.tipoPago.Type);
                            }
                            refDeuda = refTotal - refPagado;

                            grantotal += refTotal;
                            totalpagado += refPagado;
                            deudatotal += refDeuda;

                            refTotalMercado += refTotal;
                            refDeudaMercado += refDeuda;
                            refPagadoMercado += refPagado;

                            var index = mercados.IndexOf(mercado);

                            AddValueToTable(tblData, mercado.Number, normalFont, index == 0 ? 1 : 0);

                            var valueCell = new List<(string, Font)>() { (mercado.Client.Name + " " + mercado.Client.LastName, normalFont),
                            (mercado.Client.Phone != null ? mercado.Client.Phone.Number : "", normalFont)};
                            AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, mercado.User != null ? $"{mercado.User.Name} {mercado.User.LastName}" : "", normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refPagado.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                            AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                        }

                        AddValueToTable(tblData, "Totales", headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);
                        AddValueToTable(tblData, refPagadoMercado.ToString(), headFont, 0);
                        AddValueToTable(tblData, refTotalMercado.ToString(), headFont, 0);
                        AddValueToTable(tblData, refDeudaMercado.ToString(), headFont, 0);
                        AddValueToTable(tblData, string.Empty, headFont, 0);

                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    tblData = new PdfPTable(columnWidthstipopago);
                    tblData.WidthPercentage = 100;

                    AddValueToTable(tblData, "Tipo de Pago", headFont, 1);
                    AddValueToTable(tblData, "Cantidad", headFont, 1);
                    AddValueToTable(tblData, "Importe", headFont, 1);

                    foreach (var item in ventasTipoPago)
                    {
                        AddValueToTable(tblData, item.Key, normalFont, 0);
                        AddValueToTable(tblData, cantTipoPago[item.Key].ToString(), normalFont, 0);
                        AddValueToTable(tblData, item.Value.ToString(), normalFont, 0);
                    }

                    doc.Add(tblData);
                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 2 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    doc.Add(Chunk.NEWLINE);
                    cellleft.AddElement(new Phrase("RESUMEN DE TRÁMITES", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    //Remesas: 15 ordenes -- $360.00 usd
                    Phrase aux;
                    int auxcanttoal = 0;


                    int auxcant = remesas.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("REMESAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalRemesas + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = paquetesTuristicos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PAQUETES TURÍSTICOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalPaqueteTuristico + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosmaritimos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalMaritimo + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = pasaportes.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + refTotalPasaporte + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }


                    auxcant = envioscaribe.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioCaribe + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscubiq.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalCubiq + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosaereos.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioAereo + " usd " + $"({enviosaereos.Sum(x => x.CantLb + x.CantLbMedicina)} Lb)", normalFont));

                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscombos.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioCombo + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletos.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalBoletos + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletosCarrier.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalBoletosCarrier + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = recargas.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalRecarga + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    foreach (var item in tiposervicios)
                    {
                        var auxservicio = servicios.Where(x => x.tipoServicio == item).ToList();
                        auxcant = auxservicio.Count();
                        auxcanttoal += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase(item.Nombre.ToUpper() + ": ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxservicio.Sum(x => x.importeTotal).ToString() + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                    }

                    auxcant = mercados.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("Mercado: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalMercado + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);

                    cellleft.AddElement(new Phrase("Crédito de consumo", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    int auxcantTotalCredito = 0;

                    int auxcantCredito = enviosaereos.Where(x => x.credito > 0).Count();
                    auxcantTotalCredito += auxcantCredito;
                    if (auxcantCredito != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcantCredito + " Crédito de Consumo --$" + enviosaereos.Sum(x => x.credito) + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcantCredito = envioscombos.Where(x => x.credito > 0).Count();
                    auxcantTotalCredito += auxcantCredito;
                    if (auxcantCredito != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcantCredito + " Crédito de Consumo --$" + envioscombos.Sum(x => x.credito) + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcantCredito = boletos.Where(x => x.Credito > 0).Count();
                    auxcantTotalCredito += auxcantCredito;
                    if (auxcantCredito != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcantCredito + " Crédito --$" + boletos.Sum(x => x.Credito) + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Crédito de Consumo Total: ", headFont);
                    aux.AddSpecial(new Phrase(auxcantTotalCredito + " Crédito de Consumo", normalFont));
                    cellleft.AddElement(aux);
                    #endregion

                    #region CANCELADAS
                    var logs = await _context.Logs
                    .Include(x => x.Order)
                    .Include(x => x.OrderCubic)
                    .Include(x => x.Passport)
                    .Include(x => x.EnvioMaritimo)
                    .Include(x => x.Rechargue)
                    .Include(x => x.Remittance)
                    .Include(x => x.Reserva)
                    .Include(x => x.EnvioCaribe)
                    .Include(x => x.Servicio).ThenInclude(x => x.tipoServicio)
                    .Include(x => x.Mercado)
                    .Include(x => x.User)
                    .Where(x => x.AgencyId == agency.AgencyId && x.Date.Date >= dateIni && x.Date.Date <= dateFin && x.Event == LogEvent.Cancelar)
                    //.Where(x => x.Type == LogType.Orden || x.Type == LogType.Cubiq || x.Type == LogType.Pasaporte || x.Type == LogType.Recarga || x.Type == LogType.Reserva || x.Type == LogType.Servicio || x.Type == LogType.Remesa)
                    .ToListAsync();

                    decimal totalCancelaciones = logs.Sum(x => decimal.Parse(x.Precio));

                    doc.Add(Chunk.NEWLINE);
                    bool tieneCanceladas = false;
                    if (logs.Any())
                    {
                        tieneCanceladas = true;
                        float[] columnWidthscanceladas = { 1, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthscanceladas);
                        tblData.WidthPercentage = 100;

                        AddValueToTable(tblData, "No. Orden", headFont, 1);
                        AddValueToTable(tblData, "Fecha", headFont, 1);
                        AddValueToTable(tblData, "Tipo Trámite", headFont, 1);
                        AddValueToTable(tblData, "Empleado", headFont, 1);
                        AddValueToTable(tblData, "Monto", headFont, 1);
                        AddCancelationTable(normalFont, logs, tblData);
                    }

                    #endregion

                    #region //GRAN TOTAL
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph total = new Paragraph("Total: ", headFont2);
                    total.AddSpecial(new Phrase("$ " + grantotal.ToString("0.00"), normalFont2));
                    total.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(total);
                    Paragraph porpagar = new Paragraph("Pagado: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalpagado.ToString("0.00"), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);

                    if (refGastos > 0)
                    {
                        Paragraph pgastos = new Paragraph("Gastos: ", headFont2);
                        pgastos.AddSpecial(new Phrase("$ " + refGastos.ToString("0.00"), normalFont2));
                        pgastos.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(pgastos);
                        totalpagado -= refGastos;
                        porpagar = new Paragraph("Total Pagado: ", headFont2);
                        porpagar.AddSpecial(new Phrase("$ " + totalpagado.ToString("0.00"), normalFont2));
                        porpagar.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(porpagar);
                    }

                    var creditoTotal = refCreditoBoletos + refCreditoEnvioAereo;
                    if (creditoTotal > 0)
                    {
                        Paragraph credito = new Paragraph("Crédito: ", headFont2);
                        credito.AddSpecial(new Phrase("$ " + creditoTotal.ToString("0.00"), normalFont2));
                        credito.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(credito);
                    }

                    Paragraph deuda = new Paragraph("Pendiente a pago: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + deudatotal.ToString("0.00"), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);

                    Paragraph cancelaciones = new Paragraph("Cancelaciones: ", headFont2);
                    cancelaciones.AddSpecial(new Phrase("-- $ " + totalCancelaciones.ToString("0.00"), normalFont2));
                    cancelaciones.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(cancelaciones);
                    cellright.AddElement(Chunk.NEWLINE);

                    #endregion

                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);
                    if (tieneCanceladas)
                    {
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(new Phrase("CANCELACIONES", underLineFont));
                        doc.Add(tblData);
                    }

                    // reporte de productos Adrian May Scooter
                    if (agency.AgencyId == AgencyName.AdrianMyScooterFlagler)
                    {
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(new Phrase("PRODUCTOS", underLineFont));
                        doc.Add(Chunk.NEWLINE);
                        BuildTableProduct(doc, headFont, normalFont, productosBodega, servicios);
                    }

                    doc.Close();
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        private void BuildTableProduct(Document doc, Font headFont, Font normalFont, IEnumerable<(int, ProductoBodega, Guid, decimal)> productos, List<Servicio> servicios)
        {
            // producto, bodega, cantidad, disponibilidad, venta, costo, precio-inventario
            List<ItemProductReportSale> reportByBodega = new List<ItemProductReportSale>();

            Guid bodegaAlmacen = Guid.Parse("4a1851a0-cfd3-4529-bec0-f9a71f6561e0");
            var disponibilidadAlmacen = _context.BodegaProductos
                .Include(x => x.Bodega)
                .Include(x => x.Producto).ThenInclude(x => x.Categoria)
                .Where(x => x.Bodega.Id == bodegaAlmacen);

            foreach (var item in disponibilidadAlmacen.GroupBy(x => x.IdProducto))
            {
                var bodega = item.First().Bodega;
                var product = item.First().Producto;
                reportByBodega.Add(new ItemProductReportSale
                {
                    AgencyId = bodega.idAgency,
                    Name = product.Nombre,
                    Bodega = bodega.Nombre,
                    Availability = item.Sum(x => (int)x.Cantidad),
                    QtySale = 0,
                    Category = product.Categoria.Nombre,
                    Cost = product.PrecioCompraReferencial ?? 0,
                    PriceRef = product.PrecioVentaReferencial ?? 0,
                    SumSale = decimal.Zero,
                    ProductId = product.IdProducto
                });
            }

            foreach (var item in productos.GroupBy(x => new { x.Item2.IdProducto, x.Item3 }))
            {
                var product = item.First().Item2;
                var bodegaProducto = _context.BodegaProductos.Include(x => x.Bodega).Where(x => x.IdProducto == product.IdProducto && x.Bodega.idAgency == item.Key.Item3 && x.Bodega.Id != bodegaAlmacen);
                foreach (var bodega in bodegaProducto)
                {
                    string nombre = product.Nombre;
                    string bodegaName = bodega?.Bodega?.Nombre ?? "";
                    int cantidad = item.Sum(x => x.Item1); // Si es almacen no se cuenta la cantidad
                    int disponibilidad = bodega?.Cantidad != null ? (int)bodega.Cantidad : 0;
                    decimal venta = item.Sum(x => x.Item1 * x.Item4);

                    var exist = reportByBodega.FirstOrDefault(x => x.Name == nombre && x.Bodega == bodegaName && x.AgencyId == item.Key.Item3);
                    if (exist != default)
                    {
                        exist.QtySale += cantidad;
                        exist.SumSale += venta;
                    }
                    else reportByBodega.Add(new ItemProductReportSale
                    {
                        AgencyId = item.Key.Item3,
                        Name = nombre,
                        Bodega = bodegaName,
                        Availability = disponibilidad,
                        QtySale = cantidad,
                        Category = product.Categoria.Nombre,
                        Cost = product.PrecioCompraReferencial ?? 0,
                        PriceRef = product.PrecioVentaReferencial ?? 0,
                        SumSale = venta,
                        ProductId = product.IdProducto
                    });
                }
            }

            foreach (var byAgency in reportByBodega.GroupBy(x => x.AgencyId))
            {
                var agency = _context.Agency.Find(byAgency.Key);
                var motos = byAgency.Where(x => x.Category == "Motos");
                var piezas = byAgency.Where(x => x.Category != "Motos");
                var serviciosAux = servicios.Where(x => x.AgencyId == byAgency.Key);

                doc.Add(Chunk.NEWLINE);
                doc.Add(new Phrase(agency.Name.ToUpper(), headFont));
                foreach (var byBodega in motos.GroupBy(x => x.Bodega))
                {
                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase(byBodega.Key.ToUpperCase(), headFont));

                    float[] columnWidthsProductos = { 2, 1, 1, 1, 1, 1 };
                    var tblData = new PdfPTable(columnWidthsProductos);
                    tblData.WidthPercentage = 100;

                    AddValueToTable(tblData, "Moto", headFont, 1);
                    AddValueToTable(tblData, "Vendido", headFont, 1);
                    AddValueToTable(tblData, "Disponibilidad", headFont, 1);
                    AddValueToTable(tblData, "Costo", headFont, 1);
                    AddValueToTable(tblData, "Precio Venta Referido", headFont, 1);
                    AddValueToTable(tblData, "Sumatoria Venta Real", headFont, 1);

                    foreach (var moto in byBodega)
                    {
                        AddValueToTable(tblData, moto.Name, normalFont, 1);
                        AddValueToTable(tblData, moto.QtySale.ToString(), normalFont, 1);
                        AddValueToTable(tblData, moto.Availability.ToString(), normalFont, 1);
                        AddValueToTable(tblData, moto.Cost.ToString(), normalFont, 1);
                        AddValueToTable(tblData, moto.PriceRef.ToString(), normalFont, 1);
                        AddValueToTable(tblData, moto.SumSale.ToString(), normalFont, 1);
                    }

                    AddValueToTable(tblData, "Totales", headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.QtySale).ToString(), headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.Availability).ToString(), headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.Cost).ToString(), headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.PriceRef).ToString(), headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.SumSale).ToString(), headFont, 1);

                    doc.Add(tblData);
                }

                if (piezas.Any() || serviciosAux.Any())
                {
                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase("PIEZAS Y SERVICIOS", headFont));

                    float[] columnWidthsProductos = { 2, 1, 1, 1, 1, 1 };
                    var tblData = new PdfPTable(columnWidthsProductos);
                    tblData.WidthPercentage = 100;

                    AddValueToTable(tblData, "Producto", headFont, 1);
                    AddValueToTable(tblData, "Vendido", headFont, 1);
                    AddValueToTable(tblData, "Disponibilidad", headFont, 1);
                    AddValueToTable(tblData, "Costo", headFont, 1);
                    AddValueToTable(tblData, "Precio Venta Referido", headFont, 1);
                    AddValueToTable(tblData, "Sumatoria Venta Real", headFont, 1);

                    int totalQtySale = 0;
                    int totalAvailability = 0;
                    decimal totalCost = 0;
                    decimal totalPriceRef = 0;
                    decimal totalSumSale = 0;
                    foreach (var pieza in piezas)
                    {
                        totalQtySale += pieza.QtySale;
                        totalAvailability += pieza.Availability;
                        totalCost += pieza.Cost;
                        totalPriceRef += pieza.PriceRef;
                        totalSumSale += pieza.SumSale;

                        AddValueToTable(tblData, pieza.Name, normalFont, 1);
                        AddValueToTable(tblData, pieza.QtySale.ToString(), normalFont, 1);
                        AddValueToTable(tblData, pieza.Availability.ToString(), normalFont, 1);
                        AddValueToTable(tblData, pieza.Cost.ToString(), normalFont, 1);
                        AddValueToTable(tblData, pieza.PriceRef.ToString(), normalFont, 1);
                        AddValueToTable(tblData, pieza.SumSale.ToString(), normalFont, 1);
                    }

                    // agregar servicios a tabla de piezas
                    foreach (var servicio in serviciosAux.GroupBy(x => x.tipoServicio))
                    {
                        totalQtySale += servicio.Count();
                        totalCost += 1;
                        totalPriceRef += servicio.Key.Price;
                        totalSumSale += servicio.Sum(x => x.importeTotal);

                        AddValueToTable(tblData, servicio.Key.Nombre, normalFont, 1);
                        AddValueToTable(tblData, servicio.Count().ToString(), normalFont, 1);
                        AddValueToTable(tblData, "", normalFont, 1);
                        AddValueToTable(tblData, "1", normalFont, 1);
                        AddValueToTable(tblData, servicio.Key.Price.ToString(), normalFont, 1);
                        AddValueToTable(tblData, servicio.Sum(x => x.importeTotal).ToString(), normalFont, 1);
                    }

                    AddValueToTable(tblData, "Totales", headFont, 1);
                    AddValueToTable(tblData, totalQtySale.ToString(), headFont, 1);
                    AddValueToTable(tblData, totalAvailability.ToString(), headFont, 1);
                    AddValueToTable(tblData, totalCost.ToString(), headFont, 1);
                    AddValueToTable(tblData, totalPriceRef.ToString(), headFont, 1);
                    AddValueToTable(tblData, totalSumSale.ToString(), headFont, 1);

                    doc.Add(tblData);
                }
            }
        }

        private static void AddCancelationTable(Font normalFont, List<Log> logs, PdfPTable tblremesasData)
        {
            foreach (var item in logs)
            {
                string number = "";
                decimal? amount = 0;
                string serviceName = "";
                DateTime? date = null;
                switch (item.Type)
                {
                    case LogType.Reserva:
                        number = item.Reserva?.ReservationNumber;
                        serviceName = "Reserva";
                        date = item.Reserva?.RegisterDate;
                        amount = item.Reserva?.Total;
                        break;
                    case LogType.Mercado:
                        number = item.Mercado?.Number;
                        serviceName = "Mercado";
                        date = item.Mercado?.Date;
                        amount = item.Mercado?.Amount;
                        break;
                    case LogType.Combo:
                        number = item.Order?.Number;
                        serviceName = "Combo";
                        date = item.Order?.Date;
                        amount = item.Order?.Amount;
                        break;
                    case LogType.EnvioCaribe:
                        number = item.EnvioCaribe?.Number;
                        serviceName = "Envio Caribe";
                        date = item.EnvioCaribe?.Date;
                        amount = item.EnvioCaribe?.Amount;
                        break;
                    case LogType.Cubiq:
                        number = item.OrderCubic?.Number;
                        serviceName = "Carga AM";
                        date = item.OrderCubic?.Date;
                        amount = item.OrderCubic?.Amount;
                        break;
                    case LogType.EnvioMaritimo:
                        number = item.EnvioMaritimo?.Number;
                        serviceName = "Marítimo";
                        date = item.EnvioMaritimo?.Date;
                        amount = item.EnvioMaritimo?.Amount;
                        break;
                    case LogType.Orden:
                        number = item.Order?.Number;
                        serviceName = "Order";
                        date = item.Order?.Date;
                        amount = item.Order?.Amount;
                        break;
                    case LogType.Pasaporte:
                        number = item.Passport?.OrderNumber;
                        serviceName = "Pasaporte";
                        date = item.Passport?.FechaSolicitud;
                        amount = item.Passport?.Total;
                        break;
                    case LogType.Recarga:
                        number = item.Rechargue?.Number;
                        serviceName = "Recarga";
                        date = item.Rechargue?.date;
                        amount = item.Rechargue?.Import;
                        break;
                    case LogType.Remesa:
                        number = item.Remittance?.Number;
                        serviceName = "Remesa";
                        date = item.Remittance?.Date;
                        amount = item.Remittance?.Amount;
                        break;
                    case LogType.Servicio:
                        number = item.Servicio?.numero;
                        serviceName = item.Servicio?.tipoServicio?.Nombre;
                        date = item.Servicio?.fecha;
                        amount = item.Servicio?.importeTotal;
                        break;
                    default:
                        break;
                }
                var cellremesas1 = new PdfPCell();
                cellremesas1.Border = 1;
                var cellremesas2 = new PdfPCell();
                cellremesas2.Border = 1;
                var cellremesas3 = new PdfPCell();
                cellremesas3.Border = 1;
                var cellremesas4 = new PdfPCell();
                cellremesas4.Border = 1;
                var cellremesas5 = new PdfPCell();
                cellremesas5.Border = 1;

                cellremesas1.AddElement(new Phrase(number, normalFont));
                cellremesas2.AddElement(new Phrase(date != null ? ((DateTime)date).ToShortDateString() : "", normalFont));
                cellremesas3.AddElement(new Phrase(serviceName, normalFont));
                cellremesas4.AddElement(new Phrase(item.User?.FullName, normalFont));
                cellremesas5.AddElement(new Phrase(((decimal)amount).ToString("0.00"), normalFont));
                tblremesasData.AddCell(cellremesas1);
                tblremesasData.AddCell(cellremesas2);
                tblremesasData.AddCell(cellremesas3);
                tblremesasData.AddCell(cellremesas4);
                tblremesasData.AddCell(cellremesas5);
            }
        }

        private class AuxReportTipoPagoDCuba
        {
            public TipoPago TipoPago { get; set; }
            public string ServiceType { get; set; }
            public int Cantidad { get; set; }
            public decimal Venta { get; set; }
            public decimal Costo { get; set; }
            public decimal Utilidad { get; set; }
        }

        [Authorize]
        public async Task<object> ExportUtilidadRapid(string strdate, bool onlyClientsAgency = false)
        {
            try
            {
                Serilog.Log.Information("Reporte de utilidad rapid");

                User user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
                return await Reporte.GetReporteUtilidadRapid(strdate, user, _context, _env, _reportService, onlyClientsAgency);
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return "";
            }

        }

        [Authorize]
        public async Task<object> ExportUtilidad(string strdate, bool onlyClientsAgency = false)
        {
            try
            {
                User user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
                return await Reporte.GetReporteUtilidad(strdate, user, _context, _env, _reportService, onlyClientsAgency);
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return "";
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> UtilityDCubaByNumber(string number1, string number2, bool onlyClientsAgency = false)
        {
            var role = AgenciaAuthorize.getRole(User, _context);
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId);

            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.SetPageSize(PageSize.A4);
                doc.Open();
                try
                {

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font fonttransferida = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.ITALIC, BaseColor.RED);

                    Agency agency = aAgency.FirstOrDefault();
                    Models.Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    var tiposPago = _context.TipoPago.ToList();
                    tiposPago.Add(new TipoPago
                    {
                        TipoPagoId = Guid.Empty,
                        Type = "Pendiente"
                    });

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        celllogo.AddElement(imagelogo);
                    }


                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = $"DC{number1} a DC{number2}";

                    Paragraph parPaq = new Paragraph("Utilidad por servicio del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);
                    /*
                     * 0: cash
                     * 1: zelle
                     * 2: cheque
                     * 3: credito o debito
                     * 4: Transferencia Bancaria
                     * 5: Web
                     * 6: Money Order
                     * */
                    decimal[] ventastipopago = new decimal[7];
                    int[] canttipopago = new int[7];
                    decimal totalcosto = 0;
                    decimal totalprecio = 0;
                    decimal totalunitario = 0;

                    #region // ENVIOS PASAPORTE
                    float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    var passports = (await _reportService.PassportUtilityByOrderNumber(agency.AgencyId, number1, number2)).Value;

                    decimal utilidadPassport = 0;
                    decimal costoConsular = 0; //Para districtCuba
                    List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();

                    if (onlyClientsAgency)
                        passports = passports.Where(x => !x.ByTransferencia).ToList();

                    if (passports.Any())
                    {

                        PdfPTable tblremesasData = new PdfPTable(columnWidthspasaporte);
                        tblremesasData.WidthPercentage = 100;
                        PdfPCell cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        PdfPCell cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        PdfPCell cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        PdfPCell cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        PdfPCell cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        PdfPCell cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        PdfPCell cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        PdfPCell cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("S. Consular", headFont));
                        cellremesas5.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas6.AddElement(new Phrase("Costo Consular", headFont));
                        cellremesas7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipos Pagos", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        foreach (var pasaporte in passports)
                        {

                            string pagos = "";
                            if (!pasaporte.ByTransferencia)
                                foreach (var item in pasaporte.Pays)
                                {
                                    pagos += item.TipoPago + ", ";
                                }

                            var index = passports.IndexOf(pasaporte);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }
                            if (pasaporte.ByTransferencia)
                            {
                                cellremesas1.AddElement(new Phrase("T. " + pasaporte.TransferredAgencyName, fonttransferida));
                            }
                            else
                            {
                                cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                            }
                            cellremesas2.AddElement(new Phrase(pasaporte.Client.FullName, normalFont));
                            cellremesas2.AddElement(new Phrase(pasaporte.Client.PhoneNumber, normalFont));
                            cellremesas3.AddElement(new Phrase(pasaporte.Employee.FullName, normalFont));
                            string servConsular = "";
                            switch (pasaporte.ServicioConsular)
                            {
                                case ServicioConsular.None:
                                    break;
                                case ServicioConsular.PrimerVez:
                                case ServicioConsular.PrimerVez2:
                                    servConsular = "1 Vez";
                                    break;
                                case ServicioConsular.Prorroga1:
                                    servConsular = "Prorro1";
                                    break;
                                case ServicioConsular.Prorroga2:
                                    servConsular = "Prorro2";
                                    break;
                                case ServicioConsular.Renovacion:
                                    servConsular = pasaporte.ServicioConsular.GetDescription();
                                    break;
                                case ServicioConsular.HE11:
                                    servConsular = pasaporte.ServicioConsular.GetDescription();
                                    break;
                                default:
                                    servConsular = pasaporte.ServicioConsular.GetDescription();
                                    break;
                            }
                            cellremesas4.AddElement(new Phrase(servConsular, normalFont));
                            cellremesas5.AddElement(new Phrase(pasaporte.SalePrice.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(pasaporte.Cost.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(pasaporte.Utility.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pasaporte.ByTransferencia ? "Pendiente" : pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        decimal reftotalprecios = passports.Sum(x => x.SalePrice);
                        decimal reftotalcosto = passports.Sum(x => x.Cost);
                        decimal reftotalutilidad = passports.Sum(x => x.Utility);
                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase("", headFont));
                        cellremesas5.AddElement(new Phrase(reftotalprecios.ToString("0.00"), headFont));
                        cellremesas6.AddElement(new Phrase(reftotalcosto.ToString("0.00"), headFont));
                        cellremesas7.AddElement(new Phrase(reftotalutilidad.ToString("0.00"), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        utilidadPassport += reftotalutilidad;
                        totalunitario += reftotalutilidad;
                        totalcosto += reftotalcosto;
                        costoConsular += reftotalcosto;
                        totalprecio += reftotalprecios;
                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);

                        var passportDCuba = passports.OrderBy(x => x.OrderNumber).GroupBy(x => x.ServicioConsular);

                        foreach (var PassportServConsular in passportDCuba)
                        {
                            foreach (var item in tiposPago)
                            {
                                var items = PassportServConsular.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                                if (items.Any())
                                {
                                    tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                    {
                                        TipoPago = item,
                                        ServiceType = PassportServConsular.Key.GetDescription(),
                                        Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                        Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                        Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                        Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                    });

                                }
                            }
                        }
                    }

                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 2 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    doc.Add(Chunk.NEWLINE);
                    cellleft.AddElement(new Phrase("RESUMEN DE TRÁMITES", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    //Remesas: 15 ordenes -- $360.00 usd
                    Phrase aux;
                    int auxcant = passports.Count();
                    int auxcanttoal = auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + utilidadPassport + " usd", normalFont));
                        cellleft.AddElement(aux);
                        if (agency.AgencyId == AgencyName.DCubaWashington)
                        {
                            aux = new Phrase("COSTO CONSULAR: ", headFontRed);
                            aux.AddSpecial(new Phrase($"${costoConsular} usd", normalFontRed));
                            cellleft.AddElement(aux);
                        }

                    }

                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);
                    #endregion

                    #region //GRAN TOTAL
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph total = new Paragraph("Total P. Venta: ", headFont2);
                    total.AddSpecial(new Phrase("$ " + totalprecio.ToString(), normalFont2));
                    total.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(total);
                    Paragraph porpagar = new Paragraph("Total Costo: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalcosto.ToString(), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);
                    Paragraph deuda = new Paragraph("Total Utilidad: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + totalunitario.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);
                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    if (AgencyName.IsDistrictCuba(agency.AgencyId))
                    {
                        doc.Add(new Phrase("REPORTE GENERAL TIPO PAGO", underLineFont));
                        float[] columnWidth = { 3, 2, 2, 2, 2, 2 };
                        PdfPTable tbl = new PdfPTable(columnWidth);
                        tbl.WidthPercentage = 100;

                        PdfPCell cell = new PdfPCell(new Phrase("Tipo de Pago", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Tramite", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Cantidad", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Venta", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Costo", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Utilidad", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        bool verifyCash = tipoPagoDCuba.Any(x => x.TipoPago.Type == "Cash");

                        foreach (var tipoPago in tipoPagoDCuba.Where(x => x.TipoPago.Type != "Money Order").GroupBy(x => x.TipoPago))
                        {
                            foreach (var item in tipoPago)
                            {
                                cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                            }
                            List<AuxReportTipoPagoDCuba> moneyOrder = new List<AuxReportTipoPagoDCuba>();
                            if (tipoPago.Key.Type == "Cash")
                            {
                                moneyOrder = tipoPagoDCuba.Where(x => x.TipoPago.Type == "Money Order").ToList();
                                foreach (var item in moneyOrder)
                                {
                                    cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                }
                            }
                            cell = new PdfPCell(new Phrase(tipoPago.Key.Type, headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Totales", headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Cantidad) + moneyOrder.Sum(x => x.Cantidad)).ToString(), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Venta) + moneyOrder.Sum(x => x.Venta)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Costo) + moneyOrder.Sum(x => x.Costo)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Utilidad) + moneyOrder.Sum(x => x.Utilidad)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                        }
                        if (!verifyCash)
                        {
                            var moneyOrder = tipoPagoDCuba.Where(x => x.TipoPago.Type == "Money Order").ToList();
                            foreach (var item in moneyOrder)
                            {
                                cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                            }
                            cell = new PdfPCell(new Phrase("Money Order", headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Totales", headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Cantidad)).ToString(), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Venta)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Costo)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Utilidad)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                        }


                        cell = new PdfPCell(new Phrase("", normalFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Gran Total", headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase(tipoPagoDCuba.Sum(x => x.Cantidad).ToString(), headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase(tipoPagoDCuba.Sum(x => x.Venta).ToString("0.00"), headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase(tipoPagoDCuba.Sum(x => x.Costo).ToString("0.00"), headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase(tipoPagoDCuba.Sum(x => x.Utilidad).ToString("0.00"), headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);

                        doc.Add(tbl);
                    }
                    doc.Close();
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Json(new { success = true, data = Convert.ToBase64String(MStream.ToArray()) });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> UtilityOtrosServiciosByNumber(string number1, string number2)
        {
            var role = AgenciaAuthorize.getRole(User, _context);
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId);

            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.SetPageSize(PageSize.A4);
                doc.Open();
                try
                {

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font fonttransferida = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.ITALIC, BaseColor.RED);

                    Agency agency = aAgency.FirstOrDefault();
                    Models.Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        celllogo.AddElement(imagelogo);
                    }


                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = $"{number1} a {number2}";

                    Paragraph parPaq = new Paragraph("Utilidad por servicio del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);
                    /*
                     * 0: cash
                     * 1: zelle
                     * 2: cheque
                     * 3: credito o debito
                     * 4: Transferencia Bancaria
                     * 5: Web
                     * 6: Money Order
                     * */
                    decimal[] ventastipopago = new decimal[7];
                    int[] canttipopago = new int[7];
                    decimal totalcosto = 0;
                    decimal totalprecio = 0;
                    decimal totalunitario = 0;
                    List<AuxReportTipoPagoDCuba> reporteTipoPago = new List<AuxReportTipoPagoDCuba>();
                    #region // OTROS SERVICIOS
                    float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    var servicios = (await _reportService.ServicioUtilityByOrderNumber(agency.AgencyId, number1, number2)).Value;

                    if (servicios.Any())
                    {
                        foreach (var tipoServicios in servicios.GroupBy(x => x.TipoServicio))
                        {
                            doc.Add(new Phrase(tipoServicios.Key.ToUpper(), headFont));

                            PdfPTable tblremesasData = new PdfPTable(columnWidthspasaporte);
                            tblremesasData.WidthPercentage = 100;
                            PdfPCell cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            PdfPCell cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            PdfPCell cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            PdfPCell cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            PdfPCell cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            PdfPCell cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            PdfPCell cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                            cellremesas5.AddElement(new Phrase("Costo", headFont));
                            cellremesas6.AddElement(new Phrase("Utilidad", headFont));
                            cellremesas7.AddElement(new Phrase("Tipos Pagos", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);

                            foreach (var servicio in tipoServicios)
                            {
                                string pagos = "";
                                foreach (var item in servicio.Pays)
                                {
                                    pagos += item.TipoPago + ", ";
                                }

                                var index = tipoServicios.ToList().IndexOf(servicio);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                }
                                cellremesas1.AddElement(new Phrase(servicio.OrderNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(servicio.Client.FullName, normalFont));
                                cellremesas2.AddElement(new Phrase(servicio.Client.PhoneNumber, normalFont));
                                cellremesas3.AddElement(new Phrase(servicio.Employee.FullName, normalFont));
                                cellremesas4.AddElement(new Phrase(servicio.SalePrice.ToString(), normalFont));
                                cellremesas5.AddElement(new Phrase(servicio.Cost.ToString(), normalFont));
                                cellremesas6.AddElement(new Phrase(servicio.Utility.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;

                            decimal reftotalprecios = tipoServicios.Sum(x => x.SalePrice);
                            decimal reftotalcosto = tipoServicios.Sum(x => x.Cost - x.CServicio);
                            decimal reftotalutilidad = tipoServicios.Sum(x => x.Utility);
                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(reftotalprecios.ToString("0.00"), headFont));
                            cellremesas5.AddElement(new Phrase(reftotalcosto.ToString("0.00"), headFont));
                            cellremesas6.AddElement(new Phrase(reftotalutilidad.ToString("0.00"), headFont));
                            cellremesas7.AddElement(new Phrase("", normalFont));
                            totalunitario += reftotalutilidad;
                            totalcosto += reftotalcosto;
                            totalprecio += reftotalprecios;
                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);

                            foreach (var item in _context.TipoPago)
                            {
                                var items = tipoServicios.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                                if (items.Any())
                                {
                                    reporteTipoPago.Add(new AuxReportTipoPagoDCuba
                                    {
                                        TipoPago = item,
                                        ServiceType = tipoServicios.Key,
                                        Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                        Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                        Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                        Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                    });

                                }
                            }
                        }

                    }

                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 2 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    doc.Add(Chunk.NEWLINE);
                    cellleft.AddElement(new Phrase("RESUMEN DE TRÁMITES", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    //Remesas: 15 ordenes -- $360.00 usd
                    Phrase aux;
                    int auxcant = 0;
                    int auxcanttoal = 0;
                    foreach (var tipoServicio in servicios.GroupBy(x => x.TipoServicio))
                    {
                        auxcant = tipoServicio.Count();
                        auxcanttoal += auxcant;
                        if (auxcant != 0)
                        {
                            aux = new Phrase($"{tipoServicio.Key.ToUpper()} ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + tipoServicio.Sum(x => x.Utility) + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                    }

                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);
                    #endregion

                    #region //GRAN TOTAL
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph total = new Paragraph("Total P. Venta: ", headFont2);
                    total.AddSpecial(new Phrase("$ " + totalprecio.ToString(), normalFont2));
                    total.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(total);
                    Paragraph porpagar = new Paragraph("Total Costo: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalcosto.ToString(), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);
                    Paragraph deuda = new Paragraph("Total Utilidad: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + totalunitario.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);
                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);
                    if (AgencyName.IsDistrictCuba(agency.AgencyId))
                    {
                        doc.Add(new Phrase("REPORTE GENERAL TIPO PAGO", underLineFont));
                        float[] columnWidth = { 3, 2, 2, 2, 2, 2 };
                        PdfPTable tbl = new PdfPTable(columnWidth);
                        tbl.WidthPercentage = 100;

                        PdfPCell cell = new PdfPCell(new Phrase("Tipo de Pago", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Tramite", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Cantidad", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Venta", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Costo", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Utilidad", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        tbl.AddCell(cell);
                        bool verifyCash = reporteTipoPago.Any(x => x.TipoPago.Type == "Cash");

                        foreach (var tipoPago in reporteTipoPago.Where(x => x.TipoPago.Type != "Money Order").GroupBy(x => x.TipoPago))
                        {
                            foreach (var item in tipoPago)
                            {
                                cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                            }
                            List<AuxReportTipoPagoDCuba> moneyOrder = new List<AuxReportTipoPagoDCuba>();
                            if (tipoPago.Key.Type == "Cash")
                            {
                                moneyOrder = reporteTipoPago.Where(x => x.TipoPago.Type == "Money Order").ToList();
                                foreach (var item in moneyOrder)
                                {
                                    cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                }
                            }
                            cell = new PdfPCell(new Phrase(tipoPago.Key.Type, headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Totales", headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Cantidad) + moneyOrder.Sum(x => x.Cantidad)).ToString(), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Venta) + moneyOrder.Sum(x => x.Venta)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Costo) + moneyOrder.Sum(x => x.Costo)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Utilidad) + moneyOrder.Sum(x => x.Utilidad)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                        }
                        if (!verifyCash)
                        {
                            var moneyOrder = reporteTipoPago.Where(x => x.TipoPago.Type == "Money Order").ToList();
                            foreach (var item in moneyOrder)
                            {
                                cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                            }
                            cell = new PdfPCell(new Phrase("Money Order", headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Totales", headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Cantidad)).ToString(), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Venta)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Costo)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Utilidad)).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.NO_BORDER;
                            tbl.AddCell(cell);
                        }


                        cell = new PdfPCell(new Phrase("", normalFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Gran Total", headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase(reporteTipoPago.Sum(x => x.Cantidad).ToString(), headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase(reporteTipoPago.Sum(x => x.Venta).ToString("0.00"), headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase(reporteTipoPago.Sum(x => x.Costo).ToString("0.00"), headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);
                        cell = new PdfPCell(new Phrase(reporteTipoPago.Sum(x => x.Utilidad).ToString("0.00"), headFont));
                        cell.Border = PdfPCell.TOP_BORDER;
                        tbl.AddCell(cell);

                        doc.Add(tbl);
                    }
                    doc.Close();
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                    return Json(new { success = false, msg = "No se ha podido crear el reporte" });
                }
                return Json(new { success = true, data = Convert.ToBase64String(MStream.ToArray()) });
            }

        }

        [Authorize]
        public async Task<IActionResult> ExportUtilidadExcel(string strdate, bool onlyClientsAgency = false)
        {
            var role = AgenciaAuthorize.getRole(User, _context);
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = await _context.Agency.FirstOrDefaultAsync(x => x.AgencyId == aUser.FirstOrDefault().AgencyId);

            var auxDate = strdate.Split('-');
            CultureInfo culture = new CultureInfo("es-US", true);
            var dateIni = DateTime.Parse(auxDate[0], culture);
            var dateFin = DateTime.Parse(auxDate[1], culture);

            var data = (await _reportService.GetAllUtility(aAgency.AgencyId, dateIni, dateFin)).Value;

            if (data.Count() == 0)
                return BadRequest("No hay elementos a mostrar");

            var byService = data.GroupBy(x => x.Service);

            byte[] fileContents = null;
            using (var package = new ExcelPackage())
            {
                foreach (var service in byService)
                {
                    string serviceName = "";
                    switch (service.Key)
                    {
                        case STipo.Remesa:
                            serviceName = "Remesas";
                            break;
                        case STipo.PTuristico:
                            serviceName = "Paquete Turistico";
                            break;
                        case STipo.Recarga:
                            serviceName = "Recargas";
                            break;
                        case STipo.Servicio:
                            serviceName = "Otros Servicios";
                            break;
                        case STipo.Passport:
                            serviceName = "Pasaporte";
                            break;
                        case STipo.EnvioCaribe:
                            serviceName = "Envíos Caribe";
                            break;
                        case STipo.Paquete:
                            serviceName = "Evíos Aéreos";
                            break;
                        case STipo.Maritimo:
                            serviceName = "Envíos Marítimos";
                            break;
                        case STipo.Reserva:
                            serviceName = "Reservas";
                            break;
                        case STipo.Combo:
                            serviceName = "Combos";
                            break;
                        case STipo.Cubiq:
                            serviceName = "Carga AM";
                            break;
                        case STipo.Mercado:
                            serviceName = "Mercado";
                            break;
                        default:
                            serviceName = "Otros";
                            break;
                    }

                    if (service.Key == STipo.Servicio)
                    {
                        var otherServices = service.GroupBy(x => x.TipoServicio);
                        foreach (var otherService in otherServices)
                        {
                            AuxUtilityExcel(otherService.ToList(), otherService.Key, package);
                        }
                    }
                    else if (service.Key == STipo.Reserva)
                    {
                        var carriers = service.Where(x => x.IsCarrier).ToList();
                        if (carriers.Any())
                        {
                            AuxUtilityExcel(carriers.ToList(), "Reserva - Carrier", package);
                        }
                        var pasaje = service.Where(x => !x.IsCarrier && x.TipoServicio == "pasaje").ToList();
                        var auto = service.Where(x => !x.IsCarrier && x.TipoServicio == "auto").ToList();
                        var hotel = service.Where(x => !x.IsCarrier && x.TipoServicio == "hotel").ToList();
                        if (pasaje.Any())
                        {
                            AuxUtilityExcel(pasaje.ToList(), "Reserva - Pasaje", package);
                        }
                        if (auto.Any())
                        {
                            AuxUtilityExcel(auto.ToList(), "Reserva - Auto", package);
                        }
                        if (hotel.Any())
                        {
                            AuxUtilityExcel(hotel.ToList(), "Reserva - Hotel", package);
                        }
                    }
                    else if (service.Key != STipo.Servicio)
                    {
                        if (service.Key == STipo.Passport && onlyClientsAgency)
                            AuxUtilityExcel(service.Where(x => !x.ByTransferencia).ToList(), serviceName, package);
                        else
                            AuxUtilityExcel(service.ToList(), serviceName, package);
                    }
                }
                fileContents = package.GetAsByteArray();
            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: "Utilidad por servicio.xlsx"
            );
        }

        [Authorize]
        public async Task<IActionResult> ExportUtilidadExcelMPS(string strdate, bool isDC)
        {
            var role = AgenciaAuthorize.getRole(User, _context);
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = await _context.Agency.FirstOrDefaultAsync(x => x.AgencyId == aUser.FirstOrDefault().AgencyId);

            var auxDate = strdate.Split('-');
            CultureInfo culture = new CultureInfo("es-US", true);
            var dateIni = DateTime.Parse(auxDate[0], culture);
            var dateFin = DateTime.Parse(auxDate[1], culture);

            Dictionary<string, List<UtilityModel>> data = new Dictionary<string, List<UtilityModel>>();
            if (isDC)
            {
                data["District Cuba Dallas"] = (await _reportService.GetAllUtility(AgencyName.DCubaDallas, dateIni, dateFin)).Value;
                data["District Cuba Houston"] = (await _reportService.GetAllUtility(AgencyName.DCubaHouston, dateIni, dateFin)).Value;
                data["District Cuba Miami"] = (await _reportService.GetAllUtility(AgencyName.DCubaMiami, dateIni, dateFin)).Value;
            }
            else
            {
                data["Miami Plus Service"] = (await _reportService.GetAllUtility(aAgency.AgencyId, dateIni, dateFin)).Value.Where(x => !x.ByTransferencia).ToList();
            }

            if (data.Count() == 0)
                return BadRequest("No hay elementos a mostrar");

            byte[] fileContents = null;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tramites");

                //Añado los encabezados
                worksheet.Cells[1, 1].Value = "No. Orden";
                worksheet.Cells[1, 1].Style.Font.Size = 12;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 2].Value = "Fecha";
                worksheet.Cells[1, 2].Style.Font.Size = 12;
                worksheet.Cells[1, 2].Style.Font.Bold = true;
                worksheet.Cells[1, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 3].Value = "Cliente";
                worksheet.Cells[1, 3].Style.Font.Size = 12;
                worksheet.Cells[1, 3].Style.Font.Bold = true;
                worksheet.Cells[1, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 4].Value = "Empleado";
                worksheet.Cells[1, 4].Style.Font.Size = 12;
                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 5].Value = "Precio Venta";
                worksheet.Cells[1, 5].Style.Font.Size = 12;
                worksheet.Cells[1, 5].Style.Font.Bold = true;
                worksheet.Cells[1, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 6].Value = "Costo";
                worksheet.Cells[1, 6].Style.Font.Size = 12;
                worksheet.Cells[1, 6].Style.Font.Bold = true;
                worksheet.Cells[1, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 7].Value = "Utilidad";
                worksheet.Cells[1, 7].Style.Font.Size = 12;
                worksheet.Cells[1, 7].Style.Font.Bold = true;
                worksheet.Cells[1, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 8].Value = "Transferida De";
                worksheet.Cells[1, 8].Style.Font.Size = 12;
                worksheet.Cells[1, 8].Style.Font.Bold = true;
                worksheet.Cells[1, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 9].Value = "Tipo Pagos";
                worksheet.Cells[1, 9].Style.Font.Size = 12;
                worksheet.Cells[1, 9].Style.Font.Bold = true;
                worksheet.Cells[1, 9].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[1, 10].Value = "Servicio Consular";
                worksheet.Cells[1, 10].Style.Font.Size = 12;
                worksheet.Cells[1, 10].Style.Font.Bold = true;
                worksheet.Cells[1, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                int row = 2;
                foreach (var byAgency in data)
                {
                    var byService = byAgency.Value.GroupBy(x => x.Service);
                    foreach (var service in byService)
                    {
                        string serviceName = "";
                        switch (service.Key)
                        {
                            case STipo.Remesa:
                                serviceName = "Remesas";
                                break;
                            case STipo.PTuristico:
                                serviceName = "Paquete Turistico";
                                break;
                            case STipo.Recarga:
                                serviceName = "Recargas";
                                break;
                            case STipo.Servicio:
                                serviceName = "Otros Servicios";
                                break;
                            case STipo.Passport:
                                serviceName = "Pasaporte";
                                break;
                            case STipo.EnvioCaribe:
                                serviceName = "Envíos Caribe";
                                break;
                            case STipo.Paquete:
                                serviceName = "Evíos Aéreos";
                                break;
                            case STipo.Maritimo:
                                serviceName = "Envíos Marítimos";
                                break;
                            case STipo.Reserva:
                                serviceName = "Reservas";
                                break;
                            case STipo.Combo:
                                serviceName = "Combos";
                                break;
                            case STipo.Cubiq:
                                serviceName = "Carga AM";
                                break;
                            default:
                                break;
                        }

                        AuxUtilityExcel2(service.ToList(), serviceName, worksheet, ref row, byAgency.Key);
                    }
                }

                fileContents = package.GetAsByteArray();
            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: "Utilidad por servicio.xlsx"
            );
        }

        [Authorize]
        public async Task<IActionResult> ExportUtilidadExcel2(string strdate, bool onlyClientsAgency = false)
        {
            var role = AgenciaAuthorize.getRole(User, _context);
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = await _context.Agency.FirstOrDefaultAsync(x => x.AgencyId == aUser.FirstOrDefault().AgencyId);

            var auxDate = strdate.Split('-');
            CultureInfo culture = new CultureInfo("es-US", true);
            var dateIni = DateTime.Parse(auxDate[0], culture);
            var dateFin = DateTime.Parse(auxDate[1], culture);

            var data = (await _reportService.GetAllUtility(aAgency.AgencyId, dateIni, dateFin)).Value;
            if (data.Count() == 0)
                return BadRequest("No hay elementos a mostrar");

            int col = 1;
            var byService = data.GroupBy(x => x.Service);

            byte[] fileContents = null;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reporte");

                //Añado los encabezados
                worksheet.Cells[col, 1].Value = "No. Orden";
                worksheet.Cells[col, 1].Style.Font.Size = 12;
                worksheet.Cells[col, 1].Style.Font.Bold = true;
                worksheet.Cells[col, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 2].Value = "Fecha";
                worksheet.Cells[col, 2].Style.Font.Size = 12;
                worksheet.Cells[col, 2].Style.Font.Bold = true;
                worksheet.Cells[col, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 3].Value = "Cliente";
                worksheet.Cells[col, 3].Style.Font.Size = 12;
                worksheet.Cells[col, 3].Style.Font.Bold = true;
                worksheet.Cells[col, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 4].Value = "Empleado";
                worksheet.Cells[col, 4].Style.Font.Size = 12;
                worksheet.Cells[col, 4].Style.Font.Bold = true;
                worksheet.Cells[col, 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 5].Value = "Precio Venta";
                worksheet.Cells[col, 5].Style.Font.Size = 12;
                worksheet.Cells[col, 5].Style.Font.Bold = true;
                worksheet.Cells[col, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 6].Value = "Costo";
                worksheet.Cells[col, 6].Style.Font.Size = 12;
                worksheet.Cells[col, 6].Style.Font.Bold = true;
                worksheet.Cells[col, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 7].Value = "Utilidad";
                worksheet.Cells[col, 7].Style.Font.Size = 12;
                worksheet.Cells[col, 7].Style.Font.Bold = true;
                worksheet.Cells[col, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 8].Value = "Transferida De";
                worksheet.Cells[col, 8].Style.Font.Size = 12;
                worksheet.Cells[col, 8].Style.Font.Bold = true;
                worksheet.Cells[col, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 9].Value = "Tipo Pagos";
                worksheet.Cells[col, 9].Style.Font.Size = 12;
                worksheet.Cells[col, 9].Style.Font.Bold = true;
                worksheet.Cells[col, 9].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                worksheet.Cells[col, 10].Value = "Servicio";
                worksheet.Cells[col, 10].Style.Font.Size = 12;
                worksheet.Cells[col, 10].Style.Font.Bold = true;
                worksheet.Cells[col, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                col++;
                decimal saleprice = 0, cost = 0, utility = 0;

                foreach (var service in byService)
                {
                    string serviceName = "";
                    switch (service.Key)
                    {
                        case STipo.Remesa:
                            serviceName = "Remesas";
                            break;
                        case STipo.PTuristico:
                            serviceName = "Paquete Turistico";
                            break;
                        case STipo.Recarga:
                            serviceName = "Recargas";
                            break;
                        case STipo.Servicio:
                            serviceName = "Otros Servicios";
                            break;
                        case STipo.Passport:
                            serviceName = "Pasaporte";
                            break;
                        case STipo.EnvioCaribe:
                            serviceName = "Envíos Caribe";
                            break;
                        case STipo.Paquete:
                            serviceName = "Evíos Aéreos";
                            break;
                        case STipo.Maritimo:
                            serviceName = "Envíos Marítimos";
                            break;
                        case STipo.Reserva:
                            serviceName = "Reservas";
                            break;
                        case STipo.Combo:
                            serviceName = "Combos";
                            break;
                        case STipo.Cubiq:
                            serviceName = "Carga AM";
                            break;
                        default:
                            continue;
                    }

                    if (service.Key == STipo.Servicio)
                    {
                        var otherServices = service.GroupBy(x => x.TipoServicio);
                        foreach (var otherService in otherServices)
                        {
                            AuxUtilityExcel2(otherService.ToList(), otherService.Key, worksheet, ref col);
                        }
                    }
                    else if (service.Key == STipo.Reserva)
                    {
                        var carriers = service.Where(x => x.IsCarrier).ToList();
                        if (carriers.Any())
                        {
                            AuxUtilityExcel2(carriers.ToList(), "Reserva - Carrier", worksheet, ref col);
                        }
                        var pasaje = service.Where(x => !x.IsCarrier && x.TipoServicio == "pasaje").ToList();
                        var auto = service.Where(x => !x.IsCarrier && x.TipoServicio == "auto").ToList();
                        var hotel = service.Where(x => !x.IsCarrier && x.TipoServicio == "hotel").ToList();
                        if (pasaje.Any())
                        {
                            AuxUtilityExcel2(pasaje.ToList(), "Reserva - Pasaje", worksheet, ref col);
                        }
                        if (auto.Any())
                        {
                            AuxUtilityExcel2(auto.ToList(), "Reserva - Auto", worksheet, ref col);
                        }
                        if (hotel.Any())
                        {
                            AuxUtilityExcel2(hotel.ToList(), "Reserva - Hotel", worksheet, ref col);
                        }
                    }
                    else if (service.Key != STipo.Servicio)
                    {
                        if (service.Key == STipo.Passport && onlyClientsAgency)
                            AuxUtilityExcel2(service.Where(x => !x.ByTransferencia).ToList(), serviceName, worksheet, ref col);
                        else
                            AuxUtilityExcel2(service.ToList(), serviceName, worksheet, ref col);
                    }

                    saleprice += service.Sum(x => x.SalePrice);
                    cost += service.Sum(x => x.Cost);
                    utility += service.Sum(x => x.Utility);
                }

                worksheet.Cells[col, 1].Value = "Total";
                worksheet.Cells[col, 2].Value = "";
                worksheet.Cells[col, 3].Value = "";
                worksheet.Cells[col, 4].Value = "";
                worksheet.Cells[col, 5].Value = saleprice;
                worksheet.Cells[col, 6].Value = cost;
                worksheet.Cells[col, 7].Value = utility;
                worksheet.Cells[col, 8].Value = "";

                worksheet.Cells[col, 1].Style.Font.Size = 12;
                worksheet.Cells[col, 1].Style.Font.Bold = true;
                worksheet.Cells[col, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                worksheet.Cells[col, 5].Style.Font.Size = 12;
                worksheet.Cells[col, 5].Style.Font.Bold = true;
                worksheet.Cells[col, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                worksheet.Cells[col, 6].Style.Font.Size = 12;
                worksheet.Cells[col, 6].Style.Font.Bold = true;
                worksheet.Cells[col, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                worksheet.Cells[col, 7].Style.Font.Size = 12;
                worksheet.Cells[col, 7].Style.Font.Bold = true;
                worksheet.Cells[col, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                fileContents = package.GetAsByteArray();
            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: "Utilidad por servicio.xlsx"
            );
        }


        private void AuxUtilityExcel(List<UtilityModel> items, string serviceName, ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add(serviceName);

            //Añado los encabezados
            worksheet.Cells[1, 1].Value = "No. Orden";
            worksheet.Cells[1, 1].Style.Font.Size = 12;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 2].Value = "Fecha";
            worksheet.Cells[1, 2].Style.Font.Size = 12;
            worksheet.Cells[1, 2].Style.Font.Bold = true;
            worksheet.Cells[1, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 3].Value = "Cliente";
            worksheet.Cells[1, 3].Style.Font.Size = 12;
            worksheet.Cells[1, 3].Style.Font.Bold = true;
            worksheet.Cells[1, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 4].Value = "Empleado";
            worksheet.Cells[1, 4].Style.Font.Size = 12;
            worksheet.Cells[1, 4].Style.Font.Bold = true;
            worksheet.Cells[1, 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 5].Value = "Precio Venta";
            worksheet.Cells[1, 5].Style.Font.Size = 12;
            worksheet.Cells[1, 5].Style.Font.Bold = true;
            worksheet.Cells[1, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 6].Value = "Costo";
            worksheet.Cells[1, 6].Style.Font.Size = 12;
            worksheet.Cells[1, 6].Style.Font.Bold = true;
            worksheet.Cells[1, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 7].Value = "Utilidad";
            worksheet.Cells[1, 7].Style.Font.Size = 12;
            worksheet.Cells[1, 7].Style.Font.Bold = true;
            worksheet.Cells[1, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 8].Value = "Transferida De";
            worksheet.Cells[1, 8].Style.Font.Size = 12;
            worksheet.Cells[1, 8].Style.Font.Bold = true;
            worksheet.Cells[1, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 9].Value = "Tipo Pagos";
            worksheet.Cells[1, 9].Style.Font.Size = 12;
            worksheet.Cells[1, 9].Style.Font.Bold = true;
            worksheet.Cells[1, 9].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            if (serviceName == "Pasaporte")
            {
                worksheet.Cells[1, 10].Value = "Servicio Consular";
                worksheet.Cells[1, 10].Style.Font.Size = 12;
                worksheet.Cells[1, 10].Style.Font.Bold = true;
                worksheet.Cells[1, 10].Style.Border.Top.Style = ExcelBorderStyle.Hair;
            }

            var col = 2;
            foreach (var item in items)
            {
                worksheet.Cells[col, 1].Value = item.OrderNumber;
                worksheet.Cells[col, 2].Value = item.Date.ToShortDateString();
                worksheet.Cells[col, 3].Value = item.Client.FullName;
                worksheet.Cells[col, 4].Value = item.Employee.FullName;
                worksheet.Cells[col, 5].Value = item.SalePrice;
                worksheet.Cells[col, 6].Value = item.Cost;
                worksheet.Cells[col, 7].Value = item.Utility;
                worksheet.Cells[col, 8].Value = item.TransferredAgencyName;
                string pays = "";
                foreach (var pay in item.Pays)
                {
                    pays += $"{pay.TipoPago}, ";
                }
                worksheet.Cells[col, 9].Value = pays;
                if (serviceName == "Pasaporte")
                {
                    worksheet.Cells[col, 10].Value = item.ServicioConsular.GetDescription();
                }

                col++;
            }
            worksheet.Cells[col, 1].Value = "Total";
            worksheet.Cells[col, 2].Value = "";
            worksheet.Cells[col, 3].Value = "";
            worksheet.Cells[col, 4].Value = "";
            worksheet.Cells[col, 5].Value = items.Sum(x => x.SalePrice);
            worksheet.Cells[col, 6].Value = items.Sum(x => x.Cost);
            worksheet.Cells[col, 7].Value = items.Sum(x => x.Utility);
            worksheet.Cells[col, 8].Value = "";

            worksheet.Cells[col, 1].Style.Font.Size = 12;
            worksheet.Cells[col, 1].Style.Font.Bold = true;
            worksheet.Cells[col, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;
            worksheet.Cells[col, 5].Style.Font.Size = 12;
            worksheet.Cells[col, 5].Style.Font.Bold = true;
            worksheet.Cells[col, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
            worksheet.Cells[col, 6].Style.Font.Size = 12;
            worksheet.Cells[col, 6].Style.Font.Bold = true;
            worksheet.Cells[col, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
            worksheet.Cells[col, 7].Style.Font.Size = 12;
            worksheet.Cells[col, 7].Style.Font.Bold = true;
            worksheet.Cells[col, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;

        }


        private void AuxUtilityExcel2(List<UtilityModel> items, string serviceName, ExcelWorksheet worksheet, ref int col, string agency = "")
        {
            if (!items.Any())
            {
                return;
            }

            foreach (var item in items)
            {
                worksheet.Cells[col, 1].Value = item.OrderNumber;
                worksheet.Cells[col, 2].Value = item.Date.ToShortDateString();
                worksheet.Cells[col, 3].Value = item.Client.FullName;
                worksheet.Cells[col, 4].Value = item.Employee.FullName;
                worksheet.Cells[col, 5].Value = item.SalePrice;
                worksheet.Cells[col, 6].Value = item.Cost;
                worksheet.Cells[col, 7].Value = item.Utility;
                worksheet.Cells[col, 8].Value = string.IsNullOrEmpty(agency) ? item.TransferredAgencyName : agency;
                string pays = "";
                foreach (var pay in item.Pays)
                {
                    pays += $"{pay.TipoPago}, ";
                }
                worksheet.Cells[col, 9].Value = pays;
                if (serviceName == "Pasaporte")
                {
                    worksheet.Cells[col, 10].Value = $"{serviceName} - {item.ServicioConsular.GetDescription()}".ToUpper();
                }
                else
                {
                    worksheet.Cells[col, 10].Value = serviceName.ToUpper();
                }
                col++;
            }

        }


        [Authorize] //Para probar rendimiento
        public async Task<object> ExportVentasEmpleadoRapid(string strdate, Guid? idempleado)
        {
            Serilog.Log.Information("Inicio de Reporte liquidacion por empleado - Rapid");

            User aUser;

            var role = AgenciaAuthorize.getRole(User, _context);
            if (idempleado != null)
            {
                aUser = await _context.User.FirstOrDefaultAsync(x => x.UserId == idempleado);
            }
            else
            {
                aUser = await _context.User.FirstOrDefaultAsync(x => x.Username == User.Identity.Name);
            }

            return await Reporte.GetReporteVentasEmpleadoRapid(strdate, aUser, _context, _env);
        }

        [Authorize] //Para probar rendimiento
        public async Task<object> ExportVentasEmpleado(string strdate, Guid? idempleado)
        {
            Serilog.Log.Information("Inicio de Reporte liquidacion por empleado");
            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {
                    User aUser;

                    var role = AgenciaAuthorize.getRole(User, _context);
                    if (idempleado != null)
                    {
                        aUser = await _context.User.FirstOrDefaultAsync(x => x.UserId == idempleado);
                    }
                    else
                    {
                        aUser = await _context.User.FirstOrDefaultAsync(x => x.Username == User.Identity.Name);
                    }
                    var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId);

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    BaseColor colorcell = WebColors.GetRGBColor("#fdb4b4");


                    Agency agency = await aAgency.FirstOrDefaultAsync();
                    Models.Address agencyAddress = await _context.Address.FirstOrDefaultAsync(a => a.ReferenceId == agency.AgencyId);
                    Phone agencyPhone = await _context.Phone.FirstOrDefaultAsync(p => p.ReferenceId == agency.AgencyId);

                    var auxDate = strdate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateIni = DateTime.Parse(auxDate[0], culture);
                    var dateFin = DateTime.Parse(auxDate[1], culture);

                    //Datos
                    var remesas = await _context.Remittance
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { registroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Date, x.Amount, x.UserId, x.Number, ClientName = x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var paquetesTuristicos = await _context.PaquetesTuristicos
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { registroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Date, x.Amount, x.UserId, x.Number, ClientName = x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var enviosmaritimos = await _context
                        .EnvioMaritimo
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number, x.UserId, x.Date })
                        .ToListAsync();

                    var pasaportes = await _context.Passport
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => !x.AppMovil || (x.AppMovil && x.Status != Passport.STATUS_REVIEW))
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.FechaSolicitud.Date >= dateIni.Date && x.FechaSolicitud.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.UserId, x.OrderNumber, x.Total, x.FechaSolicitud, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var envioscaribe = await _context.EnvioCaribes
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.ToLocalTime().Date >= dateIni.Date && x.Date.ToLocalTime().Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Number, x.Date, x.UserId, x.Amount, x.User.FullName, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var enviosaereos = await _context.Order
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Type != "Remesas" && x.Type != "Combo")
                        .AsNoTracking().Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), Bag = x.Bag.Select(y => new { y.BagItems, }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var envioscombos = await _context.Order
                        .AsNoTracking().Include(x => x.Bag).ThenInclude(x => x.BagItems)
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.Minorista)
                        .AsNoTracking().Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Type == "Combo")
                        .AsNoTracking().Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .AsNoTracking().Where(x => x.Minorista == null)
                        .Select(x => new { RegistroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), Bag = x.Bag.Select(y => new { y.BagItems, }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var boletos = await _context.Ticket
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.PaqueteTuristicoId == null && x.State != "Cancelada" && x.AgencyId == agency.AgencyId && !x.ClientIsCarrier)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.type, x.ReservationNumber, x.RegisterDate, x.UserId, x.Total, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var boletosCarrier = await _context.Ticket
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.PaqueteTuristicoId == null && x.State != "Cancelada" && x.AgencyId == agency.AgencyId && x.ClientIsCarrier)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.type, x.ReservationNumber, x.RegisterDate, x.UserId, x.Total, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var recargas = await _context.Rechargue
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.date.Date >= dateIni.Date && x.date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { x.Number, RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.date, x.UserId, x.Client.FullData, ClientPhone = x.Client.Phone.Number, x.Import })
                        .ToListAsync();

                    var envioscubiq = await _context.OrderCubiqs
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var servicios = await _context.Servicios
                        .AsNoTracking().Include(x => x.cliente).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.PaqueteTuristicoId == null && x.estado != "Cancelado" && x.agency.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.fecha.ToLocalTime().Date >= dateIni.Date && x.fecha.ToLocalTime().Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.tipoServicio, x.numero, x.importeTotal, x.fecha, x.cliente.FullData, ClientPhone = x.cliente.Phone.Number, x.UserId })
                        .ToListAsync();

                    Serilog.Log.Information("Datos obtenidos");

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        celllogo.AddElement(imagelogo);
                    }


                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    Phrase empl = new Phrase("Empleado: ", headFont);
                    empl.AddSpecial(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                    cellAgency.AddElement(empl);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = "";
                    if (dateIni.Date == dateFin.Date)
                    {
                        texto = dateIni.Date.ToShortDateString();
                    }
                    else
                    {
                        texto = dateIni.Date.ToShortDateString() + " a " + dateFin.Date.ToShortDateString();
                    }
                    Paragraph parPaq = new Paragraph("Liquidación del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);

                    Dictionary<string, decimal> ventastipopago = new Dictionary<string, decimal>();
                    Dictionary<string, int> canttipopago = new Dictionary<string, int>();
                    foreach (var item in _context.TipoPago)
                    {
                        ventastipopago.Add(item.Type, 0);
                        canttipopago.Add(item.Type, 0);
                    }

                    decimal tramitesTotal = 0;
                    decimal tramitesDeuda = 0;
                    decimal tramitesTotalPagado = 0;
                    decimal tramitesTotalCredito = 0;
                    decimal tramitesPagado = 0;
                    decimal tramitesCredito = 0;
                    decimal totalitems = 0;

                    PdfPTable tblremesasData;
                    float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    PdfPCell cellremesas1;
                    PdfPCell cellremesas2;
                    PdfPCell cellremesas3;
                    PdfPCell cellremesas4;
                    PdfPCell cellremesas5;
                    PdfPCell cellremesas6;
                    PdfPCell cellremesas7;
                    PdfPCell cellremesas8;
                    PdfPCell cellremesas9;


                    #region // REMESAS

                    decimal refRemesasPagado = 0;

                    if (remesas.Any())
                    {

                        tblremesasData = new PdfPTable(columnWidths);
                        tblremesasData.WidthPercentage = 100;

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refRemesasTotal = 0;
                        decimal refRemesasTotalPagado = 0;
                        decimal refRemesasDebe = 0;
                        decimal refRemesasCredito = 0;
                        decimal refRemesasTotalCredito = 0;

                        if (remesas.Count != 0)
                        {
                            doc.Add(new Phrase("Remesas", headFont));
                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            foreach (var remesa in remesas)
                            {
                                decimal pagado = 0;
                                decimal totalPagado = remesa.registroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = remesa.registroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal total = remesa.Amount;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in remesa.registroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > remesa.Date.Date || item.UserId != remesa.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > remesa.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refRemesasPagado += pagado;
                                refRemesasCredito += creditoConsumo;
                                tramitesPagado += totalPagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                                {
                                    refRemesasTotalCredito += totalCredito;
                                    refRemesasTotalPagado += totalPagado;
                                    refRemesasTotal += total;
                                    refRemesasDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesDeuda += debe;
                                    tramitesTotalCredito += tramitesTotalCredito;
                                }

                                var index = remesas.IndexOf(remesa);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(remesa.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(remesa.ClientName, normalFont));
                                cellremesas2.AddElement(new Phrase(remesa.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }
                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                            if (refRemesasCredito > 0)
                                cellremesas4.AddElement(new Phrase(refRemesasCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refRemesasTotalPagado.ToString(), headFont));
                            if (refRemesasTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refRemesasTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refRemesasTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refRemesasDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }


                    #endregion

                    #region // Paquete Turístico

                    decimal refPaqueteTuristicoPagado = 0;

                    if (paquetesTuristicos.Any())
                    {

                        tblremesasData = new PdfPTable(columnWidths);
                        tblremesasData.WidthPercentage = 100;

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refPaqueteTuristicoTotal = 0;
                        decimal refPaqueteTuristicoTotalPagado = 0;
                        decimal refPaqueteTuristicoDebe = 0;
                        decimal refPaqueteTuristicoCredito = 0;
                        decimal refPaqueteTuristicoTotalCredito = 0;

                        if (paquetesTuristicos.Count != 0)
                        {
                            doc.Add(new Phrase("Paquete Turístico", headFont));
                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            foreach (var paqueteTuristico in paquetesTuristicos)
                            {
                                decimal pagado = 0;
                                decimal totalPagado = paqueteTuristico.registroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = paqueteTuristico.registroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal total = paqueteTuristico.Amount;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in paqueteTuristico.registroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > paqueteTuristico.Date.Date || item.UserId != paqueteTuristico.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > paqueteTuristico.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refPaqueteTuristicoPagado += pagado;
                                refPaqueteTuristicoCredito += creditoConsumo;
                                tramitesPagado += totalPagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                                {
                                    refPaqueteTuristicoTotalCredito += totalCredito;
                                    refPaqueteTuristicoTotalPagado += totalPagado;
                                    refPaqueteTuristicoTotal += total;
                                    refPaqueteTuristicoDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesDeuda += debe;
                                    tramitesTotalCredito += tramitesTotalCredito;
                                }

                                var index = paquetesTuristicos.IndexOf(paqueteTuristico);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(paqueteTuristico.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(paqueteTuristico.ClientName, normalFont));
                                cellremesas2.AddElement(new Phrase(paqueteTuristico.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }
                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                            if (refPaqueteTuristicoCredito > 0)
                                cellremesas4.AddElement(new Phrase(refPaqueteTuristicoCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refPaqueteTuristicoTotalPagado.ToString(), headFont));
                            if (refPaqueteTuristicoTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refPaqueteTuristicoTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refPaqueteTuristicoTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refPaqueteTuristicoDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }


                    #endregion

                    #region // ENVIOS MARITIMOS

                    float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    decimal refPagadoEnvioM = 0;

                    if (enviosmaritimos.Any())
                    {
                        tblremesasData = new PdfPTable(columnWidthsmaritimo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refTotalEnvioM = 0;
                        decimal refCreditoEnvioM = 0;
                        decimal refTotalPagadoEnvioM = 0;
                        decimal refTotalCreditoEnvioM = 0;
                        decimal refDebeEnvioM = 0;
                        if (enviosmaritimos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Marítimos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var enviomaritimo in enviosmaritimos)
                            {
                                decimal total = enviomaritimo.Amount;
                                decimal Totalpagado = enviomaritimo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal TotalCredito = enviomaritimo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                bool diffDate = false;
                                bool colorearCell = false;
                                string tipoPagos = "";
                                foreach (var item in enviomaritimo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tipoPagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviomaritimo.Date.Date || item.UserId != enviomaritimo.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviomaritimo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - (Totalpagado + TotalCredito);

                                refPagadoEnvioM += pagado;
                                refCreditoEnvioM += creditoConsumo;
                                tramitesPagado += pagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate)
                                {
                                    refTotalEnvioM += total;
                                    refTotalPagadoEnvioM += Totalpagado;
                                    refDebeEnvioM += debe;
                                    refTotalCreditoEnvioM += TotalCredito;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += Totalpagado;
                                    tramitesTotalCredito += TotalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = enviosmaritimos.IndexOf(enviomaritimo);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviomaritimo.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviomaritimo.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(enviomaritimo.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                                if (TotalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(tipoPagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refPagadoEnvioM.ToString(), headFont));
                            if (refCreditoEnvioM > 0)
                                cellremesas4.AddElement(new Phrase(refCreditoEnvioM.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refTotalPagadoEnvioM.ToString(), headFont));
                            if (refTotalCreditoEnvioM > 0)
                                cellremesas5.AddElement(new Phrase(refTotalCreditoEnvioM.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refTotalEnvioM.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refDebeEnvioM.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // Pasaportes

                    decimal refPassportPagado = 0;

                    if (pasaportes.Any())
                    {
                        float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthspasaporte);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        if (AgencyName.IsDistrictCuba(agency.AgencyId))
                        {
                            pasaportes = pasaportes.OrderByDescending(x => x.OrderNumber).ToList();
                        }

                        decimal refPassportTotal = 0;
                        decimal refPassportCredito = 0;
                        decimal refPassportTotalPagado = 0;
                        decimal refPassportTotalCredito = 0;
                        decimal refPassportDebe = 0;
                        if (pasaportes.Count != 0)
                        {
                            doc.Add(new Phrase("Pasaportes", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var pasaporte in pasaportes)
                            {
                                decimal total = pasaporte.Total;
                                decimal totalPagado = pasaporte.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = pasaporte.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string pagos = "";
                                foreach (var item in pasaporte.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date || item.UserId != pasaporte.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refPassportPagado += pagado;
                                refPassportCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refPassportTotal += total;
                                    refPassportTotalPagado += totalPagado;
                                    refPassportTotalCredito += totalCredito;
                                    refPassportDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = pasaportes.IndexOf(pasaporte);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(pasaporte.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(pasaporte.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refPassportPagado.ToString(), headFont));
                            if (refPassportCredito > 0)
                                cellremesas4.AddElement(new Phrase(refPassportCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refPassportTotalPagado.ToString(), headFont));
                            if (refPassportTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refPassportTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refPassportTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refPassportDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS CARIBE
                    decimal refEnvioCaribePagado = 0;
                    if (envioscaribe.Any())
                    {
                        float[] columnWidthscaribe = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthscaribe);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;


                        decimal refEnvioCaribeTotal = 0;
                        decimal refEnvioCaribeCredito = 0;
                        decimal refEnvioCaribeTotalPagado = 0;
                        decimal refEnvioCaribeTotalCredito = 0;
                        decimal refEnvioCaribeDebe = 0;
                        if (envioscaribe.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Caribe", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var enviocaribe in envioscaribe)
                            {
                                decimal total = enviocaribe.Amount;
                                decimal Totalpagado = enviocaribe.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal TotalCredito = enviocaribe.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;

                                string tipopagosenviocaribe = "";
                                bool paintRow = false;
                                bool diffDate = false;
                                foreach (var item in enviocaribe.RegistroPagos.Where(y => y.date.ToLocalTime() >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tipopagosenviocaribe += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviocaribe.Date.Date || item.UserId != enviocaribe.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviocaribe.Date.Date)
                                            diffDate = true;
                                        paintRow = true;
                                    }
                                }

                                debe = total - (Totalpagado + TotalCredito);
                                refEnvioCaribePagado += pagado;
                                refEnvioCaribeCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;

                                if (!diffDate)
                                {
                                    refEnvioCaribeTotal += total;
                                    refEnvioCaribeTotalPagado += Totalpagado;
                                    refEnvioCaribeTotalCredito += TotalCredito;
                                    refEnvioCaribeDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += Totalpagado;
                                    tramitesTotalCredito += TotalCredito;
                                    tramitesDeuda += debe;
                                }



                                var index = envioscaribe.IndexOf(enviocaribe);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (paintRow)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviocaribe.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocaribe.FullName, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocaribe.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                                if (TotalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(tipopagosenviocaribe, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                            if (refEnvioCaribeCredito > 0)
                                cellremesas4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                            cellremesas5.AddElement(new Phrase(refEnvioCaribeCredito.ToString(), headRedFont));
                            if (refEnvioCaribeTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnvioCaribeTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnvioCaribeTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refEnvioCaribeDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS AEREOS
                    decimal refEnviosAereosPagado = 0;
                    if (enviosaereos.Any())
                    {
                        float[] columnWidthsaereo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthsaereo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refEnviosAereosTotal = 0;
                        decimal refEnviosAereosCredito = 0;
                        decimal refEnviosAereosTotalPagado = 0;
                        decimal refEnviosAereosTotalCredito = 0;
                        decimal refEnviosAereosDebe = 0;
                        if (enviosaereos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Aéreos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var envioaereo in enviosaereos)
                            {
                                decimal total = envioaereo.Amount;
                                decimal totalPagado = envioaereo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = envioaereo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var item in envioaereo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tiposdepagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > envioaereo.Date.Date || item.UserId != envioaereo.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > envioaereo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosAereosPagado += pagado;
                                refEnviosAereosCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosAereosTotal += total;
                                    refEnviosAereosTotalPagado += totalPagado;
                                    refEnviosAereosTotalCredito += totalCredito;
                                    refEnviosAereosDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                var index = enviosaereos.IndexOf(envioaereo);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;

                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(envioaereo.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(envioaereo.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(envioaereo.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refEnviosAereosPagado.ToString(), headFont));
                            if (refEnviosAereosCredito > 0)
                                cellremesas4.AddElement(new Phrase(refEnviosAereosCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refEnviosAereosTotalPagado.ToString(), headFont));
                            if (refEnviosAereosTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnviosAereosTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnviosAereosTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refEnviosAereosDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS COMBOS
                    decimal refEnviosCombosPagado = 0;

                    if (envioscombos.Any())
                    {
                        float[] columnWidthsCombo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthsCombo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;


                        decimal refEnviosCombosTotal = 0;
                        decimal refEnviosCombosCredito = 0;
                        decimal refEnviosCombosTotalPagado = 0;
                        decimal refEnviosCombosTotalCredito = 0;
                        decimal refEnviosCombosDebe = 0;
                        if (envioscombos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Combos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Cant", headFont));
                            cellremesas5.AddElement(new Phrase("Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas7.AddElement(new Phrase("Total", headFont));
                            cellremesas8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);


                            foreach (var enviocombo in envioscombos)
                            {
                                decimal total = enviocombo.Amount;
                                decimal totalPagado = enviocombo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = enviocombo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var item in enviocombo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tiposdepagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviocombo.Date.Date || item.UserId != enviocombo.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviocombo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosCombosPagado += pagado;
                                refEnviosCombosCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosCombosTotal += total;
                                    refEnviosCombosTotalPagado += totalPagado;
                                    refEnviosCombosTotalCredito += totalCredito;
                                    refEnviosCombosDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                var index = envioscombos.IndexOf(enviocombo);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviocombo.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocombo.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocombo.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                var items = enviocombo.Bag.Select(x => x.BagItems);
                                int cantitems = 0;
                                foreach (var item in items)
                                {
                                    cantitems += item.Count();
                                }
                                totalitems += cantitems;
                                cellremesas4.AddElement(new Phrase(cantitems.ToString(), normalFont));
                                cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                                tblremesasData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(totalitems.ToString(), headFont));
                            cellremesas5.AddElement(new Phrase(refEnviosCombosPagado.ToString(), headFont));
                            if (refEnviosCombosCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnviosCombosCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnviosCombosTotalPagado.ToString(), headFont));
                            if (refEnviosCombosTotalCredito > 0)
                                cellremesas6.AddElement(new Phrase(refEnviosCombosTotalCredito.ToString(), headRedFont));
                            cellremesas7.AddElement(new Phrase(refEnviosCombosTotal.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase(refEnviosCombosDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // BOLETOS  
                    decimal refBoletoPagado = 0;

                    if (boletos.Any())
                    {
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthboletos);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;

                        decimal refBoletoTotal = 0;
                        decimal refBoletoCredito = 0;
                        decimal refBoletoTotalPagado = 0;
                        decimal refBoletoTotalCredito = 0;
                        decimal refBoletoDebe = 0;
                        if (boletos.Count != 0)
                        {
                            doc.Add(new Phrase("Boletos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Tipo", headFont));
                            cellremesas5.AddElement(new Phrase("Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas7.AddElement(new Phrase("Total", headFont));
                            cellremesas8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);


                            foreach (var boleto in boletos)
                            {
                                decimal total = boleto.Total;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = boleto.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = boleto.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in boleto.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date || item.UserId != boleto.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;
                                refBoletoPagado += pagado;
                                refBoletoCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refBoletoTotal += total;
                                    refBoletoTotalPagado += totalPagado;
                                    refBoletoTotalCredito += totalCredito;
                                    refBoletoDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = boletos.IndexOf(boleto);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                                cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                                tblremesasData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase("", normalFont));
                            cellremesas5.AddElement(new Phrase(refBoletoPagado.ToString(), headFont));
                            if (refBoletoCredito > 0)
                                cellremesas5.AddElement(new Phrase(refBoletoCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refBoletoTotalPagado.ToString(), headFont));
                            if (refBoletoTotalCredito > 0)
                                cellremesas6.AddElement(new Phrase(refBoletoTotalCredito.ToString(), headRedFont));
                            cellremesas7.AddElement(new Phrase(refBoletoTotal.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase(refBoletoDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // BOLETOS CARRIER 
                    decimal refBoletoCarrierPagado = 0;

                    if (boletosCarrier.Any())
                    {
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthboletos);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;


                        decimal refBoletoCarrierTotal = 0;
                        decimal refBoletoCarrierCredito = 0;
                        decimal refBoletoCarrierTotalPagado = 0;
                        decimal refBoletoCarrierTotalCredito = 0;
                        decimal refBoletoCarrierDebe = 0;
                        if (boletosCarrier.Count != 0)
                        {
                            doc.Add(new Phrase("Boletos Carrier", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Tipo", headFont));
                            cellremesas5.AddElement(new Phrase("Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas7.AddElement(new Phrase("Total", headFont));
                            cellremesas8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);


                            foreach (var boleto in boletosCarrier)
                            {
                                decimal total = boleto.Total;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = boleto.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = boleto.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in boleto.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date || item.UserId != boleto.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;
                                refBoletoCarrierPagado += pagado;
                                refBoletoCarrierCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refBoletoCarrierTotal += total;
                                    refBoletoCarrierTotalPagado += totalPagado;
                                    refBoletoCarrierTotalCredito += totalCredito;
                                    refBoletoCarrierDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = boletosCarrier.IndexOf(boleto);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                                cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                                tblremesasData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase("", normalFont));
                            cellremesas5.AddElement(new Phrase(refBoletoCarrierPagado.ToString(), headFont));
                            if (refBoletoCarrierCredito > 0)
                                cellremesas5.AddElement(new Phrase(refBoletoCarrierCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refBoletoCarrierTotalPagado.ToString(), headFont));
                            if (refBoletoCarrierTotalCredito > 0)
                                cellremesas6.AddElement(new Phrase(refBoletoCarrierTotalCredito.ToString(), headRedFont));
                            cellremesas7.AddElement(new Phrase(refBoletoCarrierTotal.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase(refBoletoCarrierDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // RECARGAS
                    decimal refRecharguePagado = 0;

                    if (recargas.Any())
                    {
                        float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthsrecarga);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refRechargueTotal = 0;
                        decimal refRechargueCredito = 0;
                        decimal refRechargueTotalPagado = 0;
                        decimal refRechargueTotalCredito = 0;
                        decimal refRechargueDebe = 0;
                        if (recargas.Count != 0)
                        {
                            doc.Add(new Phrase("RECARGAS", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var recarga in recargas)
                            {
                                decimal total = recarga.Import;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = recarga.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = recarga.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string pagos = "";
                                foreach (var item in recarga.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > recarga.date.Date || item.UserId != recarga.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > recarga.date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado + totalCredito;
                                refRecharguePagado += pagado;
                                refRechargueCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;

                                if (!diffDate)
                                {
                                    refRechargueTotal += total;
                                    refRechargueTotalPagado += totalPagado;
                                    refRechargueTotalCredito += totalCredito;
                                    refRechargueDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = recargas.IndexOf(recarga);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(recarga.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(recarga.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(recarga.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refRecharguePagado.ToString(), headFont));
                            if (refRechargueCredito > 0)
                                cellremesas4.AddElement(new Phrase(refRechargueCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refRechargueTotalPagado.ToString(), headFont));
                            if (refRechargueTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refRechargueTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refRechargueTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refRechargueDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS CUBIQ
                    decimal refEnviosCubiqPagado = 0;

                    if (envioscubiq.Any())
                    {
                        float[] columnWidthCubiq = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthCubiq);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refEnviosCubiqTotal = 0;
                        decimal refEnviosCubiqCredito = 0;
                        decimal refEnviosCubiqTotalPagado = 0;
                        decimal refEnviosCubiqTotalCredito = 0;
                        decimal refEnviosCubiqDebe = 0;
                        if (envioscubiq.Count() != 0)
                        {
                            doc.Add(new Phrase("Envíos Carga AM", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var enviocubiq in envioscubiq)
                            {
                                decimal total = enviocubiq.Amount;
                                decimal totalPagado = enviocubiq.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo" && x.tipoPago.Type != "Cubiq").Sum(x => x.valorPagado);
                                decimal totalCredito = enviocubiq.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var item in enviocubiq.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tiposdepagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo" && item.tipoPago.Type != "Cubiq")
                                        pagado += item.valorPagado;
                                    else if (item.tipoPago.Type == "Crédito de Consumo")
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviocubiq.Date.Date || item.UserId != enviocubiq.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviocubiq.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosCubiqPagado += pagado;
                                refEnviosCubiqCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosCubiqTotal += total;
                                    refEnviosCubiqTotalPagado += totalPagado;
                                    refEnviosCubiqTotalCredito += totalCredito;
                                    refEnviosCubiqDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                var index = envioscubiq.IndexOf(enviocubiq);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;

                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviocubiq.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocubiq.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocubiq.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refEnviosCubiqPagado.ToString(), headFont));
                            if (refEnviosCubiqCredito > 0)
                                cellremesas4.AddElement(new Phrase(refEnviosCubiqCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refEnviosCubiqTotalPagado.ToString(), headFont));
                            if (refEnviosCubiqTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnviosCubiqTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnviosCubiqTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refEnviosCubiqDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region OTROS SERVICIOS
                    float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };

                    var tiposervicios = _context.TipoServicios
                            .Where(x => x.agency.AgencyId == agency.AgencyId);
                    Dictionary<string, decimal> creditoServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> creditoTotalServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> pagadoServicios = new Dictionary<string, decimal>();

                    if (servicios.Any())
                    {
                        //Hago una tabla para cada servicio
                        foreach (var tipo in tiposervicios)
                        {

                            var auxservicios = servicios.Where(x => x.tipoServicio.TipoServicioId == tipo.TipoServicioId).ToList();
                            if (auxservicios.Count() != 0)
                            {
                                tblremesasData = new PdfPTable(columnWidthsservicios);
                                tblremesasData.WidthPercentage = 100;
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                                doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                                cellremesas1.AddElement(new Phrase("No.", headFont));
                                cellremesas2.AddElement(new Phrase("Cliente", headFont));
                                cellremesas3.AddElement(new Phrase("Empleado", headFont));
                                cellremesas4.AddElement(new Phrase("Pagado", headFont));
                                cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                                cellremesas6.AddElement(new Phrase("Total", headFont));
                                cellremesas7.AddElement(new Phrase("Debe", headFont));
                                cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);

                                creditoServicios.Add(tipo.Nombre, 0);
                                creditoTotalServicios.Add(tipo.Nombre.ToString(), 0);
                                pagadoServicios.Add(tipo.Nombre, 0);
                                decimal refServicioTotal = 0;
                                decimal refServicioPagado = 0;
                                decimal refServicioTotalPagado = 0;
                                decimal refServicioDebe = 0;
                                foreach (var servicio in auxservicios)
                                {
                                    decimal total = servicio.importeTotal;
                                    decimal totalPagado = servicio.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                    decimal totalCredito = servicio.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                    decimal pagado = 0;
                                    decimal credito = 0;
                                    decimal debe = 0;
                                    string pagos = "";
                                    bool diffDate = false;
                                    bool colorearCell = false;
                                    foreach (var item in servicio.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                    {
                                        pagos += item.tipoPago.Type + ", ";
                                        if (item.tipoPago.Type == "Crédito de Consumo")
                                            credito += item.valorPagado;
                                        else
                                            pagado += item.valorPagado;
                                        ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                        canttipopago[item.tipoPago.Type] += 1;

                                        //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                        if (item.date.ToLocalTime().Date > servicio.fecha.Date || item.UserId != servicio.UserId)
                                        {
                                            if (item.date.ToLocalTime().Date > servicio.fecha.Date)
                                                diffDate = true;
                                            colorearCell = true;
                                        }
                                    }
                                    debe = total - totalPagado - totalCredito;
                                    creditoServicios[tipo.Nombre] += credito;
                                    pagadoServicios[tipo.Nombre] += pagado;
                                    tramitesPagado += pagado;
                                    tramitesCredito += credito;
                                    if (!diffDate)
                                    {
                                        refServicioTotal += total;
                                        refServicioPagado += pagado;
                                        refServicioTotalPagado += totalPagado;
                                        creditoTotalServicios[tipo.Nombre.ToString()] += totalCredito;
                                        refServicioDebe += debe;

                                        tramitesTotal += total;
                                        tramitesTotalPagado += totalPagado;
                                        tramitesTotalCredito += totalCredito;
                                        tramitesDeuda += debe;
                                    }

                                    var index = auxservicios.IndexOf(servicio);
                                    if (index == 0)
                                    {
                                        cellremesas1 = new PdfPCell();
                                        cellremesas1.Border = 1;
                                        cellremesas2 = new PdfPCell();
                                        cellremesas2.Border = 1;
                                        cellremesas3 = new PdfPCell();
                                        cellremesas3.Border = 1;
                                        cellremesas4 = new PdfPCell();
                                        cellremesas4.Border = 1;
                                        cellremesas5 = new PdfPCell();
                                        cellremesas5.Border = 1;
                                        cellremesas6 = new PdfPCell();
                                        cellremesas6.Border = 1;
                                        cellremesas7 = new PdfPCell();
                                        cellremesas7.Border = 1;
                                        cellremesas8 = new PdfPCell();
                                        cellremesas8.Border = 1;
                                    }
                                    else
                                    {
                                        cellremesas1 = new PdfPCell();
                                        cellremesas1.Border = 0;
                                        cellremesas2 = new PdfPCell();
                                        cellremesas2.Border = 0;
                                        cellremesas3 = new PdfPCell();
                                        cellremesas3.Border = 0;
                                        cellremesas4 = new PdfPCell();
                                        cellremesas4.Border = 0;
                                        cellremesas5 = new PdfPCell();
                                        cellremesas5.Border = 0;
                                        cellremesas6 = new PdfPCell();
                                        cellremesas6.Border = 0;
                                        cellremesas7 = new PdfPCell();
                                        cellremesas7.Border = 0;
                                        cellremesas8 = new PdfPCell();
                                        cellremesas8.Border = 0;
                                    }
                                    if (colorearCell)
                                    {
                                        cellremesas1.BackgroundColor = colorcell;
                                        cellremesas2.BackgroundColor = colorcell;
                                        cellremesas3.BackgroundColor = colorcell;
                                        cellremesas4.BackgroundColor = colorcell;
                                        cellremesas5.BackgroundColor = colorcell;
                                        cellremesas6.BackgroundColor = colorcell;
                                        cellremesas7.BackgroundColor = colorcell;
                                        cellremesas8.BackgroundColor = colorcell;
                                    }
                                    cellremesas1.AddElement(new Phrase(servicio.numero, normalFont));
                                    cellremesas2.AddElement(new Phrase(servicio.FullData, normalFont));
                                    cellremesas2.AddElement(new Phrase(servicio.ClientPhone, normalFont));
                                    cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                    cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                    if (credito > 0)
                                        cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                    cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                    if (totalCredito > 0 && !diffDate)
                                        cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                    cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                    cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                    cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                    tblremesasData.AddCell(cellremesas1);
                                    tblremesasData.AddCell(cellremesas2);
                                    tblremesasData.AddCell(cellremesas3);
                                    tblremesasData.AddCell(cellremesas4);
                                    tblremesasData.AddCell(cellremesas5);
                                    tblremesasData.AddCell(cellremesas6);
                                    tblremesasData.AddCell(cellremesas7);
                                    tblremesasData.AddCell(cellremesas8);
                                }

                                // Añado el total
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;

                                cellremesas1.AddElement(new Phrase("Totales", headFont));
                                cellremesas2.AddElement(new Phrase("", normalFont));
                                cellremesas3.AddElement(new Phrase("", normalFont));
                                cellremesas4.AddElement(new Phrase(refServicioPagado.ToString(), headFont));
                                if (creditoServicios[tipo.Nombre] > 0)
                                    cellremesas4.AddElement(new Phrase(creditoServicios[tipo.Nombre].ToString(), headRedFont));
                                cellremesas5.AddElement(new Phrase(refServicioTotalPagado.ToString(), headFont));
                                if (creditoTotalServicios[tipo.Nombre.ToString()] > 0)
                                    cellremesas5.AddElement(new Phrase(creditoTotalServicios[tipo.Nombre.ToString()].ToString(), headRedFont));
                                cellremesas6.AddElement(new Phrase(refServicioTotal.ToString(), headFont));
                                cellremesas7.AddElement(new Phrase(refServicioDebe.ToString(), headFont));
                                cellremesas8.AddElement(new Phrase("", normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);

                                // Añado la tabla al documento
                                doc.Add(tblremesasData);
                                doc.Add(Chunk.NEWLINE);
                                doc.Add(Chunk.NEWLINE);
                            }
                        }

                    }

                    #endregion

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    tblremesasData = new PdfPTable(columnWidthstipopago);
                    tblremesasData.WidthPercentage = 100;

                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;

                    cellremesas1.AddElement(new Phrase("Tipo de Pago", headFont));
                    cellremesas2.AddElement(new Phrase("Cantidad", headFont));
                    cellremesas3.AddElement(new Phrase("Importe", headFont));
                    tblremesasData.AddCell(cellremesas1);
                    tblremesasData.AddCell(cellremesas2);
                    tblremesasData.AddCell(cellremesas3);
                    foreach (var item in _context.TipoPago)
                    {

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;

                        cellremesas1.AddElement(new Phrase(item.Type, normalFont));
                        cellremesas2.AddElement(new Phrase(canttipopago[item.Type].ToString(), normalFont));
                        cellremesas3.AddElement(new Phrase(ventastipopago[item.Type].ToString(), normalFont));
                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                    }

                    doc.Add(tblremesasData);
                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 2 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    doc.Add(Chunk.NEWLINE);
                    cellleft.AddElement(new Phrase("RESUMEN DE TRÁMITES", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    //Remesas: 15 ordenes -- $360.00 usd
                    Phrase aux;
                    int auxcanttoal = 0;
                    int auxcant = remesas.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("REMESAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRemesasPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = paquetesTuristicos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PAQUETES TURÍSTICOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPaqueteTuristicoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosmaritimos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPagadoEnvioM.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = pasaportes.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + refPassportPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscaribe.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnvioCaribePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosaereos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosAereosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscubiq.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCubiqPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscombos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCombosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletosCarrier.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoCarrierPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = recargas.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRecharguePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    foreach (var item in tiposervicios)
                    {
                        var auxservicio = servicios.Where(x => x.tipoServicio.TipoServicioId == item.TipoServicioId).ToList();
                        auxcant = auxservicio.Count();
                        auxcanttoal += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase(item.Nombre.ToUpper() + ": ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + pagadoServicios[item.Nombre.ToString()].ToString() + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                    }
                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);
                    #endregion

                    #region //GRAN TOTAL

                    Paragraph pPagado = new Paragraph("Pagado Total: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesPagado.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    pPagado = new Paragraph("Crédito: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesCredito.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph totalp = new Paragraph("Total Ventas Día: ", headFont2);
                    totalp.AddSpecial(new Phrase("$ " + tramitesTotal.ToString(), normalFont2));
                    totalp.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(totalp);
                    Paragraph deuda = new Paragraph("Pendiente a pago: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + tramitesDeuda.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);
                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);
                    doc.Close();
                    Serilog.Log.Information("Fin de reporte");
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        [Authorize] //Para probar rendimiento
        public async Task<object> ExportVentasEmpleado2(string strdate, Guid? idempleado)
        {
            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {
                    User aUser;

                    var role = AgenciaAuthorize.getRole(User, _context);
                    if (idempleado != null)
                    {
                        aUser = await _context.User.FirstOrDefaultAsync(x => x.UserId == idempleado);
                    }
                    else
                    {
                        aUser = await _context.User.FirstOrDefaultAsync(x => x.Username == User.Identity.Name);
                    }
                    var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId);

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    BaseColor colorcell = WebColors.GetRGBColor("#fdb4b4");


                    Agency agency = await aAgency.FirstOrDefaultAsync();
                    Models.Address agencyAddress = await _context.Address.FirstOrDefaultAsync(a => a.ReferenceId == agency.AgencyId);
                    Phone agencyPhone = await _context.Phone.FirstOrDefaultAsync(p => p.ReferenceId == agency.AgencyId);

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        celllogo.AddElement(imagelogo);
                    }


                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    Phrase empl = new Phrase("Empleado: ", headFont);
                    empl.AddSpecial(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                    cellAgency.AddElement(empl);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    var auxDate = strdate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateIni = DateTime.Parse(auxDate[0], culture);
                    var dateFin = DateTime.Parse(auxDate[1], culture);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = "";
                    if (dateIni.Date == dateFin.Date)
                    {
                        texto = dateIni.Date.ToShortDateString();
                    }
                    else
                    {
                        texto = dateIni.Date.ToShortDateString() + " a " + dateFin.Date.ToShortDateString();
                    }
                    Paragraph parPaq = new Paragraph("Liquidación del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);

                    Dictionary<string, decimal> ventastipopago = new Dictionary<string, decimal>();
                    Dictionary<string, int> canttipopago = new Dictionary<string, int>();
                    foreach (var item in _context.TipoPago)
                    {
                        ventastipopago.Add(item.Type, 0);
                        canttipopago.Add(item.Type, 0);
                    }

                    decimal tramitesTotal = 0;
                    decimal tramitesDeuda = 0;
                    decimal tramitesTotalPagado = 0;
                    decimal tramitesTotalCredito = 0;
                    decimal tramitesPagado = 0;
                    decimal tramitesCredito = 0;
                    decimal totalitems = 0;
                    #region // REMESAS
                    var remesas = await _context.Remittance
                        .Include(x => x.Client)
                        .ThenInclude(x => x.Phone)
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { registroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Date, x.Amount, x.UserId, x.Number, ClientName = x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();
                    float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    PdfPTable tblremesasData = new PdfPTable(columnWidths);
                    tblremesasData.WidthPercentage = 100;

                    PdfPCell cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    PdfPCell cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    PdfPCell cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    PdfPCell cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    PdfPCell cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    PdfPCell cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    PdfPCell cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    PdfPCell cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    decimal refRemesasTotal = 0;
                    decimal refRemesasPagado = 0;
                    decimal refRemesasTotalPagado = 0;
                    decimal refRemesasDebe = 0;
                    decimal refRemesasCredito = 0;
                    decimal refRemesasTotalCredito = 0;

                    if (remesas.Count != 0)
                    {
                        doc.Add(new Phrase("Remesas", headFont));
                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        foreach (var remesa in remesas)
                        {
                            decimal pagado = 0;
                            decimal totalPagado = remesa.registroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = remesa.registroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal total = remesa.Amount;
                            decimal debe = 0;
                            decimal creditoConsumo = 0;

                            string pagos = "";
                            bool colorearCell = false;
                            bool diffDate = false;
                            foreach (var item in remesa.registroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > remesa.Date.Date || item.UserId != remesa.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > remesa.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refRemesasPagado += pagado;
                            refRemesasCredito += creditoConsumo;
                            tramitesPagado += totalPagado;
                            tramitesCredito += creditoConsumo;

                            if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                            {
                                refRemesasTotalCredito += totalCredito;
                                refRemesasTotalPagado += totalPagado;
                                refRemesasTotal += total;
                                refRemesasDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesDeuda += debe;
                                tramitesTotalCredito += tramitesTotalCredito;
                            }

                            var index = remesas.IndexOf(remesa);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(remesa.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(remesa.ClientName, normalFont));
                            cellremesas2.AddElement(new Phrase(remesa.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (creditoConsumo > 0)
                                cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }
                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                        if (refRemesasCredito > 0)
                            cellremesas4.AddElement(new Phrase(refRemesasCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refRemesasTotalPagado.ToString(), headFont));
                        if (refRemesasTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refRemesasTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refRemesasTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refRemesasDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }

                    #endregion

                    #region // ENVIOS MARITIMOS

                    float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthsmaritimo);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    var enviosmaritimos = await _context.EnvioMaritimo
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number, x.UserId, x.Date })
                        .ToListAsync();
                    decimal refTotalEnvioM = 0;
                    decimal refPagadoEnvioM = 0;
                    decimal refCreditoEnvioM = 0;
                    decimal refTotalPagadoEnvioM = 0;
                    decimal refTotalCreditoEnvioM = 0;
                    decimal refDebeEnvioM = 0;
                    if (enviosmaritimos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Marítimos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var enviomaritimo in enviosmaritimos)
                        {
                            decimal total = enviomaritimo.Amount;
                            decimal Totalpagado = enviomaritimo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal TotalCredito = enviomaritimo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal debe = 0;
                            decimal creditoConsumo = 0;

                            bool diffDate = false;
                            bool colorearCell = false;
                            string tipoPagos = "";
                            foreach (var item in enviomaritimo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                tipoPagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > enviomaritimo.Date.Date || item.UserId != enviomaritimo.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > enviomaritimo.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - (Totalpagado + TotalCredito);

                            refPagadoEnvioM += pagado;
                            refCreditoEnvioM += creditoConsumo;
                            tramitesPagado += pagado;
                            tramitesCredito += creditoConsumo;

                            if (!diffDate)
                            {
                                refTotalEnvioM += total;
                                refTotalPagadoEnvioM += Totalpagado;
                                refDebeEnvioM += debe;
                                refTotalCreditoEnvioM += TotalCredito;

                                tramitesTotal += total;
                                tramitesTotalPagado += Totalpagado;
                                tramitesTotalCredito += TotalCredito;
                                tramitesDeuda += debe;
                            }

                            var index = enviosmaritimos.IndexOf(enviomaritimo);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(enviomaritimo.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviomaritimo.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(enviomaritimo.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (creditoConsumo > 0)
                                cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                            if (TotalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tipoPagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoEnvioM.ToString(), headFont));
                        if (refCreditoEnvioM > 0)
                            cellremesas4.AddElement(new Phrase(refCreditoEnvioM.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refTotalPagadoEnvioM.ToString(), headFont));
                        if (refTotalCreditoEnvioM > 0)
                            cellremesas5.AddElement(new Phrase(refTotalCreditoEnvioM.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refTotalEnvioM.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refDebeEnvioM.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // Pasaportes

                    float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthspasaporte);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    var pasaportes = await _context.Passport
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .Where(x => !x.AppMovil || (x.AppMovil && x.Status != Passport.STATUS_REVIEW))
                        .Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.FechaSolicitud.Date >= dateIni.Date && x.FechaSolicitud.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.UserId, x.OrderNumber, x.Total, x.FechaSolicitud, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    if (AgencyName.IsDistrictCuba(agency.AgencyId))
                    {
                        pasaportes = pasaportes.OrderByDescending(x => x.OrderNumber).ToList();
                    }

                    decimal refPassportTotal = 0;
                    decimal refPassportPagado = 0;
                    decimal refPassportCredito = 0;
                    decimal refPassportTotalPagado = 0;
                    decimal refPassportTotalCredito = 0;
                    decimal refPassportDebe = 0;
                    if (pasaportes.Count != 0)
                    {
                        doc.Add(new Phrase("Pasaportes", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var pasaporte in pasaportes)
                        {
                            decimal total = pasaporte.Total;
                            decimal totalPagado = pasaporte.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = pasaporte.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal debe = 0;
                            bool colorearCell = false;
                            bool diffDate = false;
                            string pagos = "";
                            foreach (var item in pasaporte.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date || item.UserId != pasaporte.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refPassportPagado += pagado;
                            refPassportCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refPassportTotal += total;
                                refPassportTotalPagado += totalPagado;
                                refPassportTotalCredito += totalCredito;
                                refPassportDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            var index = pasaportes.IndexOf(pasaporte);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(pasaporte.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(pasaporte.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPassportPagado.ToString(), headFont));
                        if (refPassportCredito > 0)
                            cellremesas4.AddElement(new Phrase(refPassportCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refPassportTotalPagado.ToString(), headFont));
                        if (refPassportTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refPassportTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refPassportTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refPassportDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS CARIBE

                    float[] columnWidthscaribe = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthscaribe);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    var envioscaribe = await _context.EnvioCaribes
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.ToLocalTime().Date >= dateIni.Date && x.Date.ToLocalTime().Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Number, x.Date, x.UserId, x.Amount, x.User.FullName, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    decimal refEnvioCaribeTotal = 0;
                    decimal refEnvioCaribePagado = 0;
                    decimal refEnvioCaribeCredito = 0;
                    decimal refEnvioCaribeTotalPagado = 0;
                    decimal refEnvioCaribeTotalCredito = 0;
                    decimal refEnvioCaribeDebe = 0;
                    if (envioscaribe.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Caribe", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var enviocaribe in envioscaribe)
                        {
                            decimal total = enviocaribe.Amount;
                            decimal Totalpagado = enviocaribe.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal TotalCredito = enviocaribe.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal debe = 0;

                            string tipopagosenviocaribe = "";
                            bool paintRow = false;
                            bool diffDate = false;
                            foreach (var item in enviocaribe.RegistroPagos.Where(y => y.date.ToLocalTime() >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                tipopagosenviocaribe += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > enviocaribe.Date.Date || item.UserId != enviocaribe.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > enviocaribe.Date.Date)
                                        diffDate = true;
                                    paintRow = true;
                                }
                            }

                            debe = total - (Totalpagado + TotalCredito);
                            refEnvioCaribePagado += pagado;
                            refEnvioCaribeCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;

                            if (!diffDate)
                            {
                                refEnvioCaribeTotal += total;
                                refEnvioCaribeTotalPagado += Totalpagado;
                                refEnvioCaribeTotalCredito += TotalCredito;
                                refEnvioCaribeDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += Totalpagado;
                                tramitesTotalCredito += TotalCredito;
                                tramitesDeuda += debe;
                            }



                            var index = envioscaribe.IndexOf(enviocaribe);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }
                            if (paintRow)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(enviocaribe.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocaribe.FullName, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocaribe.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                            if (TotalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tipopagosenviocaribe, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                        if (refEnvioCaribeCredito > 0)
                            cellremesas4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refEnvioCaribeCredito.ToString(), headRedFont));
                        if (refEnvioCaribeTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refEnvioCaribeTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refEnvioCaribeTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refEnvioCaribeDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS AEREOS

                    float[] columnWidthsaereo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthsaereo);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;


                    var enviosaereos = await _context.Order
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Type != "Remesas" && x.Type != "Combo")
                        .Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), Bag = x.Bag.Select(y => new { y.BagItems, }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    decimal refEnviosAereosTotal = 0;
                    decimal refEnviosAereosPagado = 0;
                    decimal refEnviosAereosCredito = 0;
                    decimal refEnviosAereosTotalPagado = 0;
                    decimal refEnviosAereosTotalCredito = 0;
                    decimal refEnviosAereosDebe = 0;
                    if (enviosaereos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Aéreos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var envioaereo in enviosaereos)
                        {
                            decimal total = envioaereo.Amount;
                            decimal totalPagado = envioaereo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = envioaereo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal debe = 0;
                            bool colorearCell = false;
                            bool diffDate = false;
                            string tiposdepagos = "";
                            foreach (var item in envioaereo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                tiposdepagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > envioaereo.Date.Date || item.UserId != envioaereo.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > envioaereo.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refEnviosAereosPagado += pagado;
                            refEnviosAereosCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refEnviosAereosTotal += total;
                                refEnviosAereosTotalPagado += totalPagado;
                                refEnviosAereosTotalCredito += totalCredito;
                                refEnviosAereosDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            if (tiposdepagos == "")
                            {
                                tiposdepagos = "-";
                            }

                            var index = enviosaereos.IndexOf(envioaereo);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;

                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(envioaereo.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(envioaereo.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(envioaereo.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tiposdepagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refEnviosAereosPagado.ToString(), headFont));
                        if (refEnviosAereosCredito > 0)
                            cellremesas4.AddElement(new Phrase(refEnviosAereosCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refEnviosAereosTotalPagado.ToString(), headFont));
                        if (refEnviosAereosTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refEnviosAereosTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refEnviosAereosTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refEnviosAereosDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS COMBOS
                    float[] columnWidthsCombo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1, 1 };

                    tblremesasData = new PdfPTable(columnWidthsCombo);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;
                    PdfPCell cellremesas9 = new PdfPCell();
                    cellremesas9.Border = 1;

                    var envioscombos = await _context.Order
                        .Include(x => x.Bag).ThenInclude(x => x.BagItems)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.Minorista)
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Type == "Combo")
                        .Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.Minorista == null)
                        .Select(x => new { RegistroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), Bag = x.Bag.Select(y => new { y.BagItems, }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    decimal refEnviosCombosTotal = 0;
                    decimal refEnviosCombosPagado = 0;
                    decimal refEnviosCombosCredito = 0;
                    decimal refEnviosCombosTotalPagado = 0;
                    decimal refEnviosCombosTotalCredito = 0;
                    decimal refEnviosCombosDebe = 0;
                    if (envioscombos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Combos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Cant", headFont));
                        cellremesas5.AddElement(new Phrase("Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas7.AddElement(new Phrase("Total", headFont));
                        cellremesas8.AddElement(new Phrase("Debe", headFont));
                        cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        tblremesasData.AddCell(cellremesas9);


                        foreach (var enviocombo in envioscombos)
                        {
                            decimal total = enviocombo.Amount;
                            decimal totalPagado = enviocombo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = enviocombo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal debe = 0;
                            bool colorearCell = false;
                            bool diffDate = false;
                            string tiposdepagos = "";
                            foreach (var item in enviocombo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                tiposdepagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > enviocombo.Date.Date || item.UserId != enviocombo.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > enviocombo.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refEnviosCombosPagado += pagado;
                            refEnviosCombosCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refEnviosCombosTotal += total;
                                refEnviosCombosTotalPagado += totalPagado;
                                refEnviosCombosTotalCredito += totalCredito;
                                refEnviosCombosDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            if (tiposdepagos == "")
                            {
                                tiposdepagos = "-";
                            }

                            var index = envioscombos.IndexOf(enviocombo);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                                cellremesas9 = new PdfPCell();
                                cellremesas9.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                                cellremesas9 = new PdfPCell();
                                cellremesas9.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                                cellremesas9.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(enviocombo.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocombo.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocombo.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            var items = enviocombo.Bag.Select(x => x.BagItems);
                            int cantitems = 0;
                            foreach (var item in items)
                            {
                                cantitems += item.Count();
                            }
                            totalitems += cantitems;
                            cellremesas4.AddElement(new Phrase(cantitems.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas9.AddElement(new Phrase(tiposdepagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(totalitems.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refEnviosCombosPagado.ToString(), headFont));
                        if (refEnviosCombosCredito > 0)
                            cellremesas5.AddElement(new Phrase(refEnviosCombosCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refEnviosCombosTotalPagado.ToString(), headFont));
                        if (refEnviosCombosTotalCredito > 0)
                            cellremesas6.AddElement(new Phrase(refEnviosCombosTotalCredito.ToString(), headRedFont));
                        cellremesas7.AddElement(new Phrase(refEnviosCombosTotal.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase(refEnviosCombosDebe.ToString(), headFont));
                        cellremesas9.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        tblremesasData.AddCell(cellremesas9);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }

                    #endregion

                    #region // BOLETOS  

                    float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthboletos);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;
                    cellremesas9 = new PdfPCell();
                    cellremesas9.Border = 1;

                    var boletos = await _context.Ticket
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.State != "Cancelada" && x.AgencyId == agency.AgencyId && !x.ClientIsCarrier)
                        .Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.type, x.ReservationNumber, x.RegisterDate, x.UserId, x.Total, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    decimal refBoletoTotal = 0;
                    decimal refBoletoPagado = 0;
                    decimal refBoletoCredito = 0;
                    decimal refBoletoTotalPagado = 0;
                    decimal refBoletoTotalCredito = 0;
                    decimal refBoletoDebe = 0;
                    if (boletos.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Tipo", headFont));
                        cellremesas5.AddElement(new Phrase("Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas7.AddElement(new Phrase("Total", headFont));
                        cellremesas8.AddElement(new Phrase("Debe", headFont));
                        cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        tblremesasData.AddCell(cellremesas9);


                        foreach (var boleto in boletos)
                        {
                            decimal total = boleto.Total;
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal totalPagado = boleto.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = boleto.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal debe = 0;
                            string pagos = "";
                            bool colorearCell = false;
                            bool diffDate = false;
                            foreach (var item in boleto.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date || item.UserId != boleto.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;
                            refBoletoPagado += pagado;
                            refBoletoCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refBoletoTotal += total;
                                refBoletoTotalPagado += totalPagado;
                                refBoletoTotalCredito += totalCredito;
                                refBoletoDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            var index = boletos.IndexOf(boleto);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                                cellremesas9 = new PdfPCell();
                                cellremesas9.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                                cellremesas9 = new PdfPCell();
                                cellremesas9.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                                cellremesas9.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                            cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas9.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase("", normalFont));
                        cellremesas5.AddElement(new Phrase(refBoletoPagado.ToString(), headFont));
                        if (refBoletoCredito > 0)
                            cellremesas5.AddElement(new Phrase(refBoletoCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refBoletoTotalPagado.ToString(), headFont));
                        if (refBoletoTotalCredito > 0)
                            cellremesas6.AddElement(new Phrase(refBoletoTotalCredito.ToString(), headRedFont));
                        cellremesas7.AddElement(new Phrase(refBoletoTotal.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase(refBoletoDebe.ToString(), headFont));
                        cellremesas9.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        tblremesasData.AddCell(cellremesas9);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // BOLETOS CARRIER 

                    tblremesasData = new PdfPTable(columnWidthboletos);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;
                    cellremesas9 = new PdfPCell();
                    cellremesas9.Border = 1;

                    var boletosCarrier = await _context.Ticket
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.State != "Cancelada" && x.AgencyId == agency.AgencyId && x.ClientIsCarrier)
                        .Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.type, x.ReservationNumber, x.RegisterDate, x.UserId, x.Total, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    decimal refBoletoCarrierTotal = 0;
                    decimal refBoletoCarrierPagado = 0;
                    decimal refBoletoCarrierCredito = 0;
                    decimal refBoletoCarrierTotalPagado = 0;
                    decimal refBoletoCarrierTotalCredito = 0;
                    decimal refBoletoCarrierDebe = 0;
                    if (boletosCarrier.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos Carrier", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Tipo", headFont));
                        cellremesas5.AddElement(new Phrase("Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas7.AddElement(new Phrase("Total", headFont));
                        cellremesas8.AddElement(new Phrase("Debe", headFont));
                        cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        tblremesasData.AddCell(cellremesas9);


                        foreach (var boleto in boletosCarrier)
                        {
                            decimal total = boleto.Total;
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal totalPagado = boleto.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = boleto.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal debe = 0;
                            string pagos = "";
                            bool colorearCell = false;
                            bool diffDate = false;
                            foreach (var item in boleto.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date || item.UserId != boleto.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;
                            refBoletoCarrierPagado += pagado;
                            refBoletoCarrierCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refBoletoCarrierTotal += total;
                                refBoletoCarrierTotalPagado += totalPagado;
                                refBoletoCarrierTotalCredito += totalCredito;
                                refBoletoCarrierDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            var index = boletosCarrier.IndexOf(boleto);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                                cellremesas9 = new PdfPCell();
                                cellremesas9.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                                cellremesas9 = new PdfPCell();
                                cellremesas9.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                                cellremesas9.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                            cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas9.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase("", normalFont));
                        cellremesas5.AddElement(new Phrase(refBoletoCarrierPagado.ToString(), headFont));
                        if (refBoletoCarrierCredito > 0)
                            cellremesas5.AddElement(new Phrase(refBoletoCarrierCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refBoletoCarrierTotalPagado.ToString(), headFont));
                        if (refBoletoCarrierTotalCredito > 0)
                            cellremesas6.AddElement(new Phrase(refBoletoCarrierTotalCredito.ToString(), headRedFont));
                        cellremesas7.AddElement(new Phrase(refBoletoCarrierTotal.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase(refBoletoCarrierDebe.ToString(), headFont));
                        cellremesas9.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        tblremesasData.AddCell(cellremesas9);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // RECARGAS

                    float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthsrecarga);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    var recargas = await _context.Rechargue
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.date.Date >= dateIni.Date && x.date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { x.Number, RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.date, x.UserId, x.Client.FullData, ClientPhone = x.Client.Phone.Number, x.Import })
                        .ToListAsync();

                    decimal refRechargueTotal = 0;
                    decimal refRecharguePagado = 0;
                    decimal refRechargueCredito = 0;
                    decimal refRechargueTotalPagado = 0;
                    decimal refRechargueTotalCredito = 0;
                    decimal refRechargueDebe = 0;
                    if (recargas.Count != 0)
                    {
                        doc.Add(new Phrase("RECARGAS", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var recarga in recargas)
                        {
                            decimal total = recarga.Import;
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal totalPagado = recarga.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = recarga.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal debe = 0;
                            bool colorearCell = false;
                            bool diffDate = false;
                            string pagos = "";
                            foreach (var item in recarga.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > recarga.date.Date || item.UserId != recarga.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > recarga.date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado + totalCredito;
                            refRecharguePagado += pagado;
                            refRechargueCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;

                            if (!diffDate)
                            {
                                refRechargueTotal += total;
                                refRechargueTotalPagado += totalPagado;
                                refRechargueTotalCredito += totalCredito;
                                refRechargueDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            var index = recargas.IndexOf(recarga);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(recarga.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(recarga.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(recarga.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refRecharguePagado.ToString(), headFont));
                        if (refRechargueCredito > 0)
                            cellremesas4.AddElement(new Phrase(refRechargueCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refRechargueTotalPagado.ToString(), headFont));
                        if (refRechargueTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refRechargueTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refRechargueTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refRechargueDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS CUBIQ

                    float[] columnWidthCubiq = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthCubiq);
                    tblremesasData.WidthPercentage = 100;
                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;


                    var envioscubiq = await _context.OrderCubiqs
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    decimal refEnviosCubiqTotal = 0;
                    decimal refEnviosCubiqPagado = 0;
                    decimal refEnviosCubiqCredito = 0;
                    decimal refEnviosCubiqTotalPagado = 0;
                    decimal refEnviosCubiqTotalCredito = 0;
                    decimal refEnviosCubiqDebe = 0;
                    if (envioscubiq.Count() != 0)
                    {
                        doc.Add(new Phrase("Envíos Carga AM", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var enviocubiq in envioscubiq)
                        {
                            decimal total = enviocubiq.Amount;
                            decimal totalPagado = enviocubiq.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo" && x.tipoPago.Type != "Cubiq").Sum(x => x.valorPagado);
                            decimal totalCredito = enviocubiq.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal debe = 0;
                            bool colorearCell = false;
                            bool diffDate = false;
                            string tiposdepagos = "";
                            foreach (var item in enviocubiq.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                            {
                                tiposdepagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo" && item.tipoPago.Type != "Cubiq")
                                    pagado += item.valorPagado;
                                else if (item.tipoPago.Type == "Crédito de Consumo")
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > enviocubiq.Date.Date || item.UserId != enviocubiq.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > enviocubiq.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refEnviosCubiqPagado += pagado;
                            refEnviosCubiqCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refEnviosCubiqTotal += total;
                                refEnviosCubiqTotalPagado += totalPagado;
                                refEnviosCubiqTotalCredito += totalCredito;
                                refEnviosCubiqDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            if (tiposdepagos == "")
                            {
                                tiposdepagos = "-";
                            }

                            var index = envioscubiq.IndexOf(enviocubiq);
                            if (index == 0)
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;

                            }
                            else
                            {
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }
                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(enviocubiq.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocubiq.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocubiq.ClientPhone, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tiposdepagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refEnviosCubiqPagado.ToString(), headFont));
                        if (refEnviosCubiqCredito > 0)
                            cellremesas4.AddElement(new Phrase(refEnviosCubiqCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refEnviosCubiqTotalPagado.ToString(), headFont));
                        if (refEnviosCubiqTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refEnviosCubiqTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refEnviosCubiqTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refEnviosCubiqDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region OTROS SERVICIOS
                    float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };


                    var servicios = await _context.Servicios
                        .Include(x => x.cliente).ThenInclude(x => x.Phone)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.estado != "Cancelado" && x.agency.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.fecha.ToLocalTime().Date >= dateIni.Date && x.fecha.ToLocalTime().Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.tipoServicio, x.numero, x.importeTotal, x.fecha, x.cliente.FullData, ClientPhone = x.cliente.Phone.Number, x.UserId })
                        .ToListAsync();
                    //Hago una tabla para cada servicio
                    var tiposervicios = _context.TipoServicios
                        .Where(x => x.agency.AgencyId == agency.AgencyId);
                    Dictionary<string, decimal> creditoServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> creditoTotalServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> pagadoServicios = new Dictionary<string, decimal>();
                    foreach (var tipo in tiposervicios)
                    {

                        var auxservicios = servicios.Where(x => x.tipoServicio == tipo).ToList();
                        if (auxservicios.Count() != 0)
                        {
                            tblremesasData = new PdfPTable(columnWidthsservicios);
                            tblremesasData.WidthPercentage = 100;
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;
                            doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            creditoServicios.Add(tipo.Nombre, 0);
                            creditoTotalServicios.Add(tipo.Nombre.ToString(), 0);
                            pagadoServicios.Add(tipo.Nombre, 0);
                            decimal refServicioTotal = 0;
                            decimal refServicioPagado = 0;
                            decimal refServicioTotalPagado = 0;
                            decimal refServicioDebe = 0;
                            foreach (var servicio in auxservicios)
                            {
                                decimal total = servicio.importeTotal;
                                decimal totalPagado = servicio.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = servicio.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                string pagos = "";
                                bool diffDate = false;
                                bool colorearCell = false;
                                foreach (var item in servicio.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type == "Crédito de Consumo")
                                        credito += item.valorPagado;
                                    else
                                        pagado += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > servicio.fecha.Date || item.UserId != servicio.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > servicio.fecha.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }
                                debe = total - totalPagado - totalCredito;
                                creditoServicios[tipo.Nombre] += credito;
                                pagadoServicios[tipo.Nombre] += pagado;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refServicioTotal += total;
                                    refServicioPagado += pagado;
                                    refServicioTotalPagado += totalPagado;
                                    creditoTotalServicios[tipo.Nombre.ToString()] += totalCredito;
                                    refServicioDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = auxservicios.IndexOf(servicio);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }
                                cellremesas1.AddElement(new Phrase(servicio.numero, normalFont));
                                cellremesas2.AddElement(new Phrase(servicio.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(servicio.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refServicioPagado.ToString(), headFont));
                            if (creditoServicios[tipo.Nombre] > 0)
                                cellremesas4.AddElement(new Phrase(creditoServicios[tipo.Nombre].ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refServicioTotalPagado.ToString(), headFont));
                            if (creditoTotalServicios[tipo.Nombre.ToString()] > 0)
                                cellremesas5.AddElement(new Phrase(creditoTotalServicios[tipo.Nombre.ToString()].ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refServicioTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refServicioDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    tblremesasData = new PdfPTable(columnWidthstipopago);
                    tblremesasData.WidthPercentage = 100;

                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;

                    cellremesas1.AddElement(new Phrase("Tipo de Pago", headFont));
                    cellremesas2.AddElement(new Phrase("Cantidad", headFont));
                    cellremesas3.AddElement(new Phrase("Importe", headFont));
                    tblremesasData.AddCell(cellremesas1);
                    tblremesasData.AddCell(cellremesas2);
                    tblremesasData.AddCell(cellremesas3);
                    foreach (var item in _context.TipoPago)
                    {

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;

                        cellremesas1.AddElement(new Phrase(item.Type, normalFont));
                        cellremesas2.AddElement(new Phrase(canttipopago[item.Type].ToString(), normalFont));
                        cellremesas3.AddElement(new Phrase(ventastipopago[item.Type].ToString(), normalFont));
                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                    }

                    doc.Add(tblremesasData);
                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 2 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    doc.Add(Chunk.NEWLINE);
                    cellleft.AddElement(new Phrase("RESUMEN DE TRÁMITES", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    //Remesas: 15 ordenes -- $360.00 usd
                    Phrase aux;
                    int auxcanttoal = 0;
                    int auxcant = remesas.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("REMESAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRemesasPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = enviosmaritimos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPagadoEnvioM.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = pasaportes.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + refPassportPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscaribe.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnvioCaribePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosaereos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosAereosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscubiq.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCubiqPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscombos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCombosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletosCarrier.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoCarrierPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = recargas.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRecharguePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    foreach (var item in tiposervicios)
                    {
                        var auxservicio = servicios.Where(x => x.tipoServicio == item).ToList();
                        auxcant = auxservicio.Count();
                        auxcanttoal += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase(item.Nombre.ToUpper() + ": ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + pagadoServicios[item.Nombre.ToString()].ToString() + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                    }
                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);
                    #endregion

                    #region //GRAN TOTAL

                    Paragraph pPagado = new Paragraph("Pagado Total: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesPagado.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    pPagado = new Paragraph("Crédito: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesCredito.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph totalp = new Paragraph("Total Ventas Día: ", headFont2);
                    totalp.AddSpecial(new Phrase("$ " + tramitesTotal.ToString(), normalFont2));
                    totalp.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(totalp);
                    Paragraph deuda = new Paragraph("Pendiente a pago: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + tramitesDeuda.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);
                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);
                    doc.Close();
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        public async Task<IActionResult> OrdenesAgencias(string strdate, bool transferidas)
        {
            ViewBag.strdate = strdate;
            ViewBag.transferidas = transferidas;

            DateTime startDate;
            DateTime endDate;
            if (string.IsNullOrEmpty(strdate))
            {
                startDate = DateTime.Now;
                endDate = DateTime.Now;
            }
            else
            {
                var auxDate = strdate.Split('-');
                CultureInfo culture = new CultureInfo("es-US", true);
                startDate = DateTime.Parse(auxDate[0], culture);
                endDate = DateTime.Parse(auxDate[1], culture);
            }

            var enviosmaritimos = await _context
                .EnvioMaritimo
                .AsNoTracking().Include(x => x.Agency)
                .AsNoTracking().Where(x => x.Status != "Cancelada")
                .AsNoTracking().Where(x => (x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date))
                .GroupBy(x => x.Agency)
                .Select(x => new AuxOrdenesAgencia()
                {
                    agency = x.Key,
                    count = x.Count()
                })
                .ToListAsync();

            var pasaportes = await _context
                .Passport
                .AsNoTracking().Include(x => x.Agency)
                .AsNoTracking().Where(x => x.Status != "Cancelada")
                .AsNoTracking().Where(x => x.FechaSolicitud.Date >= startDate.Date && x.FechaSolicitud.Date <= endDate.Date)
                .GroupBy(x => x.Agency)
                .Select(x => new AuxOrdenesAgencia()
                {
                    agency = x.Key,
                    count = x.Count()
                })
                .ToListAsync();

            //var envioscaribe = await _context.EnvioCaribes
            //    .AsNoTracking().Include(x => x.Agency)
            //    .AsNoTracking().Where(x => x.Status != "Cancelada")
            //    .AsNoTracking().Where(x => x.Date.ToLocalTime().Date >= startDate.Date && x.Date.ToLocalTime().Date <= endDate.Date)
            //    .GroupBy(x => x.Agency)
            //    .Select(x => new AuxOrdenesAgencia()
            //    {
            //        agency = x.Key,
            //        count = x.Count()
            //    })
            //    .ToListAsync();

            var enviosaereos = await _context.Order
                .AsNoTracking().Include(x => x.Agency)
                .AsNoTracking().Where(x => x.Status != "Cancelada" && x.Type != "Remesas" && x.Type != "Combo")
                .AsNoTracking().Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)
                .GroupBy(x => x.Agency)
                .Select(x => new AuxOrdenesAgencia()
                {
                    agency = x.Key,
                    count = x.Count()
                })
                .ToListAsync();

            var envioscombos = await _context.Order
                .AsNoTracking().Include(x => x.Agency)
                .AsNoTracking().Where(x => x.Status != "Cancelada" && x.Type == "Combo")
                .AsNoTracking().Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)
                .GroupBy(x => x.Agency)
                .Select(x => new AuxOrdenesAgencia()
                {
                    agency = x.Key,
                    count = x.Count()
                })
                .ToListAsync();

            var tickets = await _context.Ticket
                .AsNoTracking().Include(x => x.Agency)
                .AsNoTracking().Where(x => x.State != "Cancelada")
                .AsNoTracking().Where(x => x.RegisterDate.Date >= startDate.Date && x.RegisterDate.Date <= endDate.Date)
                .GroupBy(x => x.Agency)
                .Select(x => new AuxOrdenesAgencia()
                {
                    agency = x.Key,
                    count = x.Count()
                })
                .ToListAsync();


            var envioscubiq = await _context.OrderCubiqs
                .AsNoTracking().Include(x => x.Agency)
                .AsNoTracking().Where(x => x.Status != "Cancelada")
                .AsNoTracking().Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)
                .GroupBy(x => x.Agency)
                .Select(x => new AuxOrdenesAgencia()
                {
                    agency = x.Key,
                    count = x.Count()
                })
                .ToListAsync();

            if (transferidas)
            {
                var envioscubiqtrans = await _context.OrderCubiqs
                .AsNoTracking().Include(x => x.agencyTransferida)
                .AsNoTracking().Where(x => x.Status != "Cancelada")
                .AsNoTracking().Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date && x.agencyTransferidaId != null)
                .GroupBy(x => x.agencyTransferida)
                .Select(x => new AuxOrdenesAgencia()
                {
                    agency = x.Key,
                    count = x.Count()
                })
                .ToListAsync();
                foreach (var item in envioscubiqtrans)
                {
                    if (envioscubiq.Any(x => x.agency.AgencyId == item.agency.AgencyId))
                    {
                        envioscubiq.First(x => x.agency.AgencyId == item.agency.AgencyId).count += item.count;
                    }
                    else
                    {
                        envioscubiq.Add(item);
                    }
                }
            }


            var servicios = await _context.Servicios
                .AsNoTracking().Include(x => x.agency)
                .AsNoTracking().Where(x => x.estado != "Cancelado")
                .AsNoTracking().Where(x => x.fecha.ToLocalTime().Date >= startDate.Date && x.fecha.ToLocalTime().Date <= endDate.Date)
                .GroupBy(x => x.agency)
                .Select(x => new AuxOrdenesAgencia()
                {
                    agency = x.Key,
                    count = x.Count()
                })
                .ToListAsync();

            ViewBag.enviosmaritimos = enviosmaritimos;
            ViewBag.enviosaereos = enviosaereos;
            //ViewBag.envioscaribe = envioscaribe;
            ViewBag.envioscubiq = envioscubiq;
            ViewBag.envioscombos = envioscombos;
            ViewBag.pasaportes = pasaportes;
            ViewBag.tickets = tickets;
            ViewBag.servicios = servicios;
            ViewBag.Agencias = await _context.Agency.ToListAsync();

            return ViewAutorize(new string[] { }, null);
        }

        [Authorize]
        public async Task<object> PDFVentasporempleadoRapid(string strdate)
        {
            User user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            return await Reporte.GetReporteLiquidacion(strdate, user, _context, _env);
        }

        [Authorize]
        public async Task<object> PDFVentasporempleado(string strdate)
        {
            Serilog.Log.Information("Reporte de liquidacion general");

            using (MemoryStream MStream = new MemoryStream())
            {
                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {

                    var role = AgenciaAuthorize.getRole(User, _context);
                    var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
                    var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId);

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

                    Agency agency = aAgency.FirstOrDefault();
                    Models.Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        celllogo.AddElement(imagelogo);
                    }


                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    var auxDate = strdate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateIni = DateTime.Parse(auxDate[0], culture);
                    var dateFin = DateTime.Parse(auxDate[1], culture);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = "";
                    if (dateIni.Date == dateFin.Date)
                    {
                        texto = dateIni.Date.ToShortDateString();
                    }
                    else
                    {
                        texto = dateIni.Date.ToShortDateString() + " a " + dateFin.Date.ToShortDateString();
                    }
                    Paragraph parPaq = new Paragraph("Liquidación por empleado del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);
                    /*
                     * 0: cash
                     * 1: zelle
                     * 2: cheque
                     * 3: credito o debito
                     * 4: transferencia bancaria
                     * 5: web
                     * 6: Money Order
                     * 7: Crédito de Consumo
                     * */
                    decimal[] ventastipopago = new decimal[9];
                    int[] canttipopago = new int[9];
                    decimal grantotal = 0;
                    decimal totalpagado = 0;
                    decimal total_Tpagado = 0;
                    decimal totalcantidad = 0;

                    var dateInitUtc = dateIni.Date.ToUniversalTime();
                    var dateFintUtc = dateFin.Date.AddDays(1).ToUniversalTime();
                    var ventasEmpleado = await _context.ReporteLiquidacion
                   .Where(x => x.AgencyId == agency.AgencyId && x.Date >= dateInitUtc && x.Date <= dateFintUtc && x.CuentaBancariaId == null && x.BillId == null && x.FacturaId == null)
                   .Where(x =>
                   x.Caribe_Status != "Cancelada"
                   && x.Remesa_Status != Remittance.STATUS_CANCELADA
                   && x.Maritimo_Status != "Cancelada"
                   && x.Order_Status != "Cancelada"
                   && x.Cubiq_Status != "Cancelada"
                   && x.Passport_Status != "Cancelada"
                   && x.Recarga_Status != "Cancelada"
                   && x.Servicio_Status != "Cancelado"
                   && x.Ticket_Status != "Cancelada"
                   && x.Mercado_Status != Mercado.STATUS_CANCELADA
                   && x.PaqueteTuristico_Status != PaqueteTuristico.STATUS_CANCELADA
                   && x.Ticket_PaqueteTuristicoId == null
                   && x.Servicio_PaqueteTuristicoId == null)
                   .Where(x => x.Order_MinoristaId == null)
                   .ToListAsync();

                    //Cash 
                    ventastipopago[0] += ventasEmpleado.Where(x => x.Type == "Cash").Sum(x => x.valorPagado);
                    canttipopago[0] += ventasEmpleado.Where(x => x.Type == "Cash").Count();
                    //Zelle
                    ventastipopago[1] += ventasEmpleado.Where(x => x.Type == "Zelle").Sum(x => x.valorPagado);
                    canttipopago[1] += ventasEmpleado.Where(x => x.Type == "Zelle").Count();
                    //Cheque
                    ventastipopago[2] += ventasEmpleado.Where(x => x.Type == "Cheque").Sum(x => x.valorPagado);
                    canttipopago[2] += ventasEmpleado.Where(x => x.Type == "Cheque").Count();
                    //Crédito o Débito
                    ventastipopago[3] += ventasEmpleado.Where(x => x.Type == "Crédito o Débito").Sum(x => x.valorPagado);
                    canttipopago[3] += ventasEmpleado.Where(x => x.Type == "Crédito o Débito").Count();
                    //Transferencia Bancaria
                    ventastipopago[4] += ventasEmpleado.Where(x => x.Type == "Transferencia Bancaria").Sum(x => x.valorPagado);
                    canttipopago[4] += ventasEmpleado.Where(x => x.Type == "Transferencia Bancaria").Count();
                    //Web
                    ventastipopago[5] += ventasEmpleado.Where(x => x.Type == "Web").Sum(x => x.valorPagado);
                    canttipopago[5] += ventasEmpleado.Where(x => x.Type == "Web").Count();
                    //Money Order
                    ventastipopago[6] += ventasEmpleado.Where(x => x.Type == "Money Order").Sum(x => x.valorPagado);
                    canttipopago[6] += ventasEmpleado.Where(x => x.Type == "Money Order").Count();
                    //Crédito de Consumo
                    ventastipopago[7] += ventasEmpleado.Where(x => x.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                    canttipopago[7] += ventasEmpleado.Where(x => x.Type == "Crédito de Consumo").Count();
                    //Cash App
                    ventastipopago[8] += ventasEmpleado.Where(x => x.Type == "Cash App").Sum(x => x.valorPagado);
                    canttipopago[8] += ventasEmpleado.Where(x => x.Type == "Cash App").Count();


                    List<string> noOrders = new List<string>();
                    //var empleados = _context.User.Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && (x.Type == "Agencia" || x.Type == "Empleado")).Select(x => new { x.UserId, x.Name, x.LastName });
                    foreach (var ventas in ventasEmpleado.GroupBy(x => new { x.UserId, x.User_Name, x.User_LastName }))
                    {

                        decimal totaldeudatbl = 0;
                        decimal totalimportetbl = 0;
                        decimal totalpagadotbl = 0;
                        decimal total_Tpagadotbl = 0;
                        decimal totalcantidadtbl = 0;

                        float[] width = { (float)3, 2, 2, 2, 2, 2 };
                        PdfPTable aux = new PdfPTable(width);
                        aux.WidthPercentage = 100;

                        PdfPCell cell1 = new PdfPCell();
                        cell1.Border = 1;
                        PdfPCell cell2 = new PdfPCell();
                        cell2.Border = 1;
                        PdfPCell cell3 = new PdfPCell();
                        cell3.Border = 1;
                        PdfPCell cell4 = new PdfPCell();
                        cell4.Border = 1;
                        PdfPCell cell5 = new PdfPCell();
                        cell5.Border = 1;
                        PdfPCell cell6 = new PdfPCell();
                        cell6.Border = 1;

                        doc.Add(new Phrase(ventas.Key.User_Name + " " + ventas.Key.User_LastName, headFont));

                        cell1.AddElement(new Phrase("Tipo", headFont));
                        cell2.AddElement(new Phrase("Cantidad", headFont));
                        cell3.AddElement(new Phrase("Pagado", headFont));
                        cell4.AddElement(new Phrase("T. Pagado", headFont));
                        cell5.AddElement(new Phrase("Total", headFont));
                        cell6.AddElement(new Phrase("Debe", headFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Remesas
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        var remesas = ventas.Where(x => x.RemittanceId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.RemittanceId, x.Remesa_Amount, x.Remesa_Date, x.Remesa_Number, x.Remesa_Status, x.Remesa_Pagado });

                        decimal pagado = remesas.Sum(x => x.Sum(y => y.valorPagado));
                        decimal totalPagado = remesas.Sum(x => (decimal)x.Key.Remesa_Pagado);
                        decimal total = remesas.Sum(x => (decimal)x.Key.Remesa_Amount);
                        decimal deuda = total - totalPagado;
                        decimal cantidad = remesas.Count();
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += remesas.Where(x => !noOrders.Contains(x.Key.Remesa_Number)).Sum(x => (decimal)x.Key.Remesa_Amount);
                        totalpagado += pagado;
                        total_Tpagado += remesas.Where(x => !noOrders.Contains(x.Key.Remesa_Number)).Sum(x => (decimal)x.Key.Remesa_Pagado);
                        totalcantidad += remesas.Where(x => !noOrders.Contains(x.Key.Remesa_Number)).Count();

                        noOrders = noOrders.Union(remesas.Select(x => x.Key.Remesa_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Remesa", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Paquete Turístico
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        var paqueteTuristico = ventas.Where(x => x.PaqueteTuristicoId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.PaqueteTuristicoId, x.PaqueteTuristico_Amount, x.PaqueteTuristico_Date, x.PaqueteTuristico_Number, x.PaqueteTuristico_Status, x.PaqueteTuristico_Pagado });

                        pagado = paqueteTuristico.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = paqueteTuristico.Sum(x => (decimal)x.Key.PaqueteTuristico_Pagado);
                        total = paqueteTuristico.Sum(x => (decimal)x.Key.PaqueteTuristico_Amount);
                        deuda = total - totalPagado;
                        cantidad = paqueteTuristico.Count();
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += paqueteTuristico.Where(x => !noOrders.Contains(x.Key.PaqueteTuristico_Number)).Sum(x => (decimal)x.Key.PaqueteTuristico_Amount);
                        totalpagado += pagado;
                        total_Tpagado += paqueteTuristico.Where(x => !noOrders.Contains(x.Key.PaqueteTuristico_Number)).Sum(x => (decimal)x.Key.PaqueteTuristico_Pagado);
                        totalcantidad += paqueteTuristico.Where(x => !noOrders.Contains(x.Key.PaqueteTuristico_Number)).Count();

                        noOrders = noOrders.Union(paqueteTuristico.Select(x => x.Key.PaqueteTuristico_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Paquete Turístico", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envios
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var envios = ventas.Where(x => x.OrderId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.OrderId, x.Order_Amount, x.Order_Date, x.Order_MinoristaId, x.Order_Number, x.Order_Pagado, x.Order_Status, x.Order_Type }).Where(x => x.Key.Order_Type != "Remesas" && x.Key.Order_Type != "Combo");
                        pagado = envios.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = envios.Sum(x => (decimal)x.Key.Order_Pagado);
                        total = envios.Sum(x => (decimal)x.Key.Order_Amount);
                        deuda = total - totalPagado;
                        cantidad = envios.Count();

                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += envios.Where(x => !noOrders.Contains(x.Key.Order_Number)).Sum(x => (decimal)x.Key.Order_Amount);
                        totalpagado += pagado;
                        total_Tpagado += envios.Where(x => !noOrders.Contains(x.Key.Order_Number)).Sum(x => (decimal)x.Key.Order_Pagado);
                        totalcantidad += envios.Where(x => !noOrders.Contains(x.Key.Order_Number)).Count();

                        noOrders = noOrders.Union(envios.Select(x => x.Key.Order_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envíos", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envios Cubiqs
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var enviosCubiq = ventas.Where(x => x.OrderCubiqId != null && x.Type != "Crédito de Consumo" && x.Type != "Cubiq").GroupBy(x => new { x.OrderCubiqId, x.Cubiq_Amount, x.Cubiq_Date, x.Cubiq_Number, x.Cubiq_Pagado, x.Cubiq_Status });
                        pagado = enviosCubiq.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviosCubiq.Sum(x => (decimal)x.Key.Cubiq_Pagado);
                        total = enviosCubiq.Sum(x => (decimal)x.Key.Cubiq_Amount);
                        deuda = total - totalPagado;
                        cantidad = enviosCubiq.Count();

                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Cubiq_Number)).Sum(x => (decimal)x.Key.Cubiq_Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Cubiq_Number)).Sum(x => (decimal)x.Key.Cubiq_Pagado);
                        totalcantidad += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Cubiq_Number)).Count();

                        noOrders = noOrders.Union(enviosCubiq.Select(x => x.Key.Cubiq_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envíos Carga AM", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Combos
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var combos = ventas.Where(x => x.OrderId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.OrderId, x.Order_Amount, x.Order_Date, x.Order_MinoristaId, x.Order_Number, x.Order_Pagado, x.Order_Status, x.Order_Type }).Where(x => x.Key.Order_Type == "Combo");
                        pagado = combos.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = combos.Sum(x => (decimal)x.Key.Order_Pagado);
                        total = combos.Sum(x => (decimal)x.Key.Order_Amount);
                        deuda = total - totalPagado;
                        cantidad = 0;
                        int cantidad2 = 0;

                        foreach (var item in combos.Select(x => x.Key))
                        {
                            var bags = _context.Bag.Include(x => x.BagItems).Where(x => x.OrderId == item.OrderId);
                            foreach (var x in bags)
                            {
                                if (!noOrders.Contains(item.Order_Number))
                                    cantidad2 += x.BagItems.Count();

                                cantidad += x.BagItems.Count();
                            }
                        }
                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += combos.Where(x => !noOrders.Contains(x.Key.Order_Number)).Sum(x => (decimal)x.Key.Order_Amount);
                        totalpagado += pagado;
                        total_Tpagado += combos.Where(x => !noOrders.Contains(x.Key.Order_Number)).Sum(x => (decimal)x.Key.Order_Pagado);
                        totalcantidad += cantidad2;

                        noOrders = noOrders.Union(combos.Select(x => x.Key.Order_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Combos", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Recarga
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var recarga = ventas.Where(x => x.RechargueId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.RechargueId, x.Recarga_Date, x.Recarga_Import, x.Recarga_Number, x.Recarga_Pagado, x.Recarga_Status });
                        pagado = recarga.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = recarga.Sum(x => (decimal)x.Key.Recarga_Pagado);
                        total = recarga.Sum(x => (decimal)x.Key.Recarga_Import);
                        deuda = total - totalPagado;
                        cantidad = recarga.Count();
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += recarga.Where(x => !noOrders.Contains(x.Key.Recarga_Number)).Sum(x => (decimal)x.Key.Recarga_Import);
                        totalpagado += pagado;
                        total_Tpagado += recarga.Where(x => !noOrders.Contains(x.Key.Recarga_Number)).Sum(x => (decimal)x.Key.Recarga_Pagado);
                        totalcantidad += recarga.Where(x => !noOrders.Contains(x.Key.Recarga_Number)).Count();

                        noOrders = noOrders.Union(recarga.Select(x => x.Key.Recarga_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Recargas", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envio Maritimo
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var enviomaritimo = ventas.Where(x => x.EnvioMaritimoId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.EnvioMaritimoId, x.Maritimo_Amount, x.Maritimo_Date, x.Maritimo_Number, x.Maritimo_Pagado, x.Maritimo_Status });
                        pagado = enviomaritimo.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviomaritimo.Sum(x => (decimal)x.Key.Maritimo_Pagado);
                        total = enviomaritimo.Sum(x => (decimal)x.Key.Maritimo_Amount);
                        deuda = total - totalPagado;
                        cantidad = enviomaritimo.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Maritimo_Number)).Sum(x => (decimal)x.Key.Maritimo_Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Maritimo_Number)).Sum(x => (decimal)x.Key.Maritimo_Pagado);
                        totalcantidad += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Maritimo_Number)).Count();

                        noOrders = noOrders.Union(enviomaritimo.Select(x => x.Key.Maritimo_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envío Marítimo", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);
                        //Envio Caribe
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var enviocaribe = ventas.Where(x => x.EnvioCaribeId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.EnvioCaribeId, x.Caribe_Amount, x.Caribe_Date, x.Caribe_Number, x.Caribe_Pagado, x.Caribe_Status });
                        pagado = enviocaribe.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviocaribe.Sum(x => (decimal)x.Key.Caribe_Pagado);
                        total = enviocaribe.Sum(x => (decimal)x.Key.Caribe_Amount);
                        deuda = total - totalPagado;
                        cantidad = enviocaribe.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += enviocaribe.Where(x => !noOrders.Contains(x.Key.Caribe_Number)).Sum(x => (decimal)x.Key.Caribe_Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviocaribe.Where(x => !noOrders.Contains(x.Key.Caribe_Number)).Sum(x => (decimal)x.Key.Caribe_Pagado);
                        totalcantidad += enviocaribe.Where(x => !noOrders.Contains(x.Key.Caribe_Number)).Count();

                        noOrders = noOrders.Union(enviocaribe.Select(x => x.Key.Caribe_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envío Caribe", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);
                        //Pasaporte
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var pasaporte = ventas.Where(x => x.PassportId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.PassportId, x.Passport_Amount, x.Passport_Date, x.Passport_Number, x.Passport_Pagado, x.Passport_Status });
                        pagado = pasaporte.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = pasaporte.Sum(x => (decimal)x.Key.Passport_Pagado);
                        total = pasaporte.Sum(x => (decimal)x.Key.Passport_Amount);
                        deuda = total - totalPagado;
                        cantidad = pasaporte.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += pasaporte.Where(x => !noOrders.Contains(x.Key.Passport_Number)).Sum(x => (decimal)x.Key.Passport_Amount);
                        totalpagado += pagado;
                        total_Tpagado += pasaporte.Where(x => !noOrders.Contains(x.Key.Passport_Number)).Sum(x => (decimal)x.Key.Passport_Pagado);
                        totalcantidad += pasaporte.Where(x => !noOrders.Contains(x.Key.Passport_Number)).Count();

                        noOrders = noOrders.Union(pasaporte.Select(x => x.Key.Passport_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Pasaporte", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Otros Servicios
                        var servicios = ventas.Where(x => x.ServicioId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.ServicioId, x.TipoServicioId, x.TipoServicio_Nombre, x.Servicio_Amount, x.Servicio_Date, x.Servicio_Number, x.Servicio_Pagado, x.Servicio_Status });
                        foreach (var item in servicios.GroupBy(x => new { x.Key.TipoServicioId, x.Key.TipoServicio_Nombre }))
                        {

                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;

                            pagado = item.Sum(x => x.Sum(y => y.valorPagado));
                            totalPagado = item.Sum(x => (decimal)x.Key.Servicio_Pagado);
                            total = item.Sum(x => (decimal)x.Key.Servicio_Amount);
                            deuda = total - totalPagado;
                            cantidad = item.Count();

                            totaldeudatbl += deuda;
                            totalimportetbl += total;
                            totalpagadotbl += pagado;
                            total_Tpagadotbl += totalPagado;
                            totalcantidadtbl += cantidad;

                            grantotal += item.Where(x => !noOrders.Contains(x.Key.Servicio_Number)).Sum(x => (decimal)x.Key.Servicio_Amount);
                            totalpagado += pagado;
                            total_Tpagado += item.Where(x => !noOrders.Contains(x.Key.Servicio_Number)).Sum(x => (decimal)x.Key.Servicio_Pagado);
                            totalcantidad += item.Where(x => !noOrders.Contains(x.Key.Servicio_Number)).Count();

                            noOrders = noOrders.Union(item.Select(x => x.Key.Servicio_Number).ToList()).ToList();

                            cell1.AddElement(new Phrase(item.Key.TipoServicio_Nombre, normalFont));
                            cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                            cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                            cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                            cell5.AddElement(new Phrase(total.ToString(), normalFont));
                            cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                            aux.AddCell(cell1);
                            aux.AddCell(cell2);
                            aux.AddCell(cell3);
                            aux.AddCell(cell4);
                            aux.AddCell(cell5);
                            aux.AddCell(cell6);
                        }

                        //reserva
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var reserva = ventas.Where(x => x.TicketId != null && !((bool)x.ClientIsCarrier) && x.Type != "Crédito de Consumo").GroupBy(x => new { x.TicketId, x.Ticket_Amount, x.Ticket_Date, x.Ticket_Number, x.Ticket_Pagado, x.Ticket_Status });
                        pagado = reserva.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = reserva.Sum(x => (decimal)x.Key.Ticket_Pagado);
                        total = reserva.Sum(x => (decimal)x.Key.Ticket_Amount);
                        deuda = total - totalPagado;
                        cantidad = reserva.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += reserva.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Sum(x => (decimal)x.Key.Ticket_Amount);
                        totalpagado += pagado;
                        total_Tpagado += reserva.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Sum(x => (decimal)x.Key.Ticket_Pagado);
                        totalcantidad += reserva.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Count();

                        noOrders = noOrders.Union(reserva.Select(x => x.Key.Ticket_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Reserva", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //reserva Carrier
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var reservaCarrier = ventas.Where(x => x.TicketId != null && (bool)x.ClientIsCarrier && x.Type != "Crédito de Consumo").GroupBy(x => new { x.TicketId, x.Ticket_Amount, x.Ticket_Date, x.Ticket_Number, x.Ticket_Pagado, x.Ticket_Status });
                        pagado = reservaCarrier.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = reservaCarrier.Sum(x => (decimal)x.Key.Ticket_Pagado);
                        total = reservaCarrier.Sum(x => (decimal)x.Key.Ticket_Amount);
                        deuda = total - totalPagado;
                        cantidad = reservaCarrier.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += reservaCarrier.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Sum(x => (decimal)x.Key.Ticket_Amount);
                        totalpagado += pagado;
                        total_Tpagado += reservaCarrier.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Sum(x => (decimal)x.Key.Ticket_Pagado);
                        totalcantidad += reservaCarrier.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Count();

                        noOrders = noOrders.Union(reservaCarrier.Select(x => x.Key.Ticket_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Reserva Carrier", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        // Mercado
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var mercado = ventas.Where(x => x.MercadoId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.MercadoId, x.Mercado_Amount, x.Mercado_Date, x.Mercado_Number, x.Mercado_Pagado, x.Mercado_Status });
                        pagado = mercado.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = mercado.Sum(x => (decimal)x.Key.Mercado_Pagado);
                        total = mercado.Sum(x => (decimal)x.Key.Mercado_Amount);
                        deuda = total - totalPagado;
                        cantidad = mercado.Count();
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += mercado.Where(x => !noOrders.Contains(x.Key.Mercado_Number)).Sum(x => (decimal)x.Key.Mercado_Amount);
                        totalpagado += pagado;
                        total_Tpagado += mercado.Where(x => !noOrders.Contains(x.Key.Mercado_Number)).Sum(x => (decimal)x.Key.Mercado_Pagado);
                        totalcantidad += mercado.Where(x => !noOrders.Contains(x.Key.Mercado_Number)).Count();

                        noOrders = noOrders.Union(mercado.Select(x => x.Key.Mercado_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Mercado", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        // Añado el total
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;


                        cell1.AddElement(new Phrase("Totales", headFont));
                        cell2.AddElement(new Phrase(totalcantidadtbl.ToString(), headFont));
                        cell3.AddElement(new Phrase(totalpagadotbl.ToString(), headFont));
                        cell4.AddElement(new Phrase(total_Tpagadotbl.ToString(), headFont));
                        cell5.AddElement(new Phrase(totalimportetbl.ToString(), headFont));
                        cell6.AddElement(new Phrase(totaldeudatbl.ToString(), headFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        // Añado la tabla al documento
                        doc.Add(aux);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        #region // VENTAS POR TIPO DE PAGO CLIENTE

                        doc.Add(new Phrase($"VENTAS POR TIPO DE PAGO - {ventas.Key.User_Name} {ventas.Key.User_LastName}", headFont));
                        float[] columns = { 3, 2, 2 };
                        PdfPTable tablePagoCliente = new PdfPTable(columns);
                        tablePagoCliente.WidthPercentage = 100;
                        PdfPCell cellpago1 = new PdfPCell();
                        cellpago1.Border = 1;
                        PdfPCell cellpago2 = new PdfPCell();
                        cellpago2.Border = 1;
                        PdfPCell cellpago3 = new PdfPCell();
                        cellpago3.Border = 1;

                        cellpago1.AddElement(new Phrase("Tipo de Pago", headFont));
                        cellpago2.AddElement(new Phrase("Cantidad", headFont));
                        cellpago3.AddElement(new Phrase("Importe", headFont));
                        tablePagoCliente.AddCell(cellpago1);
                        tablePagoCliente.AddCell(cellpago2);
                        tablePagoCliente.AddCell(cellpago3);
                        cellpago1 = new PdfPCell();
                        cellpago1.Border = 1;
                        cellpago2 = new PdfPCell();
                        cellpago2.Border = 1;
                        cellpago3 = new PdfPCell();
                        cellpago3.Border = 1;
                        foreach (var item in ventas.GroupBy(x => new { x.tipoPagoId, x.Type }))
                        {
                            cellpago1.AddElement(new Phrase(item.Key.Type, normalFont));
                            cellpago2.AddElement(new Phrase(item.Count().ToString(), normalFont));
                            cellpago3.AddElement(new Phrase(item.Sum(x => x.valorPagado).ToString(), normalFont));
                            tablePagoCliente.AddCell(cellpago1);
                            tablePagoCliente.AddCell(cellpago2);
                            tablePagoCliente.AddCell(cellpago3);
                            cellpago1 = new PdfPCell();
                            cellpago1.Border = 0;
                            cellpago2 = new PdfPCell();
                            cellpago2.Border = 0;
                            cellpago3 = new PdfPCell();
                            cellpago3.Border = 0;
                        }
                        doc.Add(tablePagoCliente);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                        #endregion
                    }

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    PdfPTable tabletipopago = new PdfPTable(columnWidthstipopago);
                    tabletipopago.WidthPercentage = 100;
                    PdfPCell celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 1;
                    PdfPCell celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 1;
                    PdfPCell celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 1;

                    celltipopago1.AddElement(new Phrase("Tipo de Pago", headFont));
                    celltipopago2.AddElement(new Phrase("Cantidad", headFont));
                    celltipopago3.AddElement(new Phrase("Importe", headFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 1;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 1;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 1;

                    celltipopago1.AddElement(new Phrase("Cash", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[0].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[0].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Zelle", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[1].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[1].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Cheque", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[2].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[2].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Crédito o Débito", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[3].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[3].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Transferencia Bancaria", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[4].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[4].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Web", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[5].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[5].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Money Order", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[6].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[6].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Crédito de Consumo", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[7].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[7].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Cash App", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[8].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[8].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    doc.Add(tabletipopago);
                    #endregion

                    #region //GRAN TOTAL
                    float[] columnwhidts2 = { 5, 5 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;
                    doc.Add(Chunk.NEWLINE);

                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph totalcantidadaux = new Paragraph("Cantidad: ", headFont2);
                    totalcantidadaux.AddSpecial(new Phrase(totalcantidad.ToString(), normalFont2));
                    totalcantidadaux.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(totalcantidadaux);
                    Paragraph porpagar = new Paragraph("Pagado: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalpagado.ToString(), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);
                    Paragraph deudaaux = new Paragraph("Pendiente a pago: ", headFont2);
                    deudaaux.AddSpecial(new Phrase("$ " + (grantotal - total_Tpagado).ToString(), normalFont2));
                    deudaaux.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deudaaux);

                    #region Facturas Cobradas
                    var facturasCobradas = _context.Facturas
                    .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                    .Include(x => x.RegistroPagos).ThenInclude(x => x.User)
                    .Where(x => x.agencyId == agency.AgencyId && x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date));
                    int noFacturas = await facturasCobradas.CountAsync();
                    decimal montoFacturas = await facturasCobradas.SumAsync(x => x.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date).Sum(y => y.valorPagado));
                    if (noFacturas > 0)
                    {
                        Paragraph cobradoFacturas = new Paragraph("Cobrado por contabilidad: ", headFont2);
                        cobradoFacturas.AddSpecial(new Phrase($"{noFacturas} facturas -- ${montoFacturas}", normalFont2));
                        cobradoFacturas.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(cobradoFacturas);
                    }

                    #endregion

                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    doc.Add(Chunk.NEWLINE);

                    if (facturasCobradas.Any())
                    {
                        doc.Add(new Phrase("COBROS POR CONTABILIDAD", headFont));
                        float[] widthTable = { 3, 2, 1, 2 };
                        PdfPTable table = new PdfPTable(widthTable);
                        table.WidthPercentage = 100;

                        PdfPCell cell = new PdfPCell(new Phrase("Empleado", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Factura", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Monto", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Tipo Pago", headFont));
                        cell.Border = PdfPCell.BOTTOM_BORDER;
                        table.AddCell(cell);

                        foreach (var factura in facturasCobradas)
                        {
                            foreach (var pago in factura.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date))
                            {
                                cell = new PdfPCell(new Phrase(pago.User.FullName, normalFont));
                                cell.Border = PdfPCell.BOTTOM_BORDER;
                                table.AddCell(cell);
                                cell = new PdfPCell(new Phrase(factura.NoFactura, normalFont));
                                cell.Border = PdfPCell.BOTTOM_BORDER;
                                table.AddCell(cell);
                                cell = new PdfPCell(new Phrase(pago.valorPagado.ToString("0.00"), normalFont));
                                cell.Border = PdfPCell.BOTTOM_BORDER;
                                table.AddCell(cell);
                                cell = new PdfPCell(new Phrase(pago.tipoPago.Type, normalFont));
                                cell.Border = PdfPCell.BOTTOM_BORDER;
                                table.AddCell(cell);
                            }
                        }
                        doc.Add(table);
                        doc.Add(Chunk.NEWLINE);
                    }

                    var lbsByEmpoyee = ventasEmpleado.Where(x => x.Order_CantLb > 0 || x.Order_CantLbMedicina > 0).GroupBy(x => new { x.UserId, x.User_Name, x.User_LastName });

                    if (lbsByEmpoyee.Any())
                    {
                        doc.Add(new Phrase("Cantidad de libras por empleado", headFont));
                        PdfPTable tbl = new PdfPTable(new float[] { 2, 1, 1 });
                        tbl.WidthPercentage = 100;

                        new List<string> { "Empleado", "Paquete", "Medicina" }
                        .ForEach(x =>
                        {
                            tbl.AddCell(new PdfPCell(new Phrase(x, headFont)));
                        });

                        foreach (var user in lbsByEmpoyee)
                        {
                            decimal paquete = user.Where(x => x.Order_Type == "Paquete").Sum(x => x.Order_CantLb) ?? decimal.Zero;
                            decimal medicina = user.Where(x => x.Order_Type == "Medicinas").Sum(x => x.Order_CantLb) ?? decimal.Zero;
                            medicina = user.Where(x => x.Order_Type == "Paquete").Sum(x => x.Order_CantLbMedicina) ?? decimal.Zero;

                            tbl.AddCell(new PdfPCell(new Phrase($"{user.Key.User_Name} {user.Key.User_LastName}", headFont)));
                            tbl.AddCell(new PdfPCell(new Phrase(paquete.ToString("0.00") + " lb", headFont)));
                            tbl.AddCell(new PdfPCell(new Phrase(medicina.ToString("0.00") + " lb", headFont)));
                        }
                        doc.Add(tbl);
                    }

                    // libras carga am
                    var ordersCargaId = ventasEmpleado.Where(x => x.OrderCubiqId != null).GroupBy(x => x.OrderCubiqId).Select(x => x.Key);
                    var lbsByEmployeeCarga = _context.OrderCubiqs.Where(x => ordersCargaId.Contains(x.OrderCubiqId) && x.AgencyId == agency.AgencyId)
                        .Select(x => new
                        {
                            UserId = x.UserId,
                            Name = x.User.Name,
                            LastName = x.User.LastName,
                            TotalLibras = x.Paquetes.Sum(y => y.PesoLb)
                        })
                        .GroupBy(x => x.UserId);

                    if (lbsByEmployeeCarga.Any())
                    {
                        doc.Add(new Phrase("Cantidad de libras por empleado (Carga AM)", headFont));
                        PdfPTable tbl = new PdfPTable(new float[] { 2, 1 });
                        tbl.WidthPercentage = 100;

                        new List<string> { "Empleado", "Libras" }
                        .ForEach(x =>
                        {
                            tbl.AddCell(new PdfPCell(new Phrase(x, headFont)));
                        });

                        foreach (var item in lbsByEmployeeCarga)
                        {
                            var first = item.ElementAt(0);
                            string user = $"{first.Name} {first.LastName}";
                            decimal totalLibras = item.Sum(x => x.TotalLibras);

                            tbl.AddCell(new PdfPCell(new Phrase(user, headFont)));
                            tbl.AddCell(new PdfPCell(new Phrase(totalLibras.ToString("0.00") + " lb", headFont)));
                        }
                        doc.Add(tbl);
                    }

                    doc.Close();
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        [Authorize]
        public async Task<object> PDFVentasporempleado2(string strdate)
        {
            using (MemoryStream MStream = new MemoryStream())
            {
                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {

                    var role = AgenciaAuthorize.getRole(User, _context);
                    var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
                    var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId);

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

                    Agency agency = aAgency.FirstOrDefault();
                    Models.Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        celllogo.AddElement(imagelogo);
                    }


                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    var auxDate = strdate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateIni = DateTime.Parse(auxDate[0], culture);
                    var dateFin = DateTime.Parse(auxDate[1], culture);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = "";
                    if (dateIni.Date == dateFin.Date)
                    {
                        texto = dateIni.Date.ToShortDateString();
                    }
                    else
                    {
                        texto = dateIni.Date.ToShortDateString() + " a " + dateFin.Date.ToShortDateString();
                    }
                    Paragraph parPaq = new Paragraph("Liquidación por empleado del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);
                    /*
                     * 0: cash
                     * 1: zelle
                     * 2: cheque
                     * 3: credito o debito
                     * 4: transferencia bancaria
                     * 5: web
                     * 6: Money Order
                     * 7: Crédito de Consumo
                     * */
                    decimal[] ventastipopago = new decimal[9];
                    int[] canttipopago = new int[9];
                    decimal grantotal = 0;
                    decimal totalpagado = 0;
                    decimal total_Tpagado = 0;
                    decimal totalcantidad = 0;

                    var ventasEmpleado = await _context.RegistroPagos
                    .Include(x => x.User)
                    .Include(x => x.tipoPago)
                    .Include(x => x.EnvioCaribe).ThenInclude(x => x.RegistroPagos)
                    .Include(x => x.EnvioMaritimo).ThenInclude(x => x.RegistroPagos)
                    .Include(x => x.Order).ThenInclude(x => x.Pagos)
                    .Include(x => x.Order).ThenInclude(x => x.Bag).ThenInclude(x => x.BagItems)
                    .Include(x => x.OrderCubiq).ThenInclude(x => x.RegistroPagos)
                    .Include(x => x.Passport).ThenInclude(x => x.RegistroPagos)
                    .Include(x => x.Rechargue).ThenInclude(x => x.RegistroPagos)
                    .Include(x => x.Servicio).ThenInclude(x => x.RegistroPagos)
                    .Include(x => x.Servicio).ThenInclude(x => x.tipoServicio)
                    .Include(x => x.Ticket).ThenInclude(x => x.RegistroPagos)
                    .Where(x => x.AgencyId == agency.AgencyId && x.date.ToLocalTime().Date >= dateIni.Date && x.date.ToLocalTime().Date <= dateFin.Date && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                    .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada")
                    .Where(x => x.Order.Minorista == null)
                    .ToListAsync();

                    //Cash 
                    ventastipopago[0] += ventasEmpleado.Where(x => x.tipoPago.Type == "Cash").Sum(x => x.valorPagado);
                    canttipopago[0] += ventasEmpleado.Where(x => x.tipoPago.Type == "Cash").Count();
                    //Zelle
                    ventastipopago[1] += ventasEmpleado.Where(x => x.tipoPago.Type == "Zelle").Sum(x => x.valorPagado);
                    canttipopago[1] += ventasEmpleado.Where(x => x.tipoPago.Type == "Zelle").Count();
                    //Cheque
                    ventastipopago[2] += ventasEmpleado.Where(x => x.tipoPago.Type == "Cheque").Sum(x => x.valorPagado);
                    canttipopago[2] += ventasEmpleado.Where(x => x.tipoPago.Type == "Cheque").Count();
                    //Crédito o Débito
                    ventastipopago[3] += ventasEmpleado.Where(x => x.tipoPago.Type == "Crédito o Débito").Sum(x => x.valorPagado);
                    canttipopago[3] += ventasEmpleado.Where(x => x.tipoPago.Type == "Crédito o Débito").Count();
                    //Transferencia Bancaria
                    ventastipopago[4] += ventasEmpleado.Where(x => x.tipoPago.Type == "Transferencia Bancaria").Sum(x => x.valorPagado);
                    canttipopago[4] += ventasEmpleado.Where(x => x.tipoPago.Type == "Transferencia Bancaria").Count();
                    //Web
                    ventastipopago[5] += ventasEmpleado.Where(x => x.tipoPago.Type == "Web").Sum(x => x.valorPagado);
                    canttipopago[5] += ventasEmpleado.Where(x => x.tipoPago.Type == "Web").Count();
                    //Money Order
                    ventastipopago[6] += ventasEmpleado.Where(x => x.tipoPago.Type == "Money Order").Sum(x => x.valorPagado);
                    canttipopago[6] += ventasEmpleado.Where(x => x.tipoPago.Type == "Money Order").Count();
                    //Crédito de Consumo
                    ventastipopago[7] += ventasEmpleado.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                    canttipopago[7] += ventasEmpleado.Where(x => x.tipoPago.Type == "Crédito de Consumo").Count();
                    //Cash App
                    ventastipopago[8] += ventasEmpleado.Where(x => x.tipoPago.Type == "Cash App").Sum(x => x.valorPagado);
                    canttipopago[8] += ventasEmpleado.Where(x => x.tipoPago.Type == "Cash App").Count();


                    List<string> noOrders = new List<string>();
                    //var empleados = _context.User.Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && (x.Type == "Agencia" || x.Type == "Empleado")).Select(x => new { x.UserId, x.Name, x.LastName });
                    foreach (var ventas in ventasEmpleado.GroupBy(x => x.User))
                    {

                        decimal totaldeudatbl = 0;
                        decimal totalimportetbl = 0;
                        decimal totalpagadotbl = 0;
                        decimal total_Tpagadotbl = 0;
                        decimal totalcantidadtbl = 0;

                        float[] width = { (float)3, 2, 2, 2, 2, 2 };
                        PdfPTable aux = new PdfPTable(width);
                        aux.WidthPercentage = 100;

                        PdfPCell cell1 = new PdfPCell();
                        cell1.Border = 1;
                        PdfPCell cell2 = new PdfPCell();
                        cell2.Border = 1;
                        PdfPCell cell3 = new PdfPCell();
                        cell3.Border = 1;
                        PdfPCell cell4 = new PdfPCell();
                        cell4.Border = 1;
                        PdfPCell cell5 = new PdfPCell();
                        cell5.Border = 1;
                        PdfPCell cell6 = new PdfPCell();
                        cell6.Border = 1;

                        doc.Add(new Phrase(ventas.Key.Name + " " + ventas.Key.LastName, headFont));

                        cell1.AddElement(new Phrase("Tipo", headFont));
                        cell2.AddElement(new Phrase("Cantidad", headFont));
                        cell3.AddElement(new Phrase("Pagado", headFont));
                        cell4.AddElement(new Phrase("T. Pagado", headFont));
                        cell5.AddElement(new Phrase("Total", headFont));
                        cell6.AddElement(new Phrase("Debe", headFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Remesas
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        var remesas = ventas.Where(x => x.Remittance != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.Remittance);

                        decimal pagado = remesas.Sum(x => x.Sum(y => y.valorPagado));
                        decimal totalPagado = remesas.Sum(x => x.Key.Pagos.Sum(y => y.valorPagado));
                        decimal total = remesas.Sum(x => x.Key.Amount);
                        decimal deuda = total - totalPagado;
                        decimal cantidad = remesas.Count();
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += remesas.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Amount);
                        totalpagado += pagado;
                        total_Tpagado += remesas.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Pagos.Sum(y => y.valorPagado));
                        totalcantidad += remesas.Where(x => !noOrders.Contains(x.Key.Number)).Count();

                        noOrders = noOrders.Union(remesas.Select(x => x.Key.Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Remesa", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envios
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var envios = ventas.Where(x => x.Order != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.Order).Where(x => x.Key.Type != "Remesas" && x.Key.Type != "Combo");
                        pagado = envios.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = envios.Sum(x => x.Key.Pagos.Sum(y => y.valorPagado));
                        total = envios.Sum(x => x.Key.Amount);
                        deuda = total - totalPagado;
                        cantidad = envios.Count();

                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += envios.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Amount);
                        totalpagado += pagado;
                        total_Tpagado += envios.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Pagos.Sum(y => y.valorPagado));
                        totalcantidad += envios.Where(x => !noOrders.Contains(x.Key.Number)).Count();

                        noOrders = noOrders.Union(envios.Select(x => x.Key.Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envíos", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envios Cubiqs
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var enviosCubiq = ventas.Where(x => x.OrderCubiq != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.OrderCubiq);
                        pagado = enviosCubiq.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviosCubiq.Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        total = enviosCubiq.Sum(x => x.Key.Amount);
                        deuda = total - totalPagado;
                        cantidad = enviosCubiq.Count();

                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        totalcantidad += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Number)).Count();

                        noOrders = noOrders.Union(enviosCubiq.Select(x => x.Key.Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envíos Carga AM", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Combos
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var combos = ventas.Where(x => x.Order != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.Order).Where(x => x.Key.Type == "Combo");
                        pagado = combos.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = combos.Sum(x => x.Key.Pagos.Sum(y => y.valorPagado));
                        total = combos.Sum(x => x.Key.Amount);
                        deuda = total - totalPagado;
                        cantidad = 0;
                        int cantidad2 = 0;
                        foreach (var item in combos.Select(x => x.Key))
                        {
                            var bags = _context.Bag.Include(x => x.BagItems).Where(x => x.OrderId == item.OrderId);
                            foreach (var x in bags)
                            {
                                if (!noOrders.Contains(item.Number))
                                    cantidad2 += x.BagItems.Count();

                                cantidad += x.BagItems.Count();
                            }
                        }
                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += combos.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Amount);
                        totalpagado += pagado;
                        total_Tpagado += combos.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Pagos.Sum(y => y.valorPagado));
                        totalcantidad += cantidad2;

                        noOrders = noOrders.Union(combos.Select(x => x.Key.Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Combos", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Recarga
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var recarga = ventas.Where(x => x.Rechargue != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.Rechargue);
                        pagado = recarga.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = recarga.Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        total = recarga.Sum(x => x.Key.Import);
                        deuda = total - totalPagado;
                        cantidad = recarga.Count();
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += recarga.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Import);
                        totalpagado += pagado;
                        total_Tpagado += recarga.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        totalcantidad += recarga.Where(x => !noOrders.Contains(x.Key.Number)).Count();

                        noOrders = noOrders.Union(recarga.Select(x => x.Key.Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Recargas", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envio Maritimo
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var enviomaritimo = ventas.Where(x => x.EnvioMaritimo != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.EnvioMaritimo);
                        pagado = enviomaritimo.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviomaritimo.Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        total = enviomaritimo.Sum(x => x.Key.Amount);
                        deuda = total - totalPagado;
                        cantidad = enviomaritimo.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        totalcantidad += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Number)).Count();

                        noOrders = noOrders.Union(enviomaritimo.Select(x => x.Key.Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envío Marítimo", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);
                        //Envio Caribe
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var enviocaribe = ventas.Where(x => x.EnvioCaribe != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.EnvioCaribe);
                        pagado = enviocaribe.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviocaribe.Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        total = enviocaribe.Sum(x => x.Key.Amount);
                        deuda = total - totalPagado;
                        cantidad = enviocaribe.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += enviocaribe.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviocaribe.Where(x => !noOrders.Contains(x.Key.Number)).Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        totalcantidad += enviocaribe.Where(x => !noOrders.Contains(x.Key.Number)).Count();

                        noOrders = noOrders.Union(enviocaribe.Select(x => x.Key.Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envío Caribe", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);
                        //Pasaporte
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var pasaporte = ventas.Where(x => x.Passport != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.Passport);
                        pagado = pasaporte.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = pasaporte.Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        total = pasaporte.Sum(x => x.Key.Total);
                        deuda = total - totalPagado;
                        cantidad = pasaporte.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += pasaporte.Where(x => !noOrders.Contains(x.Key.OrderNumber)).Sum(x => x.Key.Total);
                        totalpagado += pagado;
                        total_Tpagado += pasaporte.Where(x => !noOrders.Contains(x.Key.OrderNumber)).Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        totalcantidad += pasaporte.Where(x => !noOrders.Contains(x.Key.OrderNumber)).Count();

                        noOrders = noOrders.Union(pasaporte.Select(x => x.Key.OrderNumber).ToList()).ToList();

                        cell1.AddElement(new Phrase("Pasaporte", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Otros Servicios
                        var servicios = ventas.Where(x => x.Servicio != null && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.Servicio);
                        foreach (var item in servicios.GroupBy(x => x.Key.tipoServicio))
                        {

                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;

                            pagado = item.Sum(x => x.Sum(y => y.valorPagado));
                            totalPagado = item.Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                            total = item.Sum(x => x.Key.importeTotal);
                            deuda = total - totalPagado;
                            cantidad = item.Count();

                            totaldeudatbl += deuda;
                            totalimportetbl += total;
                            totalpagadotbl += pagado;
                            total_Tpagadotbl += totalPagado;
                            totalcantidadtbl += cantidad;

                            grantotal += item.Where(x => !noOrders.Contains(x.Key.numero)).Sum(x => x.Key.importeTotal);
                            totalpagado += pagado;
                            total_Tpagado += item.Where(x => !noOrders.Contains(x.Key.numero)).Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                            totalcantidad += item.Where(x => !noOrders.Contains(x.Key.numero)).Count();

                            noOrders = noOrders.Union(item.Select(x => x.Key.numero).ToList()).ToList();

                            cell1.AddElement(new Phrase(item.Key.Nombre, normalFont));
                            cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                            cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                            cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                            cell5.AddElement(new Phrase(total.ToString(), normalFont));
                            cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                            aux.AddCell(cell1);
                            aux.AddCell(cell2);
                            aux.AddCell(cell3);
                            aux.AddCell(cell4);
                            aux.AddCell(cell5);
                            aux.AddCell(cell6);
                        }

                        //reserva
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var reserva = ventas.Where(x => x.Ticket != null && !x.Ticket.ClientIsCarrier && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.Ticket);
                        pagado = reserva.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = reserva.Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        total = reserva.Sum(x => x.Key.Total);
                        deuda = total - totalPagado;
                        cantidad = reserva.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += reserva.Where(x => !noOrders.Contains(x.Key.ReservationNumber)).Sum(x => x.Key.Total);
                        totalpagado += pagado;
                        total_Tpagado += reserva.Where(x => !noOrders.Contains(x.Key.ReservationNumber)).Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        totalcantidad += reserva.Where(x => !noOrders.Contains(x.Key.ReservationNumber)).Count();

                        noOrders = noOrders.Union(reserva.Select(x => x.Key.ReservationNumber).ToList()).ToList();

                        cell1.AddElement(new Phrase("Reserva", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //reserva Carrier
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;

                        var reservaCarrier = ventas.Where(x => x.Ticket != null && x.Ticket.ClientIsCarrier && x.tipoPago.Type != "Crédito de Consumo").GroupBy(x => x.Ticket);
                        pagado = reservaCarrier.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = reservaCarrier.Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        total = reservaCarrier.Sum(x => x.Key.Total);
                        deuda = total - totalPagado;
                        cantidad = reservaCarrier.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += reservaCarrier.Where(x => !noOrders.Contains(x.Key.ReservationNumber)).Sum(x => x.Key.Total);
                        totalpagado += pagado;
                        total_Tpagado += reservaCarrier.Where(x => !noOrders.Contains(x.Key.ReservationNumber)).Sum(x => x.Key.RegistroPagos.Sum(y => y.valorPagado));
                        totalcantidad += reservaCarrier.Where(x => !noOrders.Contains(x.Key.ReservationNumber)).Count();

                        noOrders = noOrders.Union(reservaCarrier.Select(x => x.Key.ReservationNumber).ToList()).ToList();

                        cell1.AddElement(new Phrase("Reserva Carrier", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);


                        // Añado el total
                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell6 = new PdfPCell();
                        cell6.Border = 0;


                        cell1.AddElement(new Phrase("Totales", headFont));
                        cell2.AddElement(new Phrase(totalcantidadtbl.ToString(), headFont));
                        cell3.AddElement(new Phrase(totalpagadotbl.ToString(), headFont));
                        cell4.AddElement(new Phrase(total_Tpagadotbl.ToString(), headFont));
                        cell5.AddElement(new Phrase(totalimportetbl.ToString(), headFont));
                        cell6.AddElement(new Phrase(totaldeudatbl.ToString(), headFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        // Añado la tabla al documento
                        doc.Add(aux);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        #region // VENTAS POR TIPO DE PAGO CLIENTE

                        doc.Add(new Phrase("VENTAS POR TIPO DE PAGO - " + ventas.Key.FullName, headFont));
                        float[] columns = { 3, 2, 2 };
                        PdfPTable tablePagoCliente = new PdfPTable(columns);
                        tablePagoCliente.WidthPercentage = 100;
                        PdfPCell cellpago1 = new PdfPCell();
                        cellpago1.Border = 1;
                        PdfPCell cellpago2 = new PdfPCell();
                        cellpago2.Border = 1;
                        PdfPCell cellpago3 = new PdfPCell();
                        cellpago3.Border = 1;

                        cellpago1.AddElement(new Phrase("Tipo de Pago", headFont));
                        cellpago2.AddElement(new Phrase("Cantidad", headFont));
                        cellpago3.AddElement(new Phrase("Importe", headFont));
                        tablePagoCliente.AddCell(cellpago1);
                        tablePagoCliente.AddCell(cellpago2);
                        tablePagoCliente.AddCell(cellpago3);
                        cellpago1 = new PdfPCell();
                        cellpago1.Border = 1;
                        cellpago2 = new PdfPCell();
                        cellpago2.Border = 1;
                        cellpago3 = new PdfPCell();
                        cellpago3.Border = 1;
                        foreach (var item in ventas.GroupBy(x => x.tipoPago))
                        {
                            cellpago1.AddElement(new Phrase(item.Key.Type, normalFont));
                            cellpago2.AddElement(new Phrase(item.Count().ToString(), normalFont));
                            cellpago3.AddElement(new Phrase(item.Sum(x => x.valorPagado).ToString(), normalFont));
                            tablePagoCliente.AddCell(cellpago1);
                            tablePagoCliente.AddCell(cellpago2);
                            tablePagoCliente.AddCell(cellpago3);
                            cellpago1 = new PdfPCell();
                            cellpago1.Border = 0;
                            cellpago2 = new PdfPCell();
                            cellpago2.Border = 0;
                            cellpago3 = new PdfPCell();
                            cellpago3.Border = 0;
                        }
                        doc.Add(tablePagoCliente);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                        #endregion
                    }

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    PdfPTable tabletipopago = new PdfPTable(columnWidthstipopago);
                    tabletipopago.WidthPercentage = 100;
                    PdfPCell celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 1;
                    PdfPCell celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 1;
                    PdfPCell celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 1;

                    celltipopago1.AddElement(new Phrase("Tipo de Pago", headFont));
                    celltipopago2.AddElement(new Phrase("Cantidad", headFont));
                    celltipopago3.AddElement(new Phrase("Importe", headFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 1;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 1;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 1;

                    celltipopago1.AddElement(new Phrase("Cash", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[0].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[0].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Zelle", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[1].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[1].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Cheque", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[2].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[2].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Crédito o Débito", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[3].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[3].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Transferencia Bancaria", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[4].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[4].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Web", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[5].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[5].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Money Order", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[6].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[6].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Crédito de Consumo", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[7].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[7].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Cash App", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[8].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[8].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    doc.Add(tabletipopago);
                    #endregion

                    #region //GRAN TOTAL
                    float[] columnwhidts2 = { 5, 5 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;
                    doc.Add(Chunk.NEWLINE);

                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph totalcantidadaux = new Paragraph("Cantidad: ", headFont2);
                    totalcantidadaux.AddSpecial(new Phrase(totalcantidad.ToString(), normalFont2));
                    totalcantidadaux.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(totalcantidadaux);
                    Paragraph porpagar = new Paragraph("Pagado: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalpagado.ToString(), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);
                    Paragraph deudaaux = new Paragraph("Pendiente a pago: ", headFont2);
                    deudaaux.AddSpecial(new Phrase("$ " + (grantotal - total_Tpagado).ToString(), normalFont2));
                    deudaaux.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deudaaux);

                    #region Facturas Cobradas
                    var facturasCobradas = _context.Facturas
                    .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                    .Include(x => x.RegistroPagos).ThenInclude(x => x.User)
                    .Where(x => x.agencyId == agency.AgencyId && x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date));
                    int noFacturas = await facturasCobradas.CountAsync();
                    decimal montoFacturas = await facturasCobradas.SumAsync(x => x.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date).Sum(y => y.valorPagado));
                    if (noFacturas > 0)
                    {
                        Paragraph cobradoFacturas = new Paragraph("Cobrado por contabilidad: ", headFont2);
                        cobradoFacturas.AddSpecial(new Phrase($"{noFacturas} facturas -- ${montoFacturas}", normalFont2));
                        cobradoFacturas.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(cobradoFacturas);
                    }

                    #endregion

                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase("COBROS POR CONTABILIDAD", headFont));
                    float[] widthTable = { 3, 2, 1, 2 };
                    PdfPTable table = new PdfPTable(widthTable);
                    table.WidthPercentage = 100;

                    PdfPCell cell = new PdfPCell(new Phrase("Empleado", headFont));
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase("Factura", headFont));
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase("Monto", headFont));
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase("Tipo Pago", headFont));
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    table.AddCell(cell);

                    foreach (var factura in facturasCobradas)
                    {
                        foreach (var pago in factura.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date))
                        {
                            cell = new PdfPCell(new Phrase(pago.User.FullName, normalFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            table.AddCell(cell);
                            cell = new PdfPCell(new Phrase(factura.NoFactura, normalFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            table.AddCell(cell);
                            cell = new PdfPCell(new Phrase(pago.valorPagado.ToString("0.00"), normalFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            table.AddCell(cell);
                            cell = new PdfPCell(new Phrase(pago.tipoPago.Type, normalFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            table.AddCell(cell);
                        }
                    }
                    doc.Add(table);

                    doc.Close();
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        private void InitMunicipios()
        {
            if (_context.Municipio.Any())
                return;

            var m = new Dictionary<string, List<string>>();


            m["Pinar del Río"] = new List<string>() { "Sandino", "Mantua", "Minas De Matahambre", "Viñales", "La Palma", "Los Palacios", "Consolacion del Sur", "Pinar del Rio", "San Luis", "San Juan y Martinez", "Guane" };
            m["Artemisa"] = new List<string>() { "Mariel", "Guanajay", "Caimito", "Bauta", "San Antonio de Los Baños", "Guira De Melena", "Alquizar", "Artemisa", "Bahia Honda", "San Cristobal", "Candelaria" };
            m["La Habana"] = new List<string>() { "Playa", "Plaza de la Revolución", "Centro Habana", "La Habana Vieja", "Regla", "La Habana del Este", "Guanabacoa", "San Miguel del Padrón", "Diez De Octubre", "Cerro", "Marianao", "La Lisa", "Boyeros", "Arroyo Naranjo", "Cotorro" };
            m["Mayabeque"] = new List<string>() { "Bejucal", "Quivican", "Batabano", "San Jose de das Laja", "Melena Del Sur", "Guines", "San Nicolas de Bari", "Nueva Paz", "Madruga", "Jaruco", "Santa Cruz del Norte" };
            m["Matanzas"] = new List<string>() { "Matanzas", "Cardenas", "Varadero", "Marti", "Colon", "Perico", "Jovellanos", "Pedro Betancourt", "Limonar", "Union de Reyes", "Cienaga de Zapata", "Jaguey Grande", "Calimete", "Los Arabos" };
            m["Villa Clara"] = new List<string>() { "Corralillo", "Quemado de Guines", "Sagua la Grande", "Encrucijada", "Camajuani", "Caibarien", "Remedios", "Placetas", "Santa Clara", "Cifuentes", "Santo Domingo", "Ranchuelo", "Manicaragua" };
            m["Cienfuegos"] = new List<string>() { "Aguada de Pasajeros", "Rodas", "Palmira", "Lajas", "Cruces", "Cumanayagua", "Cienfuegos", "Abreus" };
            m["Sancti Spiritus"] = new List<string>() { "Yaguajay", "Jatibonico", "Taguasco", "Cabaiguan", "Fomento", "Trinidad", "Sancti Spiritus", "La Sierpe" };
            m["Ciego de Ávila"] = new List<string>() { "Chambas", "Moron", "Bolivia", "Primero de Enero", "Ciro Redondo", "Florencia", "Majagua", "Ciego de Avila", "Venezuela", "Baragua", "Carlos Manuel de Cespedes" };
            m["Camagüey"] = new List<string>() { "Esmeralda", "Sierra De Cubitas", "Minas", "Nuevitas", "Guaimaro", "Sibanicu", "Camaguey", "Florida", "Vertientes", "Jimaguayu", "Najasa", "Santa Cruz del Sur" };
            m["Las Tunas"] = new List<string>() { "Manati", "Puerto Padre", "Jesus Menendez", "Majibacoa", "Las Tunas", "Jobabo", "Colombia", "Amancio" };
            m["Holguín"] = new List<string>() { "Holguin", "Gibara", "Rafael Freyre", "Banes", "Antilla", "Baguanos", "Calixto Garcia", "Cacocum", "Urbano Noris", "Cueto", "Mayari", "Frank Pais", "Sagua De Tanamo", "Moa" };
            m["Granma"] = new List<string>() { "Rio Cauto", "Cauto Cristo", "Jiguani", "Bayamo", "Yara", "Manzanillo", "Campechuela", "Media Luna", "Niquero", "Pilon", "Bartolome Maso", "Buey Arriba", "Guisa" };
            m["Santiago de Cuba"] = new List<string>() { "Contramaestre", "Mella", "San Luis", "Segundo Frente", "Songo - La Maya", "Santiago De Cuba", "Palma Soriano", "Tercer Frente", "Guama" };
            m["Guantánamo"] = new List<string>() { "El Salvador", "Guantanamo", "Yateras", "Baracoa", "Maisi", "Imias", "San Antonio del Sur", "Manuel Tames", "Caimanera", "Niceto Perez" };
            m["Isla de la Juventud"] = new List<string>() { "Gerona", "La Fe" };

            foreach (var item in m.Keys)
            {
                var provincia = _context.Provincia.First(pr => pr.nombreProvincia == item);
                foreach (var x in m[item])
                {
                    var municipio = new Municipio
                    {
                        nombreMunicipio = x,
                        provincia = provincia
                    };
                    _context.Municipio.Add(municipio);
                }
            }

            _context.SaveChanges();
            return;
        }

        [HttpPost]
        public async Task<JsonResult> getmenus()
        {
            try
            {

                var role = AgenciaAuthorize.getRole(User, _context);
                var aUser = _context.User.Include(x => x.AccessListUsers).Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId).FirstOrDefault();

                List<AccessList> allaccessList = _context.AccessLists.ToList();
                List<AccessList> accesslist = aUser.AccessListUsers.Select(x => x.accessList).ToList();
                if (accesslist.Count() == 0)
                {
                    return Json("");
                }
                var denied = allaccessList.Except(accesslist).ToList();
                return Json(denied);
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Json("");
            }
        }

        public JsonResult getDataTransaccionesDia()
        {
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();

            //Ventas por Dia
            AuxTransaccionesPorDia auxventasdia = new AuxTransaccionesPorDia(aAgency, _context);
            var xx = auxventasdia.getDataMes();
            var json = JsonConvert.SerializeObject(xx);

            return Json(json);
        }

        public void fix()
        {
            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            var mAgency = _context.Agency.FirstOrDefault(x => x.AgencyId == user.AgencyId);

            var orders = _context.OrderCubiqs.Include(x => x.Paquetes).Include(x => x.wholesaler).Include(x => x.Contact).ThenInclude(c => c.Address).Where(x => x.agencyTransferidaId == mAgency.AgencyId);
            foreach (var order in orders)
            {
                var provincia = _context.Provincia
                    .Include(x => x.Zona)
                    .Where(x => x.nombreProvincia == order.Contact.Address.City)
                    .FirstOrDefault();
                var cb = _context.CostosCarga.Where(x => x.Zona == provincia.Zona).FirstOrDefault();

                var transferencia = _context.CostoxModuloMayorista
                .Include(x => x.valoresTramites)
                .Include(x => x.modAsignados)
                .Where(x => x.AgencyId == order.AgencyId && x.modAsignados.Any(y => y.IdWholesaler == order.wholesaler.IdWholesaler))
                .FirstOrDefault();

                var valorxtramite = transferencia.valoresTramites.Where(x => x.Tramite == "Cubiq").FirstOrDefault();

                var valorAux = _context.ValorProvincia.Where(x => x.listValores.ValoresxTramiteId == valorxtramite.ValoresxTramiteId && x.provincia == order.Contact.Address.City).FirstOrDefault();
                var costo1 = valorAux.valor;
                var costo2 = valorAux.valor2;

                foreach (var paquete in order.Paquetes)
                {
                    paquete.Costo = paquete.PesoKg > (decimal)1.5 ?
                       cb.Value - (decimal)1.88 + costo2 * paquete.PesoLb
                       : costo1 + cb.Value;
                    paquete.Costo = Math.Round(paquete.Costo, 2);
                    _context.Paquete.Update(paquete);
                }
                var sxp = _context.ServiciosxPagar.Include(x => x.Agency).Where(x => x.SId == order.OrderCubiqId && x.Agency != null).FirstOrDefault();
                if (sxp != null)
                {
                    sxp.ImporteAPagar = order.Paquetes.Sum(x => x.Costo) + order.OtrosCostos;
                    _context.ServiciosxPagar.Update(sxp);
                }


                var sxc = _context.servicioxCobrar.Where(x => x.ServicioId == order.OrderCubiqId).FirstOrDefault();
                if (sxc != null)
                {
                    sxc.valorTramite = order.Paquetes.Sum(x => x.Costo);
                    sxc.importeACobrar = order.Paquetes.Sum(x => x.Costo) + order.OtrosCostos;
                    _context.servicioxCobrar.Update(sxc);
                }


                order.costoMayorista = order.Paquetes.Sum(x => x.Costo);
                _context.OrderCubiqs.Update(order);
            }
            _context.SaveChanges();
        }

        //Para saber el costo que aplica el Mayorista Caribe a Rapid Multiservice
        public decimal getCostoCaribe(List<EnvioCaribe> envioscaribe)
        {
            decimal costo = 0;
            //obtengo el mayorista 
            Agency agency = _context.Agency.FirstOrDefault(x => x.Name == "Rapid Multiservice");
            if (agency != null)
            {
                var mayorista = _context.Wholesalers.Include(x => x.tipoServicioHabana).Include(x => x.tipoServicioRestoProv).FirstOrDefault(x => x.EsVisible && x.AgencyId == agency.AgencyId && x.Category.category == "Maritimo-Aereo");
                if (mayorista != null)
                {
                    foreach (var item in envioscaribe)
                    {
                        List<TipoServicioMayorista> servicios = new List<TipoServicioMayorista>();
                        if (item.Contact.Address.City == "La Habana")
                        {
                            servicios = mayorista.tipoServicioHabana;
                        }
                        else
                        {
                            servicios = mayorista.tipoServicioRestoProv;
                        }
                        if (servicios != null)
                        {
                            if (item.servicio == "Correo-Aereo")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                costo += item.costo - ((item.PesoTotal * servicio.costoAereo) + item.OtrosCostos);
                            }
                            else if (item.servicio == "Correo-Maritimo")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                costo += item.costo - ((item.PesoTotal * servicio.costoMaritimo) + item.OtrosCostos);
                            }
                            else if (item.servicio == "Aerovaradero- Recogida")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Aereo_Varadero);
                                costo += item.costo - ((item.PesoTotal * servicio.costoAereo) + item.OtrosCostos);
                            }
                            else if (item.servicio == "Maritimo-Palco Almacen")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                costo += item.costo - ((item.PesoTotal * servicio.costoMaritimo) + item.OtrosCostos);
                            }
                            else if (item.servicio == "Palco ENTREGA A DOMICILIO")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                costo += item.costo - ((item.PesoTotal * servicio.costoAereo) + item.OtrosCostos);
                            }
                        }
                    }
                }

            }

            return costo;
        }

        public async Task AuxTransferirPagos()
        {
            var pagos = await _context.Pago.ToListAsync();
            foreach (var item in pagos)
            {
                if (_context.RegistroPagos.Any(x => x.AgencyId != item.AgencyId && x.number != item.Number))
                {
                    RegistroPago registro = new RegistroPago
                    {
                        AgencyId = item.AgencyId,
                        date = item.Date,
                        number = item.Number,
                        OfficeId = item.OfficeId,
                        OrderId = item.OrderId,
                        tipoPagoId = item.TipoPagoId,
                        UserId = item.UserId,
                        valorPagado = item.ValorPagado,
                        RegistroPagoId = Guid.NewGuid(),
                        ClientId = item.ClientId,
                        nota = item.nota,
                    };
                    _context.Add(registro);
                }
            }
            var pagoEM = await _context.PagoEnvioMaritimos.Include(x => x.Client).Include(x => x.envio).ToListAsync();
            foreach (var item in pagoEM)
            {
                if (_context.RegistroPagos.Any(x => x.AgencyId != item.AgencyId && x.number != item.Number))
                {
                    RegistroPago registro = new RegistroPago
                    {
                        AgencyId = item.AgencyId,
                        date = item.Date,
                        number = item.Number,
                        OfficeId = item.OfficeId,
                        EnvioMaritimoId = item.envio.Id,
                        tipoPagoId = item.TipoPagoId,
                        UserId = item.UserId,
                        valorPagado = item.ValorPagado,
                        RegistroPagoId = Guid.NewGuid(),
                        ClientId = item.Client.ClientId,
                    };
                    _context.Add(registro);
                }
            }

            var pagoticket = await _context.PaymentTicket.Include(x => x.User).Include(x => x.Office).Include(x => x.Agency).Include(x => x.Client).Include(x => x.Ticket).ToListAsync();
            foreach (PaymentTicket item in pagoticket)
            {
                if (_context.RegistroPagos.Any(x => x.AgencyId != item.Agency.AgencyId && x.number != item.Number))
                {
                    RegistroPago registro = new RegistroPago();
                    registro.AgencyId = item.Agency.AgencyId;
                    registro.date = item.Date;
                    registro.number = item.Number;
                    registro.OfficeId = item.Office.OfficeId;
                    registro.TicketId = item.TicketId;
                    registro.tipoPagoId = _context.TipoPago.FirstOrDefault(x => x.Type == item.tipoPago).TipoPagoId;
                    registro.UserId = item.User.UserId;
                    registro.valorPagado = item.ValorPagado;
                    registro.RegistroPagoId = Guid.NewGuid();
                    registro.ClientId = item.Client.ClientId;
                    _context.Add(registro);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task ServxPagarCaribe()
        {
            try
            {
                var envios = _context.EnvioCaribes.Include(x => x.paquetes).Include(x => x.AgencyTransferida).Include(x => x.Agency).Include(x => x.Wholesaler).Where(x => x.AgencyTransferida != null);
                foreach (var item in envios)
                {
                    //busco el mayorista de la agencia transferida 
                    Wholesaler wTransferida = _context.Wholesalers.Include(x => x.tipoServicioHabana).Include(x => x.tipoServicioRestoProv).FirstOrDefault(x => x.EsVisible && x.AgencyId == item.AgencyTransferidaId && x.Category.category == "Maritimo-Aereo");
                    if (wTransferida != null)
                    {
                        //Verifico si el tramite ya tiene un servicio por pagar creado del mayorista a su proveedor
                        var exist = _context.ServiciosxPagar.Any(x => x.SId == item.EnvioCaribeId && x.Mayorista == wTransferida && x.Agency == item.AgencyTransferida);
                        if (!exist)
                        {
                            //Creo el servicio por pagar del mayorista a su proveedor
                            decimal costoRapid = 0;
                            List<TipoServicioMayorista> servicios = null;
                            Contact c = _context.Contact.Include(x => x.Address).FirstOrDefault(x => x.ContactId == item.ContactId);
                            if (c.Address.City == "La Habana")
                            {
                                servicios = wTransferida.tipoServicioHabana;
                            }
                            else
                            {
                                servicios = wTransferida.tipoServicioRestoProv;
                            }

                            if (servicios != null)
                            {
                                if (item.servicio == "Correo-Aereo")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                    costoRapid = servicio.costoAereo;
                                }
                                else if (item.servicio == "Correo-Maritimo")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                    costoRapid = servicio.costoMaritimo;
                                }
                                else if (item.servicio == "Aerovaradero- Recogida")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Aereo_Varadero);
                                    costoRapid = servicio.costoAereo;
                                }
                                else if (item.servicio == "Maritimo-Palco Almacen")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                    costoRapid = servicio.costoMaritimo;
                                }
                                else if (item.servicio == "Palco ENTREGA A DOMICILIO")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                    costoRapid = servicio.costoAereo;
                                }
                            }

                            decimal refcostoRapid = Math.Round(item.paquetes.Sum(x => x.peso * costoRapid), 2);

                            var porPagar = new ServiciosxPagar
                            {
                                Date = item.Date,
                                ImporteAPagar = refcostoRapid,
                                Mayorista = wTransferida,
                                Agency = item.AgencyTransferida,
                                SId = item.EnvioCaribeId,
                                EnvioCaribe = item,
                                NoServicio = item.Number,
                                Tipo = STipo.EnvioCaribe,
                                SubTipo = item.servicio,
                                Express = false
                            };
                            _context.ServiciosxPagar.Add(porPagar);
                        }
                    }

                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
            }
        }

        public async Task UpdateSxpTicket()
        {
            var sxp = _context.ServiciosxPagar.Where(x => x.Tipo == STipo.Reserva);
            foreach (var item in sxp)
            {
                item.Reserva = _context.Ticket.Find(item.SId);
                _context.Update(item);
            }
            await _context.SaveChangesAsync();
        }

        //Borrar: para corregir problema de la fecha de los registro de pagos
        public async Task SetDateRegister()
        {
            DateTime d = DateTime.Parse("0001-01-01 00:00:00.0000000");
            var query = _context.RegistroPagos
                .Include(x => x.Passport)
                .Include(x => x.Ticket)
                .Include(x => x.EnvioCaribe)
                .Where(x => x.date == d && x.AgencyId == Guid.Parse("F3D9659C-9A9F-4824-8933-309916549B49"));

            foreach (var item in query)
            {
                if (item.Passport != null)
                    item.date = item.Passport.FechaSolicitud;
                else if (item.Ticket != null)
                    item.date = item.Ticket.RegisterDate;
                else if (item.EnvioCaribe != null)
                    item.date = item.EnvioCaribe.Date;
                _context.RegistroPagos.Update(item);
            }
            await _context.SaveChangesAsync();
        }

        public async Task createSxP()
        {
            var data = _context.Order.Include(x => x.agencyTransferida).Where(x => x.agencyTransferida != null
            && x.Type != "Combo" && x.Type != "Remesas" && x.Type != "Tienda"
            && x.Status != "Cancelada");

            // creo los servicios por pagar de los mayoristas a sus proveedores
            foreach (var order in data)
            {
                string categoryWholesaler = order.Type;
                if (order.Type == "Paquete" || order.Type == "Mixto" || order.Type == "Medicinas")
                    categoryWholesaler = "Paquete Aereo";

                var auxWholesaler = await _context.Wholesalers.Include(x => x.Category).FirstOrDefaultAsync(x => x.AgencyId == order.agencyTransferida.AgencyId && x.Category.category == categoryWholesaler);
                if (auxWholesaler != null)
                {
                    var exist = await _context.ServiciosxPagar.Include(x => x.Mayorista).Include(x => x.Agency).AnyAsync(x => x.NoServicio == order.Number && x.Mayorista == auxWholesaler && x.Agency == order.agencyTransferida);
                    if (!exist)
                    {
                        decimal importeACobrar = 0;
                        if (order.Type == "Tienda")
                            importeACobrar = auxWholesaler.CostoMayorista;
                        else
                            importeACobrar = order.CantLb * auxWholesaler.CostoMayorista + order.CustomsTax;
                        order.costoDeProveedor = importeACobrar;
                        _context.Order.Update(order);
                        var porPagar = new ServiciosxPagar
                        {

                            Date = DateTime.Now,
                            ImporteAPagar = importeACobrar,
                            Mayorista = auxWholesaler,
                            Agency = order.agencyTransferida,
                            SId = order.OrderId,
                            Order = order,
                            NoServicio = order.Number,
                            Tipo = STipo.Paquete,
                            SubTipo = "-",
                            Express = order.express
                        };
                        _context.ServiciosxPagar.Add(porPagar);
                    }

                }
            }
        }

        [HttpGet]
        public async Task<JsonResult> getTransactionCombos()
        {
            try
            {
                var response = await _reportService.NumberComboSold();
                if (response.IsFailure) return Json(new { success = false, msg = response.Error });

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        names = response.Value.Select(x => x.Name).ToArray(),
                        values = response.Value.Select(x => x.Qty).ToArray()
                    }
                });

            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Json(new { success = false, msg = "No se han podido obtener los datos del reporte de combos" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> getTransactionServicios()
        {
            try
            {
                var response = await _reportService.NumberServiceSold();
                if (response.IsFailure) return Json(new { success = false, msg = response.Error });

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        names = response.Value.Select(x => x.Name).ToArray(),
                        values = response.Value.Select(x => x.Qty).ToArray()
                    }
                });

            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Json(new { success = false, msg = "No se han podido obtener los datos del reporte de servicios" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetDataBoxDCuba()
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(x => x.Username == User.Identity.Name);
                var response = await _reportService.GetDataBoxDCuba(user.AgencyId);
                if (response.IsFailure)
                {
                    return Json(new { success = false, msg = response.Error });
                }

                return Json(new { success = true, data = response.Value });
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Json(new { success = false, msg = "No se han podido obtener los datos del reporte" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetGuiasCubiqOpen()
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(x => x.Username == User.Identity.Name);

                var guiaactual = _context.GuiaAerea.Include(x => x.Paquetes).Where(x => x.Agency.AgencyId == user.AgencyId && x.Status == "Nueva");
                var cantPaquetes = guiaactual.Sum(x => x.Paquetes.Count());
                var cantKg = guiaactual.Sum(x => x.Paquetes.Sum(y => y.PesoKg));

                var guias = await _context.GuiaAerea.Where(x => x.Agency.AgencyId == user.AgencyId && x.Status == "Nueva").ToListAsync();
                return Json(new { success = true, data = new { cantPaquetes = cantPaquetes, cantKg = cantKg }, dataGuias = guias.Select(x => new { x.GuiaAereaId, x.NoGuia, x.Type, x.PesoKg, x.Bultos }) });
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Json(new { success = false, msg = "No se han podido obtener los datos de las guias abiertas" });
            }
        }

        private async Task UpdateAgencyPhone()
        {
            foreach (var item in await _context.Agency.ToListAsync())
            {
                var phone = await _context.Phone.FirstOrDefaultAsync(x => x.ReferenceId == item.AgencyId);
                if (phone != null)
                {
                    item.Phone = phone;
                }
            }

            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> TestSendSmsByEmail([FromQuery] int qty, [FromQuery] string to)
        {
            string mensajes = "";
            for (int i = 0; i < qty; i++)
            {
                var response = await _notificationService.sendEmail(
                new SendGrid.Helpers.Mail.EmailAddress { Email = "noreply@agenciapp.com" },
                new SendGrid.Helpers.Mail.EmailAddress { Email = to },
                $"{i + 1}-Mensaje de prueba",
                $"Esto es un mensaje de prueba", null, false);

                mensajes += response.IsSuccess ? response.Value : response.Error + ". ";
            }

            return Ok(mensajes);
        }

        [HttpPost]
        [Authorize]
        public JsonResult GetChartProcedureAgency()
        {
            var user = _context.User.First(x => x.Username == User.Identity.Name);

            List<DataTransaccionesDia> aux = new List<DataTransaccionesDia>();
            var date = DateTime.Now.Date;
            var datePrincipioMes = date.AddDays(date.Day * -1 + 1);

            var data = _context.Order
                .Where(x => x.PrincipalDistributorId.Equals(user.UserId))
                .Where(x => x.fechadespacho >= datePrincipioMes && x.Status != Order.STATUS_CANCELADA)
                .Select(x => new
                {
                    AgencyName = x.agencyTransferida != null ? x.agencyTransferida.Name : x.Agency.Name,
                    DispatchDate = x.fechadespacho,
                    Id = x.OrderId
                })
                .ToList()
                .GroupBy(x => x.AgencyName);

            foreach (var orderByAgency in data)
            {
                DataTransaccionesDia item = new DataTransaccionesDia(orderByAgency.Key);
                item.data.Add(0);
                aux.Add(item);
            }

            while (datePrincipioMes.Date <= date.Date)
            {
                foreach (var item in aux)
                {
                    var dataAux = data.First(x => x.Key.Equals(item.nombreServicio))
                        .Where(x => x.DispatchDate.Date == datePrincipioMes.Date);
                    item.data.Add(dataAux.Count());
                }
                datePrincipioMes = datePrincipioMes.AddDays(1);
            }

            return Json(aux);
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetDataOrdersByAgency()
        {
            var user = _context.User.First(x => x.Username == User.Identity.Name);

            var date = DateTime.Now.Date;
            var datePrincipioMes = date.AddDays(date.Day * -1 + 1);

            var data = _context.OrdersReceivedByAgency
                .Where(x => x.PrincipalDistributorId == user.UserId)
                .GroupBy(x => x.OrderId);

            Dictionary<string, int> dictHoy = new Dictionary<string, int>();
            Dictionary<string, int> dictMes = new Dictionary<string, int>();

            foreach (var item in data)
            {
                var status = item.OrderByDescending(x => x.Date).First(x => x.Estado == Order.STATUS_RECIBIDA);
                string agency = string.IsNullOrEmpty(status.AgencyTransferredName) ? status.AgencyName : status.AgencyTransferredName;
                if (status != null)
                {
                    if (status.Date >= date)
                    {
                        if (dictHoy.Any(x => x.Key.Equals(agency)))
                            dictHoy[agency] += 1;
                        else
                            dictHoy[agency] = 1;
                    }
                    if (status.Date >= datePrincipioMes)
                    {
                        if (dictMes.Any(x => x.Key.Equals(agency)))
                            dictMes[agency] += 1;
                        else
                            dictMes[agency] = 1;

                    }
                }
            }

            var response = new
            {
                Today = dictHoy.Select(x => new Pair<string, int>(x.Key, x.Value)).ToList(),
                Month = dictMes.Select(x => new Pair<string, int>(x.Key, x.Value)).ToList(),
            };

            return Json(response);
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetDataOrdersByProvince()
        {
            var user = _context.User.First(x => x.Username == User.Identity.Name);

            var date = DateTime.Now.Date;
            var datePrincipioMes = date.AddDays(date.Day * -1 + 1);

            var data = _context.OrdersByProvince
                .Where(x => x.PrincipalDistributorId == user.UserId)
                .GroupBy(x => x.OrderId);

            var response = new
            {
                Today = new
                {
                    Dispatched = new List<Pair<string, int>>(),
                    Received = new List<Pair<string, int>>(),
                    Delivered = new List<Pair<string, int>>(),
                },
                Month = new
                {
                    Dispatched = new List<Pair<string, int>>(),
                    Received = new List<Pair<string, int>>(),
                    Delivered = new List<Pair<string, int>>(),
                }
            };
            #region necesary functions

            Action<List<Pair<string, int>>, string> addDataResponse = (resp, city) =>
            {
                var item = resp.FirstOrDefault(x => x.obj1 == city);
                if (item != null)
                {
                    item.obj2 += 1;
                }
                else
                {
                    resp.Add(new Pair<string, int>(city, 1));
                }
            };

            Action<OrdersByProvince, string, bool> addDataByDate = (item, status, isMonth) =>
            {
                if (status.Equals(Order.STATUS_DESPACHADA))
                {
                    if (isMonth)
                        addDataResponse(response.Month.Dispatched, item.City);
                    else
                        addDataResponse(response.Today.Dispatched, item.City);
                }
                else if (status.Equals(Order.STATUS_RECIBIDA))
                {
                    if (isMonth)
                        addDataResponse(response.Month.Received, item.City);
                    else
                        addDataResponse(response.Today.Received, item.City);
                }
                else if (status.Equals(Order.STATUS_ENTREGADA))
                {
                    if (isMonth)
                        addDataResponse(response.Month.Delivered, item.City);
                    else
                        addDataResponse(response.Today.Delivered, item.City);
                }
            };

            Action<IGrouping<Guid, OrdersByProvince>> addData = (item) =>
            {
                var status = item.OrderByDescending(x => x.Date).FirstOrDefault();
                if (status != null)
                {
                    if (status.Date >= date)
                    {
                        addDataByDate(status, status.Estado, false);
                    }
                    addDataByDate(status, status.Estado, true);
                }
            };

            #endregion

            foreach (var item in data)
            {
                addData(item);
            }

            return Json(response);
        }

        [Authorize]
        public async Task<IActionResult> ExportExcelPrincipalDistributor(string date, string agencies, string status)
        {

            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            CultureInfo culture = new CultureInfo("es-US", true);
            var auxDate = date.Split("-");
            var dateIni = DateTime.Parse(auxDate[0], culture);
            var dateFin = DateTime.Parse(auxDate[1], culture);
            string formatDate = "MM/dd/yy";

            var query = _context.RegistroEstado
                .Where(x => x.Date >= dateIni && x.Date <= dateFin && x.Estado != Order.STATUS_CANCELADA
                && x.Order.PrincipalDistributorId == user.UserId);


            Agency agencyExport = null;

            if (!string.IsNullOrEmpty(agencies) && agencies != "null")
            {
                agencyExport = _context.Agency.Find(Guid.Parse(agencies));
                query = query.Where(x => x.Order.AgencyId == agencyExport.AgencyId || x.Order.agencyTransferida.AgencyId == agencyExport.AgencyId);
            }

            if (!string.IsNullOrEmpty(status) && status != "null")
            {
                var statusAux = status.Split(",").ToList();
                query = query.Where(x => statusAux.Contains(x.Estado));
            }
            else
            {
                query = query.Where(x => x.Estado != Order.STATUS_DESPACHADA);
            }

            var orders = await query.OrderBy(x => x.Order.Contact.Address.City).ToListAsync();

            byte[] fileContents;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Paquetes");

                Action<ExcelWorksheet, string, int, int> AddHeader = (wsheet, title, x, y) =>
                {
                    wsheet.Cells[x, y].Value = title;
                    wsheet.Cells[x, y].Style.Font.Size = 12;
                    wsheet.Cells[x, y].Style.Font.Bold = true;
                    wsheet.Cells[x, y].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                };

                string titleHeader;
                if (agencyExport != null)
                    titleHeader = $"{agencyExport.Name.ToUpper()} DEL ${dateIni.ToString(formatDate)} AL ${dateFin.ToString(formatDate)}";
                else
                    titleHeader = $"DEL ${dateIni.ToString(formatDate)} AL ${dateFin.ToString(formatDate)}";

                worksheet.Cells[1, 4].Value = titleHeader;
                worksheet.Cells[1, 4].Style.Font.Size = 22;
                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                int row = 3;
                int col = 1;

                //Añado los encabezados
                new List<string>()
                {
                    "No", "F/Recibido", "No.Fact", "F/Entrega", "Agencia", "Nombre y Apellidos", "Direccion", "Municipio", "Provincia",
                    "Telefono", "U.S.D", "C/Libras", "Estado"
                }.ForEach(header => AddHeader(worksheet, header, row, col++));


                //Contenido
                row++;
                int count = 1;
                foreach (var statusByOrder in orders.GroupBy(x => x.OrderId))
                {
                    col = 1;
                    var order = await _context.Order
                        .Include(x => x.Agency)
                        .Include(x => x.agencyTransferida)
                        .Include(x => x.RegistroEstados)
                        .Include(x => x.Contact).ThenInclude(x => x.Address)
                        .Include(x => x.Contact).ThenInclude(x => x.Phone1)
                        .FirstOrDefaultAsync(x => x.OrderId == statusByOrder.Key);

                    var fechaRecibido = order.RegistroEstados.FirstOrDefault(x => x.Estado.Equals(Order.STATUS_RECIBIDA))?.Date;
                    var fechaEntrega = order.RegistroEstados.FirstOrDefault(x => x.Estado.Equals(Order.STATUS_ENTREGADA))?.Date;
                    string agency = order.agencyTransferida != null ? order.agencyTransferida.Name : order.Agency.Name;
                    string number = order.Number.Substring(order.Number.Length - 5);
                    if (order.AgencyId == AgencyName.MiamiPlusService || (order.agencyTransferida != null && order.agencyTransferida.AgencyId == AgencyName.MiamiPlusService))
                        number = order.Number.Substring(order.Number.Length - 6);

                    worksheet.Cells[row, col++].Value = count;
                    worksheet.Cells[row, col++].Value = fechaRecibido != null ? ((DateTime)fechaRecibido).ToString(formatDate) : string.Empty;
                    worksheet.Cells[row, col++].Value = number;
                    worksheet.Cells[row, col++].Value = fechaEntrega != null ? ((DateTime)fechaEntrega).ToString(formatDate) : "No Entregada";
                    worksheet.Cells[row, col++].Value = agency;
                    worksheet.Cells[row, col++].Value = order.Contact.FullData;
                    worksheet.Cells[row, col++].Value = order.Contact.Address.AddressLine1;
                    worksheet.Cells[row, col++].Value = order.Contact.Address.State;
                    worksheet.Cells[row, col++].Value = order.Contact.Address.City;
                    worksheet.Cells[row, col++].Value = order.Contact.Phone1.Number;
                    worksheet.Cells[row, col++].Value = "5.60";
                    worksheet.Cells[row, col++].Value = order.CantLb + order.CantLbMedicina;
                    worksheet.Cells[row, col++].Value = order.Status;

                    count++;
                    row++;
                }

                fileContents = package.GetAsByteArray();
            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: "Listado de paquetes.xlsx"
            );

        }

        [Authorize]
        public async Task<IActionResult> ExportExcelFacturaPrincipalDistributor(string date, string agencies, string retails)
        {

            var user = _context.User.FirstOrDefault(x => x.Username == User.Identity.Name);
            CultureInfo culture = new CultureInfo("es-US", true);
            var auxDate = date.Split("-");
            var dateIni = DateTime.Parse(auxDate[0], culture);
            var dateFin = DateTime.Parse(auxDate[1], culture);
            string formatDate = "MM/dd/yy";

            var prices = await _context.HMpaquetesPriceByProvinces
                .Include(x => x.Province)
                .Include(x => x.Municipality)
                .Where(x => x.AgencyId == user.AgencyId).ToListAsync();

            var query = _context.RegistroEstado
                .Where(x => x.Date >= dateIni && x.Date <= dateFin && x.Estado != Order.STATUS_CANCELADA
                && x.Order.PrincipalDistributorId == user.UserId)
                .Where(x => x.Estado == Order.STATUS_RECIBIDA);

            string nameAgency = string.Empty;

            if (!string.IsNullOrEmpty(agencies) && agencies != "null")
            {
                var agencyExport = _context.Agency.Find(Guid.Parse(agencies));
                query = query.Where(x => x.Order.AgencyId == agencyExport.AgencyId || x.Order.agencyTransferida.AgencyId == agencyExport.AgencyId);
                nameAgency = agencyExport.Name;
            }

            if (!string.IsNullOrEmpty(retails) && retails != "null")
            {
                var minorista = _context.Minoristas.Find(Guid.Parse(retails));
                query = query.Where(x => x.Order.Minorista.Id == minorista.Id);
                nameAgency = minorista.Name;
            }

            var orders = await query.OrderBy(x => x.Order.Contact.Address.City).ToListAsync();

            byte[] fileContents;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Paquetes");

                Action<ExcelWorksheet, string, int, int> AddHeader = (wsheet, title, x, y) =>
                {
                    wsheet.Cells[x, y].Value = title;
                    wsheet.Cells[x, y].Style.Font.Size = 12;
                    wsheet.Cells[x, y].Style.Font.Bold = true;
                    wsheet.Cells[x, y].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                };

                string titleHeader;
                if (!string.IsNullOrEmpty(nameAgency))
                    titleHeader = $"{nameAgency.ToUpper()} DEL ${dateIni.ToString(formatDate)} AL ${dateFin.ToString(formatDate)}";
                else
                    titleHeader = $"DEL ${dateIni.ToString(formatDate)} AL ${dateFin.ToString(formatDate)}";

                worksheet.Cells[1, 4].Value = titleHeader;
                worksheet.Cells[1, 4].Style.Font.Size = 22;
                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                int row = 3;
                int col = 1;

                //Añado los encabezados
                new List<string>()
                {
                    "No", "F/Recibido", "No.Fact", "F/Entrega", "Agencia", "Nombre y Apellidos", "Direccion", "Municipio", "Provincia",
                    "Telefono", "U.S.D", "C/Libras", "Estado", "Precio"
                }.ForEach(header => AddHeader(worksheet, header, row, col++));


                //Contenido
                row++;
                int count = 1;
                foreach (var statusByOrder in orders.GroupBy(x => x.OrderId))
                {
                    col = 1;
                    var order = await _context.Order
                        .Include(x => x.Agency)
                        .Include(x => x.Minorista)
                        .Include(x => x.agencyTransferida)
                        .Include(x => x.RegistroEstados)
                        .Include(x => x.Contact).ThenInclude(x => x.Address)
                        .Include(x => x.Contact).ThenInclude(x => x.Phone1)
                        .FirstOrDefaultAsync(x => x.OrderId == statusByOrder.Key);

                    var priceAux = prices.FirstOrDefault(x => (x.RetailId == order.Minorista?.Id || (x.RetailAgencyId == order.AgencyId || (order.agencyTransferida != null && order.agencyTransferida.AgencyId == x.RetailAgencyId)))
                    && x.Province.nombreProvincia == order.Contact.Address.City && x.Municipality.nombreMunicipio == order.Contact.Address.State);

                    var fechaRecibido = order.RegistroEstados.FirstOrDefault(x => x.Estado.Equals(Order.STATUS_RECIBIDA))?.Date;
                    var fechaEntrega = order.RegistroEstados.FirstOrDefault(x => x.Estado.Equals(Order.STATUS_ENTREGADA))?.Date;
                    string agency = order.agencyTransferida != null ? order.agencyTransferida.Name : order.Agency.Name;

                    string number = order.Number.Substring(order.Number.Length - 5);
                    if (order.Minorista != null && !string.IsNullOrEmpty(order.NoOrden))
                    {
                        number = order.NoOrden;
                    }
                    worksheet.Cells[row, col++].Value = count;
                    worksheet.Cells[row, col++].Value = fechaRecibido != null ? ((DateTime)fechaRecibido).ToString(formatDate) : string.Empty;
                    worksheet.Cells[row, col++].Value = number;
                    worksheet.Cells[row, col++].Value = fechaEntrega != null ? ((DateTime)fechaEntrega).ToString(formatDate) : "No Entregada";
                    worksheet.Cells[row, col++].Value = agency;
                    worksheet.Cells[row, col++].Value = order.Contact.FullData;
                    worksheet.Cells[row, col++].Value = order.Contact.Address.AddressLine1;
                    worksheet.Cells[row, col++].Value = order.Contact.Address.State;
                    worksheet.Cells[row, col++].Value = order.Contact.Address.City;
                    worksheet.Cells[row, col++].Value = order.Contact.Phone1.Number;
                    worksheet.Cells[row, col++].Value = "5.60";
                    worksheet.Cells[row, col++].Value = order.CantLb + order.CantLbMedicina;
                    worksheet.Cells[row, col++].Value = order.Status;
                    worksheet.Cells[row, col++].Value = priceAux?.Price ?? decimal.Zero;

                    count++;
                    row++;
                }

                fileContents = package.GetAsByteArray();
            }

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: "Factura.xlsx"
            );
        }
    }
}