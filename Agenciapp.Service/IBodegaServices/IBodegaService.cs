using Agenciapp.Service.IBodegaServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Log = Serilog.Log;

namespace Agenciapp.Service.IBodegaServices
{
    public interface IBodegaService
    {
        Task<Result<string>> DepositAsync(ProductoBodega product, Bodega bodega, decimal qty);
        Result Extract(List<ExtractProductModel> model);
        Task<Result<string>> AdjustAsync(ProductoBodega product, Bodega bodega, decimal qty);
    }

    public class BodegaService : IBodegaService
    {
        private readonly databaseContext _context;
        public BodegaService(databaseContext context)
        {
            _context = context;
        }

        private static readonly object lockAdjust = new object();
        public async Task<Result<string>> AdjustAsync(ProductoBodega product, Bodega bodega, decimal qty)
        {
            lock (lockAdjust)
            {
                try
                {
                    var movimiento = _context.TiposMovimientos.FirstOrDefault(tm => tm.Nombre == Movimiento.MOVIMIENTO_ADJUST);
                    var db_product = _context.BodegaProductos.FirstOrDefault(p => p.IdBodega == bodega.Id &&
                    p.IdProducto == product.IdProducto);
                    if (db_product == null)
                    {
                        db_product = new BodegaProducto
                        {
                            IdBodegaProducto = Guid.NewGuid(),
                            IdBodega = bodega.Id,
                            Bodega = bodega,
                            IdProducto = product.IdProducto,
                            Cantidad = 0,
                            Monto = 0
                        };
                        _context.Add(db_product);
                        _context.SaveChangesAsync();
                    }

                    if (db_product.Cantidad != qty)
                    {
                        var mov = new Movimiento
                        {
                            IdBodegaOrigen = bodega.Id,
                            IdBodegaDestino = bodega.Id,
                            IdTipoMovimiento = movimiento.IdTipoMovimiento,
                            IdAgency = product.IdAgency
                        };
                        _context.Movimientos.Add(mov);

                        var cantidad = Math.Abs(db_product.Cantidad - qty);

                        var monto = db_product.Cantidad == 0 ? 0 : db_product.Monto / db_product.Cantidad * cantidad;
                        var movProd = new MovimientoProducto
                        {
                            IdProducto = db_product.IdProducto,
                            Cantidad = cantidad,
                            Precio = monto,
                            Movimiento = mov
                        };

                        db_product.Cantidad = qty;
                        db_product.Monto -= monto;
                        _context.BodegaProductos.Attach(db_product);

                        _context.SaveChanges();
                    }
                    return Result.Success("Se ha ajustado la disponibilidad del producto");
                }
                catch (Exception e)
                {
                    Log.Fatal(e, "Server Error");
                    return Result.Failure<string>("No se ha podido ajustar la cantidad del producto");
                }
            }
        }

        private static readonly object lockDeposit = new object();
        public async Task<Result<string>> DepositAsync(ProductoBodega product, Bodega bodega, decimal qty)
        {
            lock (lockDeposit)
            {
                try
                {
                    var bodegaProduct = _context.BodegaProductos.Include(x => x.Producto).Include(x => x.Bodega).FirstOrDefault(bp => bp.IdProducto == product.IdProducto);
                    if (bodegaProduct == null)
                    {
                        bodegaProduct = new BodegaProducto
                        {
                            IdBodega = bodega.Id,
                            Bodega = bodega,
                            IdProducto = product.IdProducto,
                            Cantidad = 0,
                            Monto = 0
                        };
                    }

                    //Creo un movimiento
                    Movimiento mov = new Movimiento();
                    mov.IdMovimiento = Guid.NewGuid();
                    mov.BodegaDestino = bodegaProduct.Bodega;
                    mov.IdBodegaDestino = bodegaProduct.Bodega.Id;

                    TipoMovimiento tipoMov = _context.TiposMovimientos
                    .FirstOrDefault(tm => tm.Nombre == Movimiento.MOVIMIENTO_IN);

                    mov.TipoMovimiento = tipoMov;
                    mov.IdTipoMovimiento = tipoMov.IdTipoMovimiento;
                    _context.Movimientos.Add(mov);

                    MovimientoProducto movProd = new MovimientoProducto();
                    movProd.IdMovimiento = mov.IdMovimiento;
                    movProd.IdAgency = bodegaProduct.Bodega.idAgency;
                    movProd.Cantidad = qty;
                    movProd.IdMovimientoProducto = Guid.NewGuid();
                    movProd.Movimiento = mov;
                    movProd.Precio = 0;
                    movProd.Producto = product;
                    _context.MovimientosProductos.Add(movProd);
                    bodegaProduct.Cantidad += qty;
                    if (bodegaProduct.Cantidad == 0)
                    {
                        bodegaProduct.Monto = 0;
                    }
                    else
                    {
                        bodegaProduct.Monto -= bodegaProduct.Monto / bodegaProduct.Cantidad * qty;
                    }
                    _context.BodegaProductos.Attach(bodegaProduct);

                    _context.SaveChanges();

                    return Result.Success("El producto ha sido ingresado satisfactoriamente");
                }
                catch (Exception e)
                {
                    Log.Fatal(e, "Server Error");
                    return Result.Failure<string>("No se ha podido ingresar la cantidad del producto.");
                }
            }
        }

