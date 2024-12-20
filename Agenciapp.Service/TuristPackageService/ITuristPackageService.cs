using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using Agenciapp.Service.IPromoCodeServices;
using Agenciapp.Service.ITicketServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.ITusistPackageService
{
    public interface ITuristPackegeService
    {
        void updatePackage(Guid packageId, Guid agencyId, User user);
    }

    public class TuristPackageService : ITuristPackegeService
    {
        private readonly databaseContext _context;
        private static Guid IdDCubaWashington = Guid.Parse("4752B08A-7684-42B3-930D-FF86F496DF2F");


        public TuristPackageService(databaseContext context)
        {
            _context = context;
        }

        public void updatePackage(Guid paqueteId, Guid agencyId, User user)
        {
            var paquete = _context.PaquetesTuristicos.Include(x => x.Servicios)
                .Include(x => x.Tickets)
                .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                .Include(x => x.Client)
                .Include(x => x.Agency)
                .FirstOrDefault(x => x.PaqueteId == paqueteId);

            var agency = _context.Agency.FirstOrDefault(x => x.AgencyId == agencyId);
            var fee = agency.creditCardFee;

            paquete.Precio = paquete.Servicios.Sum(x => x.importe) + paquete.Tickets.Sum(x => x.Price);
            paquete.Costo = paquete.Servicios.Sum(x => x.costo) + paquete.Tickets.Sum(x => x.Cost);
            paquete.Amount = paquete.Precio + paquete.OtrosCostos - paquete.Tickets.Sum(x => x.Discount);
            foreach (var pago in paquete.Pagos)
            {
                if (pago.tipoPago.Type == "Crédito o Débito")
                {
                    var x = pago.valorPagado / (1 + fee / 100);
                    var feepagado = x * fee / 100;
                    paquete.Amount += feepagado;
                }
            }
            if(paquete.Amount < paquete.ValorPagado)
            {
                throw new Exception("Balance negativo");
            }

            var sxc1 = _context.servicioxCobrar
                         .Include(x => x.mayorista)
                         .Include(x => x.cliente)
                         .Where(x => x.ServicioId == paquete.PaqueteId && x.mayorista == paquete.Agency && x.cliente != null).FirstOrDefault();


            if (sxc1 != null && sxc1.cobrado != 0)
                    throw new Exception("El paquete está facturado");

            if (paquete.Balance == 0)
            {
                if (sxc1 != null)
                    _context.servicioxCobrar.Remove(sxc1);

                paquete.Status = PaqueteTuristico.STATUS_COMPLETADA;
                foreach (var item in paquete.Tickets)
                {
                    item.Payment = item.Total;
                    item.Debit = 0;
                    item.SetStatus(Ticket.STATUS_COMPLETADA, user);
                    var sxc = _context.servicioxCobrar
                         .Include(x => x.mayorista)
                         .Include(x => x.cliente)
                         .Where(x => x.cobrado == 0 && x.ServicioId == item.TicketId && x.mayorista == agency && x.cliente != null).FirstOrDefault();

                    if (sxc != null)
                    {
                        _context.servicioxCobrar.Remove(sxc);
                    }
                    _context.Ticket.Update(item);
                }
                foreach (var item in paquete.Servicios)
                {
                    item.importePagado = item.importeTotal;
                    item.debe = 0;
                    if (agency.AgencyId != IdDCubaWashington)
                        item.estado = Servicio.EstadoCompletado;
                    else
                        item.estado = Servicio.EstadoPendiente;
                    var sxc = _context.servicioxCobrar
                        .Include(x => x.mayorista)
                        .Include(x => x.cliente)
                        .Where(x => x.cobrado == 0 && x.ServicioId == item.ServicioId && x.mayorista == agency && x.cliente != null).FirstOrDefault();

                    if (sxc != null)
                    {
                        _context.servicioxCobrar.Remove(sxc);
                    }
                    _context.Servicios.Update(item);
                }
            }
            else
            {
                paquete.Status = PaqueteTuristico.STATUS_PENDIENTE;
                if(sxc1 != null)
                {
                    sxc1.importeACobrar = paquete.Balance;
                    sxc1.valorTramite = paquete.Amount;
                    _context.servicioxCobrar.Update(sxc1);
                }
                else
                {
                    sxc1 = new servicioxCobrar
                    {
                        date = DateTime.UtcNow,
                        ServicioId = paquete.PaqueteId,
                        tramite = "Paquete Turistico",
                        NoServicio = paquete.Number,
                        cliente = paquete.Client,
                        No_servicioxCobrar = "PAYT" + paquete.Date.ToString("yyyyMMddHHmmss"),
                        cobrado = 0,
                        remitente = paquete.Client,
                        destinatario = null,
                        valorTramite = paquete.Amount,
                        importeACobrar = paquete.Balance,
                        mayorista = paquete.Agency,
                    };
                    _context.Add(sxc1);
                }
                foreach (var item in paquete.Tickets)
                {
                    item.Payment = 0;
                    item.Debit = item.Total;
                    item.SetStatus(Ticket.STATUS_PENDIENTE, user);
                    /*var sxc = _context.servicioxCobrar
                         .Include(x => x.mayorista)
                         .Include(x => x.cliente)
                         .Where(x => x.cobrado == 0 && x.ServicioId == item.TicketId && x.mayorista == agency && x.cliente != null).FirstOrDefault();

                    if (sxc == null)
                    {
                        _context.servicioxCobrar.Add(new servicioxCobrar()
                        {
                            date = DateTime.UtcNow,
                            ServicioId = item.TicketId,
                            tramite = "Reserva",
                            NoServicio = item.ReservationNumber,
                            cliente = paquete.Client,
                            No_servicioxCobrar = "PAYT" + item.RegisterDate.ToString("yyyyMMddHHmmss"),
                            cobrado = 0,
                            remitente = paquete.Client,
                            destinatario = null,
                            valorTramite = item.Total,
                            importeACobrar = item.Debit,
                            mayorista = agency,
                        });
                    }
                    else
                    {
                        sxc.importeACobrar = item.Debit;
                        sxc.valorTramite = item.Total;
                        _context.servicioxCobrar.Update(sxc);
                    }*/
                    _context.Ticket.Update(item);
                }
                foreach (var item in paquete.Servicios)
                {
                    item.importePagado = 0;
                    item.debe = item.importeTotal;
                    item.estado = Servicio.EstadoPendiente;

                    /*var sxc = _context.servicioxCobrar
                        .Include(x => x.mayorista)
                        .Include(x => x.cliente)
                        .Where(x => x.cobrado == 0 && x.ServicioId == item.ServicioId && x.mayorista == agency && x.cliente != null).FirstOrDefault();

                    if (sxc == null)
                    {
                        _context.servicioxCobrar.Add(new servicioxCobrar()
                        {
                            date = DateTime.UtcNow,
                            ServicioId = item.ServicioId,
                            tramite = "Servicio",
                            NoServicio = item.numero,
                            cliente = item.cliente,
                            No_servicioxCobrar = "PAYT" + item.fecha.ToString("yyyyMMddHHmmss"),
                            cobrado = 0,
                            remitente = item.cliente,
                            destinatario = null,
                            valorTramite = item.importeTotal,
                            importeACobrar = item.debe,
                            mayorista = agency,
                        });
                    }
                    else
                    {
                        sxc.importeACobrar = item.debe;
                        sxc.valorTramite = item.importeTotal;
                        _context.servicioxCobrar.Update(sxc);
                    }*/
                    _context.Servicios.Update(item);
                }

            }
            _context.PaquetesTuristicos.Update(paquete);
            _context.SaveChanges();
        }
    }
}
