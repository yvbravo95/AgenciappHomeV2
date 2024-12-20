using AgenciappHome.Controllers.Class;
using AgenciappHome.Models;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers
{
    public class Base: Controller
    {
        protected readonly Guid AgencyHoustonId = Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0");
        protected readonly Guid AgencyDallasId = Guid.Parse("4F1DDEF5-0592-46AD-BEEE-3316CB84385B");
        protected readonly databaseContext _context;
        protected IWebHostEnvironment _env;
        protected Settings _settings;
        protected readonly IMapper _mapper;

        public Base(databaseContext context, IWebHostEnvironment env, IOptions<Settings> settings)
        {
            _context = context;
            _env = env;
            _settings = settings.Value;
        }

        public Base(databaseContext context, IWebHostEnvironment env, IOptions<Settings> settings, IMapper mapper)
        {
            _context = context;
            _env = env;
            _settings = settings.Value;
            _mapper = mapper;
        }

        public IActionResult ViewAutorize(string[] Roles, object parm = null)
        {
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();
            var role = AgenciaAuthorize.getRole(User, _context);
            ViewData["agencyId"] = aAgency.AgencyId;
            var office = _context.Office.Where(x => x.AgencyId == aAgency.AgencyId).ToList();
            ViewData["isChangeOffice"] = false;
            ViewData["IsCarga"] = DataCookie.getCookie("IsCarga", Request);
            if (office.Count > 1 && role != AgenciaAuthorize.TypeAutorize.Empleado)
                ViewData["isChangeOffice"] = true;


            if (Roles == null)
            {
                Roles = new string[] { role.ToString() };
            }
            else if (Roles.Length == 0)
            {
                Roles = new string[] { role.ToString() };
            }

            // Obtengo el path para verificar si el usuario tiene acceso a la ruta
            string controller = this.Url.ActionContext.RouteData.Values["controller"].ToString();
            string action = this.Url.ActionContext.RouteData.Values["action"].ToString();

            if (!AgenciaAuthorize.Autorize(User, _context, Roles, controller, action))
            {
                return RedirectToAction("Index", "AccessDenied");
            }
            else
            {
                ViewData["Role"] = role.ToString();
            }
            return View(parm);
        }
        public IActionResult ViewAutorize(string[] Roles, string parm1 = null, object parm2 = null)
        {
            var aUser = _context.User.Where(x => x.Username == User.Identity.Name);
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.FirstOrDefault().AgencyId).FirstOrDefault();
            var role = AgenciaAuthorize.getRole(User, _context);
            ViewData["agencyId"] = aAgency.AgencyId;
            ViewData["agencyCode"] = aAgency.Code;
            ViewData["IsCarga"] = DataCookie.getCookie("IsCarga", Request);
            var office = _context.Office.Where(x => x.AgencyId == aAgency.AgencyId).ToList();
            ViewData["isChangeOffice"] = false;
            if (office.Count > 1 && role != AgenciaAuthorize.TypeAutorize.Empleado)
                ViewData["isChangeOffice"] = true;

            if (Roles == null)
            {
                Roles = new string[] { role.ToString() };
            }
            else if (Roles.Length == 0)
            {
                Roles = new string[] { role.ToString() };
            }

            // Obtengo el path para verificar si el usuario tiene acceso a la ruta
            string controller = this.Url.ActionContext.RouteData.Values["controller"].ToString();
            string action = this.Url.ActionContext.RouteData.Values["action"].ToString();

            if (!AgenciaAuthorize.Autorize(User, _context, Roles, controller, action))
            {
                return RedirectToAction("Index", "AccessDenied");
            }
            else
            {
                ViewData["Role"] = role.ToString();
            }
            return View(parm1, parm2);
        }
        public IQueryable<Bill> GetBillsOrder(Guid orderId){
            var bills = _context.ServiciosxPagar
                    .Include(x => x.Bill)
                    .Where(x => x.SId == orderId && x.Bill != null).GroupBy(x => x.Bill).Select(x => x.Key);

            return bills;
        }
        public IQueryable<Factura> GetFacturasOrder(Guid orderId){
            var facturas = _context.servicioxCobrar
            .Include(x => x.factura)
            .Where(x => x.factura != null && x.ServicioId == orderId).GroupBy(x => x.factura).Select(x => x.Key);
        
            return facturas;
        }
        public Result ServicesByPayWhenCancelOrder(Guid orderId){
            var servicesByPay = _context.ServiciosxPagar
            .Include(x => x.Agency)
            .Include(x => x.EnvioCaribe)
            .Include(x => x.EnvioMaritimo)
            .Include(x => x.Mayorista)
            .Include(x => x.Order)
            .Include(x => x.OrderCubic)
            .Include(x => x.Passport)
            .Include(x => x.Rechargue)
            .Include(x => x.Invoice)
            .Include(x => x.Remittance)
            .Include(x => x.Reserva)
            .Include(x => x.Servicio)
            .Include(x => x.Bill).Where(x => x.SId == orderId);
            _context.ServiciosxPagar.RemoveRange(servicesByPay.Where(x => x.Bill == null));

            foreach (var item in servicesByPay.Where(x => x.Bill != null && x.Mayorista != null))
            {
                var newSxp = new ServiciosxPagar{
                    Agency = item.Agency,
                    Date = DateTime.Now,
                    EnvioCaribe = item.EnvioCaribe,
                    EnvioMaritimo = item.EnvioMaritimo,
                    Express = item.Express,
                    ImporteAPagar = item.ImporteAPagar * (-1),
                    Invoice = item.Invoice,
                    Mayorista = item.Mayorista,
                    NoServicio = item.NoServicio,
                    Order = item.Order,
                    OrderCubic = item.OrderCubic,
                    Passport = item.Passport,
                    Rechargue = item.Rechargue,
                    Remittance = item.Remittance,
                    Reserva = item.Reserva,
                    Servicio = item.Servicio,
                    SId = item.SId,
                    SubTipo = item.SubTipo,
                    Tipo = item.Tipo,
                    NotaCredito = true
                };
                _context.Add(newSxp);
            }
            return Result.Success();
        }
        public Result ServicesReceivableWhenCancelOrder(Guid orderId){
            var services = _context.servicioxCobrar.Include(x => x.factura).Where(x => x.ServicioId == orderId);
            _context.servicioxCobrar.RemoveRange(services.Where(x => x.factura == null));

            return Result.Success();
        }
    }
}
