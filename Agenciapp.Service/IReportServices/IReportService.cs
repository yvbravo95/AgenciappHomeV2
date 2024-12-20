using System.Runtime.InteropServices;
using Agenciapp.Common.Services;
using Agenciapp.Service.IReportServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agenciapp.Service.IReportServices.Reports;
using Microsoft.AspNetCore.Hosting;

namespace Agenciapp.Service.IReportServices
{
    public interface IReportService
    {
        Task<Result<List<QtySalesModel>>> NumberComboSold();
        Task<Result<List<QtySalesModel>>> NumberServiceSold();
        Task<Result<List<UtilityModel>>> UtilityByService(Guid agencyId, STipo type, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false);
        Task<Result<List<UtilityModel>>> GetAllUtility(Guid agencyId,DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false);
        Task<Result<List<UtilityModel>>> GetAllUtilityRapid(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false);
        Task<Result<List<UtilityModel>>> PassportUtilityByOrderNumber(Guid agencyId, string firstNumber, string secondNumber);
        Task<Result<DataBoxDCubaModel>> GetDataBoxDCuba(Guid agencyId);
        Task<Result<List<UtilityModel>>> ServicioUtilityByOrderNumber(Guid agencyId, string firstNumber, string secondNumber);
        Task<Result<List<UtilityModel>>> UtilityByServiceRapid(Guid agencyId, STipo type, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false);
        Task<string> GetReportPending(DateTime init, DateTime end, Guid userId);
    }

    public class ReportService : IReportService
    {
        private readonly databaseContext _context;
        private readonly IUserResolverService _userService;
        private readonly IUtilityService _utilityService;
        private readonly IHostingEnvironment _env;
        public ReportService(databaseContext context, IUserResolverService userService, IUtilityService utilityService, IHostingEnvironment env)
        {
            _context = context;
            _userService = userService;
            _utilityService = utilityService;
            _env = env;
        }
        public async Task<Result<List<QtySalesModel>>> NumberComboSold()
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<List<QtySalesModel>>("Debe estar authenticado");
            var date = DateTime.Now;
            var orders = _context.Order
                .AsNoTracking().Include(x => x.Package).ThenInclude(x => x.PackageItem).ThenInclude(x => x.Product).ThenInclude(x => x.ProductoBodega)
                .AsNoTracking().Where(x => x.Date.Month == date.Month && (x.AgencyId == user.AgencyId || x.agencyTransferida.AgencyId == user.AgencyId) && x.Status != "Cancelada" && x.Type == "Combo")
                .Select(x => new 
                {
                    products = x.Package.PackageItem.Select(y => new
                    {
                        qty = y.Qty,
                        productId = y.Product.ProductoBodega.IdProducto,
                        productName = y.Product.ProductoBodega.Nombre
                    })
                });
            List<QtySalesModel> response = new List<QtySalesModel>();
            foreach (var item in orders)
            {
                foreach (var product in item.products)
                {
                    var verify = response.FirstOrDefault(x => x.Id == product.productId);
                    if (verify == null)
                    {
                        response.Add(new QtySalesModel
                        {
                            Id = product.productId,
                            Name = product.productName,
                            Qty = (int)product.qty
                        });
                    }
                    else
                    {
                        verify.Qty += (int)product.qty;
                    }
                }
            }
            return Result.Success(response);
        }
        public async Task<Result<List<QtySalesModel>>> NumberServiceSold()
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<List<QtySalesModel>>("Debe estar autenticado");

            var date = DateTime.Now;
            var services = await _context.Servicios
                .AsNoTracking().Where(x => x.agency.AgencyId == user.AgencyId && x.fecha.ToLocalTime().Month == date.Month)
                .Where(x => x.estado != Servicio.EstadoCancelado)
                .GroupBy(x => x.tipoServicio)
                .Select(x => new QtySalesModel
                {
                    Id = x.Key.TipoServicioId,
                    Name = x.Key.Nombre,
                    Qty = x.Count()
                }).ToListAsync();

