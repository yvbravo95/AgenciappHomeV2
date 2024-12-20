using System.Reflection.Metadata;
using Agenciapp.Common.Class;
using Agenciapp.Common.Services;
using Agenciapp.Domain.Enums;
using Agenciapp.Service.IBodegaServices;
using Agenciapp.Service.IBodegaServices.Models;
using Agenciapp.Service.IClientServices;
using Agenciapp.Service.IComboServices.Models;
using Agenciapp.Service.IContactServices;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using Microsoft.Extensions.Configuration;
using AgenciappHome.Models.ApiModel;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;
using SendGrid.Helpers.Mail;
using Agenciapp.Common.Services.INotificationServices;
using Agenciapp.Common.Constants;

namespace Agenciapp.Service.IComboServices
{
    public interface IComboService
    {
        Task<Result<Order>> Create(CreateComboModel model);
        Task<Result> ImportExcelCombos(ISheet sheet, User user);
        Task<Result> CreateOrdersInvoice(Guid invoiceId);
        Task<Result> Despachar(List<Guid> wholesalersId, List<Guid> ordersId, string emails, User aUser, DateTime? dateIni, DateTime? dateFin);
    }

    public class ComboService : IComboService
    {
        private readonly databaseContext _context;
        private readonly IBodegaService _bodegaService;
        private readonly IClientService _clientService;
        private readonly IContactService _contactService;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        public ComboService(databaseContext context, INotificationService notificationService, IBodegaService bodegaService, IClientService clientService, IContactService contactService, IConfiguration configuration)
        {
            _context = context;
            _bodegaService = bodegaService;
            _clientService = clientService;
            _contactService = contactService;
            _configuration = configuration;
            _notificationService = notificationService;
        }

        public async Task<Result<Order>> Create(CreateComboModel model)
        {
            var user = await _context.User.FindAsync(model.UserId);
            if (user == null) return Result.Failure<Order>("El usuario no existe");

            var agency = await _context.Agency.FindAsync(model.AgencyId);
            if (agency == null) return Result.Failure<Order>("La agencia no existe");

            Office office;
            if (model.OfficeId != null)
            {
                office = await _context.Office.FindAsync(model.OfficeId);
                if (office == null) return Result.Failure<Order>("La oficina no existe");
            }
            else
            {
                office = await _context.Office.FirstOrDefaultAsync(x => x.AgencyId == model.AgencyId);
                if (office == null) return Result.Failure<Order>("La oficina no existe");
            }


            var client = await _context.Client.FindAsync(model.ClientId);
            if (client == null) return Result.Failure<Order>("El cliente no existe");

            var contact = await _context.Contact.Include(x => x.Address).FirstOrDefaultAsync(x => x.ContactId == model.ContactId);
            if (contact == null) return Result.Failure<Order>("El contacto no existe");

            // Extraer de la bodega los productos
            var pBodega = await _context.ProductosBodegas
                    .Include(x => x.Precio1Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio1Minorista).ThenInclude(x => x.PriceByProvince).ThenInclude(x => x.Province)
                    .Include(x => x.Precio2Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio2Minorista).ThenInclude(x => x.PriceByProvince).ThenInclude(x => x.Province)
                    .Include(x => x.Precio3Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio3Minorista).ThenInclude(x => x.PriceByProvince).ThenInclude(x => x.Province)
                    .Include(x => x.Proveedor)
                    .Include(x => x.Categoria)
                    .Include(x => x.productoBodegaCatalogItems).ThenInclude(x => x.CatalogItem)
                    //.Where(x => x.IdAgency == user.AgencyId)
                    .Where(x => model.Products.Any(y => y.Id == x.IdProducto))
                    .ToListAsync();

            if (pBodega.Any(x => x.Categoria.Nombre != "Combos"))
            {
                return Result.Failure<Order>("Los productos deben pertenecer a la categoría Combos");
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
            var extraer = _bodegaService.Extract(extractProduct);
            if (extraer.IsFailure) return Result.Failure<Order>(extraer.Error);

            _context.AuthorizationCards.Add(model.AuthorizationCard);
            //Tramite
            Order order = new Order
            {
                Editable = model.Editable,
                StoreType = model.StoreType,
                OrderId = Guid.NewGuid(),
                Client = client,
                User = user,
                Agency = agency,
                Office = office,
                Contact = contact,
                Date = DateTime.Now,
                Type = OrderType.Combo.GetDescription(),
                addCosto = model.AddCosto,
                addPrecio = model.AddPrecio,
                Amount = model.Amount,
                OtrosCostos = model.CostService,
                express = model.Express,
                ProductsShipping = model.Shipping,
                Nota = model.Nota,
                authorizationCard = model.AuthorizationCard,
                IdAuthorizationCard = model.AuthorizationCard.Id,
                Number = model.OrderNumber != null ? model.OrderNumber : $"CO{DateTime.Now.ToString("yMMddHHmmssff")}",
                NoOrden = model.OrderNumber2,
                cantidad = 1,
                CantLb = 1,
                ValorAduanal = 0,
                PriceLb = model.ProductsPrice
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

            var firstProduct = pBodega.FirstOrDefault();
            Wholesaler w = null;
            if (firstProduct.IdAgency != order.Agency.AgencyId)
            {
                var cxm = await _context.CostoxModuloMayorista.Include(x => x.modAsignados).FirstOrDefaultAsync(x => x.AgencyId == agency.AgencyId && x.AgencyMayoristaId == firstProduct.IdAgency);
                if(cxm == null) throw new Exception("No se ha definido un mayorista para el trámite");
                var idWholesaler = cxm.modAsignados.FirstOrDefault(x => x.Tramite == CategoryType.Combos).IdWholesaler;
                w = await _context.Wholesalers.FindAsync(idWholesaler);
            }
            else
            {
                w = firstProduct.Proveedor;
            }

            if (order.PriceLb == 0)
                order.PriceLb = (decimal)extractProduct.Sum(x => x.Product.PrecioVentaReferencial * x.Qty);

            order.wholesaler = w;
            order.ValorPagado = order.Pagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
            order.Balance = order.Amount - order.ValorPagado - order.credito;
            order.Status = Order.STATUS_INICIADA;

            if (order.Balance != 0)
            {
                servicioxCobrar sxc = new servicioxCobrar
                {
                    date = DateTime.UtcNow,
                    ServicioId = order.OrderId,
                    tramite = "Combos",
                    NoServicio = order.Number,
                    cliente = order.Client,
                    No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss"),
                    cobrado = 0,
                    remitente = order.Client,
                    destinatario = order.Contact,
                    valorTramite = order.Balance,
                    importeACobrar = order.Balance,
                    mayorista = order.Agency,
                    Order = order
                };
                _context.servicioxCobrar.Add(sxc);
            }
            else
                order.Status = Order.STATUS_INICIADA;

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
            if (order.wholesaler == null) return Result.Failure<Order>("El trámite debe tener un mayorista");

            string combos = "";

            //verifico si existe un mayorista por transferencia para combos
            bool bytransferencia = false;
            if (order.wholesaler != null)
            {
                var transferencia = _context.CostoxModuloMayorista.Include(x => x.modAsignados).Where(x => x.AgencyId == order.Agency.AgencyId && x.modAsignados.Where(y => y.IdWholesaler == order.wholesaler.IdWholesaler).Any()).FirstOrDefault();
                if (transferencia != null)
                {
                    bytransferencia = true;
                    order.agencyTransferida = _context.Agency.Find(transferencia.AgencyMayoristaId);
                }
            }

            Wholesaler providerProduct = extractProduct.First().Product.Proveedor;
            foreach (var producto in extractProduct)
            {
                if (producto.Product.Proveedor != null)
                {
                    combos += producto.Product.Nombre + ", ";
                    if (bytransferencia)
                    {
                        // Si es por transferencia se toma el precioreferencialminorista
                        //Busco el precio a minorista que definio la agencia mayorista 
                        decimal precioreferencialminorista = 0;
                        decimal priceprovince = 0;
                        if (producto.Product.Precio1Minorista != null)
                        {
                            if (producto.Product.Precio1Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == order.Agency.AgencyId))
                            {
                                precioreferencialminorista = producto.Product.Precio1Minorista.precio;
                                var province = producto.Product.Precio1Minorista.PriceByProvince.FirstOrDefault(x => x.Province.nombreProvincia == order.Contact.Address?.City);
                                if (province != null)
                                    priceprovince = province.Price;
                            }
                        }
                        if (producto.Product.Precio2Minorista != null)
                        {
                            if (producto.Product.Precio2Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == order.Agency.AgencyId))
                            {
                                precioreferencialminorista = producto.Product.Precio2Minorista.precio;
                                var province = producto.Product.Precio2Minorista.PriceByProvince.FirstOrDefault(x => x.Province.nombreProvincia == order.Contact.Address?.City);
                                if (province != null)
                                    priceprovince = province.Price;
                            }
                        }
                        if (producto.Product.Precio3Minorista != null)
                        {
                            if (producto.Product.Precio3Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == order.Agency.AgencyId))
                            {
                                precioreferencialminorista = producto.Product.Precio3Minorista.precio;
                                var province = producto.Product.Precio3Minorista.PriceByProvince.FirstOrDefault(x => x.Province.nombreProvincia == order.Contact.Address?.City);
                                if (province != null)
                                    priceprovince = province.Price;
                            }
                        }
                        order.costoMayorista += ((precioreferencialminorista == 0 ? (decimal)producto.Product.PrecioVentaReferencial : precioreferencialminorista) + priceprovince) * producto.Qty;
                        order.costoDeProveedor += ((decimal)producto.Product.PrecioCompraReferencial + priceprovince) * producto.Qty;
                    }
                    else
                    {
                        order.costoMayorista += (decimal)producto.Product.PrecioCompraReferencial * producto.Qty;
                    }
                }
            }
            //Valor incrementado al costo en caso de los combos
            order.costoMayorista += order.addCosto;

