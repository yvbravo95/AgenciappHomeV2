using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Common.Class;
using Agenciapp.Domain.Enums;
using Agenciapp.Service.IBodegaServices;
using Agenciapp.Service.IBodegaServices.Models;
using Agenciapp.Service.IStoreServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace Agenciapp.Service.IStoreServices
{

    public interface IStoreService
    {
        Task<Result<Order>> Create(CreateOrderTiendaModel model);
    }

    public class StoreService : IStoreService
    {
        private readonly databaseContext _context;
        private readonly IBodegaService _bodegaServices;
        private readonly Guid agencyReyEnvios = Guid.Parse("2F7B03FB-4BE1-474D-8C95-3EE8C6EAEAC1");

        public StoreService(databaseContext context, IBodegaService bodegaServices)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _bodegaServices = bodegaServices ?? throw new ArgumentNullException(nameof(bodegaServices));
        }

        public async Task<Result<Order>> Create(CreateOrderTiendaModel model)
        {
            var user = await _context.User.FindAsync(model.UserId);
            if (user == null) return Result.Failure<Order>("El usuario no existe");

            var agency = await _context.Agency.FindAsync(model.AgencyId);
            if (agency == null) return Result.Failure<Order>("La agencia no existe");

            var office = await _context.Office.FindAsync(model.OfficeId);
            if (office == null) return Result.Failure<Order>("La oficina no existe");

            var client = await _context.Client.FindAsync(model.ClientId);
            if (client == null) return Result.Failure<Order>("El cliente no existe");

            var contact = await _context.Contact.Include(x => x.Address).FirstOrDefaultAsync(x => x.ContactId == model.ContactId);
            if (contact == null) return Result.Failure<Order>("El contacto no existe");

            // Extraer de la bodega los productos
            var pBodega = await _context.ProductosBodegas
                    .Include(x => x.Precio1Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio2Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio3Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Proveedor)
                    .Include(x => x.Categoria)
                    .Include(x => x.productoBodegaCatalogItems).ThenInclude(x => x.CatalogItem)
                    .Where(x => x.IdAgency == user.AgencyId)
                    .Where(x => model.Products.Any(y => y.Id == x.IdProducto))
                    .ToListAsync();
            if (pBodega.Any(x => x.Categoria.Nombre != "Tienda"))
            {
                return Result.Failure<Order>("Los productos deben pertenecer a la categoría Tienda");
            }

            if (!pBodega.Any()) return Result.Failure<Order>("El trámite debe tener al menos un producto");

            if (pBodega.GroupBy(x => x.Proveedor).Count() > 1) return Result.Failure<Order>("Los productos deben ser de un mismo proveedor");

            List<ExtractProductModel> extractProduct = new List<ExtractProductModel>();
            foreach (var item in pBodega)
            {
                if (item.productoBodegaCatalogItems.Any())
                {
                    var province = item.productoBodegaCatalogItems.FirstOrDefault(x => x.CatalogItem.Name == contact.Address?.City);
                    if (province == null)
                        return Result.Failure<Order>("El producto no está disponible para la provincia " + contact.Address?.City);
                }

                extractProduct.Add(new ExtractProductModel
                {
                    Product = item,
                    Qty = model.Products.FirstOrDefault(x => x.Id == item.IdProducto).Qty
                });
            }
            var extraer = _bodegaServices.Extract(extractProduct);
            if (extraer.IsFailure) return Result.Failure<Order>(extraer.Error);

            _context.AuthorizationCards.Add(model.AuthorizationCard);
            Order order = new Order
            {
                OrderId = Guid.NewGuid(),
                Client = client,
                User = user,
                Agency = agency,
                Office = office,
                Contact = contact,
                Date = DateTime.Now,
                Type = OrderType.Tienda.GetDescription(),
                addCosto = 0,
                addPrecio = 0,
                ProductsShipping = model.Shipping,
                Amount = model.Amount,
                OtrosCostos = model.CostService,
                express = model.Express,
                Nota = model.Nota,
                authorizationCard = model.AuthorizationCard,
                IdAuthorizationCard = model.AuthorizationCard.Id,
                Number = model.OrderNumber != null ? model.OrderNumber : $"TI{DateTime.Now.ToString("yMMddHHmmssff")}",
                NoOrden = model.OrderNumber2,
                cantidad = 1,
                CantLb = 1,
                ValorAduanal = 0,
                CustomsTax = model.CustomTaxes
            };
            if (!_context.ClientContact.Where(cc => cc.ClientId == order.ClientId && cc.ContactId == order.ContactId).Any())
            {
                ClientContact c_c = new ClientContact();
                c_c.CCId = Guid.NewGuid();
                c_c.ClientId = order.Client.ClientId;
                c_c.ContactId = order.Contact.ContactId;
                _context.Add(c_c);
            }

            var j = 0;
            foreach (var pay in model.Pays)
            {
                var tipoPago = _context.TipoPago.Find(pay.TipoPago);
                order.Pagos.Add(new RegistroPago
                {
                    RegistroPagoId = Guid.NewGuid(),
                    Agency = order.Agency,
                    date = DateTime.UtcNow,
                    number = "PAY" + order.Date.ToString("yyyyMMddHHmmss") + j++,
                    Office = order.Office,
                    OrderId = order.OrderId,
                    tipoPago = tipoPago,
                    UserId = model.UserId,
                    valorPagado = pay.ValorPagado,
                    Client = order.Client,
                    nota = "",
                });
            }

            if (model.Credito > 0)
            {
                order.credito = model.Credito;
                order.Pagos.Add(new RegistroPago
                {
                    AgencyId = order.Agency.AgencyId,
                    date = DateTime.UtcNow,
                    number = "PAY" + order.Date.ToString("yyyyMMddHHmmss") + j++,
                    OfficeId = order.Agency.AgencyId,
                    OrderId = order.OrderId,
                    tipoPagoId = _context.TipoPago.FirstOrDefault(x => x.Type == "Crédito de Consumo").TipoPagoId,
                    UserId = order.User.UserId,
                    valorPagado = model.Credito,
                    RegistroPagoId = Guid.NewGuid(),
                    ClientId = order.Client.ClientId,
                    nota = "",
                });

                var creditosClient = _context.Credito.Where(x => x.Client == order.Client);
                foreach (var item in creditosClient)
                {
                    if (item.value > model.Credito)
                    {
                        item.value -= model.Credito;
                        _context.Credito.Update(item);
                        break;
                    }
                    else
                    {
                        model.Credito -= item.value;
                        _context.Credito.Remove(item);
                    }
                }
            }

            order.PriceLb = (decimal)extractProduct.Sum(x => x.Product.PrecioVentaReferencial * x.Qty);
            order.wholesaler = pBodega.GroupBy(x => x.Proveedor).Select(x => x.Key).FirstOrDefault();
            order.ValorPagado = order.Pagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
            order.Balance = order.Amount - order.ValorPagado - order.credito;
            if (order.Balance != 0)
            {
                order.Status = Order.STATUS_PENDIENTE;
            }
            else
            {
                order.Status = Order.STATUS_INICIADA;
            }

            order.RegistroEstados.Add(new RegistroEstado
            {
                Estado = order.Status,
                Date = DateTime.Now,
                User = order.User
            });

            //Agregar los productos al tramite
            //Para los productos
            Package package = new Package();
            package.PackageId = Guid.NewGuid();
            package.PackageNavigation = order;
            order.Package = package;
            _context.Add(package);

            var bag = new Bag();
            bag.BagId = Guid.NewGuid();
            bag.Code = "BL" + DateTime.Now.ToString("yMMddHHmmssff");
            bag.OrderId = order.OrderId;
            _context.Add(bag);
            foreach (var item in extractProduct)
            {
                var cantidad = item.Qty;

                //Proceso de crear un producto
                PackageItem packageItem = new PackageItem();
                packageItem.PackageItemId = Guid.NewGuid();
                packageItem.PackageId = package.PackageId;
                packageItem.Package = package;
                packageItem.Qty = cantidad;

                Product prod = new Product();
                prod.ProductId = Guid.NewGuid();
                prod.Agency = _context.Agency.FirstOrDefault();
                prod.AgencyId = prod.Agency.AgencyId;
                prod.Code = _context.Product.Count().ToString();
                prod.Tipo = item.Product.Nombre;
                prod.Color = "";
                prod.TallaMarca = "";
                prod.Description = item.Product.Descripcion;
                prod.ProductoBodega = item.Product;
                prod.Wholesaler = item.Product.Proveedor;

                _context.Add(prod);
                packageItem.Product = prod;
                packageItem.ProductId = prod.ProductId;
                packageItem.Description = item.Product.Descripcion;

                package.PackageItem.Add(packageItem);

                _context.Add(packageItem);

                //Bolsa
                var bagItem = new BagItem();
                bagItem.BagItemId = Guid.NewGuid();
                bagItem.BagId = bag.BagId;

                bagItem.ProductId = prod.ProductId;
                bagItem.Qty = (int)cantidad;
                _context.BagItem.Add(bagItem);
            }

            // Mayorista y Servicios por cobrar y pagar
            if (order.wholesaler != null)
            {
                var transferencia = _context.CostoxModuloMayorista.Include(x => x.valoresTramites).Include(x => x.modAsignados).Where(x => x.AgencyId == agency.AgencyId && x.modAsignados.Where(y => y.IdWholesaler == order.wholesaler.IdWholesaler).Any()).FirstOrDefault();
                if (transferencia != null)
                {
                    //Servicio por cobrar
                    order.agencyTransferida = _context.Agency.Find(transferencia.AgencyMayoristaId);
                    decimal valortramite = order.Amount;

                    if (order.TipoPago.Type == "Crédito o Débito")
                    {
                        //Se le quita el porciento fee
                        //valortramite = order.CantLb * order.PriceLb + order.ValorAduanal + order.OtrosCostos;
                        valortramite = order.CantLb * (order.PriceLb * order.cantidad) + order.CustomsTax;
                    }
                    if (order.agencyTransferida.Name == "Rey Envios")
                    {
                        decimal importeACobrar = order.PriceLb;
                        order.costoDeProveedor = Math.Round((decimal)0.95 * (order.PriceLb - 10), 2);
                        servicioxCobrar tramitexPagar = new servicioxCobrar();
                        tramitexPagar.servicioxCobrarId = Guid.NewGuid();
                        tramitexPagar.date = DateTime.UtcNow;
                        tramitexPagar.ServicioId = order.OrderId;
                        tramitexPagar.tramite = "Tienda";
                        tramitexPagar.NoServicio = order.Number;
                        tramitexPagar.mayorista = order.agencyTransferida;
                        tramitexPagar.minorista = order.Agency;
                        tramitexPagar.No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss");
                        tramitexPagar.cobrado = 0;
                        tramitexPagar.remitente = order.Client;
                        tramitexPagar.destinatario = order.Contact;
                        tramitexPagar.valorTramite = valortramite;
                        tramitexPagar.importeACobrar = importeACobrar + order.OtrosCostos;
                        _context.servicioxCobrar.Add(tramitexPagar);

                        //Tramite por pagar
                        var porPagar = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = importeACobrar,
                            Mayorista = order.wholesaler,
                            Agency = agency,
                            SId = order.OrderId,
                            Order = order,
                            NoServicio = order.Number,
                            Tipo = STipo.Tienda,
                            SubTipo = "-",
                            Express = order.express
                        };
                        porPagar.ServiciosxPagarId = Guid.NewGuid();
                        _context.ServiciosxPagar.Add(porPagar);
                        //Guardo el valor del costo en el tramite
                        order.costoMayorista = importeACobrar + order.OtrosCostos;

                        var wholesalerReyEnvios = await _context.Wholesalers.FirstOrDefaultAsync(x => x.EsVisible && x.AgencyId == Guid.Parse("2F7B03FB-4BE1-474D-8C95-3EE8C6EAEAC1") && x.Category.category == "Tienda");
                        if (wholesalerReyEnvios != null)
                        {
                            //Servicio por pagar del proveedor
                            porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = order.costoDeProveedor,
                                Mayorista = wholesalerReyEnvios,
                                Agency = order.agencyTransferida,
                                SId = order.OrderId,
                                Order = order,
                                NoServicio = order.Number,
                                Tipo = STipo.Tienda,
                                SubTipo = "-",
                                Express = order.express
                            };
                            _context.ServiciosxPagar.Add(porPagar);
                        }

                    }
                }
                else
                {
                    decimal importeACobrar = 0;
                    if (order.Agency.Name == "Rey Envios")
                    {
                        importeACobrar = Math.Round((decimal)0.95 * (order.PriceLb - 10), 2);
                    }
                    else
                    {
                        var costByProvince = order.wholesaler.CostByProvinces.FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));
                        importeACobrar = costByProvince != null? order.CantLb * (decimal)costByProvince?.Cost + order.CustomsTax: 0;
                    }

                    if (importeACobrar > 0)
                    {
                        //Tramite por pagar
                        var porPagar = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = importeACobrar,
                            Mayorista = order.wholesaler,
                            Agency = agency,
                            SId = order.OrderId,
                            Order = order,
                            NoServicio = order.Number,
                            Tipo = STipo.Tienda,
                            SubTipo = "-",
                            Express = order.express,
                        };
                        porPagar.ServiciosxPagarId = Guid.NewGuid();
                        //Guardo el valor del costo en el tramite
                        _context.ServiciosxPagar.Add(porPagar);
                    }
                    order.costoMayorista = importeACobrar;
                }
            }

            //Para AgenciApp
            if (order.agencyTransferida != null) //Si el tramite es transferido el costo por agencia pasa al mayorista
            {
                var s = new ServiciosxPagar
                {
                    ServiciosxPagarId = Guid.NewGuid(),
                    Date = DateTime.Now,
                    ImporteAPagar = order.OtrosCostos,
                    Mayorista = null,
                    Agency = order.agencyTransferida,
                    SId = order.OrderId,
                    Order = order,
                    NoServicio = order.Number,
                    Tipo = STipo.Paquete,
                    SubTipo = order.Type
                };
                _context.ServiciosxPagar.Add(s);
            }
            else
            {
                var s = new ServiciosxPagar
                {
                    ServiciosxPagarId = Guid.NewGuid(),
                    Date = DateTime.Now,
                    ImporteAPagar = order.OtrosCostos,
                    Mayorista = null,
                    Agency = order.Agency,
                    SId = order.OrderId,
                    Order = order,
                    NoServicio = order.Number,
                    Tipo = STipo.Paquete,
                    SubTipo = order.Type
                };
                _context.ServiciosxPagar.Add(s);
            }

            foreach (var wholesaler in extractProduct.GroupBy(x => x.Product.Proveedor))
            {
                decimal shippingAux = (decimal)wholesaler.Select(x => x.Product).Distinct().Sum(x => x.EnableShipping ? x.Shipping : 0);
                if (order.Agency.AgencyId == agencyReyEnvios)
                {
                    var firstProduct = wholesaler.Select(x => x.Product).FirstOrDefault(x => x.EnableShipping);
                    shippingAux = firstProduct != null ? firstProduct.Shipping : 0;
                }

                if (shippingAux > 0)
                {
                    var porPagar = new ServiciosxPagar
                    {
                        Date = DateTime.Now,
                        ImporteAPagar = shippingAux,
                        Order = order,
                        Mayorista = wholesaler.Key,
                        Agency = agency,
                        SId = order.OrderId,
                        NoServicio = order.Number,
                        Tipo = order.Type == "Tienda" ? STipo.Tienda : STipo.Paquete,
                        SubTipo = "-",
                        IsPaymentProductShipping = true
                    };
                    _context.ServiciosxPagar.Add(porPagar);
                }
            }

            //Guardo que empleado realizo el tramite
            TramiteEmpleado tramite = new TramiteEmpleado();
            tramite.fecha = DateTime.UtcNow;
            tramite.Id = Guid.NewGuid();
            tramite.IdEmpleado = order.User.UserId;
            tramite.IdTramite = order.OrderId;
            tramite.tipoTramite = TramiteEmpleado.tipo_ENVIO;
            tramite.IdAgency = order.Agency.AgencyId;
            _context.TramiteEmpleado.Add(tramite);

            if (order.TipoPago == null)
            {
                order.TipoPago = _context.TipoPago.FirstOrDefault(x => x.Type == "Cash");
            }

            _context.Logs.Add(new Log
            {
                Date = DateTime.Now,
                Event = LogEvent.Crear,
                Type = order.Type == "Combo" ? LogType.Combo : LogType.Orden,
                LogId = Guid.NewGuid(),
                Message = order.Number,
                User = order.User,
                Client = order.Client,
                Precio = order.Amount.ToString(),
                Pagado = order.Pagos.Sum(x => x.valorPagado).ToString(),
                AgencyId = order.AgencyId,
                Order = order
            });

            _context.Add(order);
            await _context.SaveChangesAsync();
            return Result.Success(order);

        }
    }
}