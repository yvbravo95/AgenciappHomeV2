using Agenciapp.Common.Services;
using Agenciapp.Service.IWholesalerServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log = Serilog.Log;

namespace Agenciapp.Service.IWholesalerServices
{
    public interface IWholesalerService
    {
        Task<Result<List<Supplier>>> GetWholesalersByCategory(string Category);
        Task<Result<Supplier>> GetWholesalerByName(string name, string category, Guid? agencyId);
    }

    public class WholesalerService : IWholesalerService
    {
        private readonly databaseContext _context;
        private readonly IUserResolverService _user;

        public WholesalerService(databaseContext context, IUserResolverService user)
        {
            _context = context;
            _user = user;
        }
        public async Task<Result<List<Supplier>>> GetWholesalersByCategory(string Category)
        {
            try
            {
                var user = _user.GetUser();
                if(user == null)
                {
                    return Result.Failure<List<Supplier>>("Debe estar autenticado");
                }
                List<Supplier> suppliers = await _context.Wholesalers.Where(x => x.AgencyId == user.AgencyId && (x.Category.category == "Combos" || x.Category.category == "Tienda"))
                .Where(x => Category != null? string.Equals(x.Category.category, Category, StringComparison.OrdinalIgnoreCase): true)
                    .Select(x => new Supplier { Id = x.IdWholesaler, Name = x.name}).ToListAsync();
                return Result.Success(suppliers);
            }
            catch(Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<List<Supplier>>("No se han podido obtener los proveedores");
            }
        }

        public async Task<Result<Supplier>> GetWholesalerByName(string name, string category, Guid? agencyId)
        {
            try
            {
                Supplier supplier = await _context.Wholesalers
                .Include(x => x.CostByProvinces)
                .Where(x => x.AgencyId == agencyId)
                .Where(x => x.Category.category.ToLower() == category.ToLower() && x.name.Equals(name) && x.EsVisible)
                    .Select(x => new Supplier 
                    { 
                        Id = x.IdWholesaler, 
                        Name = x.name,
                        CostByProvinces = x.CostByProvinces.Select(y => new Supplier.CostByProvince
                        {
                            Cost = y.Cost,
                            Id = y.CostByProvinceId,
                            Type = (Supplier.TypeCost) y.Type,
                            Province = new ReyEnviosStore.Product.Models.Province
                            {
                                Id = y.Provincia.Id,
                                Name = y.Provincia.nombreProvincia
                            }
                        }).ToList()
                    }).FirstOrDefaultAsync();

                if (supplier == null)
                    return Result.Failure<Supplier>("Proveedor no encontrado");

                return Result.Success(supplier);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<Supplier>("No se han podido obtener los proveedores");
            }
        }

    }
}
