using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Agenciapp.Service.ReyEnviosStore.Product.Models;
using AgenciappHome.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Log = Serilog.Log;
using Agenciapp.Common.Services;
using Agenciapp.Service.IProductoBodegaServices;
using Agenciapp.Service.IWholesalerServices.Models;
using Agenciapp.Service.IBodegaServices;

namespace Agenciapp.Service.ReyEnviosStore.Product
{
    public interface IProductService
    {
        Task<Result<ProductStore>> GetProduct(Guid AgencyId, string sku);
        Task<Result<List<ProductStore>>> ProductsAvailable(Guid AgencyId);
        Task<Result<List<ProductStore>>> ProductsAvailable(Guid AgencyId, List<string> skus);
        Task<Result<string>> CreateUpdateProduct(Guid AgencyId, ProductStore request);
    }

    public class ProductService: IProductService
    {
        private readonly databaseContext _context;
        private readonly IBodegaService _bodegaService;
        private readonly IProductoBodegaService _productoBodega;
        private readonly IUserResolverService _user;
        public ProductService(databaseContext context, IBodegaService bodegaService, IUserResolverService user, IProductoBodegaService productoBodega)
        {
            _context = context;
            _bodegaService = bodegaService;
            _user = user;
            _productoBodega = productoBodega;
        }

        public async Task<Result<ProductStore>> GetProduct(Guid AgencyId,string sku)
        {
            try
            {
                var product = await _context.ProductosBodegas
                    .Include(x => x.Categoria)
                    .Include(x => x.Proveedor)
                    .Include(x => x.productoBodegaCatalogItems).ThenInclude(x => x.CatalogItem)
                    .FirstOrDefaultAsync(x => x.Codigo == sku && x.IdAgency == AgencyId);
                if (product == null)
                {
                    return Result.Failure<ProductStore>("Product does not exist");
                }
                decimal disponibilidad = _context.BodegaProductos.Where(x => x.Bodega.idAgency == AgencyId && x.IdProducto == product.IdProducto).Sum(x => x.Cantidad);
                return Result.Success(new ProductStore
                {
                    Id = product.IdProducto,
                    Category = product.Categoria.Nombre,
                    Code = product.Codigo,
                    Description = product.Descripcion,
                    IsAvailable = product.esVisible,
                    Name = product.Nombre,
                    Supplier = product.Proveedor != null? new Supplier
                    {
                        Id = product.Proveedor.IdWholesaler,
                        Name = product.Proveedor.name
                    }: null,
                    Availability = (int)disponibilidad,
                    Shipping = product.EnableShipping ? product.Shipping : 0,
                    PurchasePrice = product.PrecioCompraReferencial != null? (decimal)product.PrecioCompraReferencial: 0,
                    SalePrice = product.PrecioVentaReferencial != null? (decimal)product.PrecioVentaReferencial: 0,
                    Provinces = product.productoBodegaCatalogItems.Select(x => new Province { Id = (Guid)x.CatalogItem.RefrenceId,Name = x.CatalogItem.Name }).ToList()
                });
                
            }
            catch(Exception e)
            {
                Serilog.Log.Error(e, "Server Error");
                return Result.Failure<ProductStore>("Product could not be obtained");
            }
        }

