using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public static class PagosTramites
    {
        public static async Task<List<ItemPago>> getPagosTramites(databaseContext _context,Guid agencyId,DateTime dateIni, DateTime dateFin)
        {
            List<ItemPago> response = new List<ItemPago>();
            var registro = await _context.RegistroPagos
                    .Include(x => x.User)
                    .Include(x => x.tipoPago)
                    .Include(x => x.EnvioCaribe).ThenInclude(x => x.Client)
                    .Include(x => x.EnvioMaritimo).ThenInclude(x => x.Client)
                    .Include(x => x.Order).ThenInclude(x => x.Client)
                    .Include(x => x.Order).ThenInclude(x => x.Client)
                    .Include(x => x.OrderCubiq).ThenInclude(x => x.Client)
                    .Include(x => x.Passport).ThenInclude(x => x.Client)
                    .Include(x => x.Rechargue).ThenInclude(x => x.Client)
                    .Include(x => x.Servicio).ThenInclude(x => x.cliente)
                    .Include(x => x.Servicio).ThenInclude(x => x.tipoServicio)
                    .Include(x => x.Ticket).ThenInclude(x => x.Client)
                    .Include(x => x.Remittance).ThenInclude(x => x.Client)
                    .Where(x => x.AgencyId == agencyId && x.date.ToLocalTime().Date >= dateIni.Date && x.date.ToLocalTime().Date <= dateFin.Date && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                    .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada")
                    .ToListAsync();
            foreach (var item in registro)
            {
                if (item.EnvioCaribe != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.EnvioCaribe.Balance,
                        cliente = item.EnvioCaribe.Client,
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.EnvioCaribeId,
                        tipoPago = item.tipoPago,
                        TipoTramite = TipoTramite.EnvioCaribe,
                        Total = item.EnvioCaribe.Amount,
                        User = item.User,
                        FechaTramite = item.EnvioCaribe.Date,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.EnvioCaribe.Status,
                        Numero = item.EnvioCaribe.Number
                    });
                }
                else if(item.EnvioMaritimo != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.EnvioMaritimo.Balance,
                        cliente = item.EnvioMaritimo.Client,
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.EnvioMaritimoId,
                        tipoPago = item.tipoPago,
                        TipoTramite = TipoTramite.EnvioMaritimo,
                        Total = item.EnvioMaritimo.Amount,
                        User = item.User,
                        FechaTramite = item.EnvioMaritimo.Date,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.EnvioMaritimo.Status,
                        Numero = item.EnvioMaritimo.Number
                    });
                }
                else if (item.Order != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.Order.Balance,
                        cliente = item.Order.Client,
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.OrderId,
                        tipoPago = item.tipoPago,
                        TipoTramite = item.Order.Type == "Remesas"?TipoTramite.Remesa:TipoTramite.EnvioAereo,
                        Total = item.Order.Amount,
                        User = item.User,
                        FechaTramite = item.Order.Date,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.Order.Status,
                        Numero = item.Order.Number
                    });
                }
                else if (item.Remittance != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.Remittance.Balance,
                        cliente = item.Remittance.Client,
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.RemittanceId,
                        tipoPago = item.tipoPago,
                        TipoTramite = TipoTramite.Remesa,
                        Total = item.Remittance.Amount,
                        User = item.User,
                        FechaTramite = item.Remittance.Date,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.Remittance.Status,
                        Numero = item.Remittance.Number
                    });
                }
                else if(item.OrderCubiq != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.OrderCubiq.Balance,
                        cliente = item.OrderCubiq.Client,
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.OrderCubiqId,
                        tipoPago = item.tipoPago,
                        TipoTramite = TipoTramite.Cubiq,
                        Total = item.OrderCubiq.Amount,
                        User = item.User,
                        FechaTramite = item.OrderCubiq.Date,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.OrderCubiq.Status,
                        Numero = item.OrderCubiq.Number
                    });
                }
                else if(item.Passport != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.Passport.Balance,
                        cliente = item.Passport.Client,
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.PassportId,
                        tipoPago = item.tipoPago,
                        TipoTramite = TipoTramite.Pasaporte,
                        Total = item.Passport.Total,
                        User = item.User,
                        FechaTramite = item.Passport.FechaSolicitud,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.Passport.Status,
                        Numero = item.Passport.OrderNumber
                    });
                }
                else if(item.Rechargue != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.Rechargue.Import - item.Rechargue.valorPagado,
                        cliente = item.Rechargue.Client,
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.RechargueId,
                        tipoPago = item.tipoPago,
                        TipoTramite = TipoTramite.Recarga,
                        Total = item.Rechargue.Import,
                        User = item.User,
                        FechaTramite = item.Rechargue.date,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.Rechargue.estado,
                        Numero = item.Rechargue.Number
                    });
                }
                else if(item.Servicio != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.Servicio.importeTotal - item.Servicio.importePagado,
                        cliente = item.Servicio.cliente,
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.ServicioId,
                        tipoPago = item.tipoPago,
                        TipoTramite = TipoTramite.OtroServicio,
                        TipoServicio = item.Servicio.tipoServicio.Nombre,
                        Total = item.Servicio.importeTotal,
                        User = item.User,
                        FechaTramite = item.Servicio.fecha,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.Servicio.estado,
                        Numero = item.Servicio.numero
                    });
                }
                else if(item.Ticket != null)
                {
                    response.Add(new ItemPago
                    {
                        Balance = item.Ticket.Total - item.Ticket.Payment,
                        cliente = _context.Client.Find(item.Ticket.ClientId),
                        Pagado = item.valorPagado,
                        IdTramite = (Guid)item.TicketId,
                        tipoPago = item.tipoPago,
                        TipoTramite = TipoTramite.Reserva,
                        Total = item.Ticket.Total,
                        User = item.User,
                        FechaTramite  = item.Ticket.RegisterDate,
                        FechaPago = item.date.ToLocalTime(),
                        Status = item.Ticket.State,
                        Numero = item.Ticket.ReservationNumber
                    });
                }
            }

            return response;
        }
    }

    public class ItemPago
    {
        public Guid IdTramite { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaTramite { get; set; }
        public string Numero { get; set; }
        public string Status { get; set; }
        public TipoTramite TipoTramite { get; set; }
        public decimal Total { get; set; }
        public decimal Pagado { get; set; }
        public decimal Balance { get; set; }
        public TipoPago tipoPago { get; set; }
        public Client cliente { get; set; }
        public User User { get; set; }
        public string TipoServicio { get; set; } //Para cuando sea un tramite de tipo Servicio
    }

    public enum TipoTramite{ 
        EnvioMaritimo,
        EnvioAereo,
        Remesa,
        Reserva,
        Recarga,
        Pasaporte,
        OtroServicio,
        EnvioCaribe,
        Cubiq
    }
}