        private static readonly object lockExtraer = new object();
        public Result Extract(List<ExtractProductModel> model)
        {
            lock (lockExtraer)
            {
                foreach (var extract in model)
                {
                    var response = ExtractProduct(extract);
                    if (response.IsFailure) return response;
                }
                _context.SaveChanges();
                return Result.Success();
            }
        }
        
        private Result ExtractProduct(ExtractProductModel extractProduct)
        {
            var bodegasProducto = _context.BodegaProductos.Include(x => x.Producto).Include(x => x.Bodega).Where(bp => bp.IdProducto == extractProduct.Product.IdProducto);
            if (bodegasProducto.Count() == 0)
            {
                return Result.Failure("El producto no esta disponible.");
            }
            if (extractProduct.Qty > bodegasProducto.Sum(x => x.Cantidad))
            {
                return Result.Failure("No es posible extraer esa cantidad.");
            }
            if (extractProduct.Qty > 0)
            {
                foreach (var item in bodegasProducto)
                {
                    //Creo un movimiento
                    Movimiento mov = new Movimiento();
                    mov.IdMovimiento = Guid.NewGuid();
                    mov.BodegaOrigen = item.Bodega;
                    mov.IdBodegaOrigen = item.Bodega.Id;

                    TipoMovimiento tipoMov = _context.TiposMovimientos
                    .FirstOrDefault(tm => tm.Nombre == Movimiento.MOVIMIENTO_OUT);

                    mov.TipoMovimiento = tipoMov;
                    mov.IdTipoMovimiento = tipoMov.IdTipoMovimiento;
                    _context.Movimientos.Add(mov);

                    if (item.Cantidad < extractProduct.Qty)
                    {
                        MovimientoProducto movProd = new MovimientoProducto();
                        movProd.IdAgency = item.Bodega.idAgency;
                        movProd.IdMovimiento = mov.IdMovimiento;
                        movProd.Cantidad = item.Cantidad;
                        movProd.IdMovimientoProducto = Guid.NewGuid();
                        movProd.Movimiento = mov;
                        movProd.Precio = 0;
                        movProd.Producto = extractProduct.Product;
                        _context.MovimientosProductos.Add(movProd);

                        extractProduct.Qty -= (int)item.Cantidad;
                        item.Cantidad = 0;
                        item.Monto = 0;
                        _context.BodegaProductos.Update(item);
                    }
                    else
                    {
                        MovimientoProducto movProd = new MovimientoProducto();
                        movProd.IdMovimiento = mov.IdMovimiento;
                        movProd.IdAgency = item.Bodega.idAgency;
                        movProd.Cantidad = extractProduct.Qty;
                        movProd.IdMovimientoProducto = Guid.NewGuid();
                        movProd.Movimiento = mov;
                        movProd.Precio = 0;
                        movProd.Producto = extractProduct.Product;
                        _context.MovimientosProductos.Add(movProd);
                        item.Cantidad -= extractProduct.Qty;
                        if (item.Cantidad == 0)
                        {
                            item.Monto = 0;
                        }
                        else
                        {
                            item.Monto -= item.Monto / item.Cantidad * extractProduct.Qty;
                        }
                        _context.BodegaProductos.Update(item);
                        break;
                    }
                }
            }
            return Result.Success();
        }
    }
}