            return Result.Success(services);
        }
        public async Task<Result<List<UtilityModel>>> UtilityByService(Guid agencyId, STipo type, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false)
        {
            return Result.Success(await _utilityService.GetByService(agencyId,type,dateIni,dateEnd, onlyCanceled));
        }
        public async Task<Result<List<UtilityModel>>> UtilityByServiceRapid(Guid agencyId, STipo type, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false)
        {
            UtilityServiceRapid utility = new UtilityServiceRapid(_context);
            return Result.Success(await utility.GetByService(agencyId,type,dateIni,dateEnd, onlyCanceled));
        }
        public async Task<Result<List<UtilityModel>>> GetAllUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false)
        {
            return Result.Success(await _utilityService.getAllUtility(agencyId,dateIni,dateEnd, onlyCanceled));
        }
        public async Task<Result<List<UtilityModel>>> GetAllUtilityRapid(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false)
        {
            UtilityServiceRapid utility = new UtilityServiceRapid(_context);
            return Result.Success(await utility.getAllUtility(agencyId, dateIni, dateEnd, onlyCanceled));
        }
        public async Task<Result<List<UtilityModel>>> PassportUtilityByOrderNumber(Guid agencyId, string firstNumber, string secondNumber){
            return Result.Success(await _utilityService.PassportUtilityByOrderNumber(agencyId,firstNumber,secondNumber));
        }
        public async Task<Result<List<UtilityModel>>> ServicioUtilityByOrderNumber(Guid agencyId, string firstNumber, string secondNumber){
            return Result.Success(await _utilityService.ServicioUtilityByOrderNumber(agencyId,firstNumber,secondNumber));
        }
        public async Task<Result<DataBoxDCubaModel>> GetDataBoxDCuba(Guid agencyId){
            
            var model = new DataBoxDCubaModel();
            DateTime dateInit = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1,0,0,0);
            DateTime dateEnd = DateTime.Now.Date;
            DateTime dateLastMonth = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1,0,0,0);
            DateTime dateLastMonth_Day = DateTime.Now.AddMonths(-1).Date;
            var getUtility = await _utilityService.GetByService(agencyId, STipo.Passport, dateInit, dateEnd);
            
            var passports = _context.Passport
            .Where(x => (x.AgencyId == agencyId || x.AgencyTransferidaId == agencyId) && x.Status != "Cancelada");
            
            model.ProrrogasThisMonth = await passports.Where(x => (x.ServicioConsular == ServicioConsular.Prorroga1 || x.ServicioConsular == ServicioConsular.Prorroga2) && x.FechaSolicitud.Date >= dateInit).CountAsync();
            model.ProrrogasDiffLastMonth = model.ProrrogasThisMonth - (await passports.Where(x =>(x.ServicioConsular == ServicioConsular.Prorroga1 || x.ServicioConsular == ServicioConsular.Prorroga2) && x.FechaSolicitud.Date >= dateLastMonth && x.FechaSolicitud.Date < dateLastMonth_Day).CountAsync());
            
            model.PrimerVezThisMonth = await passports.Where(x => x.ServicioConsular == ServicioConsular.PrimerVez && x.FechaSolicitud.Date >= dateInit).CountAsync();
            model.PrimerVezDiffLastMonth =  model.PrimerVezThisMonth - (await passports.Where(x =>x.ServicioConsular == ServicioConsular.PrimerVez && x.FechaSolicitud.Date >= dateLastMonth && x.FechaSolicitud.Date < dateLastMonth_Day).CountAsync());
            
            model.RenovarThisMonth = await passports.Where(x => x.ServicioConsular == ServicioConsular.Renovacion && x.FechaSolicitud.Date >= dateInit).CountAsync();
            model.RenovarDiffLastMonth =  model.RenovarThisMonth - (await passports.Where(x =>x.ServicioConsular == ServicioConsular.Renovacion && x.FechaSolicitud.Date >= dateLastMonth && x.FechaSolicitud.Date < dateLastMonth_Day).CountAsync());
            
            model.UtilityPrimerVez = getUtility.Where(x => x.ServicioConsular == ServicioConsular.PrimerVez).Sum(x => x.Utility);
            model.UtilityProrrogas = getUtility.Where(x => x.ServicioConsular == ServicioConsular.Prorroga1 || x.ServicioConsular == ServicioConsular.Prorroga2).Sum(x => x.Utility);
            model.UtilityRenovar = getUtility.Where(x => x.ServicioConsular == ServicioConsular.Renovacion).Sum(x => x.Utility);

            model.ClientsTotal =  _context.Client.Where(x => x.AgencyId == agencyId).Count();
            model.ClientsThisMonth = _context.Client.Where(x => x.AgencyId == agencyId && x.CreatedAt >= dateInit && x.CreatedAt <= dateEnd).Count();

            return Result.Success(model);
        }
        public async Task<string> GetReportPending(DateTime init, DateTime end, Guid userId)
        {
            var document = new ReportPendings(_context, _env);
            var user = await _context.User.FindAsync(userId);
            return await document.GetResportPendings(init,end,user.AgencyId);
        }
    }
}
