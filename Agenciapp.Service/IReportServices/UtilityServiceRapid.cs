using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Service.IReportServices.Models;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;

namespace Agenciapp.Service.IReportServices
{
    public class UtilityServiceRapid : IUtilityService
    {
        private readonly Guid agencyDCubaId = Guid.Parse("4752B08A-7684-42B3-930D-FF86F496DF2F");
        private readonly Guid IdDCubaHouston = Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0");
        private readonly databaseContext _context;
        public UtilityServiceRapid(databaseContext context)
        {
            _context = context;
        }

        public async Task<List<UtilityModel>> getAllUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false)
        {
            List<UtilityModel> response = new List<UtilityModel>();

            response.AddRange(await GetByService(agencyId, STipo.Paquete, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.EnvioCaribe, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Combo, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Maritimo, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Servicio, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Passport, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Recarga, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Remesa, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Reserva, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Cubiq, dateIni, dateEnd, onlyCanceled));
            response.AddRange(await GetByService(agencyId, STipo.Mercado, dateIni, dateEnd, onlyCanceled));
            return response;
        }

        public async Task<List<UtilityModel>> GetByService(Guid agencyId, STipo type, DateTime dateIni, DateTime dateEnd, bool onlyCanceled = false)
        {
            switch (type)
            {
                case STipo.Passport:
                    return await PassportUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Reserva:
                    return await TicketUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Maritimo:
                    return await MaritimeShippingUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Paquete:
                    return await AirShipmentUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.EnvioCaribe:
                    return await CaribbeanShippinUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Combo:
                    return await ComboUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Remesa:
                    return await RemittanceUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.PTuristico:
                    return await PTuristicoUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Servicio:
                    return await OtherServiceUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Recarga:
                    return await RechargueUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Cubiq:
                    return await CubiqUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                case STipo.Mercado:
                    return await MarketUtility(agencyId, dateIni, dateEnd, onlyCanceled);
                default:
                    return new List<UtilityModel>();
            }
        }

