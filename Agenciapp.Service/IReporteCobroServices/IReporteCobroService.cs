using Agenciapp.Domain.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.IReporteCobroServices
{
    public interface IReporteCobroService
    {
        Task CreateReport();
        Task<ReporteCobro> GetReport(Agency agency, DateTime date);
    }

    public class ReporteCobroService : IReporteCobroService
    {
        private readonly databaseContext _context;
        public ReporteCobroService(databaseContext databaseContext)
        {
            _context = databaseContext;
        }

        public async Task CreateReport()
        {
            var date = DateTime.Now.Date;
            var agencies = _context.Agency;
            foreach (var agency in agencies.Where(x => x.AgencyId == Guid.Parse("680B03D4-A92D-44F5-8B34-FD70E0D9847C")))
            {
                var report = await GetReport(agency, date);
                _context.Attach(report);
            }
            await _context.SaveChangesAsync();

        }

        public async Task<ReporteCobro> GetReport(Agency agency, DateTime date)
        {
            DateTime initUtc = date.ToUniversalTime();
            DateTime endUtc = initUtc.AddDays(1);
            Serilog.Log.Information($"REPORTE COBRO - {agency.Name.ToUpper()}:");
            var ticket = _context.Ticket.Include(x => x.RegistroPagos).Where(x => x.PaqueteTuristicoId == null && x.AgencyId == agency.AgencyId && x.State != "Cancelada" && x.RegisterDate.Date == date);
            var order = _context.Order.Include(x => x.Pagos).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Type != "Remesas" && x.Type != "Combo" && x.Date.Date == date);
            var combos = _context.Order.Include(x => x.Pagos).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Type == "Combo" && x.Date.Date == date);
            var remesas = _context.Remittance.Include(x => x.Pagos).Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Date.Date == date);
            var recarga = _context.Rechargue.Include(x => x.RegistroPagos).Where(x => x.estado != "Cancelada" && x.AgencyId == agency.AgencyId && x.date.Date == date);
            var enviomatitimo = _context.EnvioMaritimo.Include(x => x.RegistroPagos).Where(x => x.Status != "Cancelada" && x.AgencyId == agency.AgencyId && x.Date.Date == date);
            var pasaporte = _context.Passport.Include(x => x.RegistroPagos).Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId) && x.FechaSolicitud.Date.Date == date);
            var cubiq = _context.OrderCubiqs.Include(x => x.RegistroPagos).Include(x => x.Paquetes).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Date.Date == date);
            var enviocaribe = _context.EnvioCaribes.Include(x => x.RegistroPagos).Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId) && x.Date.Date == date);
            var servicios = await _context.Servicios.Include(x => x.RegistroPagos).Include(x => x.tipoServicio).Where(x => x.PaqueteTuristicoId == null && x.estado != "Cancelado" && x.agency.AgencyId == agency.AgencyId && x.fecha >= initUtc && x.fecha < endUtc).ToListAsync();
            var paqueteTuristico = _context.PaquetesTuristicos.Include(x => x.Pagos).Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.AgencyId == agency.AgencyId && x.Date.Date == date);

            //Ventas totales
            decimal salesDay = 0;
            salesDay += await ticket.SumAsync(x => x.Total);
            salesDay += await order.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Amount) + await order.Where(x => x.agencyTransferida.AgencyId == agency.AgencyId).SumAsync(x => x.costoMayorista);
            salesDay += await combos.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Amount) + await combos.Where(x => x.agencyTransferida.AgencyId == agency.AgencyId).SumAsync(x => x.costoMayorista + x.OtrosCostos);
            salesDay += await remesas.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Amount) + await remesas.Where(x => x.agencyTransferida.AgencyId == agency.AgencyId).SumAsync(x => x.costoMayorista + x.OtrosCostos);
            salesDay += await recarga.SumAsync(x => x.Import);
            salesDay += await enviomatitimo.SumAsync(x => x.Amount);
            salesDay += await pasaporte.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Total) + await pasaporte.Where(x => x.AgencyTransferidaId == agency.AgencyId).SumAsync(x => x.costo + x.OtrosCostos);
            salesDay += await cubiq.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Amount);
            salesDay += await enviocaribe.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Amount) + await enviocaribe.Where(x => x.AgencyTransferida.AgencyId == agency.AgencyId).SumAsync(x => x.costo + x.OtrosCostos); ;
            salesDay += servicios.Sum(x => x.importeTotal);
            salesDay += await paqueteTuristico.SumAsync(x => x.Amount);
            Serilog.Log.Information($"Ventas Día - {salesDay}");

            //Cobrado
            /*charged += await ticket.SumAsync(x => x.RegistroPagos.Sum(y => y.valorPagado));
            charged += await order.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Pagos.Sum(y => y.valorPagado));
            charged += await combos.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Pagos.Sum(y => y.valorPagado));
            charged += await remesas.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.Pagos.Sum(y => y.valorPagado));
            charged += await recarga.SumAsync(x => x.RegistroPagos.Sum(y => y.valorPagado));
            charged += await enviomatitimo.SumAsync(x => x.RegistroPagos.Sum(y => y.valorPagado));
            charged += await pasaporte.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.RegistroPagos.Sum(y => y.valorPagado));
            charged += await cubiq.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.RegistroPagos.Sum(y => y.valorPagado));
            charged += await enviocaribe.Where(x => x.AgencyId == agency.AgencyId).SumAsync(x => x.RegistroPagos.Sum(y => y.valorPagado));
            charged += await servicios.SumAsync(x => x.RegistroPagos.Sum(y => y.valorPagado));
            */
            var pagos = await _context.RegistroPagosToday
            .Include(x => x.User)
            .Include(x => x.tipoPago)
            .Include(x => x.EnvioCaribe).ThenInclude(x => x.RegistroPagos)
            .Include(x => x.EnvioMaritimo).ThenInclude(x => x.RegistroPagos)
            .Include(x => x.Order).ThenInclude(x => x.Pagos)
            .Include(x => x.Order).ThenInclude(x => x.Bag).ThenInclude(x => x.BagItems)
            .Include(x => x.OrderCubiq).ThenInclude(x => x.RegistroPagos)
            .Include(x => x.Passport).ThenInclude(x => x.RegistroPagos)
            .Include(x => x.Rechargue).ThenInclude(x => x.RegistroPagos)
            .Include(x => x.Servicio).ThenInclude(x => x.RegistroPagos)
            .Include(x => x.Ticket).ThenInclude(x => x.RegistroPagos)
            .Include(x => x.PaqueteTuristico).ThenInclude(x => x.Pagos)
            .Where(x => x.AgencyId == agency.AgencyId && x.date >= initUtc && x.date < endUtc && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
            .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.Ticket.PaqueteTuristicoId == null && x.Servicio.PaqueteTuristicoId == null)
            //.Where(x => x.tipoPago.Type != "Crédito de Consumo")
            .ToListAsync();

            decimal charged = 0;
            decimal chargedDiferido = 0;

            foreach (var pago in pagos)
            {
                if (pago.EnvioCaribe != null)
                {
                    if (pago.EnvioCaribe.Date.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.EnvioMaritimo != null)
                {
                    if (pago.EnvioMaritimo.Date.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.Order != null)
                {
                    if (pago.Order.Date.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.OrderCubiq != null)
                {
                    if (pago.OrderCubiq.Date.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.Passport != null)
                {
                    if (pago.Passport.FechaSolicitud.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.Rechargue != null)
                {
                    if (pago.Rechargue.date.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.Remittance != null)
                {
                    if (pago.Remittance.Date.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.Servicio != null)
                {
                    if (pago.Servicio.fecha.ToLocalTime().Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.Ticket != null)
                {
                    if (pago.Ticket.RegisterDate.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
                else if (pago.PaqueteTuristico != null)
                {
                    if (pago.PaqueteTuristico.Date.Date == date.Date)
                        charged += pago.valorPagado;
                    else
                        chargedDiferido += pago.valorPagado;
                }
            }
            Serilog.Log.Information($"Cobrado - {charged}");
            Serilog.Log.Information($"Cobrado Diferido - {chargedDiferido}");

            var registroCobro = _context.RegistroCobro
            .Where(x => x.Mayorista == agency.AgencyId && x.Date.Date == date.Date)
            .OrderBy(x => x.Date).GroupBy(x => x.OrderNumber);

            decimal amountRegistro = 0; // Para tramites diferidos
            foreach (var orderItems in registroCobro)
            {
                string orderDate = orderItems.Key.Substring(2, 6);
                string dateCompare = date.ToString("yyMMdd");
                if (dateCompare == orderDate)
                {
                    continue;
                }
                decimal aux = 0;
                foreach (var item in orderItems)
                {
                    if (item.Action == "Insert")
                    {
                        aux += item.Value;
                    }
                    else if (item.Action == "Update")
                    {
                        aux += item.Value - item.OldValue;
                    }
                    else if (item.Action == "Delete")
                    {
                        aux -= item.Value;
                    }
                }
                amountRegistro += aux;
            }
            Serilog.Log.Information($"Monto en Servicios por cobrar de tramites diferidos - {amountRegistro}");
            //Cobrado en facturas de minoristas
            var accountsPayMinorista = _context.Facturas.Include(x => x.RegistroPagos).Where(x => x.agencyId == agency.AgencyId);
            charged += await accountsPayMinorista.SumAsync(x => x.RegistroPagos.Where(y => y.date.ToLocalTime().Date == date).Sum(y => y.valorPagado));
            Serilog.Log.Information($"Total Cobrado (+ cobrado Facturas) - {charged}");
            decimal saldoReal = 0;
            //Cuentas por Cobrar
            saldoReal += await _context.servicioxCobrar.Where(x => x.mayorista.AgencyId == agency.AgencyId && x.factura == null).SumAsync(x => x.importeACobrar);
            Serilog.Log.Information($"Cuentas por Cobrar - {saldoReal}");
            //Cuentas facturadas no pagadas
            saldoReal += await _context.Facturas.Include(x => x.RegistroPagos).Where(x => x.agencyId == agency.AgencyId && x.estado == "Facturada").SumAsync(x => x.valorPagado - x.pagado);
            Serilog.Log.Information($"Cuentas por Cobrar (+ Facturadas no pagadas) - {saldoReal}");

            decimal initialValue = 0;
            var lastReport = await _context.ReporteCobros.FirstOrDefaultAsync(x => x.Agency == agency && x.CreatedAt.Date == date.AddDays(-1));
            if (lastReport != null)
            {
                initialValue = lastReport.SaldoReal;
            }
            Serilog.Log.Information($"Saldo Inicial - {initialValue}");

            var report = new ReporteCobro(salesDay, initialValue, charged, chargedDiferido, saldoReal, amountRegistro, agency, date);
            return report;
        }
    }
}
