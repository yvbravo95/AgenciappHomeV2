using Agenciapp.Service.IBodegaServices;
using Agenciapp.Service.IClientServices;
using Agenciapp.Service.IComboServices;
using Agenciapp.Service.IComboServices.Models;
using Agenciapp.Service.IContactServices;
using Agenciapp.Service.IStoreServices;
using Agenciapp.Service.ReyEnviosStore.Order.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Agenciapp.Service.ReyEnviosStore.Order
{
    public interface IOrderService
    {
        Task<Result<InvoiceDto>> Create(CreateOrderModel model, User user);
        Task<Result> UpdateStatusOrder(UdateStatusOrderModel model, User user);
        Task<InvoiceDto> GetInvoiceByOrderNumber(string orderNumberBase64, Guid agencyId);
        Task<Result> OrderCalback(InvoiceDto data, string url);
    }
    public class OrderService : IOrderService
    {
        private readonly IComboService _comboService;
        private readonly IClientService _clientService;
        private readonly IContactService _contactService;
        private readonly databaseContext _context;
        private readonly IStoreService _storeService;
        private readonly IBodegaService _bodegaService;
        public OrderService(databaseContext context, IBodegaService bodegaService, IComboService comboService, IStoreService storeService, IClientService clientService, IContactService contactServices)
        {
            _clientService = clientService;
            _comboService = comboService;
            _contactService = contactServices;
            _context = context;
            _storeService = storeService;
            _bodegaService = bodegaService;
        }

        public async Task<Result<InvoiceDto>> Create(CreateOrderModel model, User user)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    string orderBase64 = model.OrderNumber;
                    model.OrderNumber = Base64Decode(model.OrderNumber);
                    if (user == null) return Result.Failure<InvoiceDto>("Debe estar autenticado");

                    var agency = _context.Agency.FirstOrDefault(x => x.AgencyId == user.AgencyId);

                    var exist = _context.Order.Any(x => x.AgencyId == user.AgencyId && x.NoOrden == model.OrderNumber);
                    if (exist)
                        return Result.Failure<InvoiceDto>("La órden ya existe");

                    //Client
                    var client = await _clientService.Exist(model.Client, user.AgencyId);
                    if (client == null)
                    {
                        var clientResult = await _clientService.Create(model.Client, user);
                        if (clientResult.IsFailure)
                        {
                            return Result.Failure<InvoiceDto>(clientResult.Error);
                        }
                        client = clientResult.Value;
                    }

                    //Contact
                    var contact = await _contactService.Exist(model.Contact, user);
                    if (contact == null)
                    {
                        var result = await _contactService.Create(model.Contact, user);
                        if (result.IsFailure)
                        {
                            return Result.Failure<InvoiceDto>(result.Error);
                        }
                        contact = result.Value;
                    }
                    else
                    {
                        contact = await _contactService.Edit(contact.ContactId, new IContactServices.Models.EditContactModel
                        {
                            AddressLine1 = model.Contact.Address.AddressLine1,
                            AddressLine2 = model.Contact.Address.AddressLine2,
                            CI = model.Contact.CI,
                            Name = model.Contact.Name,
                            LastName = model.Contact.LastName,
                            PhoneNumberCasa = model.Contact.PhoneNumberCasa,
                            PhoneNumberMovil = model.Contact.PhoneNumberMovil,
                            Province = model.Contact.Address.Province,
                            Municipality = model.Contact.Address.Municipality,
                            Reparto = model.Contact.Address.Reparto
                        });
                    }

                    //Products
                    var products = _context.ProductosBodegas.Include(x => x.Proveedor).Include(x => x.Categoria)
                        .Where(x => x.esVisible && model.Products.Any(y => y.Code == x.Codigo) && x.IdAgency == user.AgencyId);


                    if (products.Count() != model.Products.Count)
                    {
                        return Result.Failure<InvoiceDto>("Hay productos que no existen, verifique que sean correctos");
                    }

                    TipoPago tipoPago = null;
                    switch (model.PaymentType)
                    {
                        case Enums.PaymentType.None:
                            break;
                        case Enums.PaymentType.CreditCard:
                            tipoPago = _context.TipoPago.FirstOrDefault(x => x.Type == "Crédito o Débito");
                            break;
                        case Enums.PaymentType.Zelle:
                            //tipoPago = _context.TipoPago.FirstOrDefault(x => x.Type == "Zelle");
                            break;
                        case Enums.PaymentType.Cash:
                            tipoPago = _context.TipoPago.FirstOrDefault(x => x.Type == "Cash");
                            break;
                        default:
                            break;
                    }
                    int count = 1;
                    foreach (var product in products.Where(x => x.Categoria.Nombre == "Combos").GroupBy(x => x.Proveedor))
                    {
                        decimal amount = 0;
                        decimal cServicio = 1;
                        decimal shipping = 0;
                        decimal priceProducts = 0;
                        List<WarehouseProduct> auxProd = new List<WarehouseProduct>();

                        foreach (var item in product)
                        {
                            priceProducts += (decimal)item.PrecioVentaReferencial * model.Products.FirstOrDefault(y => y.Code == item.Codigo).Qty;
                            if (item.EnableShipping)
                            {
                                shipping = item.Shipping;
                            }
                            auxProd.Add(new WarehouseProduct { Id = item.IdProducto, Qty = model.Products.FirstOrDefault(y => y.Code == item.Codigo).Qty });
                        }

                        amount = priceProducts + shipping + cServicio;

                        if (model.PaymentType == Enums.PaymentType.CreditCard && model.CreditCardFee == null)
                        {
                            if (model.CreditCardFee == null)
                                amount += Math.Round((amount * agency.creditCardFee / 100), 2);
                            else
                                amount += Math.Round((amount * (decimal)model.CreditCardFee / 100), 2);
                        }


                        var resultCombo = await _comboService.Create(new CreateComboModel
                        {
                            Products = auxProd,
                            AddCosto = 0,
                            AddPrecio = 0,
                            AgencyId = user.AgencyId,
                            Amount = amount,
                            AuthorizationCard = new AuthorizationCard(),
                            ClientId = client.ClientId,
                            ContactId = contact.ContactId,
                            CostService = (decimal)1.0,
                            Credito = 0,
                            Shipping = shipping,
                            Express = false,
                            Nota = model.Nota,
                            OfficeId = _context.Office.FirstOrDefault(x => x.AgencyId == user.AgencyId).OfficeId,
                            OrderNumber = $"CO{DateTime.Now.ToString("yMMddHHmmssff")}" + count,
                            OrderNumber2 = model.OrderNumber,
                            Pays = tipoPago == null ? new List<Pay>() : new List<Pay> { new Pay { TipoPago = tipoPago.TipoPagoId, ValorPagado = amount } },
                            //Pays = new List<Pay>(),
                            UserId = user.UserId
                        });
                        if (resultCombo.IsFailure)
                        {
                            transaction.Rollback();
                            return Result.Failure<InvoiceDto>(resultCombo.Error);
                        }

                        count++;
                    }

                    foreach (var product in products.Where(x => x.Categoria.Nombre == "Tienda").GroupBy(x => x.Proveedor))
                    {
                        decimal amount = 0;
                        decimal cServicio = 1;
                        decimal shipping = 0;
                        decimal priceProducts = 0;
                        List<WarehouseProduct> auxProd = new List<WarehouseProduct>();

                        foreach (var item in product)
                        {
                            priceProducts += ((decimal)item.PrecioVentaReferencial * model.Products.FirstOrDefault(y => y.Code == item.Codigo).Qty);
                            if (item.EnableShipping)
                            {
                                shipping = item.Shipping;
                            }
                            auxProd.Add(new WarehouseProduct { Id = item.IdProducto, Qty = model.Products.FirstOrDefault(y => y.Code == item.Codigo).Qty });
                        }

                        amount = shipping + priceProducts + cServicio;

                        if (model.PaymentType == Enums.PaymentType.CreditCard)
                        {
                            amount += Math.Round((amount * agency.creditCardFee / 100), 2);
                        }

                        var resultStore = await _storeService.Create(new IStoreServices.Models.CreateOrderTiendaModel
                        {
                            AgencyId = user.AgencyId,
                            Amount = amount,
                            AuthorizationCard = new AuthorizationCard(),
                            ClientId = client.ClientId,
                            ContactId = contact.ContactId,
                            CostService = (decimal)1.0,
                            Credito = 0,
                            CustomTaxes = 0,
                            Shipping = shipping,
                            Express = false,
                            Nota = model.Nota,
                            OfficeId = _context.Office.FirstOrDefault(x => x.AgencyId == user.AgencyId).OfficeId,
                            OrderNumber = $"TI{DateTime.Now.ToString("yMMddHHmmssff")}" + count,
                            OrderNumber2 = model.OrderNumber,
                            Pays = tipoPago == null ? new List<Pay>() : new List<Pay> { new Pay { TipoPago = tipoPago.TipoPagoId, ValorPagado = amount } },
                            //Pays = new List<Pay>(),
                            Price = priceProducts,
                            Products = auxProd,
                            UserId = user.UserId,
                            WholesalerId = product.Key.IdWholesaler
                        });

                        if (resultStore.IsFailure)
                        {
                            transaction.Rollback();
                            return Result.Failure<InvoiceDto>(resultStore.Error);
                        }

                        count++;
                    }

                    transaction.Commit();

                    InvoiceDto invoice = await GetInvoiceByOrderNumber(orderBase64, user.AgencyId);
                    if (!string.IsNullOrEmpty(agency.UrlCalbackApiReyenvios))
                    {
                        await OrderCalback(invoice, agency.UrlCalbackApiReyenvios);
                    }

                    return Result.Success(invoice);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Serilog.Log.Fatal(e, "Server Error");
                    return Result.Failure<InvoiceDto>("No se ha podido crear la órden");
                }
            }
        }

        public async Task<Result> UpdateStatusOrder(UdateStatusOrderModel model, User user)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (user == null) return Result.Failure("El usuario no existe");
                    var agency = await _context.Agency.FindAsync(user.AgencyId);
                    string number = Base64Decode(model.OrderNumber);

                    var orders = _context.Order
                            .Include(x => x.Agency)
                            .Include(x => x.Office)
                            .Include(x => x.Client)
                            .Include(x => x.User)
                            .Include(x => x.Contact)
                            .Include(x => x.Pagos)
                            .Include(x => x.Bag).ThenInclude(x => x.BagItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductoBodega)
                            .Where(x => x.NoOrden == number && x.AgencyId == agency.AgencyId);
                    foreach (var order in orders)
                    {
                        if (order.Type == "Combo")
                        {
                            var response = await UpdateStatusCombo(order, model.Status, user);
                            if (response.IsFailure)
                            {
                                transaction.Rollback();
                                return Result.Failure("No se ha podido actualizar el trámite");
                            }
                        }
                        else if (order.Type == "Tienda")
                        {
                            var responseTienda = await UpdateStatusTienda(order, model.Status, user);
                            if (responseTienda.IsFailure)
                            {
                                transaction.Rollback();
                                return Result.Failure("No se ha podido actualizar el trámite");
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            return Result.Failure("El tipo de trámite no es válido");
                        }

                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    if (string.IsNullOrEmpty(agency.UrlCalbackApiReyenvios))
                    {
                        await OrderCalback(await GetInvoiceByOrderNumber(model.OrderNumber, user.AgencyId), agency.UrlCalbackApiReyenvios);
                    }
                    return Result.Success();
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    transaction.Rollback();
                    return Result.Failure("Error. No se ha podido actualizar el estado del trámite");
                }
            }
        }

        private async Task<Result> UpdateStatusCombo(AgenciappHome.Models.Order combo, OrderStatus status, User user)
        {
            switch (status)
            {
                case OrderStatus.Pendiente:
                    if (combo.Status == "Iniciada" && combo.Balance == 0)
                    {
                        _context.RegistroPagos.RemoveRange(combo.Pagos);
                        combo.ValorPagado = 0;
                        combo.Balance = combo.Amount;
                        _context.Update(combo);
                        var sxc = new servicioxCobrar
                        {
                            date = DateTime.UtcNow,
                            ServicioId = combo.OrderId,
                            tramite = "Combos",
                            NoServicio = combo.Number,
                            cliente = combo.Client,
                            No_servicioxCobrar = "PAYT" + combo.Date.ToString("yyyyMMddHHmmss"),
                            cobrado = 0,
                            remitente = combo.Client,
                            destinatario = combo.Contact,
                            valorTramite = combo.Balance,
                            importeACobrar = combo.Balance,
                            mayorista = combo.Agency,
                        };
                        _context.Add(sxc);
                        return Result.Success();
                    }
                    break;
                case OrderStatus.Iniciada:
                    if (combo.Status == "Iniciada" && combo.Balance != 0)
                    {
                        var tipoPago = _context.TipoPago.FirstOrDefault(x => x.Type == "Crédito o Débito");
                        var pago = new RegistroPago
                        {
                            RegistroPagoId = Guid.NewGuid(),
                            Agency = combo.Agency,
                            date = DateTime.UtcNow,
                            number = "PAY" + combo.Date.ToString("yyyyMMddHHmmss"),
                            Office = combo.Office,
                            OrderId = combo.OrderId,
                            tipoPago = tipoPago,
                            UserId = user.UserId,
                            valorPagado = combo.Amount,
                            Client = combo.Client,
                            nota = "",
                        };
                        _context.Add(pago);
                        combo.Balance = 0;
                        combo.ValorPagado = combo.Amount;
                        _context.Update(combo);

                        var sxc = _context.servicioxCobrar
                            .Include(x => x.mayorista)
                            .Include(x => x.cliente)
                            .Where(x => x.cobrado == 0 && x.ServicioId == combo.OrderId && x.mayorista == combo.Agency && x.cliente != null).FirstOrDefault();
                        if (sxc != null) _context.Remove(sxc);

                        return Result.Success();
                    }
                    break;
                case OrderStatus.Cancelada:
                    if (combo.Status != "Cancelada")
                    {
                        combo.Status = "Cancelada";
                        _context.Logs.Add(new Log
                        {
                            Date = DateTime.Now,
                            Event = LogEvent.Cancelar,
                            Type = LogType.Combo,
                            LogId = Guid.NewGuid(),
                            Message = combo.Number,
                            User = combo.User,
                            Client = combo.Client,
                            Precio = combo.Amount.ToString(),
                            Pagado = combo.Pagos.Sum(x => x.valorPagado).ToString(),
                            AgencyId = combo.AgencyId,
                            Order = combo
                        });
                        var spp = _context.ServiciosxPagar.FirstOrDefault(s => s.Mayorista == null && s.SId == combo.OrderId);

                        if (spp != null) _context.ServiciosxPagar.Remove(spp);

                        var spp2 = _context.ServiciosxPagar.Where(s => s.Mayorista != null && s.SId == combo.OrderId).ToList();
                        foreach (var item in spp2)
                        {
                            _context.ServiciosxPagar.Remove(item);
                        }

                        // Elimino el servicio por cobrar
                        var servcobrar = _context.servicioxCobrar.Where(x => x.NoServicio == combo.Number && x.cobrado == 0).ToList();
                        foreach (var item in servcobrar)
                        {
                            _context.servicioxCobrar.Remove(item);
                        }
                        _context.Update(combo);

                        var bodega = await _context.Bodegas.FirstOrDefaultAsync(x => x.idAgency == combo.AgencyId);
                        foreach (var bag in combo.Bag)
                        {
                            foreach (var bagItem in bag.BagItems)
                            {
                                var responseBodega = await _bodegaService.DepositAsync(bagItem.Product.ProductoBodega, bodega, (decimal)bagItem.Qty);
                                if (responseBodega.IsFailure)
                                    return Result.Failure(responseBodega.Error);
                            }
                        }
                        return Result.Success();
                    }

                    break;
                default:
                    break;

            }
            return Result.Failure("No se ha actualizado el estado del trámite");

        }

        private async Task<Result> UpdateStatusTienda(AgenciappHome.Models.Order tienda, OrderStatus status, User user)
        {
            switch (status)
            {
                case OrderStatus.Pendiente:
                    if (tienda.Status == AgenciappHome.Models.Order.STATUS_INICIADA)
                    {
                        _context.RegistroPagos.RemoveRange(tienda.Pagos);
                        tienda.ValorPagado = 0;
                        tienda.Balance = tienda.Amount;
                        tienda.Status = AgenciappHome.Models.Order.STATUS_PENDIENTE;
                        _context.Update(tienda);
                        var sxc = new servicioxCobrar
                        {
                            date = DateTime.UtcNow,
                            ServicioId = tienda.OrderId,
                            tramite = "Combos",
                            NoServicio = tienda.Number,
                            cliente = tienda.Client,
                            No_servicioxCobrar = "PAYT" + tienda.Date.ToString("yyyyMMddHHmmss"),
                            cobrado = 0,
                            remitente = tienda.Client,
                            destinatario = tienda.Contact,
                            valorTramite = tienda.Balance,
                            importeACobrar = tienda.Balance,
                            mayorista = tienda.Agency,
                        };
                        _context.Add(sxc);
                        return Result.Success();
                    }
                    break;
                case OrderStatus.Iniciada:
                    if (tienda.Status == AgenciappHome.Models.Order.STATUS_PENDIENTE)
                    {
                        var tipoPago = _context.TipoPago.FirstOrDefault(x => x.Type == "Crédito o Débito");
                        var pago = new RegistroPago
                        {
                            RegistroPagoId = Guid.NewGuid(),
                            Agency = tienda.Agency,
                            date = DateTime.UtcNow,
                            number = "PAY" + tienda.Date.ToString("yyyyMMddHHmmss"),
                            Office = tienda.Office,
                            OrderId = tienda.OrderId,
                            tipoPago = tipoPago,
                            UserId = user.UserId,
                            valorPagado = tienda.Amount,
                            Client = tienda.Client,
                            nota = "",
                        };
                        _context.Add(pago);
                        tienda.Balance = 0;
                        tienda.ValorPagado = tienda.Amount;
                        tienda.Status = AgenciappHome.Models.Order.STATUS_INICIADA;
                        _context.Update(tienda);

                        var sxc = _context.servicioxCobrar
                            .Include(x => x.mayorista)
                            .Include(x => x.cliente)
                            .Where(x => x.cobrado == 0 && x.ServicioId == tienda.OrderId && x.mayorista == tienda.Agency && x.cliente != null).FirstOrDefault();
                        if (sxc != null) _context.Remove(sxc);

                        return Result.Success();
                    }
                    break;
                case OrderStatus.Cancelada:
                    if (tienda.Status != "Cancelada")
                    {
                        tienda.Status = "Cancelada";
                        _context.Logs.Add(new Log
                        {
                            Date = DateTime.Now,
                            Event = LogEvent.Cancelar,
                            Type = LogType.Orden,
                            LogId = Guid.NewGuid(),
                            Message = tienda.Number,
                            User = tienda.User,
                            Client = tienda.Client,
                            Precio = tienda.Amount.ToString(),
                            Pagado = tienda.Pagos.Sum(x => x.valorPagado).ToString(),
                            AgencyId = tienda.AgencyId,
                            Order = tienda
                        });
                        var spp = _context.ServiciosxPagar.FirstOrDefault(s => s.Mayorista == null && s.SId == tienda.OrderId);

                        if (spp != null) _context.ServiciosxPagar.Remove(spp);

                        var spp2 = _context.ServiciosxPagar.Where(s => s.Mayorista != null && s.SId == tienda.OrderId).ToList();
                        foreach (var item in spp2)
                        {
                            _context.ServiciosxPagar.Remove(item);
                        }

                        // Elimino el servicio por cobrar
                        var servcobrar = _context.servicioxCobrar.Where(x => x.NoServicio == tienda.Number && x.cobrado == 0).ToList();
                        foreach (var item in servcobrar)
                        {
                            _context.servicioxCobrar.Remove(item);
                        }
                        _context.Update(tienda);

                        var bodega = await _context.Bodegas.FirstOrDefaultAsync(x => x.idAgency == tienda.AgencyId);
                        foreach (var bag in tienda.Bag)
                        {
                            foreach (var bagItem in bag.BagItems)
                            {
                                var responseBodega = await _bodegaService.DepositAsync(bagItem.Product.ProductoBodega, bodega, (decimal)bagItem.Qty);
                                if (responseBodega.IsFailure)
                                    return Result.Failure(responseBodega.Error);
                            }
                        }

                        //Marcar registro de pago como cancelado
                        tienda.Pagos.ForEach(x => x.wasCanceled = true);
                        _context.RegistroPagos.UpdateRange(tienda.Pagos);

                        return Result.Success();
                    }
                    break;
                default:
                    break;
            }
            return Result.Failure("No se ha actualizado el estado del trámite");

        }

        public async Task<Result> OrderCalback(InvoiceDto data, string url)
        {
            try
            {
                if (!data.Orders.Any())
                {
                    return Result.Failure("Order not found");
                }

                string response = JsonConvert.SerializeObject(data);

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    String userName = "AgencyApp";
                    String passWord = "wLmrquyRQxPS22psuXqYMjawhTcNTpmB";
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + passWord));
                    wc.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                    //wc.Headers[HttpRequestHeader.Authorization] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIwMDciLCJ1bmlxdWVfbmFtZSI6IjAwNyIsIm5iZiI6MTYzOTY4MDU4OSwiZXhwIjoxNjM5NzY2OTg5LCJpYXQiOjE2Mzk2ODA1ODl9.CHMkiY9KLxsTIAzw2yMW9CAasTUuMck9V-LoFTWYvuM";
                    string HtmlResult = wc.UploadString(url, "POST", response);
                }
                return Result.Success();

            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server error");
                return Result.Failure(e.Message);
            }

        }

        public async Task<InvoiceDto> GetInvoiceByOrderNumber(string orderNumberBase64, Guid agencyId)
        {
            string number = Base64Decode(orderNumberBase64);
            var data = new InvoiceDto
            {
                Number = number,
                Orders = await _context.Order
                .Include(x => x.Bag).ThenInclude(x => x.BagItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductoBodega)
                .Where(x => x.NoOrden.Equals(number) && x.AgencyId == agencyId).Select(x => new OrderDto
                {
                    Id = x.OrderId,
                    Number = x.Number,
                    CreatedAt = x.Date,
                    Status = x.Status,
                    Amount = x.Amount,
                    Balance = x.Balance,
                    Products = x.Bag.SelectMany(y => y.BagItems.Select(z => new ProductItemDto
                    {
                        Qty = z.Qty,
                        Product = new ProductDto
                        {
                            Code = z.Product.ProductoBodega.Codigo,
                            Id = z.Product.ProductoBodega.IdProducto,
                            Name = z.Product.ProductoBodega.Nombre,
                            Description = z.Product.ProductoBodega.Descripcion,

                        }
                    })).ToList()
                }).ToListAsync()
            };

            return data;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
