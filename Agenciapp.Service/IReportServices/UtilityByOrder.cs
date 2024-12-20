using Agenciapp.Service.IReportServices.Models;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.IReportServices
{
    public class UtilityByOrder
    {
        private static readonly Guid _IdDCubaDallas = Guid.Parse("4F1DDEF5-0592-46AD-BEEE-3316CB84385B");
        private readonly Guid agencyDCubaId = Guid.Parse("4752B08A-7684-42B3-930D-FF86F496DF2F");
        private readonly databaseContext _context;
        public UtilityByOrder(databaseContext context)
        {
            _context = context;
        }

        public async Task<List<UtilityModel>> GetCanceledDay(Guid agencyId, DateTime dateIni, DateTime dateFin)
        {
            var logs = await _context.Logs
                   .AsNoTracking()
                   .Where(x => x.AgencyId == agencyId && x.Date.Date >= dateIni && x.Date.Date <= dateFin && x.Event == LogEvent.Cancelar)
                   //.Where(x => x.Type == LogType.Orden || x.Type == LogType.Cubiq || x.Type == LogType.Pasaporte || x.Type == LogType.Recarga || x.Type == LogType.Reserva || x.Type == LogType.Servicio || x.Type == LogType.Remesa)
                   .Select(x => new
                   {
                       x.Type,
                       x.Reserva.TicketId,
                       x.Order.OrderId,
                       x.EnvioCaribe.EnvioCaribeId,
                       x.OrderCubic.OrderCubiqId,
                       EnvioMaritimoId = x.EnvioMaritimo.Id,
                       x.Passport.PassportId,
                       x.Rechargue.RechargueId,
                       x.Remittance.RemittanceId,
                       x.Servicio.ServicioId
                   })
                   .ToListAsync();

            List<UtilityModel> response = new List<UtilityModel>();

            foreach (var item in logs)
            {
                switch (item.Type)
                {
                    case LogType.Reserva:
                        response.Add(await TicketUtility(item.TicketId, agencyId));
                        break;
                    case LogType.Combo:
                        response.Add(await ComboUtility(item.OrderId, agencyId));
                        break;
                    case LogType.EnvioCaribe:
                        response.Add(await CaribbeanShippinUtility(item.EnvioCaribeId, agencyId));
                        break;
                    case LogType.Cubiq:
                        response.Add(await CubiqUtility(item.OrderCubiqId, agencyId));
                        break;
                    case LogType.EnvioMaritimo:
                        response.Add(await MaritimeShippingUtility(item.EnvioMaritimoId, agencyId));
                        break;
                    case LogType.Orden:
                        response.Add(await AirShipmentUtility(item.OrderId, agencyId));
                        break;
                    case LogType.Pasaporte:
                        response.Add(await PassportUtility(item.PassportId, agencyId));
                        break;
                    case LogType.Recarga:
                        response.Add(await RechargueUtility(item.RechargueId, agencyId));
                        break;
                    case LogType.Remesa:
                        response.Add(await RemittanceUtility(item.RemittanceId, agencyId));
                        break;
                    case LogType.Servicio:
                        response.Add(await OtherServiceUtility(item.ServicioId, agencyId));
                        break;
                    default:
                        break;
                }
            }

            return response;
        }

        public async Task<UtilityModel> AirShipmentUtility(Guid id, Guid agencyId)
        {
            var item = await _context.Order
                        .AsNoTracking()
                        .Where(x => x.OrderId == id)
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
                        .FirstOrDefaultAsync();


            decimal refcosto = item.costoMayorista + item.costoProductosBodega;
            decimal refutilidad = item.Amount - item.costoMayorista;
            decimal refprecio = item.Amount;
            bool bytransferencia = false;

            if (item.AgencyTransferida.AgencyId != null)
            {
                if (item.AgencyTransferida.AgencyId == agencyId)
                {
                    bytransferencia = true;
                    refcosto = item.costoDeProveedor + item.OtrosCostos;
                    refprecio = item.costoMayorista;
                    refutilidad = refprecio - refcosto;
                }
            }
            if (item.usedCredito)
                refutilidad = 0;

            return new UtilityModel
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
            };
        }

        public async Task<UtilityModel> CaribbeanShippinUtility(Guid id, Guid agencyId)
        {
            var item = await _context.EnvioCaribes
                        .AsNoTracking()
                        .Where(x => x.EnvioCaribeId == id)
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
                        .FirstOrDefaultAsync();

            //Auxiliar para obtener el costo que aplica el mayorista Caribe a Rapid
            var rapid = await _context.Agency.FirstOrDefaultAsync(x => x.Name == "Rapid Multiservice");
            var mayorista = await _context.Wholesalers.Include(x => x.tipoServicioHabana).Include(x => x.tipoServicioRestoProv).FirstOrDefaultAsync(x => x.EsVisible && x.AgencyId == rapid.AgencyId && x.Category.category == "Maritimo-Aereo");

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

            return new UtilityModel
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
            };
        }

        public async Task<UtilityModel> OtherServiceUtility(Guid id, Guid agencyId)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var item = await _context.Servicios
                .AsNoTracking().Where(x => x.ServicioId == id)
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
                .FirstOrDefaultAsync();


            decimal costo = item.costoMayorista;
            decimal utilidad = item.importeTotal - item.costoMayorista;
            decimal precio = item.importeTotal;

            return new UtilityModel
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
            };
        }

        public async Task<UtilityModel> RechargueUtility(Guid id, Guid agencyId)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var item = await _context.Rechargue
                .AsNoTracking().Where(x => x.RechargueId == id)
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
                .FirstOrDefaultAsync();

            decimal costo = item.costoMayorista;
            decimal utilidad = item.Import - item.costoMayorista;
            decimal precio = item.Import;

            return new UtilityModel
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
            };
        }

        public async Task<UtilityModel> RemittanceUtility(Guid id, Guid agencyId)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var item = await _context.Remittance
                        .AsNoTracking().Where(x => x.RemittanceId == id)
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
                        .FirstOrDefaultAsync();

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
            if (!isbytransferencia && agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0") || agency.AgencyId == _IdDCubaDallas)
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

            return new UtilityModel
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
            };
        }

        public async Task<UtilityModel> TicketUtility(Guid id, Guid agencyId)
        {
            var item = await _context.Ticket
                .AsNoTracking().Where(x => x.TicketId == id)
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
                .FirstOrDefaultAsync();


            decimal cost = item.Cost + item.Charges;
            decimal utilidad = item.Total - item.Cost - item.Charges;
            decimal precio = item.Total;

            return new UtilityModel
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

            };
        }

        public async Task<UtilityModel> MaritimeShippingUtility(Guid id, Guid agencyId)
        {
            var item = await _context.EnvioMaritimo
                        .AsNoTracking().Where(x => x.Id == id)
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
                        .FirstOrDefaultAsync();

            return new UtilityModel
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
            };
        }

        public async Task<UtilityModel> PTuristicoUtility(Guid id, Guid agencyId)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var item = await _context.PaquetesTuristicos
                        .AsNoTracking().Where(x => x.PaqueteId == id)
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
                        .FirstOrDefaultAsync();

            decimal cost = item.Costo + item.OtrosCostos;
            decimal utility = item.Amount - item.Costo;
            decimal price = item.Amount;

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

            return new UtilityModel
            {
                Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                Cost = cost,
                ByTransferencia = isbytransferencia,
                Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                OrderNumber = item.Number,
                SalePrice = price,
                Service = STipo.Remesa,
                Utility = utility,
                TransferredAgencyName = "",
                Date = item.Date,
                Pays = newPays.Any() ? newPays : item.Pays.ToList()
            };
        }

        public async Task<UtilityModel> ComboUtility(Guid id, Guid agencyId)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var item = await _context.Order
                        .AsNoTracking().Where(x => x.OrderId == id)
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
                        .FirstOrDefaultAsync();


            decimal refcosto = item.costoMayorista + item.OtrosCostos;
            decimal refutilidad = item.Amount - item.costoMayorista - item.OtrosCostos;
            decimal refprecio = item.Amount;
            bool bytransferencia = false;
            if (item.AgencyTransferida.AgencyId != null)
            {
                if (item.AgencyTransferida.AgencyId == agency.AgencyId)
                {
                    bytransferencia = true;
                    refcosto = item.costoDeProveedor;
                    refprecio = item.costoMayorista + item.OtrosCostos;
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
            if (item.usedCredito)
                refutilidad = 0;

            List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
            if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0") || agencyId == _IdDCubaDallas)
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

            return new UtilityModel
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
            };
        }

        public async Task<UtilityModel> PassportUtility(Guid id, Guid agencyId)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var item = await _context.Passport
                        .AsNoTracking().Where(x => (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId))
                        .AsNoTracking().Where(x => x.PassportId == id)
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
                        .FirstOrDefaultAsync();


            decimal costo = 0;
            if (agency.AgencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0") || agencyId == _IdDCubaDallas)
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
            if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0") || agencyId == _IdDCubaDallas)
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
            return new UtilityModel
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
            };
        }

        public async Task<UtilityModel> CubiqUtility(Guid id, Guid agencyId)
        {
            var agency = await _context.Agency.FindAsync(agencyId);
            var item = await _context.OrderCubiqs
            .AsNoTracking().Where(x => x.OrderCubiqId == id)
            .Select(x => new
            {
                x.OrderCubiqId,
                x.Number,
                x.Date,
                x.Amount,
                x.Costo,
                x.costoMayorista,
                x.OtrosCostos,
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
            }).FirstOrDefaultAsync();

            decimal costo = item.Costo + item.OtrosCostos;
            decimal utilidad = item.Amount - costo;
            decimal precio = item.Amount;

            bool isbytransferencia = false;
            if (item.AgencyTransferida.agencyId != null)
            {
                if (item.AgencyTransferida.agencyId == agency.AgencyId)
                {
                    isbytransferencia = true;
                    costo = item.Costo + item.OtrosCostos;
                    utilidad = item.costoMayorista - item.Costo;
                    precio = item.costoMayorista + item.OtrosCostos;
                }
                else
                {
                    costo = item.costoMayorista + item.OtrosCostos;
                    utilidad = item.Amount - costo;
                    precio = item.Amount;
                }
            }

            List<UtilityModel.Pay> newPays = new List<UtilityModel.Pay>();
            if (agencyId == agencyDCubaId || agencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0") || agencyId == _IdDCubaDallas)
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

            return new UtilityModel
            {
                ByTransferencia = isbytransferencia,
                Client = new UtilityModel.ClientUtility { ClientId = item.Client.Id, FullName = item.Client.FullName, PhoneNumber = item.Client.PhoneNumber },
                Cost = costo,
                Employee = new UtilityModel.EmployeeUtility { EmployeeId = item.Employee.Id, FullName = item.Employee.FullName },
                OrderNumber = item.Number,
                SalePrice = precio,
                Service = STipo.Passport,
                TransferredAgencyName = item.AgencyTransferida != null ? agency?.Name : "",
                Utility = utilidad,
                Date = item.Date,
                Pays = newPays.Any() ? newPays : item.Pays.ToList(),
                CServicio = item.OtrosCostos
            };
        }
    }
}