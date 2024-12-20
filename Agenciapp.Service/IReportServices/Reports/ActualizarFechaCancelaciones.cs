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
        public static async Task UpdateCancelaciones(databaseContext _context)
        {
            Serilog.Log.Information("Actualizacion de fecha de cancelaciones iniciada");
            var estados = await _context.RegistroEstado
                .Include(x => x.EnvioCaribe)
                .Include(x => x.EnvioMaritimo)
                .Include(x => x.Order)
                .Include(x => x.OrderCubiq)
                .Include(x => x.PaqueteTuristico)
                .Include(x => x.Rechargue)
                .Include(x => x.Remittance)
                .Include(x => x.Servicio)
                .Include(x => x.Ticket)
                .Where(x => x.Estado == "Cancelada" || x.Estado == "Cancelado").ToListAsync();

            foreach (var item in estados)
            {
                if(item.EnvioCaribe != null)
                    item.EnvioCaribe.CanceledDate = item.Date;
                else if(item.EnvioMaritimo != null)
                    item.EnvioMaritimo.CanceledDate = item.Date;
                else if (item.Order != null)
                    item.Order.CanceledDate = item.Date;
                else if (item.OrderCubiq != null)
                    item.OrderCubiq.CanceledDate = item.Date;
                else if (item.PaqueteTuristico != null)
                    item.PaqueteTuristico.CanceledDate = item.Date;
                else if (item.Rechargue != null)
                    item.Rechargue.CanceledDate = item.Date;
                else if (item.Remittance != null)
                    item.Remittance.CanceledDate = item.Date;
                else if (item.Servicio != null)
                    item.Servicio.CanceledDate = item.Date;
                else if (item.Ticket != null)
                    item.Ticket.CanceledDate = item.Date;

                _context.Attach(item);
            }

            if (estados.Any())
                await _context.SaveChangesAsync();

            Serilog.Log.Information("Actualizacion de fecha de cancelaciones terminada");
        }
    }
}