            if (bytransferencia)
            {
                //Creo el servicio por cobrar
                servicioxCobrar tramitexPagar = new servicioxCobrar();
                tramitexPagar.servicioxCobrarId = Guid.NewGuid();
                tramitexPagar.date = DateTime.UtcNow;
                tramitexPagar.ServicioId = order.OrderId;
                tramitexPagar.tramite = "Combos";
                tramitexPagar.NoServicio = order.Number;
                tramitexPagar.mayorista = order.agencyTransferida;
                tramitexPagar.minorista = order.Agency;
                tramitexPagar.No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss");
                tramitexPagar.cobrado = 0;
                tramitexPagar.remitente = order.Client;
                tramitexPagar.destinatario = order.Contact;
                tramitexPagar.valorTramite = order.PriceLb; //Este es el precio de los combos
                tramitexPagar.importeACobrar = order.costoMayorista + order.OtrosCostos;
                tramitexPagar.Order = order;
                _context.servicioxCobrar.Add(tramitexPagar);

                //Servicio por pagar del mayorista a su proveedor
                if (providerProduct != null && order.agencyTransferida != null)
                {
                    var porPagarMayorista = new ServiciosxPagar
                    {
                        ServiciosxPagarId = Guid.NewGuid(),
                        Date = DateTime.Now,
                        ImporteAPagar = order.costoDeProveedor,
                        Mayorista = providerProduct,
                        Agency = order.agencyTransferida,
                        SId = order.OrderId,
                        Order = order,
                        NoServicio = order.Number,
                        Tipo = STipo.Paquete,
                        SubTipo = combos,
                        Express = order.express
                    };
                    _context.ServiciosxPagar.Add(porPagarMayorista);
                }

                if (order.ProductsShipping > 0)
                {
                    _context.ServiciosxPagar.Add(new ServiciosxPagar
                    {
                        Date = DateTime.Now,
                        ImporteAPagar = order.ProductsShipping,
                        Order = order,
                        Mayorista = providerProduct,
                        Agency = order.agencyTransferida,
                        SId = order.OrderId,
                        NoServicio = order.Number,
                        Tipo = STipo.Paquete,
                        SubTipo = "-",
                        IsPaymentProductShipping = true
                    });
                }
            }
            else
            {
                if (order.ProductsShipping > 0)
                {
                    _context.ServiciosxPagar.Add(new ServiciosxPagar
                    {
                        Date = DateTime.Now,
                        ImporteAPagar = order.ProductsShipping,
                        Order = order,
                        Mayorista = order.wholesaler,
                        Agency = agency,
                        SId = order.OrderId,
                        NoServicio = order.Number,
                        Tipo = STipo.Paquete,
                        SubTipo = "-",
                        IsPaymentProductShipping = true
                    });
                }
            }

            //Creo el servicio por pagar
            var porPagar = new ServiciosxPagar
            {
                ServiciosxPagarId = Guid.NewGuid(),
                Date = DateTime.Now,
                ImporteAPagar = order.costoMayorista + (bytransferencia ? order.OtrosCostos : 0),
                Mayorista = order.wholesaler,
                Agency = order.Agency,
                SId = order.OrderId,
                Order = order,
                NoServicio = order.Number,
                Tipo = STipo.Paquete,
                SubTipo = combos,
                Express = order.express
            };
            _context.ServiciosxPagar.Add(porPagar);

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

        public async Task<Result> ImportExcelCombos(ISheet sheet, User user)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    IRow headerRow = sheet.GetRow(0); //Get Header Row
                    int cellCount = headerRow.LastCellNum;
                    //Verificar columnas y establecer indices
                    if (cellCount != 13) return Result.Failure("El excel no posee la cantidad de columnas necesarias.");
                    int indexAgencia = -1;
                    int indexFecha = -1;
                    int indexProducto = -1;
                    int indexCantidad = -1;
                    int indexRemitente = -1;
                    int indexNotaEnvio = -1;
                    int indexDestinatario = -1;
                    int indexCI = -1;
                    int indexTelefono = -1;
                    int indexDireccion = -1;
                    int indexMunicipio = -1;
                    int indexProvincia = -1;
                    int indexCodigoProducto = -1;

                    for (int i = 0; i < cellCount; i++)
                    {
                        var value = headerRow.GetCell(i).ToString();
                        switch (value)
                        {
                            case "AGENCIA":
                                indexAgencia = i;
                                break;
                            case "FECHA":
                                indexFecha = i;
                                break;
                            case "PRODUCTO":
                                indexProducto = i;
                                break;
                            case "CODIGO":
                                indexCodigoProducto = i;
                                break;
                            case "CANTIDAD":
                                indexCantidad = i;
                                break;
                            case "REMITENTE":
                                indexRemitente = i;
                                break;
                            case "NOTAS DEL ENVIO":
                                indexNotaEnvio = i;
                                break;
                            case "DESTINATARIO":
                                indexDestinatario = i;
                                break;
                            case "CARNET DE IDENTIDAD":
                                indexCI = i;
                                break;
                            case "TELEFONOS":
                                indexTelefono = i;
                                break;
                            case "DIRECCION":
                                indexDireccion = i;
                                break;
                            case "MUNICIPIO":
                                indexMunicipio = i;
                                break;
                            case "PROVINCIA":
                                indexProvincia = i;
                                break;
                            default:
                                return Result.Failure($"La columna {value} no es válida");
                        }
                    }
                    List<ExcelComboModel> listCombos = new List<ExcelComboModel>();
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;
                        if (row.LastCellNum != 13) return Result.Failure("No se ha podido importar, revise que los datos sean correctos");
                        listCombos.Add(new ExcelComboModel
                        {
                            Agencia = row.GetCell(indexAgencia).ToString(),
                            Cantidad = int.Parse(row.GetCell(indexCantidad).ToString()),
                            CI = row.GetCell(indexCI).ToString(),
                            Destinatario = row.GetCell(indexDestinatario).ToString(),
                            Direccion = row.GetCell(indexDireccion).ToString(),
                            Fecha = DateTime.Parse(row.GetCell(indexFecha).ToString()),
                            Municipio = row.GetCell(indexMunicipio).ToString(),
                            NotasEnvio = row.GetCell(indexNotaEnvio).ToString(),
                            Producto = row.GetCell(indexProducto).ToString(),
                            Codigo = row.GetCell(indexCodigoProducto).ToString(),
                            Provincia = row.GetCell(indexProvincia).ToString(),
                            Remitente = row.GetCell(indexRemitente).ToString(),
                            Telefono = row.GetCell(indexTelefono).ToString()
                        });
                    }
                    int count = 1;
                    foreach (var item in listCombos)
                    {
                        var response = await CreateTramiteItemExel(item, user, $"CO{DateTime.Now.ToString("yMMddHHmmssff")}{count++}");
                        if (response.IsFailure) return Result.Failure(response.Error);
                    }
                    transaction.Commit();

