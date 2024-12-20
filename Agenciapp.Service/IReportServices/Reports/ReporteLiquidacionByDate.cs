using Agenciapp.Service.IReportServices.Models;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.IReportServices.Reports
{
    public static partial class Reporte
    {
        public static async Task<AuxVentasxServicio> GetReporteLiquidacionByDate(DateTime date, User user, databaseContext _context)
        {
            var aAgency = _context.Agency.Find(user.AgencyId);
            AuxVentasxServicio response = new AuxVentasxServicio();
            //Ventas por empleado Hoy
            AuxVentasPorEmpleado ventasxempleado = new AuxVentasPorEmpleado();
            var ventasEmpleado = await _context.RegistroPagos
                .Include(x => x.User)
                .Include(x => x.tipoPago)
                .Include(x => x.EnvioCaribe)
                .Include(x => x.EnvioMaritimo)
                .Include(x => x.Order)
                .Include(x => x.Order)
                .Include(x => x.OrderCubiq)
                .Include(x => x.Passport)
                .Include(x => x.Rechargue)
                .Include(x => x.Remittance)
                .Include(x => x.PaqueteTuristico)
                .Include(x => x.Servicio)
                .Include(x => x.Ticket)
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date.ToLocalTime().Date == date && x.tipoPago.Type != "Crédito de Consumo" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.Remittance.Status != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada")
                .Where(x => x.Order.Minorista == null).ToListAsync()
                //.Select(x => new { x.User, x.valorPagado })
                //.GroupBy(x => x.User)
                ;

            List<RegistroPago> aux = new List<RegistroPago>();
            foreach (var item in ventasEmpleado)
            {
                if(item.EnvioCaribe != null && (item.EnvioCaribe.Status != EnvioCaribe.STATUS_CANCELADA || ((DateTime)item.EnvioCaribe.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.EnvioMaritimo != null && (item.EnvioMaritimo.Status != EnvioMaritimo.STATUS_CANCELADA || ((DateTime)item.EnvioMaritimo.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Order != null && (item.Order.Status != Order.STATUS_CANCELADA || ((DateTime)item.Order.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.OrderCubiq != null && (item.OrderCubiq.Status != OrderCubiq.STATUS_CANCELADA || ((DateTime)item.OrderCubiq.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.PaqueteTuristico != null && (item.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA || ((DateTime)item.PaqueteTuristico.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Passport != null && (item.Passport.Status != Passport.STATUS_CANCELADA || ((DateTime)item.Passport.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Rechargue != null && (item.Rechargue.estado != Rechargue.STATUS_CANCELADA || ((DateTime)item.Rechargue.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Remittance != null && (item.Remittance.Status != Remittance.STATUS_CANCELADA || ((DateTime)item.Remittance.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Servicio != null && (item.Servicio.estado != Servicio.EstadoCancelado || ((DateTime)item.Servicio.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
                else if (item.Ticket != null && (item.Ticket.State != Ticket.STATUS_CANCELADA || ((DateTime)item.Ticket.CanceledDate).Date > date.Date))
                {
                    aux.Add(item);
                }
            }

            foreach (var t in aux.Select(x => new { x.User, x.valorPagado }).GroupBy(x => x.User))
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionHoy = ventasxempleado.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionHoy = ventasxempleado.getTotal();

            return response;
        }
    }
}