        private async Task<List<UtilityModel>> MarketUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var market = await _context.Mercado
                .AsNoTracking()
                .Where(x => x.AgencyId == agencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateEnd.Date)
                .Where(x => (onlyCanceled && x.Status == Mercado.STATUS_CANCELADA) || (!onlyCanceled && x.Status != Mercado.STATUS_CANCELADA))
                .Select(x => new
                {
                    x.MercadoId,
                    x.Agency,
                    x.Number,
                    x.Date,
                    x.Amount,
                    x.Cargos,
                    Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                    {
                        Id = y.RegistroPagoId,
                        PaidValue = y.valorPagado,
                        TipoPago = y.tipoPago.Type,
                        TipoPagoId = y.tipoPagoId
                    }),
                    Client = new
                    {
                        Id = x.Client != null ? (Guid?)x.ClientId : null,
                        FullName = x.Client != null ? x.Client.FullData : "",
                        PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                    },
                    Employee = new
                    {
                        Id = x.User != null ? (Guid?)x.User.UserId : null,
                        FullName = x.User != null ? x.User.FullName : null
                    },
                })
                .ToListAsync();

            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in market)
            {
                response.Add(new UtilityModel
                {
                    ByTransferencia = false,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    Cost = item.Cargos,
                    SalePrice = item.Amount,
                    Utility = item.Amount - item.Cargos,
                    OrderNumber = item.Number,
                    Service = STipo.Mercado,
                    Date = item.Date,
                    Pays = item.Pays.ToList()
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> TicketUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var boletos = await _context.Ticket
                .AsNoTracking()
                .Where(x => x.PaqueteTuristicoId == null && x.AgencyId == agencyId && x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateEnd.Date)
                .Where(x => (onlyCanceled && x.State == Ticket.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.State != Ticket.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                .Select(x => new
                {
                    x.TicketId,
                    x.Agency,
                    x.ReservationNumber,
                    x.RegisterDate,
                    x.Total,
                    x.Cost,
                    x.Charges,
                    x.type,
                    x.ClientIsCarrier,
                    Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                    {
                        Id = y.RegistroPagoId,
                        PaidValue = y.valorPagado,
                        TipoPago = y.tipoPago.Type,
                        TipoPagoId = y.tipoPagoId
                    }),
                    AgencyTransferida = new
                    {
                        AgencyId = (Guid?)null,
                        Name = ""
                    },
                    Client = new
                    {
                        Id = x.Client != null ? (Guid?)x.ClientId : null,
                        FullName = x.Client != null ? x.Client.FullData : "",
                        PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                    },
                    Employee = new
                    {
                        Id = x.User != null ? (Guid?)x.User.UserId : null,
                        FullName = x.User != null ? x.User.FullName : null
                    },
                })
                .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in boletos)
            {
                decimal cost = item.Cost + item.Charges;
                decimal utilidad = item.Total - item.Cost - item.Charges;
                decimal precio = item.Total;

                response.Add(new UtilityModel
                {
                    ByTransferencia = false,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    Cost = cost,
                    SalePrice = precio,
                    Utility = utilidad,
                    OrderNumber = item.ReservationNumber,
                    Service = STipo.Reserva,
                    TransferredAgencyName = item.AgencyTransferida != null ? item.Agency?.Name : "",
                    Date = item.RegisterDate,
                    Pays = item.Pays.ToList(),
                    TipoServicio = item.type,
                    IsCarrier = item.ClientIsCarrier

                });
            }
            return response;
        }
        private async Task<List<UtilityModel>> MaritimeShippingUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var em = await _context.EnvioMaritimo
                        .AsNoTracking()
                        .Where(x => x.AgencyId == agencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateEnd.Date)
                        .Where(x => (onlyCanceled && x.Status == EnvioMaritimo.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.Status != EnvioMaritimo.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                        .Select(x => new
                        {
                            x.Id,
                            x.Agency,
                            x.Number,
                            x.Date,
                            x.Amount,
                            x.costoMayorista,
                            Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                            {
                                Id = y.RegistroPagoId,
                                PaidValue = y.valorPagado,
                                TipoPago = y.tipoPago.Type,
                                TipoPagoId = y.tipoPagoId
                            }),
                            AgencyTransferida = new
                            {
                                AgencyId = x.agencyTransferida != null ? (Guid?)x.agencyTransferida.AgencyId : null,
                                Name = x.agencyTransferida != null ? x.agencyTransferida.Name : ""
                            },
                            Client = new
                            {
                                Id = x.Client != null ? (Guid?)x.Client.ClientId : null,
                                FullName = x.Client != null ? x.Client.FullData : "",
                                PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                            },
                            Employee = new
                            {
                                Id = x.User != null ? (Guid?)x.User.UserId : null,
                                FullName = x.User != null ? x.User.FullName : null
                            },
                        })
                        .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in em)
            {
                response.Add(new UtilityModel
                {
                    ByTransferencia = false,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = item.costoMayorista,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.Number,
                    SalePrice = item.Amount,
                    Utility = item.Amount - item.costoMayorista,
                    Service = STipo.Maritimo,
                    TransferredAgencyName = item.AgencyTransferida != null ? item.Agency?.Name : "",
                    Date = item.Date,
                    Pays = item.Pays.ToList()
                });
            }
            return response;
        }
        private async Task<List<UtilityModel>> AirShipmentUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var enviosaereos = await _context.Order
                        .AsNoTracking()
                        .Where(x => (x.AgencyId == agencyId || x.agencyTransferida.AgencyId == agencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateEnd.Date && x.Type != "Remesas" && x.Type != "Combo")
                        .Where(x => (onlyCanceled && x.Status == Order.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.Status != Order.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                        .Select(x => new
                        {
                            x.OrderId,
                            x.Agency,
                            x.Number,
                            x.Date,
                            x.Amount,
                            x.costoMayorista,
                            x.costoporDespacho,
                            x.OtrosCostos,
                            x.costoProductosBodega,
                            x.costoDeProveedor,
                            x.ProductsShipping,
                            x.Delivery,
                            Pays = x.Pagos.Select(y => new UtilityModel.Pay
                            {
                                Id = y.RegistroPagoId,
                                PaidValue = y.valorPagado,
                                TipoPago = y.tipoPago.Type,
                                TipoPagoId = y.tipoPagoId
                            }),
                            usedCredito = x.Pagos.Any(y => y.tipoPago.Type == "Crédito de Consumo"),
                            AgencyTransferida = new
                            {
                                AgencyId = x.agencyTransferida != null ? (Guid?)x.agencyTransferida.AgencyId : null,
                                Name = x.agencyTransferida != null ? x.agencyTransferida.Name : ""
                            },
                            Client = new
                            {
                                Id = x.Client != null ? (Guid?)x.ClientId : null,
                                FullName = x.Client != null ? x.Client.FullData : "",
                                PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                            },
                            Employee = new
                            {
                                Id = x.User != null ? (Guid?)x.User.UserId : null,
                                FullName = x.User != null ? x.User.FullName : null
                            },
                        })
                        .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in enviosaereos)
            {
                decimal refcosto = item.costoMayorista + item.costoProductosBodega + item.ProductsShipping + item.Delivery;
                decimal refutilidad = item.Amount - refcosto;
                decimal refprecio = item.Amount;
                bool bytransferencia = false;
                if (item.AgencyTransferida.AgencyId != null)
                {
                    if (item.AgencyTransferida.AgencyId == agencyId)
                    {
                        bytransferencia = true;
                        refcosto = item.costoDeProveedor + item.OtrosCostos + item.ProductsShipping + item.Delivery;
                        refprecio = item.costoMayorista + item.ProductsShipping + item.Delivery;
                        refutilidad = refprecio - refcosto;
                    }
                }

                response.Add(new UtilityModel
                {
                    ByTransferencia = bytransferencia,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    Cost = refcosto,
                    SalePrice = refprecio,
                    Utility = refutilidad,
                    OrderNumber = item.Number,
                    Service = STipo.Paquete,
                    TransferredAgencyName = item.AgencyTransferida != null ? item.Agency?.Name : "",
                    Date = item.Date,
                    Pays = item.Pays.ToList()
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> CaribbeanShippinUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var envioscaribe = await _context.EnvioCaribes
                        .AsNoTracking()
                        .Where(x => x.User.Username != "Manuel14" && (x.AgencyId == agencyId || x.AgencyTransferidaId == agencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateEnd.Date)
                        .Where(x => (onlyCanceled && x.Status == EnvioCaribe.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.Status != EnvioCaribe.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                        .Select(x => new
                        {
                            x.EnvioCaribeId,
                            x.Agency,
                            x.Number,
                            x.Date,
                            x.Amount,
                            x.costo,
                            x.servicio,
                            x.OtrosCostos,
                            x.paquetes,
                            Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                            {
                                Id = y.RegistroPagoId,
                                PaidValue = y.valorPagado,
                                TipoPago = y.tipoPago.Type,
                                TipoPagoId = y.tipoPagoId
                            }),
                            Contact = new
                            {
                                City = x.Contact != null ? x.Contact.Address != null ? x.Contact.Address.City : "" : ""
                            },
                            AgencyTransferida = new
                            {
                                AgencyId = x.AgencyTransferida != null ? (Guid?)x.AgencyTransferida.AgencyId : null,
                                Name = x.AgencyTransferida != null ? x.AgencyTransferida.Name : ""
                            },
                            Client = new
                            {
                                Id = x.Client != null ? (Guid?)x.ClientId : null,
                                FullName = x.Client != null ? x.Client.FullData : "",
                                PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                            },
                            Employee = new
                            {
                                Id = x.User != null ? (Guid?)x.User.UserId : null,
                                FullName = x.User != null ? x.User.FullName : null
                            },
                        })
                        .ToListAsync();
            //Auxiliar para obtener el costo que aplica el mayorista Caribe a Rapid
            var rapid = await _context.Agency.FirstOrDefaultAsync(x => x.Name == "Rapid Multiservice");
            var mayorista = await _context.Wholesalers.Include(x => x.tipoServicioHabana).Include(x => x.tipoServicioRestoProv).FirstOrDefaultAsync(x => x.EsVisible && x.AgencyId == rapid.AgencyId && x.Category.category == "Maritimo-Aereo");

            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in envioscaribe)
            {
                decimal refcostoEC = item.costo + item.OtrosCostos;
                decimal refunitarioEC = item.Amount - (item.costo + item.OtrosCostos);
                decimal refprecioEC = item.Amount;

                bool isbytransferencia = false;
                if (item.AgencyTransferida.AgencyId != null)
                {
                    if (item.AgencyTransferida.AgencyId == agencyId)
                    {
                        isbytransferencia = true;
                        refcostoEC = 0;
                        refunitarioEC = 0;
                        refprecioEC = 0;

                        if (mayorista != null)
                        {
                            refprecioEC = item.costo;
                            List<TipoServicioMayorista> serv = new List<TipoServicioMayorista>();
                            if (item.Contact.City == "La Habana")
                            {
                                serv = mayorista.tipoServicioHabana;
                            }
                            else
                            {
                                serv = mayorista.tipoServicioRestoProv;
                            }

                            if (serv != null)
                            {
                                if (item.servicio == "Correo-Aereo")
                                {
                                    var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                    decimal costo = (item.paquetes.Sum(x => x.peso) * servicio.costoAereo) + item.OtrosCostos;
                                    refunitarioEC = item.costo - costo;
                                    refcostoEC = costo;
                                }
                                else if (item.servicio == "Correo-Maritimo")
                                {
                                    var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                    decimal costo = (item.paquetes.Sum(x => x.peso) * servicio.costoMaritimo) + item.OtrosCostos;
                                    refunitarioEC = item.costo - costo;
                                    refcostoEC = costo;
                                }
                                else if (item.servicio == "Aerovaradero- Recogida")
                                {
                                    var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Aereo_Varadero);
                                    decimal costo = (item.paquetes.Sum(x => x.peso) * servicio.costoAereo) + item.OtrosCostos;
                                    refunitarioEC = item.costo - costo;
                                    refcostoEC = costo;

                                }
                            }
                        }
                    }
                }

                response.Add(new UtilityModel
                {
                    ByTransferencia = isbytransferencia,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    Cost = refcostoEC,
                    SalePrice = refprecioEC,
                    Utility = refunitarioEC,
                    OrderNumber = item.Number,
                    Service = STipo.EnvioCaribe,
                    TransferredAgencyName = item.AgencyTransferida != null ? item.Agency?.Name : "",
                    Date = item.Date,
                    Pays = item.Pays.ToList()
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> OtherServiceUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var servicios = await _context.Servicios
                .AsNoTracking()
                .Where(x => x.PaqueteTuristicoId == null && x.agency.AgencyId == agencyId && x.fecha.Date >= dateIni.Date && x.fecha.Date <= dateEnd.Date)
                .Where(x => (onlyCanceled && x.estado == Servicio.EstadoCancelado && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.estado != Servicio.EstadoCancelado || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                .Select(x => new
                {
                    x.ServicioId,
                    x.agency,
                    x.numero,
                    x.fecha,
                    x.importeTotal,
                    x.costoMayorista,
                    Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                    {
                        Id = y.RegistroPagoId,
                        PaidValue = y.valorPagado,
                        TipoPago = y.tipoPago.Type,
                        TipoPagoId = y.tipoPagoId
                    }),
                    TipoServicio = x.tipoServicio.Nombre,
                    AgencyTransferida = new
                    {
                        AgencyId = (Guid?)null,
                        Name = ""
                    },
                    Client = new
                    {
                        Id = x.cliente != null ? (Guid?)x.cliente.ClientId : null,
                        FullName = x.cliente != null ? x.cliente.FullData : "",
                        PhoneNumber = x.cliente != null ? x.cliente.Phone != null ? x.cliente.Phone.Number : "" : ""
                    },
                    Employee = new
                    {
                        Id = x.User != null ? (Guid?)x.User.UserId : null,
                        FullName = x.User != null ? x.User.FullName : null
                    },
                })
                .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in servicios)
            {
                decimal costo = item.costoMayorista;
                decimal utilidad = item.importeTotal - item.costoMayorista;
                decimal precio = item.importeTotal;

                response.Add(new UtilityModel
                {
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = costo,
                    ByTransferencia = false,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.numero,
                    SalePrice = precio,
                    Service = STipo.Servicio,
                    Utility = utilidad,
                    TransferredAgencyName = item.AgencyTransferida != null ? item.agency?.Name : "",
                    TipoServicio = item.TipoServicio,
                    Date = item.fecha,
                    Pays = item.Pays.ToList()
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> RechargueUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var recargas = await _context.Rechargue
                .AsNoTracking()
                .Where(x => x.AgencyId == agencyId && x.date.Date >= dateIni.Date && x.date.Date <= dateEnd.Date)
                .Where(x => (onlyCanceled && x.estado == Rechargue.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.estado != Rechargue.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                .Select(x => new
                {
                    x.RechargueId,
                    x.Agency,
                    x.Number,
                    x.date,
                    x.Import,
                    x.costoMayorista,
                    Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                    {
                        Id = y.RegistroPagoId,
                        PaidValue = y.valorPagado,
                        TipoPago = y.tipoPago.Type,
                        TipoPagoId = y.tipoPagoId
                    }),
                    AgencyTransferida = new
                    {
                        AgencyId = (Guid?)null,
                        Name = ""
                    },
                    Client = new
                    {
                        Id = x.Client != null ? (Guid?)x.ClientId : null,
                        FullName = x.Client != null ? x.Client.FullData : "",
                        PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                    },
                    Employee = new
                    {
                        Id = x.User != null ? (Guid?)x.User.UserId : null,
                        FullName = x.User != null ? x.User.FullName : null
                    },
                })
                .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in recargas)
            {
                decimal costo = item.costoMayorista;
                decimal utilidad = item.Import - item.costoMayorista;
                decimal precio = item.Import;

                response.Add(new UtilityModel
                {
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = costo,
                    ByTransferencia = false,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.Number,
                    SalePrice = precio,
                    Service = STipo.Recarga,
                    Utility = utilidad,
                    TransferredAgencyName = item.AgencyTransferida != null ? item.Agency?.Name : "",
                    Date = item.date,
                    Pays = item.Pays.ToList()
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> RemittanceUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var re = await _context.Remittance
                        .AsNoTracking()
                        .Where(x => (x.AgencyId == agencyId || x.agencyTransferida.AgencyId == agencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateEnd.Date)
                        .Where(x => (onlyCanceled && x.Status == Remittance.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.Status != Remittance.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                        .Select(x => new
                        {
                            x.RemittanceId,
                            x.Agency,
                            x.Number,
                            x.Date,
                            x.Amount,
                            x.costoMayorista,
                            x.costoporDespacho,
                            x.ValorPagado,
                            Pays = x.Pagos.Select(y => new UtilityModel.Pay
                            {
                                Id = y.RegistroPagoId,
                                PaidValue = y.valorPagado,
                                TipoPago = y.tipoPago.Type,
                                TipoPagoId = y.tipoPagoId
                            }),
                            AgencyTransferida = new
                            {
                                AgencyId = x.agencyTransferida != null ? (Guid?)x.agencyTransferida.AgencyId : null,
                                Name = x.agencyTransferida != null ? x.agencyTransferida.Name : ""
                            },
                            Client = new
                            {
                                Id = x.Client != null ? (Guid?)x.ClientId : null,
                                FullName = x.Client != null ? x.Client.FullData : "",
                                PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                            },
                            Employee = new
                            {
                                Id = x.User != null ? (Guid?)x.User.UserId : null,
                                FullName = x.User != null ? x.User.FullName : null
                            },
                        })
                        .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in re)
            {
                decimal cost = item.costoMayorista + agency.remesa_entregaCuba;
                decimal utility = item.Amount - (item.costoMayorista + item.costoporDespacho + agency.remesa_entregaCuba);
                decimal price = item.Amount;

                bool isbytransferencia = false;
                if (item.AgencyTransferida.AgencyId != null)
                {
                    if (item.AgencyTransferida.AgencyId == agency.AgencyId)
                    {
                        isbytransferencia = true;
                        cost = item.ValorPagado + agency.remesa_entregaCuba;
                        utility = item.costoMayorista - item.ValorPagado - agency.remesa_entregaCuba;
                        price = item.costoMayorista;
                    }
                }
                List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
                if (!isbytransferencia && agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    bool costIsLower = item.Pays.Any(x => x.PaidValue >= cost);

                    foreach (var pay in item.Pays.OrderByDescending(x => x.PaidValue))
                    {
                        UtilityModel.Pay aux = new UtilityModel.Pay
                        {
                            Costo = 0,
                            Id = pay.Id,
                            PaidValue = pay.PaidValue,
                            TipoPago = pay.TipoPago,
                            TipoPagoId = pay.TipoPagoId,
                            Utility = pay.Utility
                        };
                        if (costIsLower)
                        {
                            if (pay.PaidValue >= cost)
                            {
                                aux.Utility = pay.PaidValue - cost;
                                aux.Costo = cost;
                            }
                            else
                            {
                                aux.Utility = pay.PaidValue;
                                aux.Costo = 0;
                            }
                        }
                        else
                        {
                            decimal porcientoDelTotal = (pay.PaidValue * 100) / price;
                            decimal valorPorcientoEnCosto = (porcientoDelTotal * cost / 100);
                            aux.Utility = pay.PaidValue - valorPorcientoEnCosto;
                            aux.Costo = valorPorcientoEnCosto;
                        }
                        newPays.Add(aux);
                    }
                }

                response.Add(new UtilityModel
                {
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = cost,
                    ByTransferencia = isbytransferencia,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.Number,
                    SalePrice = price,
                    Service = STipo.Remesa,
                    Utility = utility,
                    TransferredAgencyName = item.AgencyTransferida.AgencyId != null ? item.Agency?.Name : "",
                    Date = item.Date,
                    Pays = item.AgencyTransferida.AgencyId == null ? newPays.Any() ? newPays : item.Pays.ToList() : new List<UtilityModel.Pay>()
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> PTuristicoUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var re = await _context.PaquetesTuristicos
                        .AsNoTracking()
                        .Where(x => x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == agencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateEnd.Date)
                       .Where(x => (onlyCanceled && x.Status == PaqueteTuristico.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.Status != PaqueteTuristico.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                        .Select(x => new
                        {
                            x.PaqueteId,
                            x.Agency,
                            x.Number,
                            x.Date,
                            x.Amount,
                            x.Costo,
                            x.OtrosCostos,
                            x.ValorPagado,
                            Pays = x.Pagos.Select(y => new UtilityModel.Pay
                            {
                                Id = y.RegistroPagoId,
                                PaidValue = y.valorPagado,
                                TipoPago = y.tipoPago.Type,
                                TipoPagoId = y.tipoPagoId
                            }),
                            Client = new
                            {
                                Id = x.Client != null ? (Guid?)x.ClientId : null,
                                FullName = x.Client != null ? x.Client.FullData : "",
                                PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                            },
                            Employee = new
                            {
                                Id = x.User != null ? (Guid?)x.User.UserId : null,
                                FullName = x.User != null ? x.User.FullName : null
                            },
                        })
                        .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in re)
            {
                decimal cost = item.Costo + item.OtrosCostos;
                decimal price = item.Amount;
                decimal utility = item.Amount - cost;

                bool isbytransferencia = false;
                
                List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
                if (!isbytransferencia && agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    bool costIsLower = item.Pays.Any(x => x.PaidValue >= cost);

                    foreach (var pay in item.Pays.OrderByDescending(x => x.PaidValue))
                    {
                        UtilityModel.Pay aux = new UtilityModel.Pay
                        {
                            Costo = 0,
                            Id = pay.Id,
                            PaidValue = pay.PaidValue,
                            TipoPago = pay.TipoPago,
                            TipoPagoId = pay.TipoPagoId,
                            Utility = pay.Utility
                        };
                        if (costIsLower)
                        {
                            if (pay.PaidValue >= cost)
                            {
                                aux.Utility = pay.PaidValue - cost;
                                aux.Costo = cost;
                            }
                            else
                            {
                                aux.Utility = pay.PaidValue;
                                aux.Costo = 0;
                            }
                        }
                        else
                        {
                            decimal porcientoDelTotal = (pay.PaidValue * 100) / price;
                            decimal valorPorcientoEnCosto = (porcientoDelTotal * cost / 100);
                            aux.Utility = pay.PaidValue - valorPorcientoEnCosto;
                            aux.Costo = valorPorcientoEnCosto;
                        }
                        newPays.Add(aux);
                    }
                }

                response.Add(new UtilityModel
                {
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = cost,
                    ByTransferencia = isbytransferencia,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.Number,
                    SalePrice = price,
                    Service = STipo.PTuristico,
                    Utility = utility,
                    TransferredAgencyName = "",
                    Date = item.Date,
                    Pays = newPays.Any() ? newPays : item.Pays.ToList()
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> ComboUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var combos = await _context.Order
                        .AsNoTracking()
                        .Where(x => (x.AgencyId == agencyId || x.agencyTransferida.AgencyId == agencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateEnd.Date && x.Type == "Combo")
                        .Where(x => (onlyCanceled && x.Status == Order.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.Status != Order.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                        .Select(x => new
                        {
                            x.Minorista,
                            x.OrderId,
                            x.Agency,
                            x.Number,
                            x.Date,
                            x.Amount,
                            x.costoMayorista,
                            x.costoporDespacho,
                            x.OtrosCostos,
                            x.costoDeProveedor,
                            x.ProductsShipping,
                            usedCredito = x.Pagos.Any(y => y.tipoPago.Type == "Crédito de Consumo"),
                            AgencyTransferida = new
                            {
                                AgencyId = x.agencyTransferida != null ? (Guid?)x.agencyTransferida.AgencyId : null,
                                Name = x.agencyTransferida != null ? x.agencyTransferida.Name : ""
                            },
                            Client = new
                            {
                                Id = x.Client != null ? (Guid?)x.ClientId : null,
                                FullName = x.Client != null ? x.Client.FullData : "",
                                PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                            },
                            Employee = new
                            {
                                Id = x.User != null ? (Guid?)x.User.UserId : null,
                                FullName = x.User != null ? x.User.FullName : null
                            },
                            Pays = x.Pagos.Select(y => new UtilityModel.Pay
                            {
                                Id = y.RegistroPagoId,
                                PaidValue = y.valorPagado,
                                TipoPago = y.tipoPago.Type,
                                TipoPagoId = y.tipoPagoId
                            })
                        })
                        .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in combos)
            {
                decimal refcosto = item.costoMayorista + item.OtrosCostos + item.ProductsShipping;
                decimal refutilidad = item.Amount - item.costoMayorista - item.OtrosCostos - item.ProductsShipping;
                decimal refprecio = item.Amount;
                bool bytransferencia = false;
                if (item.AgencyTransferida.AgencyId != null)
                {
                    if (item.AgencyTransferida.AgencyId == agency.AgencyId)
                    {
                        bytransferencia = true;
                        refcosto = item.costoDeProveedor + item.ProductsShipping;
                        refprecio = item.costoMayorista + item.OtrosCostos + item.ProductsShipping;
                        refutilidad = refprecio - refcosto - item.OtrosCostos;
                    }
                }
                else if (item.Minorista != null)
                {
                    bytransferencia = true;
                    refcosto = item.costoDeProveedor + item.OtrosCostos;
                    refprecio = item.costoMayorista;
                    refutilidad = refprecio - refcosto;
                }

                List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
                if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    if (!bytransferencia)
                    {
                        bool costIsLower = item.Pays.Any(x => x.PaidValue >= refcosto);

                        foreach (var pay in item.Pays.OrderByDescending(x => x.PaidValue))
                        {
                            UtilityModel.Pay aux = new UtilityModel.Pay
                            {
                                Costo = 0,
                                Id = pay.Id,
                                PaidValue = pay.PaidValue,
                                TipoPago = pay.TipoPago,
                                TipoPagoId = pay.TipoPagoId,
                                Utility = pay.Utility
                            };
                            if (costIsLower)
                            {
                                if (pay.PaidValue >= refcosto)
                                {
                                    aux.Utility = pay.PaidValue - refcosto;
                                    aux.Costo = refcosto;
                                }
                                else
                                {
                                    aux.Utility = pay.PaidValue;
                                    aux.Costo = 0;
                                }
                            }
                            else
                            {
                                decimal porcientoDelTotal = (pay.PaidValue * 100) / refprecio;
                                decimal valorPorcientoEnCosto = (porcientoDelTotal * refcosto / 100);
                                aux.Utility = pay.PaidValue - valorPorcientoEnCosto;
                                aux.Costo = valorPorcientoEnCosto;
                            }
                            newPays.Add(aux);
                        }
                    }
                    else
                    {
                        UtilityModel.Pay aux = new UtilityModel.Pay
                        {
                            Costo = 0,
                            Id = Guid.Empty,
                            PaidValue = refprecio,
                            TipoPago = "Pendiente",
                            TipoPagoId = Guid.Empty,
                            Utility = refutilidad
                        };
                        newPays.Add(aux);
                    }
                }

                response.Add(new UtilityModel
                {
                    ByTransferencia = bytransferencia,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    Cost = refcosto,
                    SalePrice = refprecio,
                    Utility = refutilidad,
                    OrderNumber = item.Number,
                    Service = STipo.Combo,
                    TransferredAgencyName = item.AgencyTransferida.AgencyId != null ? item.Agency?.Name : item.Minorista != null ? item.Minorista.Name : "",
                    Date = item.Date,
                    CServicio = item.OtrosCostos,
                    Pays = item.AgencyTransferida.AgencyId == null ? newPays.Any() ? newPays : item.Pays.ToList() : new List<UtilityModel.Pay>()
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> PassportUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var pasaportes = await _context.Passport
                        .AsNoTracking()
                        .Where(x => (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId))
                        .Where(x => !x.AppMovil || (x.AppMovil && x.Status != Passport.STATUS_REVIEW))
                        .Where(x => x.FechaSolicitud.Date >= dateIni.Date && x.FechaSolicitud.Date <= dateEnd.Date)
                        .Where(x => (onlyCanceled && x.Status == Passport.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.Status != Passport.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
                        .Select(x => new
                        {
                            x.PassportId,
                            x.Agency,
                            x.OrderNumber,
                            x.FechaSolicitud,
                            x.Total,
                            x.costo,
                            x.OtrosCostos,
                            x.ServicioConsular,
                            x.Minorista,
                            x.PassportExpress,
                            x.Express,
                            Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                            {
                                Id = y.RegistroPagoId,
                                PaidValue = y.valorPagado,
                                TipoPago = y.tipoPago.Type,
                                TipoPagoId = y.tipoPagoId,
                                Costo = 0,
                                Utility = 0
                            }).ToList(),
                            AgencyTransferida = new
                            {
                                agencyId = x.AgencyTransferida != null ? (Guid?)x.AgencyTransferida.AgencyId : null,
                                Name = x.AgencyTransferida != null ? x.AgencyTransferida.Name : ""
                            },
                            Client = new
                            {
                                Id = x.Client != null ? (Guid?)x.ClientId : null,
                                FullName = x.Client != null ? x.Client.FullData : "",
                                PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                            },
                            Employee = new
                            {
                                Id = x.User != null ? (Guid?)x.User.UserId : null,
                                FullName = x.User != null ? x.User.FullName : null
                            },
                        })
                        .ToListAsync();

            if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
            {
                pasaportes = pasaportes.OrderBy(x => x.OrderNumber).ToList();
            }

            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in pasaportes)
            {

                decimal costo = 0;
                if (agency.AgencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    costo = item.costo + (item.Express ? (decimal)item.PassportExpress?.Costo : 0);
                }
                else
                {
                    costo = item.costo + item.OtrosCostos + (item.Express ? (decimal)item.PassportExpress?.Costo : 0);
                }
                decimal utilidad = item.Total - costo;
                decimal precio = item.Total;
                bool bytransferencia = false;
                if (item.AgencyTransferida.agencyId != null)
                {
                    if (item.AgencyTransferida.agencyId == agency.AgencyId)
                    {
                        bytransferencia = true;
                        precio = item.costo + item.OtrosCostos + (item.Express ? (decimal)item.PassportExpress?.Costo : 0);
                        costo = 0;
                        var wholesaler = _context.Wholesalers.Include(x => x.ServConsularMayoristas).FirstOrDefault(x => x.EsVisible && x.AgencyId == agency.AgencyId && x.Category.category == "Pasaporte");
                        if (wholesaler != null)
                        {
                            ServConsularMayorista auxserv = wholesaler.ServConsularMayoristas.FirstOrDefault(x => x.servicio == item.ServicioConsular);
                            if (auxserv != null)
                            {
                                SettingPassportExpress spe = _context.SettingPassportExpresses.FirstOrDefault(x => x.AgencyId == agency.AgencyId && x.ServicioConsular == item.ServicioConsular.ToString());
                                costo = auxserv.costo + (item.Express ? (decimal)spe?.Costo : 0);
                            }
                        }
                        utilidad = precio - costo - item.OtrosCostos;
                    }
                }
                if (item.Minorista != null)
                {
                    bytransferencia = true;
                    precio = item.costo + (item.Express ? (decimal)item.PassportExpress?.Costo : 0);
                    costo = 0;
                    var wholesaler = _context.Wholesalers.Include(x => x.ServConsularMayoristas).FirstOrDefault(x => x.EsVisible && x.AgencyId == agency.AgencyId && x.Category.category == "Pasaporte");
                    if (wholesaler != null)
                    {
                        ServConsularMayorista auxserv = wholesaler.ServConsularMayoristas.FirstOrDefault(x => x.servicio == item.ServicioConsular);
                        if (auxserv != null)
                        {
                            SettingPassportExpress spe = _context.SettingPassportExpresses.FirstOrDefault(x => x.AgencyId == agency.AgencyId && x.ServicioConsular == item.ServicioConsular.ToString());
                            costo = auxserv.costo + (item.Express ? (decimal)spe?.Costo : 0);
                        }
                    }
                    utilidad = precio - costo;
                }
                
                List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
                if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    if (!bytransferencia)
                    {
                        bool costIsLower = item.Pays.Any(x => x.PaidValue >= costo);

                        foreach (var pay in item.Pays.OrderByDescending(x => x.PaidValue))
                        {
                            UtilityModel.Pay aux = new UtilityModel.Pay
                            {
                                Costo = 0,
                                Id = pay.Id,
                                PaidValue = pay.PaidValue,
                                TipoPago = pay.TipoPago,
                                TipoPagoId = pay.TipoPagoId,
                                Utility = pay.Utility
                            };
                            if (costIsLower)
                            {
                                if (pay.PaidValue >= costo)
                                {
                                    aux.Utility = pay.PaidValue - costo;
                                    aux.Costo = costo;
                                }
                                else
                                {
                                    aux.Utility = pay.PaidValue;
                                    aux.Costo = 0;
                                }
                            }
                            else
                            {
                                decimal porcientoDelTotal = (pay.PaidValue * 100) / precio;
                                decimal valorPorcientoEnCosto = (porcientoDelTotal * costo) / 100;
                                aux.Utility = pay.PaidValue - valorPorcientoEnCosto;
                                aux.Costo = valorPorcientoEnCosto;
                            }
                            newPays.Add(aux);
                        }
                    }
                    else
                    {
                        UtilityModel.Pay aux = new UtilityModel.Pay
                        {
                            Costo = 0,
                            Id = Guid.Empty,
                            PaidValue = precio,
                            TipoPago = "Pendiente",
                            TipoPagoId = Guid.Empty,
                            Utility = utilidad
                        };
                        newPays.Add(aux);
                    }

                }
                response.Add(new UtilityModel
                {
                    ByTransferencia = bytransferencia,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = costo,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.OrderNumber,
                    SalePrice = precio,
                    Service = STipo.Passport,
                    TransferredAgencyName = item.AgencyTransferida != null ? item.Agency?.Name : "",
                    Utility = utilidad,
                    Date = item.FechaSolicitud,
                    Pays = item.AgencyTransferida.agencyId == null ? newPays.Any() ? newPays : item.Pays : new List<UtilityModel.Pay>(),
                    ServicioConsular = item.ServicioConsular,
                    CServicio = item.OtrosCostos
                });
            }

            return response;
        }
        private async Task<List<UtilityModel>> CubiqUtility(Guid agencyId, DateTime dateIni, DateTime dateEnd, bool onlyCanceled)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var envioscubiq = await _context.OrderCubiqs
            .AsNoTracking()
            .Where(x => x.User.Username != "Manuel14" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Date.Date >= dateIni && x.Date.Date <= dateEnd)
            .Where(x => (onlyCanceled && x.Status == OrderCubiq.STATUS_CANCELADA && ((DateTime)x.CanceledDate).Date <= dateEnd.Date) || (!onlyCanceled && (x.Status != OrderCubiq.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateEnd.Date)))
            .Select(x => new
            {
                x.OrderCubiqId,
                x.Number,
                x.Date,
                x.Amount,
                x.Costo,
                x.costoMayorista,
                x.OtrosCostos,  
                x.HandlingAndTransportation,
                Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                {
                    Id = y.RegistroPagoId,
                    PaidValue = y.valorPagado,
                    TipoPago = y.tipoPago.Type,
                    TipoPagoId = y.tipoPagoId,
                    Costo = 0,
                    Utility = 0
                }).ToList(),
                AgencyTransferida = new
                {
                    agencyId = x.agencyTransferida != null ? (Guid?)x.agencyTransferida.AgencyId : null,
                    Name = x.agencyTransferida != null ? x.agencyTransferida.Name : ""
                },
                Client = new
                {
                    Id = x.Client != null ? (Guid?)x.ClientId : null,
                    FullName = x.Client != null ? x.Client.FullData : "",
                    PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                },
                Employee = new
                {
                    Id = x.User != null ? (Guid?)x.User.UserId : null,
                    FullName = x.User != null ? x.User.FullName : null
                },
            }).ToListAsync();


            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in envioscubiq)
            {
                decimal costo = item.Costo + item.OtrosCostos;
                decimal utilidad = item.Amount - costo;
                decimal precio = item.Amount;

                bool isbytransferencia = false;
                if (item.AgencyTransferida.agencyId != null)
                {
                    if (item.AgencyTransferida.agencyId == agency.AgencyId)
                    {
                        isbytransferencia = true;
                        costo = item.Costo + item.HandlingAndTransportation.CostCubiq + item.OtrosCostos;
                        precio = item.costoMayorista + item.HandlingAndTransportation.Cost + item.OtrosCostos;
                        utilidad = precio - costo;
                    }
                    else
                    {
                        costo = item.costoMayorista + item.HandlingAndTransportation.Cost + item.OtrosCostos;
                        utilidad = item.Amount - costo;
                        precio = item.Amount;
                    }
                }

                List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
                if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    if (!isbytransferencia)
                    {
                        bool costIsLower = item.Pays.Any(x => x.PaidValue >= costo);

                        foreach (var pay in item.Pays.OrderByDescending(x => x.PaidValue))
                        {
                            UtilityModel.Pay aux = new UtilityModel.Pay
                            {
                                Costo = 0,
                                Id = pay.Id,
                                PaidValue = pay.PaidValue,
                                TipoPago = pay.TipoPago,
                                TipoPagoId = pay.TipoPagoId,
                                Utility = pay.Utility
                            };
                            if (costIsLower)
                            {
                                if (pay.PaidValue >= costo)
                                {
                                    aux.Utility = pay.PaidValue - costo;
                                    aux.Costo = costo;
                                }
                                else
                                {
                                    aux.Utility = pay.PaidValue;
                                    aux.Costo = 0;
                                }
                            }
                            else
                            {
                                decimal porcientoDelTotal = (pay.PaidValue * 100) / precio;
                                decimal valorPorcientoEnCosto = (porcientoDelTotal * costo) / 100;
                                aux.Utility = pay.PaidValue - valorPorcientoEnCosto;
                                aux.Costo = valorPorcientoEnCosto;
                            }
                            newPays.Add(aux);
                        }
                    }
                    else
                    {
                        UtilityModel.Pay aux = new UtilityModel.Pay
                        {
                            Costo = 0,
                            Id = Guid.Empty,
                            PaidValue = precio,
                            TipoPago = "Pendiente",
                            TipoPagoId = Guid.Empty,
                            Utility = utilidad
                        };
                        newPays.Add(aux);
                    }

                }

                response.Add(new UtilityModel
                {
                    ByTransferencia = isbytransferencia,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = costo,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.Number,
                    SalePrice = precio,
                    Service = STipo.Cubiq,
                    TransferredAgencyName = item.AgencyTransferida != null ? agency?.Name : "",
                    Utility = utilidad,
                    Date = item.Date,
                    Pays = newPays.Any() ? newPays : item.Pays.ToList(),
                    CServicio = item.OtrosCostos
                });
            }

            return response;
        }
        public async Task<List<UtilityModel>> PassportUtilityByOrderNumber(Guid agencyId, string firstNumber, string secondNumber)
        {
            BigInteger number1 = -1;
            BigInteger number2 = -1;

            if (!BigInteger.TryParse(firstNumber.Replace("DC", "").Replace("PS", ""), out number1) || !BigInteger.TryParse(secondNumber.Replace("DC", "").Replace("PS", ""), out number2))
            {
                return new List<UtilityModel>();
            }
            var agency = await _context.Agency.FindAsync(agencyId);
            var pasaportes = await _context.Passport
                        .AsNoTracking()
                        .Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId))
                        .Where(x => x.getNumberToValue() != -1 && x.getNumberToValue() >= number1 && x.getNumberToValue() <= number2)
                        .Select(x => new
                        {
                            x.PassportId,
                            x.Agency,
                            x.OrderNumber,
                            x.FechaSolicitud,
                            x.Total,
                            x.costo,
                            x.OtrosCostos,
                            x.ServicioConsular,
                            x.PassportExpress,
                            x.Express,
                            Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                            {
                                Id = y.RegistroPagoId,
                                PaidValue = y.valorPagado,
                                TipoPago = y.tipoPago.Type,
                                TipoPagoId = y.tipoPagoId
                            }),
                            AgencyTransferida = new
                            {
                                agencyId = x.AgencyTransferida != null ? (Guid?)x.AgencyTransferida.AgencyId : null,
                                Name = x.AgencyTransferida != null ? x.AgencyTransferida.Name : ""
                            },
                            Client = new
                            {
                                Id = x.Client != null ? (Guid?)x.ClientId : null,
                                FullName = x.Client != null ? x.Client.FullData : "",
                                PhoneNumber = x.Client != null ? x.Client.Phone != null ? x.Client.Phone.Number : "" : ""
                            },
                            Employee = new
                            {
                                Id = x.User != null ? (Guid?)x.User.UserId : null,
                                FullName = x.User != null ? x.User.FullName : null
                            },
                        })
                        .ToListAsync();

            if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
            {
                pasaportes = pasaportes.OrderBy(x => x.OrderNumber).ToList();
            }

            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in pasaportes)
            {
                decimal costo = 0;
                if (agency.AgencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    costo = item.costo + (item.Express ? (decimal)item.PassportExpress?.Costo : 0);
                }
                else
                {
                    costo = item.costo + item.OtrosCostos + (item.Express ? (decimal)item.PassportExpress?.Costo : 0);
                }
                decimal utilidad = item.Total - costo;
                decimal precio = item.Total;
                bool bytransferencia = false;
                if (item.AgencyTransferida.agencyId != null)
                {
                    if (item.AgencyTransferida.agencyId == agency.AgencyId)
                    {
                        bytransferencia = true;
                        precio = item.costo + item.OtrosCostos + (item.Express ? (decimal)item.PassportExpress?.Costo : 0);
                        costo = 0;
                        var wholesaler = _context.Wholesalers.Include(x => x.ServConsularMayoristas).FirstOrDefault(x => x.EsVisible && x.AgencyId == agency.AgencyId && x.Category.category == "Pasaporte");
                        if (wholesaler != null)
                        {
                            ServConsularMayorista auxserv = wholesaler.ServConsularMayoristas.FirstOrDefault(x => x.servicio == item.ServicioConsular);
                            if (auxserv != null)
                            {
                                SettingPassportExpress spe = _context.SettingPassportExpresses.FirstOrDefault(x => x.AgencyId == agency.AgencyId && x.ServicioConsular == item.ServicioConsular.ToString());
                                costo = auxserv.costo + (item.Express ? (decimal)spe?.Costo : 0);
                            }
                        }
                        utilidad = precio - costo - item.OtrosCostos;
                    }
                }
                List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
                if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    if (!bytransferencia)
                    {
                        bool costIsLower = item.Pays.Any(x => x.PaidValue >= costo);

                        foreach (var pay in item.Pays.OrderByDescending(x => x.PaidValue))
                        {
                            UtilityModel.Pay aux = new UtilityModel.Pay
                            {
                                Costo = 0,
                                Id = pay.Id,
                                PaidValue = pay.PaidValue,
                                TipoPago = pay.TipoPago,
                                TipoPagoId = pay.TipoPagoId,
                                Utility = pay.Utility
                            };
                            if (costIsLower)
                            {
                                if (pay.PaidValue >= costo)
                                {
                                    aux.Utility = pay.PaidValue - costo;
                                    aux.Costo = costo;
                                }
                                else
                                {
                                    aux.Utility = pay.PaidValue;
                                    aux.Costo = 0;
                                }
                            }
                            else
                            {
                                decimal porcientoDelTotal = (pay.PaidValue * 100) / precio;
                                decimal valorPorcientoEnCosto = (porcientoDelTotal * costo) / 100;
                                aux.Utility = pay.PaidValue - valorPorcientoEnCosto;
                                aux.Costo = valorPorcientoEnCosto;
                            }
                            newPays.Add(aux);
                        }
                    }
                    else
                    {
                        UtilityModel.Pay aux = new UtilityModel.Pay
                        {
                            Costo = 0,
                            Id = Guid.Empty,
                            PaidValue = precio,
                            TipoPago = "Pendiente",
                            TipoPagoId = Guid.Empty,
                            Utility = utilidad
                        };
                        newPays.Add(aux);
                    }

                }
                response.Add(new UtilityModel
                {
                    ByTransferencia = bytransferencia,
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = costo,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.OrderNumber,
                    SalePrice = precio,
                    Service = STipo.Passport,
                    TransferredAgencyName = item.AgencyTransferida != null ? item.Agency?.Name : "",
                    Utility = utilidad,
                    Date = item.FechaSolicitud,
                    Pays = item.AgencyTransferida.agencyId == null ? newPays.Any() ? newPays : item.Pays.ToList() : new List<UtilityModel.Pay>(),
                    ServicioConsular = item.ServicioConsular,
                    CServicio = item.OtrosCostos
                });
            }

            return response;
        }
        public async Task<List<UtilityModel>> ServicioUtilityByOrderNumber(Guid agencyId, string firstNumber, string secondNumber)
        {
            BigInteger number1 = -1;
            BigInteger number2 = -1;

            if (!BigInteger.TryParse(firstNumber.Replace("OS", ""), out number1) || !BigInteger.TryParse(secondNumber.Replace("OS", ""), out number2))
            {
                return new List<UtilityModel>();
            }
            var agency = await _context.Agency.FindAsync(agencyId);
            var servicios = await _context.Servicios
                .AsNoTracking().Where(x => x.estado != "Cancelado" && x.agency.AgencyId == agencyId)
                .Where(x => x.getNumberToValue() != -1 && x.getNumberToValue() >= number1 && x.getNumberToValue() <= number2)
                .Select(x => new
                {
                    x.ServicioId,
                    x.agency,
                    x.numero,
                    x.fecha,
                    x.importeTotal,
                    x.costoMayorista,
                    Pays = x.RegistroPagos.Select(y => new UtilityModel.Pay
                    {
                        Id = y.RegistroPagoId,
                        PaidValue = y.valorPagado,
                        TipoPago = y.tipoPago.Type,
                        TipoPagoId = y.tipoPagoId
                    }),
                    TipoServicio = x.tipoServicio.Nombre,
                    AgencyTransferida = new
                    {
                        AgencyId = (Guid?)null,
                        Name = ""
                    },
                    Client = new
                    {
                        Id = x.cliente != null ? (Guid?)x.cliente.ClientId : null,
                        FullName = x.cliente != null ? x.cliente.FullData : "",
                        PhoneNumber = x.cliente != null ? x.cliente.Phone != null ? x.cliente.Phone.Number : "" : ""
                    },
                    Employee = new
                    {
                        Id = x.User != null ? (Guid?)x.User.UserId : null,
                        FullName = x.User != null ? x.User.FullName : null
                    },
                })
                .ToListAsync();
            List<UtilityModel> response = new List<UtilityModel>();
            foreach (var item in servicios)
            {
                decimal costo = item.costoMayorista;
                decimal utilidad = item.importeTotal - item.costoMayorista;
                decimal precio = item.importeTotal;

                List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
                if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                {
                    bool costIsLower = item.Pays.Any(x => x.PaidValue >= costo);

                    foreach (var pay in item.Pays.OrderByDescending(x => x.PaidValue))
                    {
                        UtilityModel.Pay aux = new UtilityModel.Pay
                        {
                            Costo = 0,
                            Id = pay.Id,
                            PaidValue = pay.PaidValue,
                            TipoPago = pay.TipoPago,
                            TipoPagoId = pay.TipoPagoId,
                            Utility = pay.Utility
                        };
                        if (costIsLower)
                        {
                            if (pay.PaidValue >= costo)
                            {
                                aux.Utility = pay.PaidValue - costo;
                                aux.Costo = costo;
                            }
                            else
                            {
                                aux.Utility = pay.PaidValue;
                                aux.Costo = 0;
                            }
                        }
                        else
                        {
                            decimal porcientoDelTotal = (pay.PaidValue * 100) / precio;
                            decimal valorPorcientoEnCosto = (porcientoDelTotal * costo) / 100;
                            aux.Utility = pay.PaidValue - valorPorcientoEnCosto;
                            aux.Costo = valorPorcientoEnCosto;
                        }
                        newPays.Add(aux);
                    }
                }

                response.Add(new UtilityModel
                {
                    Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                    Cost = costo,
                    ByTransferencia = false,
                    Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                    OrderNumber = item.numero,
                    SalePrice = precio,
                    Service = STipo.Servicio,
                    Utility = utilidad,
                    TransferredAgencyName = item.AgencyTransferida.AgencyId != null ? item.agency?.Name : "",
                    TipoServicio = item.TipoServicio,
                    Date = item.fecha,
                    Pays = newPays.Any() ? newPays : item.AgencyTransferida.AgencyId == null ? item.Pays.ToList() : new List<UtilityModel.Pay>()
                });
            }

            return response;
        }

    }


}