                    return Result.Success($"El excel ha sido importado");
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Serilog.Log.Fatal(e, "Server Error");
                    return Result.Failure("No se ha podido importar el excel");
                }
            }
        }

        private async Task<Result> CreateTramiteItemExel(ExcelComboModel model, User user, string OrderNumber)
        {
            try
            {
                //Validar remitente y destinantario
                string[] remitente = model.Remitente.Trim().Split(" ");
                string[] destinatario = model.Destinatario.Trim().Split(" ");
                var clientCreateModel = new IClientServices.Models.CreateClientModel
                {
                    Address = new Common.Models.CreateAddressModel
                    {
                        AddressLine1 = "",
                        AddressLine2 = "",
                        City = "",
                        Country = "",
                        Countryiso2 = "",
                        State = "",
                    },
                    BirthDate = DateTime.MinValue,
                    Conflictivo = false,
                    Email = "",
                    EnableNotifications = true,
                    ID = "",
                    IsCarrier = false,
                    LastName = remitente.Length == 4 ? remitente[2] : remitente.Length == 3 ? remitente[1] : remitente.Length == 2 ? remitente[1] : "",
                    LastName2 = remitente.Length == 4 ? remitente[3] : remitente.Length == 3 ? remitente[2] : "",
                    Name = remitente.Length > 1 ? remitente[0] : "",
                    Name2 = remitente.Length == 4 ? remitente[1] : "",
                    PhoneCubaNumber = "",
                    PhoneNumber = "0000000000"
                };
                var client = await _clientService.Exist(clientCreateModel, user.AgencyId);
                if (client == null)
                {
                    var responseClient = await _clientService.Create(clientCreateModel, user);
                    if (responseClient.IsFailure) return Result.Failure(responseClient.Error);
                    else client = responseClient.Value;
                }

                var contactCreateModel = new IContactServices.Models.CreateContactModel
                {
                    Address = new Common.Models.CreateAddressContactModel
                    {
                        AddressLine1 = model.Direccion,
                        AddressLine2 = "",
                        Municipality = model.Municipio,
                        Province = model.Provincia,
                        Reparto = ""
                    },
                    CI = model.CI,
                    PhoneNumberMovil = model.Telefono,
                    PhoneNumberCasa = "",
                    Name = destinatario.Length == 4 ? $"{destinatario[0]} {destinatario[1]}" : destinatario.Length > 1 ? destinatario[0] : "",
                    LastName = destinatario.Length == 4 ? $"{destinatario[2]} {destinatario[3]}" : destinatario.Length == 3 ? $"{destinatario[1]} {destinatario[2]}" : destinatario.Length == 2 ? destinatario[1] : "",
                };
                var contact = await _contactService.Exist(contactCreateModel, user);
                if (contact == null)
                {
                    var responseContact = await _contactService.Create(contactCreateModel, user);
                    if (responseContact.IsFailure) return Result.Failure(responseContact.Error);
                    else contact = responseContact.Value;
                }


                //Verificar Minorista Combo
                var minoristaCombo = await _context.Minoristas.FirstOrDefaultAsync(x => x.Name == model.Agencia && x.Agency.AgencyId == user.AgencyId && x.Type == STipo.Combo);
                if (minoristaCombo == null) return Result.Failure($"No existe el minorista {model.Agencia}");

                var agency = await _context.Agency.FindAsync(user.AgencyId);
                if (agency == null) return Result.Failure<Order>("La agencia no existe");

                var office = await _context.Office.FirstOrDefaultAsync(x => x.AgencyId == agency.AgencyId);
                if (office == null) return Result.Failure<Order>("La oficina no existe");

                // Extraer de la bodega los productos
                //Verificar Producto
                var product = await _context.ProductosBodegas
                .Include(x => x.Precio1Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio2Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Precio3Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Proveedor)
                    .Include(x => x.Categoria)
                .FirstOrDefaultAsync(x => x.IdAgency == user.AgencyId && x.Codigo == model.Codigo && x.Categoria.Nombre == "Combos");

                if (product == null) return Result.Failure($"El producto {model.Producto} no existe");

                List<ExtractProductModel> extractProduct = new List<ExtractProductModel>{
                new ExtractProductModel{
                    Product = product,
                    Qty = model.Cantidad
                }
            };

                var extraer = _bodegaService.Extract(extractProduct);
                if (extraer.IsFailure) return Result.Failure<Order>(extraer.Error);

                var authCard = new AuthorizationCard
                {
                    Id = Guid.NewGuid()
                };
                _context.AuthorizationCards.Add(authCard);

                //Tramite
                Order order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    Client = client,
                    User = user,
                    Agency = agency,
                    Office = office,
                    Contact = contact,
                    Date = DateTime.Now,
                    Type = OrderType.Combo.GetDescription(),
                    addCosto = 0,
                    addPrecio = 0,
                    OtrosCostos = 1,
                    express = false,
                    Nota = model.NotasEnvio,
                    authorizationCard = authCard,
                    IdAuthorizationCard = authCard.Id,
                    Number = OrderNumber,
                    NoOrden = "",
                    cantidad = 1,
                    CantLb = 1,
                    ValorAduanal = 0,
                    Minorista = minoristaCombo,
                };
                if (!_context.ClientContact.Where(cc => cc.ClientId == order.ClientId && cc.ContactId == order.ContactId).Any())
                {
                    ClientContact c_c = new ClientContact();
                    c_c.CCId = Guid.NewGuid();
                    c_c.ClientId = order.Client.ClientId;
                    c_c.ContactId = order.Contact.ContactId;
                    _context.Add(c_c);
                }
                order.PriceLb = (decimal)extractProduct.Sum(x => x.Product.PrecioVentaReferencial * x.Qty);
                order.wholesaler = product.Proveedor;
                order.Amount = order.PriceLb + order.OtrosCostos;


                var tipoPago = await _context.TipoPago.FirstOrDefaultAsync(x => x.Type == "Cash");
                order.Pagos.Add(new RegistroPago
                {
                    RegistroPagoId = Guid.NewGuid(),
                    Agency = order.Agency,
                    date = DateTime.UtcNow,
                    number = "PAY" + order.Date.ToString("yyyyMMddHHmmss"),
                    Office = order.Office,
                    OrderId = order.OrderId,
                    tipoPago = tipoPago,
                    UserId = user.UserId,
                    valorPagado = order.Amount,
                    Client = order.Client,
                    nota = "",
                });

                order.ValorPagado = order.Pagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                order.Balance = 0;
                order.Status = Order.STATUS_INICIADA;

                if (order.Balance != 0)
                {
                    servicioxCobrar sxc = new servicioxCobrar
                    {
                        date = DateTime.UtcNow,
                        ServicioId = order.OrderId,
                        tramite = "Combos",
                        NoServicio = order.Number,
                        cliente = order.Client,
                        No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss"),
                        cobrado = 0,
                        remitente = order.Client,
                        destinatario = order.Contact,
                        valorTramite = order.Balance,
                        importeACobrar = order.Balance,
                        mayorista = order.Agency,
                        Order = order
                    };
                    _context.servicioxCobrar.Add(sxc);
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
                if (order.wholesaler == null) return Result.Failure<Order>("El trámite debe tener un mayorista");

                string combos = "";

                decimal pagoMayorista = 0; //Cuando es por tranferencia el mayorista debe pagarle a su proveedor
                foreach (var producto in extractProduct)
                {
                    if (producto.Product.Proveedor != null)
                    {
                        combos += producto.Product.Nombre + ", ";

                        // Si es por transferencia se toma el precioreferencialminorista
                        //Busco el precio a minorista que definio la agencia mayorista 
                        decimal precioreferencialminorista = 0;
                        if (producto.Product.Precio1Minorista != null)
                        {
                            precioreferencialminorista = producto.Product.Precio1Minorista.precio;
                        }
                        order.costoMayorista += (precioreferencialminorista == 0 ? (decimal)producto.Product.PrecioVentaReferencial : precioreferencialminorista) * producto.Qty;
                        order.costoDeProveedor += (decimal)producto.Product.PrecioCompraReferencial * producto.Qty;
                        pagoMayorista += (decimal)producto.Product.PrecioCompraReferencial * producto.Qty; //lo que paga el mayorista a su proveedor
                    }
                }
                //Valor incrementado al costo en caso de los combos
                order.costoMayorista += order.addCosto;

                //Creo el servicio por cobrar
                servicioxCobrar xCobrar = new servicioxCobrar();
                xCobrar.servicioxCobrarId = Guid.NewGuid();
                xCobrar.date = DateTime.UtcNow;
                xCobrar.ServicioId = order.OrderId;
                xCobrar.tramite = "Combos";
                xCobrar.NoServicio = order.Number;
                xCobrar.mayorista = order.Agency;
                xCobrar.minorista = null;
                xCobrar.MinoristaTramite = minoristaCombo;
                xCobrar.No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss");
                xCobrar.cobrado = 0;
                xCobrar.remitente = order.Client;
                xCobrar.destinatario = order.Contact;
                xCobrar.valorTramite = order.PriceLb; //Este es el precio de los combos
                xCobrar.importeACobrar = order.costoMayorista;
                xCobrar.Order = order;
                _context.servicioxCobrar.Add(xCobrar);

                //Servicio por pagar del mayorista a su proveedor
                if (order.wholesaler != null)
                {
                    var porPagarMayorista = new ServiciosxPagar
                    {
                        ServiciosxPagarId = Guid.NewGuid(),
                        Date = DateTime.Now,
                        ImporteAPagar = pagoMayorista,
                        Mayorista = order.wholesaler,
                        Agency = agency,
                        SId = order.OrderId,
                        Order = order,
                        NoServicio = order.Number,
                        Tipo = STipo.Paquete,
                        SubTipo = combos,
                        Express = order.express
                    };
                    _context.ServiciosxPagar.Add(porPagarMayorista);
                }

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

                return Result.Success();
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                Serilog.Log.Information("Error al importar la fila en excel de combos: \n" + JsonConvert.SerializeObject(model));
                return Result.Failure($"No se ha podido crear el trámite - {model.Producto}");
            }
        }

        public async Task<Result> CreateOrdersInvoice(Guid invoiceId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var employee = _context.User.FirstOrDefault(x => x.UserId == _configuration.GetValue<Guid>("EmployeeId"));
                    if (employee == null)
                        return Result.Failure("El empleado no existe");
                    var agency = await _context.Agency.FindAsync(employee.AgencyId);
                    var office = _context.Office.FirstOrDefault(x => x.AgencyId == agency.AgencyId);
                    var invoice = await _context.Invoices
                    .Include(x => x.InvoiceProductoBodega).ThenInclude(x => x.ProductoBodega).ThenInclude(x => x.Proveedor)
                    .Include(x => x.InvoiceProductoBodega).ThenInclude(x => x.ProductoBodega).ThenInclude(x => x.Precio1Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.InvoiceProductoBodega).ThenInclude(x => x.ProductoBodega).ThenInclude(x => x.Precio2Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.InvoiceProductoBodega).ThenInclude(x => x.ProductoBodega).ThenInclude(x => x.Precio3Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                    .Include(x => x.Discount)
                    .Include(x => x.UserClient).ThenInclude(x => x.Client)
                    .Include(x => x.Contact)
                    .Include(x => x.Orders)
                    .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId);
                    if (invoice == null)
                        return Result.Failure("El invoice no existe");
                    if (invoice.Orders.Any())
                    {
                        return Result.Failure("El trámite ya posee órdenes");
                    }
                    if (!invoice.InvoiceProductoBodega.Any())
                        return Result.Failure("El invoice no posee productos");

                    decimal totalCServicio = _configuration.GetValue<decimal>("CostoService");
                    decimal cServicio = totalCServicio / invoice.InvoiceProductoBodega.Count();
                    bool isFeeZelleApply = false;
                    //Verificar si se aplico fee en caso de ser pago zelle
                    if (invoice.PaymentType == PaymentType.Zelle)
                    {
                        var priceProducts = (decimal)invoice.InvoiceProductoBodega.Sum(x => x.ProductoBodega.PrecioVentaReferencial * x.Amount);
                        decimal feeTramite = Math.Round(priceProducts * (agency.creditCardFee_Combos / 100) + totalCServicio, 2);
                        var totalAmount = Math.Round(priceProducts + feeTramite, 2);

                        if (invoice.Discount != null)
                        {
                            decimal discount = invoice.Discount.Type == RateType.Porciento ? invoice.Discount.Value * (totalAmount / 100) : invoice.Discount.Value / invoice.InvoiceProductoBodega.Count;
                            totalAmount = Math.Round(totalAmount - discount, 2);
                        }
                        if (totalAmount == invoice.TotalPrice)
                        {
                            isFeeZelleApply = true;
                        }
                    }


                    foreach (var invoiceProduct in invoice.InvoiceProductoBodega)
                    {
                        decimal productPrice = (decimal)invoiceProduct.ProductoBodega.PrecioVentaReferencial;

                        decimal priceTramite = productPrice * invoiceProduct.Amount;
                        decimal feeTramite = Math.Round(priceTramite * (agency.creditCardFee_Combos / 100) + cServicio, 2);
                        if (!isFeeZelleApply && invoice.PaymentType == PaymentType.Zelle)
                        {
                            feeTramite = cServicio;
                        }
                        decimal amount = Math.Round(priceTramite + feeTramite, 2);

                        if (invoice.Discount != null)
                        {
                            decimal discount = invoice.Discount.Type == RateType.Porciento ? invoice.Discount.Value * (amount / 100) : invoice.Discount.Value / invoice.InvoiceProductoBodega.Count;
                            amount = Math.Round(amount - discount, 2);
                        }

                        var order = new Order
                        {
                            OrderId = Guid.NewGuid(),
                            AgencyId = agency.AgencyId,
                            Amount = amount,
                            Balance = 0,
                            ValorPagado = amount,
                            ClientId = invoice.UserClient.Client.ClientId,
                            ContactId = invoice.ContactId,
                            Date = DateTime.Now,
                            Type = "Combo",
                            Status = Order.STATUS_INICIADA,
                            OtrosCostos = cServicio,
                            FeeService = feeTramite,
                            Discount = invoice.Discount
                        };

                        var client = _context.Client.Include(x => x.Address).Include(x => x.Phone).FirstOrDefault(x => x.ClientId == order.ClientId);
                        //Creo el registro de pago

                        TipoPago tipopago;
                        if (invoice.PaymentType == AgenciappHome.Models.ApiModel.PaymentType.Card)
                        {
                            //verifico que exista el tipo de pago sino lo creo
                            tipopago = _context.TipoPago.FirstOrDefault(t => t.Type == "Crédito o Débito");
                            if (tipopago == null)
                            {
                                tipopago = new TipoPago
                                {
                                    TipoPagoId = Guid.NewGuid(),
                                    Type = "Crédito o Débito"
                                };
                                _context.TipoPago.Add(tipopago);
                            }
                        }
                        else
                        {
                            //verifico que exista el tipo de pago sino lo creo
                            tipopago = _context.TipoPago.FirstOrDefault(t => t.Type == "Zelle");
                            if (tipopago == null)
                            {
                                tipopago = new TipoPago
                                {
                                    TipoPagoId = Guid.NewGuid(),
                                    Type = "Zelle"
                                };
                                _context.TipoPago.Add(tipopago);
                            }

                        }

                        order.OfficeId = office.OfficeId;
                        order.TipoPago = tipopago;
                        RegistroPago pago = new RegistroPago
                        {
                            AgencyId = order.AgencyId,
                            date = DateTime.UtcNow,
                            number = "PAY" + order.Date.ToString("yyyyMMddHHmmss"),
                            OfficeId = order.OfficeId,
                            OrderId = order.OrderId,
                            tipoPagoId = tipopago.TipoPagoId,
                            UserId = employee.UserId,
                            valorPagado = order.ValorPagado,
                            RegistroPagoId = Guid.NewGuid(),
                            ClientId = order.ClientId,
                            nota = "",
                        };
                        _context.RegistroPagos.Add(pago);

                        order.Number = "CO" + DateTime.Now.ToString("yMMddHHmmssff");
                        order.UserId = employee.UserId;
                        order.cantidad = 1;
                        order.CantLb = 1;
                        order.PriceLb = priceTramite;
                        order.ValorAduanal = 0;
                        order.IsCreatedMovileApp = true;
                        order.wholesaler = invoiceProduct.ProductoBodega.Proveedor;
                        order.InvoiceId = invoice.InvoiceId;

                        var re = new RegistroEstado
                        {
                            Estado = order.Status,
                            Date = DateTime.Now,
                            User = employee
                        };

                        order.RegistroEstados = new List<RegistroEstado>();
                        order.RegistroEstados.Add(re);
                        order.Nota = "";

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

                        var cantidad = invoiceProduct.Amount;

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
                        prod.Tipo = invoiceProduct.ProductoBodega.Nombre;
                        prod.Color = "";
                        prod.TallaMarca = "";
                        prod.Description = invoiceProduct.ProductoBodega.Descripcion;
                        prod.ProductoBodega = invoiceProduct.ProductoBodega;
                        prod.Wholesaler = invoiceProduct.ProductoBodega.Proveedor;

                        _context.Add(prod);
                        packageItem.Product = prod;
                        packageItem.ProductId = prod.ProductId;
                        packageItem.Description = invoiceProduct.ProductoBodega.Descripcion;

                        package.PackageItem.Add(packageItem);

                        _context.Add(packageItem);

                        //Bolsa
                        var bagItem = new BagItem();
                        bagItem.BagItemId = Guid.NewGuid();
                        bagItem.BagId = bag.BagId;

                        bagItem.ProductId = prod.ProductId;
                        bagItem.Qty = (int)cantidad;
                        _context.BagItem.Add(bagItem);

                        _context.Add(order);

                        if (order.Type == "Combo" && order.wholesaler != null)
                        {
                            bool bytransferencia = false;
                            var transferencia = _context.CostoxModuloMayorista.Include(x => x.modAsignados).Where(x =>
                                    x.AgencyId == agency.AgencyId && x.modAsignados
                                        .Where(y => y.IdWholesaler == order.wholesaler.IdWholesaler).Any())
                                .FirstOrDefault();
                            if (transferencia != null)
                            {
                                bytransferencia = true;
                                order.agencyTransferida = _context.Agency.Find(transferencia.AgencyMayoristaId);
                            }

                            if (bytransferencia)
                            {
                                // Si es por transferencia se toma el precioreferencialminorista
                                //Busco el precio a minorista que definio la agencia mayorista 
                                decimal precioreferencialminorista = 0;
                                if (invoiceProduct.ProductoBodega.Precio1Minorista != null)
                                {
                                    if (invoiceProduct.ProductoBodega.Precio1Minorista.AgencyPrecioRefMinoristas.Any(x =>
                                        x.AgencyId == agency.AgencyId))
                                    {
                                        precioreferencialminorista = invoiceProduct.ProductoBodega.Precio1Minorista.precio;
                                    }
                                }

                                if (invoiceProduct.ProductoBodega.Precio2Minorista != null)
                                {
                                    if (invoiceProduct.ProductoBodega.Precio2Minorista.AgencyPrecioRefMinoristas.Any(x =>
                                        x.AgencyId == agency.AgencyId))
                                    {
                                        precioreferencialminorista = invoiceProduct.ProductoBodega.Precio2Minorista.precio;
                                    }
                                }

                                if (invoiceProduct.ProductoBodega.Precio3Minorista != null)
                                {
                                    if (invoiceProduct.ProductoBodega.Precio3Minorista.AgencyPrecioRefMinoristas.Any(x =>
                                        x.AgencyId == agency.AgencyId))
                                    {
                                        precioreferencialminorista = invoiceProduct.ProductoBodega.Precio3Minorista.precio;
                                    }
                                }

                                order.costoMayorista +=
                                    (precioreferencialminorista == 0
                                        ? (decimal)invoiceProduct.ProductoBodega.PrecioVentaReferencial
                                        : precioreferencialminorista) * invoiceProduct.Amount;
                                order.costoDeProveedor +=
                                    (decimal)invoiceProduct.ProductoBodega.PrecioCompraReferencial * invoiceProduct.Amount;
                                order.pagoMayorista +=
                                    (decimal)invoiceProduct.ProductoBodega.PrecioCompraReferencial *
                                    invoiceProduct.Amount; //lo que paga el mayorista a su proveedor

                                // Servicios por pagar y cobrar por trnsferencia
                                //Creo el servicio por cobrar
                                servicioxCobrar sxc = new servicioxCobrar();
                                sxc.servicioxCobrarId = Guid.NewGuid();
                                sxc.date = DateTime.UtcNow;
                                sxc.ServicioId = order.OrderId;
                                sxc.tramite = "Combos";
                                sxc.NoServicio = order.Number;
                                sxc.mayorista = order.agencyTransferida;
                                sxc.minorista = _context.Agency.Find(order.AgencyId);
                                sxc.No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss");
                                sxc.cobrado = 0;
                                sxc.remitente = client;
                                sxc.destinatario = invoice.Contact;
                                sxc.valorTramite = order.PriceLb; //Este es el precio de los combos
                                sxc.importeACobrar = order.costoMayorista + order.OtrosCostos;
                                sxc.Order = order;
                                _context.servicioxCobrar.Add(sxc);

                                //Servicio por pagar del mayorista a su proveedor
                                if (invoiceProduct.ProductoBodega.Proveedor != null && order.agencyTransferida != null)
                                {
                                    var porPagarMayorista = new ServiciosxPagar
                                    {
                                        Date = DateTime.Now,
                                        ImporteAPagar = order.pagoMayorista,
                                        Mayorista = invoiceProduct.ProductoBodega.Proveedor,
                                        Agency = order.agencyTransferida,
                                        SId = order.OrderId,
                                        Order = order,
                                        NoServicio = order.Number,
                                        Tipo = STipo.Paquete,
                                        SubTipo = invoiceProduct.ProductoBodega.Nombre,
                                        Express = order.express
                                    };
                                    _context.ServiciosxPagar.Add(porPagarMayorista);
                                }
                            }
                            else
                            {
                                order.costoMayorista += (decimal)invoiceProduct.ProductoBodega.PrecioCompraReferencial * invoiceProduct.Amount;
                            }

                            //Creo el servicio por pagar
                            var porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = order.costoMayorista + (decimal)(bytransferencia ? order.OtrosCostos : 0),
                                Mayorista = order.wholesaler,
                                Agency = agency,
                                SId = order.OrderId,
                                Order = order,
                                NoServicio = order.Number,
                                Tipo = STipo.Paquete,
                                SubTipo = invoiceProduct.ProductoBodega.Nombre,
                                Express = order.express
                            };
                            _context.ServiciosxPagar.Add(porPagar);
                        }
                    }

                    invoice.Status = Invoice.STATUS_PROCESADA;
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return Result.Success();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Serilog.Log.Fatal(e, "Server Error");
                    return Result.Failure("No se han podido crear los trámites");
                }
            }

        }

        public async Task<Result> Despachar(List<Guid> wholesalersId, List<Guid> ordersId, string emails, User aUser, DateTime? dateIni, DateTime? dateFin)
        {

            try
            {
                CultureInfo culture = new CultureInfo("es-US", true);

                if (wholesalersId.Count > 0)
                {
                    var aAgency = _context.Agency.FirstOrDefault(x => x.AgencyId == aUser.AgencyId);
                    var office = _context.Office.FirstOrDefault(x => x.Agency == aAgency);

                    var query = _context.Order
                        .Include(o => o.Client)
                        .Include(o => o.Contact)
                        .Include(o => o.agencyTransferida)
                        .Include(o => o.Bag)
                        .ThenInclude(b => b.BagItems)
                        .ThenInclude(x => x.Product)
                        .ThenInclude(x => x.ProductoBodega)
                        .Include(o => o.Bag)
                        .ThenInclude(b => b.BagItems)
                        .ThenInclude(x => x.Product)
                        .ThenInclude(x => x.Wholesaler)
                        .Where(x => x.Type == "Combo" && (x.Status == Order.STATUS_INICIADA))
                        .Where(x => (x.AgencyId == aAgency.AgencyId && x.agencyTransferida == null) || (x.agencyTransferida.AgencyId == aAgency.AgencyId) || (x.OfficeTransferida.OfficeId == office.OfficeId));

                    if (dateIni != null)
                    {
                        query = query.Where(x => x.Date.Date >= ((DateTime)dateIni).Date);
                    }

                    if (dateFin != null)
                    {
                        query = query.Where(x => x.Date.Date <= ((DateTime)dateFin).Date);
                    }

                    var orders = await query.OrderByDescending(x => x.Date).ToListAsync();

                    List<DespachoCombos> listadespachos = new List<DespachoCombos>();

                    int canttotalDespachos = 0;

                    foreach (var order in orders)
                    {
                        if (ordersId.Count > 0)
                        {
                            if (!ordersId.Contains(order.OrderId))
                            {
                                continue;
                            }
                        }

                        //Verifico en cada tramites los productos que contengan el mayorista y los despacho
                        AuxListOrder auxorder = new AuxListOrder(order, _context);
                        foreach (var item in auxorder.productos)
                        {
                            if (!item.producto.esDespachado && item.producto.Wholesaler != null)//Si no es despachado
                            {
                                if (wholesalersId.Contains(item.producto.Wholesaler.IdWholesaler)) //Si el producto contiene algun mayorista de la lista a despachar
                                {
                                    DespachoCombos dc = new DespachoCombos();
                                    dc.productocantidad = item;
                                    dc.mayorista = item.producto.Wholesaler;
                                    dc.order = order;
                                    listadespachos.Add(dc);
                                    Product prod = await _context.Product.FindAsync(item.producto.ProductId);
                                    prod.esDespachado = true;
                                    _context.Product.Update(prod);

                                    item.producto.esDespachado = true;
                                }
                            }
                        }


                        // Para el estado verifico si todos sus productos fueron despachados
                        int cantdespachados = auxorder.productos.Where(x => x.producto.esDespachado).Count();
                        if (cantdespachados == auxorder.productos.Count())
                        {
                            order.Status = Order.STATUS_DESPACHADA;
                            var re = new RegistroEstado
                            {
                                Date = DateTime.Now,
                                Estado = order.Status,
                                User = aUser
                            };
                            if (order.RegistroEstados == null)
                            {
                                order.RegistroEstados = new List<RegistroEstado>();
                            }
                            order.RegistroEstados.Add(re);
                            canttotalDespachos++;
                        }

                        order.fechadespacho = DateTime.UtcNow;
                        order.UpdatedDate = DateTime.Now;
                        _context.Order.Update(order);
                        //await _context.SaveChangesAsync();
                    }

                    if (listadespachos.Count == 0)
                    {
                        return Result.Failure("No existen trámites para despachar");
                    }

                    //Para cada mayorista se crea un pdf y se envia
                    string sWebRootFolder = _configuration["PathAgencia"];
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "UserFiles" + Path.DirectorySeparatorChar + "DespachoCombo";
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);
                    int count = 0;
                    foreach (var ordersmayorista in listadespachos.GroupBy(x => x.mayorista.IdWholesaler)) //Agrupo por mayorista
                    {
                        count++;
                        string numerodespacho = "DP" + DateTime.Now.ToString("yMMddHHmmss") + count;
                        foreach (var item in ordersmayorista)
                        {
                            item.order.numerodespacho = numerodespacho;
                        }
                        Wholesaler ww = await _context.Wholesalers.FindAsync(ordersmayorista.Key);

                        List<Attachment> attachs = new List<Attachment>();
                        if (ordersmayorista.Key == Guid.Parse("b43f5eda-1d4a-4186-9f35-2e306958b8df")) // Para DCuba cuando sea al mayorista PamyIsland poner un combo por pdf
                        {
                            int count2 = 0;
                            foreach (var combo in ordersmayorista)
                            {
                                count2++;
                                var aux = new List<DespachoCombos> { combo }.GroupBy(x => x.mayorista.IdWholesaler);

                                string auxAttach = await CreateReporteDespacho(aux.FirstOrDefault(), aUser);

                                //Guardo el pdf 
                                string filename = $"{numerodespacho}_{count2}.pdf";
                                string path = Path.Combine(filePath, filename);
                                using (FileStream fs = new FileStream(path, FileMode.Create))
                                {
                                    using (BinaryWriter bw = new BinaryWriter(fs))
                                    {
                                        byte[] data = Convert.FromBase64String(auxAttach);
                                        bw.Write(data);
                                        bw.Close();
                                    }
                                }

                                attachs.Add(new Attachment { Content = auxAttach, Filename = filename });

                            }
                        }
                        else
                        {
                            string attach = "";
                            if (aAgency.AgencyId == Guid.Parse("680b03d4-a92d-44f5-8b34-fd70e0d9847c") || aAgency.AgencyId == Guid.Parse("B7C2355B-8415-4AAD-B31E-41D0A3D0DD31"))
                            {
                                attach = await CreateReporteDespachoRapidM(ordersmayorista, aUser);
                            }
                            else
                            {
                                attach = await CreateReporteDespacho(ordersmayorista, aUser);
                            }

                            //Guardo el pdf 
                            string filename = $"{numerodespacho}.pdf";
                            string path = Path.Combine(filePath, filename);
                            using (FileStream fs = new FileStream(path, FileMode.Create))
                            {
                                using (BinaryWriter bw = new BinaryWriter(fs))
                                {
                                    byte[] data = Convert.FromBase64String(attach);
                                    bw.Write(data);
                                    bw.Close();
                                }
                            }

                            attachs.Add(new Attachment { Content = attach, Filename = filename });
                        }

                        string title = aAgency.Name + " - Combos";
                        string msg = "Número de despacho: " + numerodespacho;

                        var response = await _notificationService.sendEmail(
                            new EmailAddress
                            {
                                Email = "do_not_reply@agenciapp.com",
                                Name = aAgency.Name
                            },
                            new EmailAddress
                            {
                                Email = ww.email,
                                Name = ww.name
                            },
                            title,
                            msg,
                            attachs,
                            false
                        );
                        RegistroEnvioEmails registro = new RegistroEnvioEmails
                        {
                            RegistroEnvioEmailsId = Guid.NewGuid(),
                            AgencyId = aAgency.AgencyId,
                            AgencyName = aAgency.Name,
                            fecha = DateTime.Now,
                            destinatario = ww.email,
                            descripción = "Despacho de Combos",
                            status = response.IsSuccess ? response.Value : response.Error
                        };
                        _context.RegistroEnvioEmails.Add(registro);

                        //Envio email a los emails entrados por ;
                        if (emails != null)
                        {
                            var aux = emails.Split(';');
                            foreach (var item in aux)
                            {
                                response = await _notificationService.sendEmail(
                                    new EmailAddress
                                    {
                                        Email = "do_not_reply@agenciapp.com",
                                        Name = aAgency.Name
                                    },
                                    new EmailAddress
                                    {
                                        Email = item.Trim(),
                                    },
                                    title,
                                    msg,
                                    attachs,
                                    false
                                );
                                registro = new RegistroEnvioEmails
                                {
                                    RegistroEnvioEmailsId = Guid.NewGuid(),
                                    AgencyId = aAgency.AgencyId,
                                    AgencyName = aAgency.Name,
                                    fecha = DateTime.Now,
                                    destinatario = item.Trim(),
                                    descripción = "Despacho de Combos",
                                    status = response.IsSuccess ? response.Value : response.Error
                                };
                                _context.RegistroEnvioEmails.Add(registro);
                            }
                        }

                        //Envio el eamil a los usuarios con rol agencia
                        var users = _context.User.Where(x => x.AgencyId == aAgency.AgencyId && x.Type == "Agencia" && x.UserId != Guid.Parse("98FC5DAE-33E3-4EA4-81F0-1590217F7E77")).ToList();
                        if (aAgency.Name == "Rey Envios")
                        {
                            User orly = _context.User.Find(Guid.Parse("B318E8D8-D050-43EC-9E67-C08A04CB9F28"));
                            if (orly != null)
                            {
                                users.Add(orly);
                            }
                        }
                        foreach (var item in users)
                        {
                            response = await _notificationService.sendEmail(
                                    new EmailAddress
                                    {
                                        Email = "do_not_reply@agenciapp.com",
                                        Name = aAgency.Name
                                    },
                                    new EmailAddress
                                    {
                                        Email = item.Email,
                                    },
                                    title,
                                    msg,
                                    attachs,
                                    false
                                );

                            registro = new RegistroEnvioEmails
                            {
                                RegistroEnvioEmailsId = Guid.NewGuid(),
                                AgencyId = aAgency.AgencyId,
                                AgencyName = aAgency.Name,
                                fecha = DateTime.Now,
                                destinatario = item.Email,
                                descripción = "Despacho de Combos",
                                status = response.IsSuccess ? response.Value : response.Error
                            };
                            _context.RegistroEnvioEmails.Add(registro);
                        }

                        if (aAgency.Name == "Rapid Multiservice")
                        {
                            response = await _notificationService.sendEmail(
                                    new EmailAddress
                                    {
                                        Email = "do_not_reply@agenciapp.com",
                                        Name = aAgency.Name
                                    },
                                    new EmailAddress
                                    {
                                        Email = "analaurapainceirapino@gmail.com",
                                    },
                                    title,
                                    msg,
                                    attachs,
                                    false
                                );

                            registro = new RegistroEnvioEmails
                            {
                                RegistroEnvioEmailsId = Guid.NewGuid(),
                                AgencyId = aAgency.AgencyId,
                                AgencyName = aAgency.Name,
                                fecha = DateTime.Now,
                                destinatario = "analaurapainceirapino@gmail.com",
                                descripción = "Despacho de Combos",
                                status = response.IsSuccess ? response.Value : response.Error
                            };
                            _context.RegistroEnvioEmails.Add(registro);
                        }
                    }
                    await _context.SaveChangesAsync();
                    return Result.Success();
                }
                else
                {
                    return Result.Failure("No existen mayoristas para despachar");
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Result.Failure("No se ha podido despachar");
            }
        }


        public async Task<string> CreateReporteDespacho(IGrouping<Guid, DespachoCombos> ordersmayorista, User aUser)
        {
            using (MemoryStream MStream = new MemoryStream())
            {

                iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.LETTER);
                PdfWriter writer = PdfWriter.GetInstance(doc, MStream);

                doc.Open();

                iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font headFontExpress = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.RED);
                iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

                try
                {
                    // VER AGENCIA
                    var agency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId).FirstOrDefault();
                    Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();
                    var wholesaler = await _context.Wholesalers.FindAsync(ordersmayorista.Key);
                    var orders = ordersmayorista.GroupBy(x => x.order);
                    foreach (var despacho in orders)
                    {
                        var order = despacho.Key;
                        order.Client = await _context.Client.FindAsync(order.ClientId);
                        order.Contact = await _context.Contact.FindAsync(order.ContactId);
                        // Logo de la agencia
                        float[] columnWidths = { 5, 1 };
                        PdfPTable table = new PdfPTable(columnWidths);
                        table.WidthPercentage = 100;
                        PdfPCell cell1 = new PdfPCell();
                        cell1.BorderWidthLeft = 0;
                        cell1.BorderWidthBottom = 1;
                        cell1.BorderWidthTop = 1;
                        cell1.BorderWidthRight = 1;
                        PdfPCell cell2 = new PdfPCell();
                        cell2.Padding = 10;
                        cell2.BorderWidthLeft = 1;
                        cell2.BorderWidthBottom = 1;
                        cell2.BorderWidthTop = 1;
                        cell2.BorderWidthRight = 0;
                        string sWebRootFolder = _configuration["PathAgencia"];
                        if (agency.Name != "Rey Envios")
                        {
                            if (agency.logoName != null)
                            {
                                string namelogo = agency.logoName;
                                string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                                filePathQR = Path.Combine(filePathQR, namelogo);
                                iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                                imagelogo.ScaleAbsolute(75, 75);
                                cell1.AddElement(imagelogo);
                            }
                        }

                        if (agency.Name != "Rey Envios")
                        {
                            cell2.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                        }
                        if (order.express)
                        {
                            cell2.AddElement(new Phrase("EXPRESS", headFontExpress));
                        }
                        cell2.AddElement(new Phrase("Fecha: " + DateTime.Now.ToShortDateString(), headFont));
                        cell2.AddElement(new Phrase("Orden No: " + order.Number, headFont));
                        if (!string.IsNullOrEmpty(order.NoOrden))
                        {
                            cell2.AddElement(new Phrase("Tienda No: " + order.NoOrden, headFont));
                        }
                        cell2.AddElement(new Phrase("No. Despacho: " + order.numerodespacho, headFont));
                        Phrase aux;
                        cell2.AddElement(new Phrase("Remitente: ", underLineFont)); // Dirección de la empresa
                        aux = new Phrase("Nombre: ", headFont);
                        aux.AddSpecial(new Phrase(order.Client.Name + " " + order.Client.LastName, normalFont));
                        cell2.AddElement(aux);

                        cell2.AddElement(Chunk.NEWLINE);
                        table.AddCell(cell2);
                        table.AddCell(cell1);

                        cell1 = new PdfPCell();
                        cell1.Padding = 10;
                        cell1.BorderWidthLeft = 1;
                        cell1.BorderWidthBottom = 1;
                        cell1.BorderWidthTop = 1;
                        cell1.BorderWidthRight = 0;
                        cell2 = new PdfPCell();
                        cell2.BorderWidthLeft = 0;
                        cell2.BorderWidthBottom = 1;
                        cell2.BorderWidthTop = 1;
                        cell2.BorderWidthRight = 1;

                        aux = new Phrase("Destinatario: ", underLineFont);
                        cell1.AddElement(aux);
                        aux = new Phrase("Nombre: ", headFont);
                        aux.AddSpecial(new Phrase(order.Contact.Name + " " + order.Contact.LastName, normalFont));
                        cell1.AddElement(aux);

                        var phonemovil = _context.Phone.FirstOrDefault(x => x.Type == "Móvil" && x.ReferenceId == order.ContactId);
                        var phonecasa = _context.Phone.FirstOrDefault(x => x.Type == "Casa" && x.ReferenceId == order.ContactId);

                        aux = new Phrase("Teléfono Primario: ", headFont);
                        aux.AddSpecial(new Phrase(phonemovil?.Number, normalFont));
                        cell1.AddElement(aux);

                        aux = new Phrase("Teléfono Secundario: ", headFont);
                        aux.AddSpecial(new Phrase(phonecasa?.Number, normalFont));
                        cell1.AddElement(aux);

                        var address = _context.Address.FirstOrDefault(x => x.ReferenceId == order.ContactId);

                        aux = new Phrase("Dirección: ", headFont);
                        aux.AddSpecial(new Phrase(address.AddressLine1 + ", " + address.Zip + ", " + address.State + ", " + address.City, normalFont));
                        cell1.AddElement(aux);

                        aux = new Phrase("Mensaje: ", headFont);
                        cell1.AddElement(aux);
                        cell1.AddElement(Chunk.NEWLINE);

                        if (order.Nota != "" && order.Nota != null)
                        {
                            Phrase nota = new Phrase("Nota: ", headFont);
                            nota.AddSpecial(new Phrase(order.Nota, normalFont));
                            cell1.AddElement(nota);
                            cell1.AddElement(Chunk.NEWLINE);
                        }
                        cell1.AddElement(Chunk.NEWLINE);
                        table.AddCell(cell1);
                        table.AddCell(cell2);
                        doc.Add(table);

                        // Parte del Combo
                        float[] columnP = { 1, 1 };
                        table = new PdfPTable(columnP);
                        table.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.PaddingLeft = 10;
                        cell1.BorderWidthLeft = 1;
                        cell1.BorderWidthBottom = 0;
                        cell1.BorderWidthTop = 1;
                        cell1.BorderWidthRight = 0;
                        cell2 = new PdfPCell();
                        cell2.BorderWidthLeft = 0;
                        cell2.BorderWidthBottom = 0;
                        cell2.BorderWidthTop = 1;
                        cell2.BorderWidthRight = 1;
                        aux = new Phrase("Productos", underLineFont);
                        cell1.AddElement(aux);
                        aux = new Phrase("Descripción: ", underLineFont);
                        cell2.AddElement(aux);
                        table.AddCell(cell1);
                        table.AddCell(cell2);

                        int count = 1;
                        int countTtotal = despacho.Count();
                        foreach (DespachoCombos pi in despacho)
                        {
                            if (countTtotal != count)
                            {
                                cell1 = new PdfPCell();
                                cell1.PaddingLeft = 10;
                                cell1.BorderWidthLeft = 1;
                                cell1.BorderWidthBottom = 0;
                                cell1.BorderWidthTop = 0;
                                cell1.BorderWidthRight = 0;
                                cell2 = new PdfPCell();
                                cell2.BorderWidthLeft = 0;
                                cell2.BorderWidthBottom = 0;
                                cell2.BorderWidthTop = 0;
                                cell2.BorderWidthRight = 1;
                            }
                            else
                            {
                                cell1 = new PdfPCell();
                                cell1.PaddingLeft = 10;
                                cell1.BorderWidthLeft = 1;
                                cell1.BorderWidthBottom = 1;
                                cell1.BorderWidthTop = 0;
                                cell1.BorderWidthRight = 0;
                                cell2 = new PdfPCell();
                                cell2.BorderWidthLeft = 0;
                                cell2.BorderWidthBottom = 1;
                                cell2.BorderWidthTop = 0;
                                cell2.BorderWidthRight = 1;
                            }

                            aux = new Phrase("Nombre: ", headFont);
                            aux.AddSpecial(new Phrase(pi.productocantidad.producto.ProductoBodega?.Nombre, normalFont));
                            cell1.AddElement(aux);
                            aux = new Phrase("Cantidad: ", headFont);
                            aux.AddSpecial(new Phrase(pi.productocantidad.cantidad.ToString(), normalFont));
                            cell1.AddElement(aux);
                            Paragraph parr = new Paragraph(pi.productocantidad.producto.Description, normalFont);
                            cell2.AddElement(parr);

                            if (count == countTtotal)
                            {
                                cell2.AddElement(Chunk.NEWLINE);
                            }
                            count++;
                            table.AddCell(cell1);
                            table.AddCell(cell2);
                        }
                        doc.Add(table);

                        // Firma
                        float[] column = { 1, 1 };
                        table = new PdfPTable(column);
                        table.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.PaddingLeft = 10;
                        cell1.BorderWidthLeft = 1;
                        cell1.BorderWidthBottom = 1;
                        cell1.BorderWidthTop = 1;
                        cell1.BorderWidthRight = 0;
                        cell2 = new PdfPCell();
                        cell2.BorderWidthLeft = 0;
                        cell2.BorderWidthBottom = 1;
                        cell2.BorderWidthTop = 1;
                        cell2.BorderWidthRight = 1;

                        aux = new Phrase("Entrega", underLineFont);
                        cell1.AddElement(aux);
                        aux = new Phrase("Nombre: ", headFont);
                        aux.AddSpecial(new Phrase("______________________________", normalFont));
                        cell1.AddElement(aux);
                        aux = new Phrase("Firma: _________________", headFont);
                        Phrase aux1 = new Phrase("Fecha: _______________", headFont);
                        aux.AddSpecial(aux1);
                        cell1.AddElement(aux);


                        aux = new Phrase("Recibe", underLineFont);
                        cell2.AddElement(aux);
                        aux = new Phrase("Nombre: ", headFont);
                        aux.AddSpecial(new Phrase("________________________________", normalFont));
                        cell2.AddElement(aux);
                        aux = new Phrase("Firma: __________________", headFont);
                        aux1 = new Phrase("Fecha: _______________", headFont);
                        aux.AddSpecial(aux1);
                        cell2.AddElement(aux);

                        cell1.AddElement(Chunk.NEWLINE);

                        table.AddCell(cell1);
                        table.AddCell(cell2);

                        doc.Add(table);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                        if (wholesaler != null)
                        {
                            doc.Add(new Phrase("Términos y Condiciones: ", headFont));
                            doc.Add(new Phrase(wholesaler?.CancellationPolicies, normalFont));
                        }
                        doc.NewPage();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                    writer.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        public async Task<string> CreateReporteDespachoRapidM(IGrouping<Guid, DespachoCombos> ordersmayorista, User aUser)
        {
            using (MemoryStream MStream = new MemoryStream())
            {
                iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.LETTER);
                PdfWriter writer = PdfWriter.GetInstance(doc, MStream);

                doc.Open();

                iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font headFontExpress = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.RED);

                try
                {
                    // VER AGENCIA
                    var agency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId).FirstOrDefault();
                    Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    var orders = ordersmayorista.GroupBy(x => x.order);

                    foreach (var despacho in orders)
                    {
                        var order = despacho.Key;
                        order.Client = await _context.Client.FindAsync(order.ClientId);
                        order.Contact = await _context.Contact.FindAsync(order.ContactId);
                        // Logo de la agencia
                        float[] columnWidths = { 1, 1 };
                        PdfPTable table = new PdfPTable(columnWidths);
                        table.WidthPercentage = 100;
                        PdfPCell cell1 = new PdfPCell();
                        cell1.BorderWidthLeft = 0;
                        cell1.BorderWidthBottom = 0;
                        cell1.BorderWidthTop = 1;
                        cell1.BorderWidthRight = 1;
                        PdfPCell cell2 = new PdfPCell();
                        cell2.Padding = 10;
                        cell2.BorderWidthLeft = 1;
                        cell2.BorderWidthBottom = 0;
                        cell2.BorderWidthTop = 1;
                        cell2.BorderWidthRight = 0;
                        string sWebRootFolder = _configuration["PathAgencia"];
                        if (order.express)
                        {
                            cell2.AddElement(new Phrase("EXPRESS", headFontExpress));
                        }
                        cell2.AddElement(new Phrase("Fecha: " + DateTime.Now.ToShortDateString(), headFont));
                        cell2.AddElement(new Phrase("Order No: " + order.Number, headFont));
                        cell2.AddElement(new Phrase("No. Despacho: " + order.numerodespacho, headFont));
                        Phrase aux;
                        cell2.AddElement(new Phrase("Remitente: ", underLineFont)); // Dirección de la empresa
                        aux = new Phrase("Nombre: ", headFont);
                        aux.AddSpecial(new Phrase(order.Client.Name + " " + order.Client.LastName, normalFont));
                        cell2.AddElement(aux);

                        aux = new Phrase("Destinatario: ", underLineFont);
                        cell1.AddElement(aux);
                        aux = new Phrase("Nombre: ", headFont);
                        aux.AddSpecial(new Phrase(order.Contact.Name + " " + order.Contact.LastName, normalFont));
                        cell1.AddElement(aux);

                        var phonemovil = _context.Phone.FirstOrDefault(x => x.Type == "Móvil" && x.ReferenceId == order.ContactId);
                        var phonecasa = _context.Phone.FirstOrDefault(x => x.Type == "Casa" && x.ReferenceId == order.ContactId);

                        aux = new Phrase("Teléfono Primario: ", headFont);
                        aux.AddSpecial(new Phrase(phonemovil?.Number, normalFont));
                        cell1.AddElement(aux);

                        aux = new Phrase("Teléfono Secundario: ", headFont);
                        aux.AddSpecial(new Phrase(phonecasa?.Number, normalFont));
                        cell1.AddElement(aux);

                        var address = _context.Address.FirstOrDefault(x => x.ReferenceId == order.ContactId);

                        aux = new Phrase("Dirección: ", headFont);
                        aux.AddSpecial(new Phrase(address.AddressLine1 + ", " + address.Zip + ", " + address.State + ", " + address.City, normalFont));
                        cell1.AddElement(aux);

                        aux = new Phrase("Mensaje: ", headFont);
                        if (order.Nota != "" && order.Nota != null)
                        {
                            cell1.AddElement(Chunk.NEWLINE);
                            Phrase nota = new Phrase("Nota: ", headFont);
                            nota.AddSpecial(new Phrase(order.Nota, normalFont));
                            cell1.AddElement(nota);
                            cell1.AddElement(Chunk.NEWLINE);
                        }
                        cell1.AddElement(aux);
                        table.AddCell(cell2);
                        table.AddCell(cell1);
                        doc.Add(table);
                        /* Combos */

                        // Parte del Combo
                        float[] columnP = { 1, 1 };
                        table = new PdfPTable(columnP);
                        table.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.PaddingLeft = 10;
                        cell1.BorderWidthLeft = 1;
                        cell1.BorderWidthBottom = 0;
                        cell1.BorderWidthTop = 0;
                        cell1.BorderWidthRight = 0;
                        cell2 = new PdfPCell();
                        cell2.BorderWidthLeft = 0;
                        cell2.BorderWidthBottom = 0;
                        cell2.BorderWidthTop = 0;
                        cell2.BorderWidthRight = 1;
                        aux = new Phrase("Productos", underLineFont);
                        cell1.AddElement(aux);
                        aux = new Phrase("Descripción: ", underLineFont);
                        cell2.AddElement(aux);

                        table.AddCell(cell1);
                        table.AddCell(cell2);

                        int count = 1;
                        int countTtotal = despacho.Count();
                        foreach (DespachoCombos pi in despacho)
                        {
                            if (countTtotal != count)
                            {
                                cell1 = new PdfPCell();
                                cell1.PaddingLeft = 10;
                                cell1.BorderWidthLeft = 1;
                                cell1.BorderWidthBottom = 0;
                                cell1.BorderWidthTop = 0;
                                cell1.BorderWidthRight = 0;
                                cell2 = new PdfPCell();
                                cell2.BorderWidthLeft = 0;
                                cell2.BorderWidthBottom = 0;
                                cell2.BorderWidthTop = 0;
                                cell2.BorderWidthRight = 1;
                            }
                            else
                            {
                                cell1 = new PdfPCell();
                                cell1.PaddingLeft = 10;
                                cell1.BorderWidthLeft = 1;
                                cell1.BorderWidthBottom = 1;
                                cell1.BorderWidthTop = 0;
                                cell1.BorderWidthRight = 0;
                                cell2 = new PdfPCell();
                                cell2.BorderWidthLeft = 0;
                                cell2.BorderWidthBottom = 1;
                                cell2.BorderWidthTop = 0;
                                cell2.BorderWidthRight = 1;
                            }

                            aux = new Phrase("Nombre: ", headFont);
                            aux.AddSpecial(new Phrase(pi.productocantidad.producto.ProductoBodega?.Nombre, normalFont));
                            cell1.AddElement(aux);
                            aux = new Phrase("Cantidad: ", headFont);
                            aux.AddSpecial(new Phrase(pi.productocantidad.cantidad.ToString(), normalFont));
                            cell1.AddElement(aux);
                            Paragraph parr = new Paragraph(pi.productocantidad.producto.Description, normalFont);
                            cell2.AddElement(parr);

                            if (count == countTtotal)
                            {
                                cell2.AddElement(Chunk.NEWLINE);
                            }
                            count++;
                            table.AddCell(cell1);
                            table.AddCell(cell2);
                        }

                        doc.Add(table);

                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                    writer.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }


    }
}
