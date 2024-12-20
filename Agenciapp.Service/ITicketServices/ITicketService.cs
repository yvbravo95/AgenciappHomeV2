using Agenciapp.Domain.Models;
using Agenciapp.Service.IPromoCodeServices;
using Agenciapp.Service.ITicketServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.ITicketServices
{
    public interface ITicketService
    {
        Task<Result<Ticket>> CreatePasaje(CreatePasajeModel model);
        Task<Ticket> CreateHotel(CreateHotelModel model);
    }

    public class TicketService : ITicketService
    {
        private readonly databaseContext _context;
        private readonly IPromoCodeService _promoCodeService;

        public TicketService(databaseContext context, IPromoCodeService promoCodeService)
        {
            _context = context;
            _promoCodeService = promoCodeService;
        }

        public async Task<Result<Ticket>> CreatePasaje(CreatePasajeModel model)
        {
            var user = await _context.User.FindAsync(model.UserId);
            if (user == null) return Result.Failure<Ticket>("El usuario no existe");
            var agency = await _context.Agency.FindAsync(model.AgencyId);
            if (agency == null) return Result.Failure<Ticket>("La agencia no existe");
            if (model.Pasajeros == null || model.Pasajeros.Any() == false) return Result.Failure<Ticket>("La reserva debe tener al menos un pasajero.");
            var office = _context.Office.FirstOrDefault(x => x.AgencyId == agency.AgencyId);

            var wholesaler = await _context.Wholesalers.FindAsync(model.IdWholesaler);

            var client = await _context.Client.FindAsync(model.ClientId);
            if (client == null) return Result.Failure<Ticket>("El cliente no existe");

            PromoCode code = null;
            if (model.PromoCode != null)
            {
                var promoCode = await _promoCodeService.GetAvailableCode(model.PromoCode, agency.AgencyId);
                if (promoCode.IsFailure)
                {
                    return Result.Failure<Ticket>(promoCode.Error);
                }
                else if (promoCode.Value?.OrderType != Agenciapp.Domain.Enums.OrderType.ReservaPasaje)
                {
                    return Result.Failure<Ticket>("El código no puede ser usado en este trámite.");
                }
                else
                {
                    code = promoCode.Value;
                }
            }
            var ticket = new Ticket
            {
                TicketId = Guid.NewGuid(),
                DiscountCode = code != null ? new Discount
                {
                    Id = Guid.NewGuid(),
                    Type = code.PromoType,
                    Value = code.Value
                } : null,
                User = user,
                ReservationNumber = model.ReservationNumber != null ? model.ReservationNumber : "RP" + DateTime.Now.ToString("yMMddHHmmssff"),
                Category = model.Category,
                type = "pasaje",
                Agency = agency,
                AmountPassengers = model.Pasajeros.Count,
                TicketBy = model.Pnr ?? "",
                HoraSalida = model.DepartureDate,
                Flight = model.DepartureNumberFlight,
                NoVueloRegreso = model.ReturnNumberFlight,
                HoraRegreso = model.ReturnDate == null ? DateTime.Now : (DateTime)model.ReturnDate,
                DateIn = model.ReturnDate == null ? DateTime.Now : (DateTime)model.ReturnDate,
                DateOut = model.DepartureDate,
                Pasajeros = model.Pasajeros,
                IdWholesaler = wholesaler?.IdWholesaler,
                Description = model.Description,
                ProvinciaReferencia = model.ProvinciaReferencia,
                MunicipioReferencia = model.MunicipioReferencia,
                TelefonoReferencia = model.TelefonoReferencia,
                OfficeId = office.OfficeId,
                ClientId = model.ClientId,
                Charges = model.Charges,
                Cost = model.Cost,
                Discount = model.Discount,
                Payment = model.Pays == null ? 0 : model.Pays.Sum(x => x.ValorPagado),
                Price = model.Price,
                Total = model.Total,
                Fee = model.Fee,
                RegisterDate = DateTime.Now,
                CreatedDate = model.CreatedDate != null ? (DateTime)model.CreatedDate : DateTime.Now,
                TypePayment = "Cash",
                RegistroPagos = new System.Collections.Generic.List<RegistroPago>(),
                ClientIsCarrier = model.ClientIsCarrier,
                MerchantTransactionId = model.MerchantTransactionId,
                CantidadAdultos = model.Adults,
                CantidadaMenores = model.Childs,
                CantidadInfantes = model.Infants,
                DepartureTravelClass = model.DepartureTravelClass,
                ReturnTRavelClass = model.ReturnTravelClass,
                DepartureCity = model.DepartureCity,
                DestinationCity = model.DestinationCity,
                ReturnFromCity = model.ReturnFromCity,
                ReturnToCity = model.ReturnToCity,
                Checked = "",
                Door = "",
                IsCharter = model.IsCharter,
                NombreCharter = model.NombreCharter,
                NombreAerolinea = model.NombreAerolinea,
                IsMovileApp = model.IsMovileApp,
                ReferenceContactName = model.ReferenceContactName,
                ReferenceContactPhone = model.ReferenceContactPhone,
                CategoriaAuto = "",
                ModeloAuto = "",
                TransmisionAuto = "",

            };
            ticket.Debit = ticket.Total - ticket.Payment;
            _context.Ticket.Add(ticket);

            if (ticket.Debit == 0)
                ticket.SetStatus("Completada", user);
            else
            {
                ticket.SetStatus("Pendiente", user);
                var sxc = new servicioxCobrar
                {
                    date = DateTime.UtcNow,
                    ServicioId = ticket.TicketId,
                    tramite = "Reserva",
                    NoServicio = ticket.ReservationNumber,
                    cliente = client,
                    No_servicioxCobrar = "PAYT" + ticket.RegisterDate.ToString("yyyyMMddHHmmss"),
                    cobrado = 0,
                    remitente = client,
                    destinatario = null,
                    valorTramite = ticket.Total,
                    importeACobrar = ticket.Debit,
                    mayorista = agency,
                };
                _context.Add(sxc);
            }
            int i = 1;
            if (model.Pays != null)
            {
                foreach (var pay in model.Pays)
                {
                    var tipoPago = _context.TipoPago.FirstOrDefault(x => x.TipoPagoId == pay.TipoPago);
                    var rp = new RegistroPago
                    {
                        Client = client,
                        Agency = agency,
                        Office = office,
                        valorPagado = pay.ValorPagado,
                        Ticket = ticket,
                        date = DateTime.UtcNow,
                        tipoPago = tipoPago,
                        number = "PAY" + DateTime.Now.ToString("yMMddHHmmssff") + i,
                        User = user,
                        RegistroPagoId = Guid.NewGuid()
                    };
                    ticket.RegistroPagos.Add(rp);
                    i++;

                    if (tipoPago.Type == "Crédito o Débito")
                    {
                        if (model.authorizationCard != null)
                        {
                            _context.AuthorizationCards.Add(model.authorizationCard);
                            ticket.authorizationCard = model.authorizationCard;
                        }
                        else
                        {
                            Serilog.Log.Information("Authorization Card is null. Create Reservation with payment Crédit or Debit");
                            return Result.Failure<Ticket>("No se ha podido crear la reserva");
                        }
                    }
                }
            }

            if (model.Credito > 0)
            {
                if (model.Credito >= ticket.Total)
                {
                    model.Credito = ticket.Total;
                }
                ticket.Credito = model.Credito;

                //Creo un pago para credito
                RegistroPago pagocredito = new RegistroPago
                {
                    Agency = agency,
                    date = DateTime.UtcNow,
                    number = "PAY" + DateTime.Now.ToString("yyyyMMddHHmmss") + i,
                    Office = office,
                    Ticket = ticket,
                    tipoPagoId = _context.TipoPago.FirstOrDefault(x => x.Type == "Crédito de Consumo").TipoPagoId,
                    User = user,
                    valorPagado = model.Credito,
                    RegistroPagoId = Guid.NewGuid(),
                    ClientId = client.ClientId,
                    nota = "",
                };

                _context.Logs.Add(new Log
                {
                    Date = DateTime.Now,
                    Event = LogEvent.Consumir,
                    Type = LogType.Credito,
                    LogId = Guid.NewGuid(),
                    Message = pagocredito.valorPagado.ToString() + " Factura: " + ticket.ReservationNumber,
                    User = user,
                    Client = client,
                    AgencyId = client.AgencyId,
                    Reserva = ticket
                });
                _context.RegistroPagos.Add(pagocredito);

                foreach (var item in client.Creditos)
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

            if (wholesaler != null && ticket.Cost > 0)
            {
                var porPagar = new ServiciosxPagar
                {
                    Date = DateTime.Now,
                    ImporteAPagar = ticket.Cost,
                    Mayorista = wholesaler,
                    Agency = agency,
                    SId = ticket.TicketId,
                    NoServicio = ticket.ReservationNumber,
                    Tipo = STipo.Reserva,
                    Reserva = ticket,
                    SubTipo = ticket.type,
                };
                _context.ServiciosxPagar.Add(porPagar);
            }

            //Guardo que empleado realizo el tramite
            TramiteEmpleado tramite = new TramiteEmpleado();
            tramite.fecha = DateTime.UtcNow;
            tramite.Id = Guid.NewGuid();
            tramite.IdEmpleado = user.UserId;
            tramite.IdTramite = ticket.TicketId;
            tramite.tipoTramite = TramiteEmpleado.tipo_RESERVA;
            tramite.IdAgency = agency.AgencyId;
            _context.TramiteEmpleado.Add(tramite);

            //Servicio por pagar de agenciapp
            var s = new ServiciosxPagar
            {
                Date = DateTime.Now,
                ImporteAPagar = ticket.Charges,
                Mayorista = null,
                Agency = agency,
                SId = ticket.TicketId,
                NoServicio = ticket.ReservationNumber,
                Reserva = ticket,
                Tipo = STipo.Reserva,
                SubTipo = ticket.type,
            };
            _context.ServiciosxPagar.Add(s);

            if (model.ClientIsCarrier)
            {
                client.IsCarrier = true;
                _context.Client.Update(client);
                if (!_context.Carrier.Any(x => x.ClientId == client.ClientId))
                {
                    var carrier = new Carrier()
                    {
                        Agency = agency,
                        Client = client,
                        Name = client.Name,
                        LastName = client.LastName,
                        Email = client.Email,
                        CreateAt = DateTime.Now,
                        Address = client.Address,
                        CarrierId = Guid.NewGuid()
                    };
                    _context.Carrier.Add(carrier);
                }
            }

            _context.Logs.Add(new Log
            {
                Date = DateTime.Now,
                Event = LogEvent.Crear,
                Type = LogType.Reserva,
                LogId = Guid.NewGuid(),
                Message = ticket.ReservationNumber,
                User = user,
                Client = client,
                Precio = ticket.Total.ToString(),
                Pagado = ticket.RegistroPagos.Sum(x => x.valorPagado).ToString(),
                AgencyId = ticket.AgencyId,
                Reserva = ticket
            });
            await _context.SaveChangesAsync();
            return Result.Success<Ticket>(ticket);
        }

        public async Task<Ticket> CreateHotel(CreateHotelModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    string number = string.IsNullOrEmpty(model.Number) ? "RH" + DateTime.Now.ToString("yMMddHHmmssff"): model.Number;
                    bool exist = _context.Ticket.Any(x => x.ReservationNumber == number);
                    if (exist) throw new Exception("El número de reserva ya existe");

                    var user = await _context.User.FindAsync(model.UserId);
                    if (user == null) throw new Exception("El usuario no existe");
                    var agency = await _context.Agency.FindAsync(user.AgencyId);
                    if (agency == null) throw new Exception("La agencia no existe");
                    var office = await _context.Office.FindAsync(model.OfficeId);
                    if (office == null) throw new Exception("La oficina no existe");

                    Wholesaler wholesaler = null;
                    if (model.WholesalerId != null)
                    {
                        wholesaler = await _context.Wholesalers.FindAsync(model.WholesalerId);
                        if (wholesaler == null) throw new Exception("El mayorista no existe");
                    }

                    var client = await _context.Client.Include(c => c.Creditos).Include(x => x.Address)
                    .FirstOrDefaultAsync(x => x.ClientId == model.ClientId && x.AgencyId == agency.AgencyId);

                    Ticket ticket = new Ticket
                    { 
                        TicketId = Guid.NewGuid(),
                        CreatedDate = DateTime.Now,
                        RegisterDate = DateTime.Now,
                        Client = client,
                        Agency = agency,
                        OfficeId = model.OfficeId,
                        User = user,
                        ReservationNumber = number,
                        type = "hotel",
                        DateIn = model.DateIn,
                        DateOut = model.DateOut,
                        TicketBy = model.ReferenceNumber,
                        Wholesaler = wholesaler,
                        Category = "idavuelta",
                        Description = model.Description,
                        CantidadAdultos = model.Adults,
                        CantidadaMenores = model.Children,
                        CantidadInfantes = model.Infants,
                        CantidadHabitaciones = model.Rooms,
                        PaqueteTuristicoId = model.PaqueteId,
                        Checked = string.Empty,
                        Charges = model.Charges,
                        Commission = model.Commission,
                        Cost = model.Cost,
                        Discount = model.Discount,
                        Price = model.Price,
                        Total = model.Total,
                        Debit = model.Total,
                        ClientIsCarrier = client.IsCarrier,
                        Door = string.Empty,
                        TypePayment = string.Empty
                    };
                    _context.Ticket.Add(ticket);

                    #region Pays
                    if (model.Pays != null)
                    {
                        var paymentTypes = _context.TipoPago.ToList();
                        for (int i = 0; i < model.Pays.Count; i++)
                        {
                            PayTicket payDto = model.Pays[i];
                            var paymentType = paymentTypes.FirstOrDefault(x => x.Type == payDto.Type) ?? throw new Exception("El tipo de pago no existe");
                            var pay = new RegistroPago
                            {
                                valorPagado = payDto.Amount,
                                tipoPago = paymentType,
                                date = DateTime.UtcNow,
                                User = user,
                                Agency = agency,
                                Office = office,
                                number = "PAY" + DateTime.Now.ToString("yMMddHHmmssff" + i),
                                RegistroPagoId = Guid.NewGuid()
                            };
                            ticket.RegistroPagos.Add(pay);
                            ticket.Debit -= payDto.Amount;
                            ticket.Payment += payDto.Amount;
                            if (paymentType.Type == "Crédito de Consumo")
                            {
                                ticket.Credito += pay.valorPagado;
                                _context.Logs.Add(new Log
                                {
                                    Date = DateTime.Now,
                                    Event = LogEvent.Consumir,
                                    Type = LogType.Credito,
                                    LogId = Guid.NewGuid(),
                                    Message = pay.valorPagado.ToString() + " Factura: " + ticket.ReservationNumber,
                                    User = user,
                                    Client = ticket.Client,
                                    Agency = agency,
                                    Reserva = ticket
                                });
                            }

                        }

                        if (ticket.Debit < 0) throw new Exception("El monto pagado es mayor al total");
                    }
                    #endregion
                    #region Status
                    if (ticket.Debit == 0 || ticket.type == "auto")
                    {
                        ticket.SetStatus(Ticket.STATUS_COMPLETADA, user);
                    }
                    else
                    {
                        ticket.SetStatus(Ticket.STATUS_PENDIENTE, user);

                        if (ticket.PaqueteTuristicoId == null)
                        {
                            var sxc = new servicioxCobrar
                            {
                                date = DateTime.UtcNow,
                                ServicioId = ticket.TicketId,
                                tramite = "Reserva",
                                NoServicio = ticket.ReservationNumber,
                                cliente = ticket.Client,
                                No_servicioxCobrar = "PAYT" + ticket.RegisterDate.ToString("yyyyMMddHHmmss"),
                                cobrado = 0,
                                remitente = ticket.Client,
                                destinatario = null,
                                valorTramite = ticket.Total,
                                importeACobrar = ticket.Debit,
                                mayorista = agency,
                                MinoristaTramite = ticket.Minorista
                            };
                            _context.Add(sxc);
                        }
                    }
                    #endregion
                    #region Credit
                    if (ticket.Credito > 0)
                    {
                        decimal credito = ticket.Credito;
                        if (client.Creditos.Sum(x => x.value) < credito) throw new Exception("El cliente no tiene suficiente crédito");

                        foreach (var item in client.Creditos)
                        {
                            if (item.value > credito)
                            {
                                item.value -= credito;
                                _context.Credito.Update(item);
                                break;
                            }
                            else
                            {
                                credito -= item.value;
                                _context.Credito.Remove(item);
                            }
                        }
                    }
                    #endregion

                    #region Cuentas por pagar y por cobrar
                    if (ticket.Wholesaler != null)
                    {
                        if (ticket.Wholesaler.byTransferencia)
                        {
                            var transferencia = _context.CostoxModuloMayorista.Include(x => x.modAsignados).Where(x => x.AgencyId == ticket.Agency.AgencyId && x.modAsignados.Where(y => y.IdWholesaler == ticket.Wholesaler.IdWholesaler).Any()).FirstOrDefault();
                            if (transferencia == null) throw new Exception("No se ha asignado un módulo de transferencia para el mayorista");
                            ticket.AgencyTransferida = _context.Agency.Find(transferencia.AgencyMayoristaId);

                            // servicio por cobrar del mayorista
                            _context.servicioxCobrar.Add(new servicioxCobrar
                            {
                                date = DateTime.UtcNow,
                                ServicioId = ticket.TicketId,
                                tramite = "Reserva",
                                NoServicio = ticket.ReservationNumber,
                                remitente = client,
                                No_servicioxCobrar = "PAYT" + ticket.RegisterDate.ToString("yyyyMMddHHmmss"),
                                cobrado = 0,
                                minorista = ticket.Agency,
                                destinatario = null,
                                valorTramite = ticket.Total,
                                importeACobrar = ticket.Cost + ticket.Charges,
                                mayorista = ticket.AgencyTransferida,
                            });

                            //servicio por pagar al mayorista
                            var porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = ticket.Cost + ticket.Charges,
                                Mayorista = _context.Wholesalers.Find(ticket.IdWholesaler),
                                Agency = ticket.Agency,
                                SId = ticket.TicketId,
                                NoServicio = ticket.ReservationNumber,
                                Tipo = STipo.Reserva,
                                Reserva = ticket,
                                SubTipo = ticket.type,
                            };
                            _context.ServiciosxPagar.Add(porPagar);

                            //Servicio por pagar de agenciapp
                            var s = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = ticket.Charges,
                                Mayorista = null,
                                Agency = ticket.AgencyTransferida,
                                SId = ticket.TicketId,
                                NoServicio = ticket.ReservationNumber,
                                Reserva = ticket,
                                Tipo = STipo.Reserva,
                                SubTipo = ticket.type,
                            };
                            _context.ServiciosxPagar.Add(s);

                            // servicio por pagar a rentadora
                            if (ticket.Rentadora != null)
                            {
                                var rentadora = _context.Rentadora.FirstOrDefault(x => x.RentadoraId == ticket.Rentadora.RentadoraId);

                                var porPagarR = new ServiciosxPagar
                                {
                                    Date = DateTime.Now,
                                    ImporteAPagar = ticket.CostoMayorista,
                                    Mayorista = _context.Wholesalers.FirstOrDefault(x => x.IdWholesaler == rentadora.WholesalerId),
                                    SId = ticket.TicketId,
                                    NoServicio = ticket.ReservationNumber,
                                    Tipo = STipo.Reserva,
                                    Reserva = ticket,
                                    SubTipo = ticket.type,
                                    Agency = ticket.AgencyTransferida
                                };
                                _context.ServiciosxPagar.Add(porPagarR);
                            }
                        }
                        else
                        {
                            var porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = ticket.Cost,
                                Mayorista = ticket.Wholesaler,
                                Agency = ticket.Agency,
                                SId = ticket.TicketId,
                                NoServicio = ticket.ReservationNumber,
                                Tipo = STipo.Reserva,
                                Reserva = ticket,
                                SubTipo = ticket.type,
                            };
                            _context.ServiciosxPagar.Add(porPagar);

                            //Servicio por pagar de agenciapp
                            var s = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = ticket.Charges,
                                Mayorista = null,
                                Agency = ticket.Agency,
                                SId = ticket.TicketId,
                                NoServicio = ticket.ReservationNumber,
                                Reserva = ticket,
                                Tipo = STipo.Reserva,
                                SubTipo = ticket.type,
                            };
                            _context.ServiciosxPagar.Add(s);
                        }
                    }

                    #endregion

                    //Guardo que empleado realizo el tramite
                    TramiteEmpleado tramite = new TramiteEmpleado
                    {
                        fecha = DateTime.UtcNow,
                        Id = Guid.NewGuid(),
                        IdEmpleado = user.UserId,
                        IdTramite = ticket.TicketId,
                        tipoTramite = TramiteEmpleado.tipo_RESERVA,
                        IdAgency = agency.AgencyId
                    };
                    _context.TramiteEmpleado.Add(tramite);

                    _context.Logs.Add(new Log
                    {
                        Date = DateTime.Now,
                        Event = LogEvent.Crear,
                        Type = LogType.Reserva,
                        LogId = Guid.NewGuid(),
                        Message = ticket.ReservationNumber,
                        User = user,
                        Client = client,
                        Precio = ticket.Total.ToString(),
                        Pagado = ticket.RegistroPagos.Sum(x => x.valorPagado).ToString(),
                        AgencyId = ticket.Agency.AgencyId,
                        Reserva = ticket
                    });

                    await _context.SaveChangesAsync();

                    transaction.Commit();

                    return ticket;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }
    }
}