        public async Task<Result<List<ProductStore>>> ProductsAvailable(Guid AgencyId)
        {
            try
            {
                var products = await _context.ProductosBodegas
                    .Include(x => x.Categoria)
                    .Include(x => x.Proveedor)
                    .Include(x => x.productoBodegaCatalogItems).ThenInclude(x => x.CatalogItem)
                    .Where(x => x.IdAgency == AgencyId && x.esVisible && x.productoBodegaCatalogItems.Any()).ToListAsync();
                List<ProductStore> response = new List<ProductStore>();
                foreach (var item in products)
                {
                    decimal disponibilidad = _context.BodegaProductos.Where(x => x.Bodega.idAgency == AgencyId && x.IdProducto == item.IdProducto).Sum(x => x.Cantidad);
                    if (disponibilidad > 0)
                    {
                        response.Add(new ProductStore
                        {
                            Availability = (int)disponibilidad,
                            Category = item.Categoria.Nombre,
                            Code = item.Codigo,
                            Description = item.Descripcion,
                            IsAvailable = item.esVisible,
                            Name = item.Nombre,
                            Shipping = item.EnableShipping ? item.Shipping : 0,
                            Supplier = item.Proveedor != null? new Supplier { Id = (Guid)item.Proveedor?.IdWholesaler, Name = item.Proveedor?.name}: null,
                            PurchasePrice = item.PrecioCompraReferencial != null ? (decimal)item.PrecioCompraReferencial : 0,
                            SalePrice = item.PrecioVentaReferencial != null ? (decimal)item.PrecioVentaReferencial : 0,
                            Provinces = item.productoBodegaCatalogItems.Select(x => new Province { Id = (Guid)x.CatalogItem.RefrenceId, Name = x.CatalogItem.Name }).ToList()
                        });
                    }
                }
                return Result.Success(response);
            }
            catch(Exception e)
            {
                Log.Error(e, "Server Error");
                return Result.Failure<List<ProductStore>>("Products could not be obtained");
            }
        }

        public async Task<Result<string>> CreateUpdateProduct(Guid AgencyId,ProductStore request)
        {
            try
            {
                var product = await _context.ProductosBodegas
                    .Include(x => x.UnidadMedida)
                    .Include(x => x.Categoria)
                    .Include(x => x.Precio1Minorista)
                    .Include(x => x.Precio2Minorista)
                    .Include(x => x.Precio3Minorista)
                    .Include(x => x.productoBodegaCatalogItems).ThenInclude(x => x.CatalogItem)
                    .Include(x => x.Categoria)
                    .FirstOrDefaultAsync(x => x.Codigo == request.Code && x.IdAgency == AgencyId);
                Result<ProductoBodega> result = null;
                if(request.Category != "Combos")
                {
                    request.Category = "Tienda";
                }
                var category = await _context.CategoriaBodegas.FirstOrDefaultAsync(x => x.Nombre == request.Category);

                if(category == null)
                {
                    category = new CategoriaBodega
                    {
                        Nombre = request.Category
                    };
                    _context.Attach(category);
                    await _context.SaveChangesAsync();
                }

                if (product == null)
                {
                    var unidadMedida = _context.UnidadMedidas.FirstOrDefault();
                    result = await _productoBodega.Create(AgencyId, new IProductoBodegaServices.Model.CreateProductoBodegaModel
                    {
                        Codigo = request.Code,
                        Descripcion = request.Description,
                        IdCategoria = category.IdCategoria,
                        Nombre = request.Name,
                        IdProveedor = request.Supplier.Id,
                        IsAvailable = request.IsAvailable,
                        IdUnidadMedida = unidadMedida.IdUnidadMedida,
                        PrecioCompraReferencial = request.PurchasePrice,
                        PrecioVentaReferencial = request.SalePrice,
                        CatalogItems = _context.CatalogItems.Where(x => x.AgencyId == AgencyId && request.Provinces.Any(y => y.Id == x.RefrenceId)).Select(x => x.Id).ToList()
                    });
                    if (result.IsFailure)
                    {
                        return Result.Failure<string>(result.Error);
                    }

                    result.Value.Shipping = request.Shipping;
                    result.Value.EnableShipping = request.Shipping > 0;
                    _context.Update(result.Value);
                    await _context.SaveChangesAsync();

                    var bodega = await _context.Bodegas.FirstOrDefaultAsync(x => x.idAgency == AgencyId);
                    if (bodega == null)
                        return Result.Failure<string>("No existen bodegas para ingresar el producto");
                    var resultExtract = await _bodegaService.DepositAsync(result.Value, bodega, request.Availability);
                    if (resultExtract.IsFailure)
                    {
                        return Result.Failure<string>(resultExtract.Error);
                    }
                    return Result.Success("El producto ha sido creado satisfactoriamente");
                }
                else
                {
                    result = await _productoBodega.Update(new IProductoBodegaServices.Model.EditProductoBodegaModel
                    {
                        Codigo = request.Code,
                        Descripcion = request.Description,
                        IdCategoria = category.IdCategoria,
                        Nombre = request.Name,
                        IdProveedor = request.Supplier.Id,
                        IsAvailable = request.IsAvailable,
                        IdProducto = product.IdProducto,
                        CodigoEan = product.CodigoEan,
                        FichaTecnica = product.FichaTecnica,
                        IdUnidadMedida = product.IdUnidadMedida,
                        PrecioCompraReferencial = request.PurchasePrice,
                        PrecioVentaReferencial = request.SalePrice,
                        Precio1MinoristaId = product.Precio1MinoristaId,
                        Precio2MinoristaId = product.Precio2MinoristaId,
                        Precio3MinoristaId = product.Precio3MinoristaId,
                        Terms = product.Terms,
                        Ubicacion = product.Ubicacion,
                        AgencyId = product.IdAgency,
                        CatalogItems = _context.CatalogItems.Where(x =>x.AgencyId == AgencyId && request.Provinces.Any(y => y.Id == x.RefrenceId)).Select(x => x.Id).ToList()
                    });

                    if (result.IsFailure)
                    {
                        return Result.Failure<string>(result.Error);
                    }

                    if(request.Shipping > 0)
                    {
                        result.Value.Shipping = request.Shipping;
                        result.Value.EnableShipping = true;
                        _context.Update(result.Value);
                        await _context.SaveChangesAsync();
                    }

                    var bodega = await _context.Bodegas.FirstOrDefaultAsync(x => x.idAgency == AgencyId);
                    if (bodega == null)
                        return Result.Failure<string>("No existen bodegas para ingresar el producto");
                    var resultAdjust = await _bodegaService.AdjustAsync(result.Value,bodega , request.Availability);
                    if (resultAdjust.IsFailure)
                    {
                        return Result.Failure<string>(resultAdjust.Error);
                    }
                    
                    return Result.Success("El producto ha sido editado satisfactoriamente");

                }
            }
            catch(Exception e)
            {
                Log.Error(e, "Server Error");
                return Result.Failure<string>("El producto no ha podido ser actualizado.");
            }
        }

