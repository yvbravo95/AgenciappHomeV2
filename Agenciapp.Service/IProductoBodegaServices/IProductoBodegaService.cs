
using Agenciapp.Common.Class;
using Agenciapp.Domain.Enums;
using Agenciapp.Service.IProductoBodegaServices.Model;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log = Serilog.Log;
namespace Agenciapp.Service.IProductoBodegaServices
{
    public interface IProductoBodegaService
    {
        Task<Result<ProductoBodega>> Create(Guid AgencyId, CreateProductoBodegaModel product);
        Task<Result<ProductoBodega>> Update(EditProductoBodegaModel product);
    }

    public class ProductoBodegaService : IProductoBodegaService
    {
        private readonly databaseContext _context;
        private readonly IHostingEnvironment _env;
        public ProductoBodegaService(databaseContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Result<ProductoBodega>> Create(Guid AgencyId, CreateProductoBodegaModel productRequest)
        {
            try
            {
                var proveedor = await _context.Wholesalers.Include(x => x.Category).FirstOrDefaultAsync(x => x.AgencyId == AgencyId && x.IdWholesaler == productRequest.IdProveedor);
                var category = await _context.CategoriaBodegas.FindAsync(productRequest.IdCategoria);
                if (category == null) return Result.Failure<ProductoBodega>("La categoría no existe");
                if(category.Nombre == "Combos")
                {
                    if (proveedor.Category?.category != "Combos")
                    {
                        return Result.Failure<ProductoBodega>("El mayorista no pertenece a la categoría " + OrderType.Combo.GetDescription());
                    }
                }
                else if(category.Nombre == "Tienda")
                {
                    if (proveedor.Category?.category != OrderType.Tienda.GetDescription())
                    {
                        return Result.Failure<ProductoBodega>("El mayorista no pertenece a la categoría " + OrderType.Tienda.GetDescription());
                    }
                }
                else
                {
                    return Result.Failure<ProductoBodega>("No es posible crear un producto para la categoría " + category.Nombre);
                }
                
                var product = new ProductoBodega
                {
                    IdProducto = Guid.NewGuid(),
                    IdAgency = AgencyId,
                    Descripcion = productRequest.Descripcion,
                    Codigo = productRequest.Codigo,
                    IdCategoria = productRequest.IdCategoria,
                    CodigoEan = productRequest.CodigoEan,
                    esVisible = productRequest.IsAvailable,
                    FichaTecnica = productRequest.FichaTecnica,
                    IdProveedor = productRequest.IdProveedor,
                    IdUnidadMedida = productRequest.IdUnidadMedida,
                    Nombre = productRequest.Nombre,
                    Terms = productRequest.Terms,
                    Ubicacion = productRequest.Ubicacion,
                    Precio1Minorista = new PrecioRefMinorista(),
                    Precio2Minorista = new PrecioRefMinorista(),
                    Precio3Minorista = new PrecioRefMinorista(),
                    Proveedor = proveedor,
                    PrecioCompraReferencial = productRequest.PrecioCompraReferencial,
                    PrecioVentaReferencial = productRequest.PrecioVentaReferencial
                };
                var date = DateTime.Now.ToString("yMMddHHmmssff");
                string sWebRootFolder = _env.WebRootPath;

                if (productRequest.ImageFile != null)
                {
                    var auxName = productRequest.ImageFile.FileName;
                    var arrName = auxName.Split('.');
                    string filename = date + '.' + arrName[1];
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "ProductosBodega";
                    filePath = Path.Combine(filePath, filename);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await productRequest.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ImagenProducto = filename;
                }

                var aAgency = _context.Agency.Find(AgencyId);
                if (aAgency.Type == "Agencia" && category.Nombre == "Combos")
                {
                    product.Precio1Minorista.PrecioRefMinoristaId = Guid.NewGuid();
                    foreach (var item in productRequest.agenciasprecio1)
                    {
                        product.Precio1Minorista.AgencyPrecioRefMinoristas.Add(new AgencyPrecioRefMinorista
                        {
                            AgencyId = item,
                            AgencyPrecioRefMinoristaId = Guid.NewGuid(),
                            PrecioRefMinoristaId = product.Precio1Minorista.PrecioRefMinoristaId
                        });
                    }

                    product.Precio2Minorista.PrecioRefMinoristaId = Guid.NewGuid();
                    foreach (var item in productRequest.agenciasprecio2)
                    {
                        product.Precio2Minorista.AgencyPrecioRefMinoristas.Add(new AgencyPrecioRefMinorista
                        {
                            AgencyId = item,
                            AgencyPrecioRefMinoristaId = Guid.NewGuid(),
                            PrecioRefMinoristaId = product.Precio2Minorista.PrecioRefMinoristaId
                        });
                    }

                    product.Precio3Minorista.PrecioRefMinoristaId = Guid.NewGuid();
                    foreach (var item in productRequest.agenciasprecio3)
                    {
                        product.Precio3Minorista.AgencyPrecioRefMinoristas.Add(new AgencyPrecioRefMinorista
                        {
                            AgencyId = item,
                            AgencyPrecioRefMinoristaId = Guid.NewGuid(),
                            PrecioRefMinoristaId = product.Precio3Minorista.PrecioRefMinoristaId
                        });
                    }
                }

                if(productRequest.CatalogItems != null)
                {
                    foreach (var item in productRequest.CatalogItems.GroupBy(x => x))
                    {
                        _context.ProductoBodegaCatalogItems.Add(new ProductoBodegaCatalogItem
                        {
                            CatalogItemId = item.Key,
                            ProductoBodegaId = product.IdProducto,
                        });
                    }
                }
               
                _context.Add(product);
                await _context.SaveChangesAsync();
                return Result.Success(product);
            }
            catch (Exception e)
            {
                Log.Error(e, "Server Error");
                return Result.Failure<ProductoBodega>("No se ha podido crear el producto");
            }
        }
        public async Task<Result<ProductoBodega>> Update(EditProductoBodegaModel productRequest)
        {
            try
            {
                var proveedor = await _context.Wholesalers.FirstOrDefaultAsync(x => x.AgencyId == productRequest.AgencyId && x.IdWholesaler == productRequest.IdProveedor);
                var product = await _context.ProductosBodegas
                    .Include(x => x.Precio1Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio2Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio3Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.productoBodegaCatalogItems)
                    .FirstOrDefaultAsync(x => x.IdProducto == productRequest.IdProducto);

                var agency = await _context.Agency.FindAsync(product.IdAgency);

                if (product == null)
                {
                    return Result.Failure<ProductoBodega>("Product does not exist");
                }
                var date = DateTime.Now.ToString("yMMddHHmmssff");
                string sWebRootFolder = _env.WebRootPath;
                if (productRequest.ImageFile != null)
                {
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "ProductosBodega";

                    if (product.ImagenProducto != null)
                    {
                        //Elimino la imagen anterior
                        File.Delete(Path.Combine(filePath, product.ImagenProducto));
                    }

                    //Creo una nueva
                    var auxName = productRequest.ImageFile.FileName;
                    var arrName = auxName.Split('.');
                    string filename = date + '.' + arrName[1];
                    filePath = Path.Combine(filePath, filename);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await productRequest.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ImagenProducto = filename;
                }
                product.Codigo = productRequest.Codigo;
                product.CodigoEan = productRequest.CodigoEan;
                product.Descripcion = productRequest.Descripcion;
                product.FichaTecnica = productRequest.FichaTecnica;
                product.IdCategoria = productRequest.IdCategoria;
                product.IdProveedor = productRequest.IdProveedor;
                product.IdCategoria = productRequest.IdCategoria;
                product.Proveedor = proveedor;
                product.esVisible = productRequest.IsAvailable;
                product.PrecioCompraReferencial = productRequest.PrecioCompraReferencial;
                product.PrecioVentaReferencial = productRequest.PrecioVentaReferencial;

                var category = _context.CategoriaBodegas.Find(product.IdCategoria);

                if (agency.Type == "Mayorista" && category.Nombre == "Combos")
                {
                    if (product.Precio1Minorista.PrecioRefMinoristaId == null || product.Precio1Minorista.PrecioRefMinoristaId == Guid.Empty)
                    {
                        product.Precio1Minorista.PrecioRefMinoristaId = Guid.NewGuid();
                        product.Precio1MinoristaId = product.Precio1Minorista.PrecioRefMinoristaId;
                        _context.Add(product.Precio1Minorista);

                    }
                    else
                    {
                        var agencyprecio = _context.AgencyPrecioRefMinoristas.Where(x => x.PrecioRefMinoristaId == product.Precio1Minorista.PrecioRefMinoristaId);
                        foreach (var item in agencyprecio)
                        {
                            _context.Remove(item);
                        }
                        product.Precio1MinoristaId = product.Precio1Minorista.PrecioRefMinoristaId;
                        _context.Update(product.Precio1Minorista);
                    }

                    if (product.Precio2Minorista.PrecioRefMinoristaId == null || product.Precio2Minorista.PrecioRefMinoristaId == Guid.Empty)
                    {
                        product.Precio2Minorista.PrecioRefMinoristaId = Guid.NewGuid();
                        product.Precio2MinoristaId = product.Precio2Minorista.PrecioRefMinoristaId;
                        _context.Add(product.Precio2Minorista);
                    }
                    else
                    {
                        var agencyprecio = _context.AgencyPrecioRefMinoristas.Where(x => x.PrecioRefMinoristaId == product.Precio2Minorista.PrecioRefMinoristaId);
                        foreach (var item in agencyprecio)
                        {
                            _context.Remove(item);
                        }
                        product.Precio2MinoristaId = product.Precio2Minorista.PrecioRefMinoristaId;

                        _context.Update(product.Precio2Minorista);
                    }

                    if (product.Precio3Minorista.PrecioRefMinoristaId == null || product.Precio3Minorista.PrecioRefMinoristaId == Guid.Empty)
                    {
                        product.Precio3Minorista.PrecioRefMinoristaId = Guid.NewGuid();
                        product.Precio3MinoristaId = product.Precio3Minorista.PrecioRefMinoristaId;
                        _context.Add(product.Precio3Minorista);
                    }
                    else
                    {
                        var agencyprecio = _context.AgencyPrecioRefMinoristas.Where(x => x.PrecioRefMinoristaId == product.Precio3Minorista.PrecioRefMinoristaId);
                        foreach (var item in agencyprecio)
                        {
                            _context.Remove(item);
                        }
                        product.Precio3MinoristaId = product.Precio3Minorista.PrecioRefMinoristaId;
                        _context.Update(product.Precio3Minorista);
                    }

                    if(productRequest.agenciasprecio1 != null)
                    {
                        foreach (var item in productRequest.agenciasprecio1)
                        {
                            var agencyprice = new AgencyPrecioRefMinorista
                            {
                                AgencyId = item,
                                AgencyPrecioRefMinoristaId = Guid.NewGuid(),
                                PrecioRefMinoristaId = (Guid)product.Precio1MinoristaId
                            };
                            _context.Add(agencyprice);
                        }
                    }
                    if(productRequest.agenciasprecio2 != null)
                    {
                        foreach (var item in productRequest.agenciasprecio2)
                        {
                            var agencyprice = new AgencyPrecioRefMinorista
                            {
                                AgencyId = item,
                                AgencyPrecioRefMinoristaId = Guid.NewGuid(),
                                PrecioRefMinoristaId = (Guid)product.Precio2MinoristaId
                            };
                            _context.Add(agencyprice);
                        }
                    }
                    
                    if(productRequest.agenciasprecio3 != null)
                    {
                        foreach (var item in productRequest.agenciasprecio3)
                        {
                            var agencyprice = new AgencyPrecioRefMinorista
                            {
                                AgencyId = item,
                                AgencyPrecioRefMinoristaId = Guid.NewGuid(),
                                PrecioRefMinoristaId = (Guid)product.Precio3MinoristaId
                            };
                            _context.Add(agencyprice);
                        }
                    }
                }
                //Catalog Items
                if (productRequest.CatalogItems != null)
                {
                    _context.ProductoBodegaCatalogItems.RemoveRange(_context.ProductoBodegaCatalogItems.Where(x => x.ProductoBodegaId == product.IdProducto));
                    foreach (var item in productRequest.CatalogItems.GroupBy(x => x))
                    {
                        _context.ProductoBodegaCatalogItems.Add(new ProductoBodegaCatalogItem
                        {
                            ProductoBodegaId = product.IdProducto,
                            CatalogItemId = item.Key
                        });
                    }
                }
                await _context.SaveChangesAsync();
                return Result.Success(product);
            }
            catch (Exception e)
            {
                Log.Error(e, "Server Error");
                return Result.Failure<ProductoBodega>("El producto no ha podido ser editado");
            }
        }
      
    }
}