        public async Task<Result<List<ProductStore>>> ProductsAvailable(Guid AgencyId, List<string> skus)
        {
            try
            {
                var products = await _context.ProductosBodegas
                    .Include(x => x.Categoria)
                    .Include(x => x.Proveedor)
                    .Include(x => x.productoBodegaCatalogItems).ThenInclude(x => x.CatalogItem)
                    .Where(x => x.IdAgency == AgencyId && x.esVisible && skus.Contains(x.Codigo)).ToListAsync();
                List<ProductStore> response = new List<ProductStore>();
                foreach (var item in products)
                {
                    decimal disponibilidad = _context.BodegaProductos.Where(x => x.Bodega.idAgency == AgencyId && x.IdProducto == item.IdProducto).Sum(x => x.Cantidad);
                    if (disponibilidad > 0)
                    {
                        response.Add(new ProductStore
                        {
                            Availability = (int)disponibilidad,
                            Category = item.Categoria.Nombre,
                            Code = item.Codigo,
                            Description = item.Descripcion,
                            IsAvailable = item.esVisible,
                            Name = item.Nombre,
                            Shipping = item.EnableShipping ? item.Shipping : 0,
                            Supplier = item.Proveedor != null ? new Supplier { Id = (Guid)item.Proveedor?.IdWholesaler, Name = item.Proveedor?.name } : null,
                            PurchasePrice = item.PrecioCompraReferencial != null ? (decimal)item.PrecioCompraReferencial : 0,
                            SalePrice = item.PrecioVentaReferencial != null ? (decimal)item.PrecioVentaReferencial : 0,
                            Provinces = item.productoBodegaCatalogItems.Select(x => new Province { Id = (Guid)x.CatalogItem.RefrenceId, Name = x.CatalogItem.Name }).ToList()
                        });
                    }
                }
                return Result.Success(response);
            }
            catch (Exception e)
            {
                Log.Error(e, "Server Error");
                return Result.Failure<List<ProductStore>>("Products could not be obtained");
            }
        }
    }
}

