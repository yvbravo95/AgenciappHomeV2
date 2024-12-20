using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxVentasPorDia
    {
        private databaseContext _context;
        private Agency agency;
        List<Remittance> remesas;
        List<Ticket> reservas;
        List<Order> envios;
        List<Rechargue> recargas;
        List<EnvioMaritimo> enviosMaritimos;
        List<Servicio> servicios;

        public AuxVentasPorDia(Agency agency, databaseContext _context)
        {
            this.agency = agency;
            this._context =_context;
            this.remesas = this._context.Remittance.Where(x => x.Status != "Cancelada" && x.AgencyId == agency.AgencyId ).ToList();
            this.envios = this._context.Order.Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Type != "Remesas").ToList();
            this.enviosMaritimos = this._context.EnvioMaritimo.Where(x =>x.Status != "Cancelada" && x.AgencyId == agency.AgencyId).ToList();
            this.reservas = this._context.Ticket.Where(x => x.State != "Cancelada" && x.AgencyId == agency.AgencyId).ToList();
            this.recargas = this._context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == agency.AgencyId).ToList();
        }

        public List<DataVentasDia> getDataMes()
        {
            List<DataVentasDia> aux = new List<DataVentasDia>();
            var date = DateTime.Now;
            var datePrincipioMes = Convert.ToDateTime(date.Month + "/01/" + date.Year);

            DataVentasDia ventasRemesas = new DataVentasDia("Remesas");
            DataVentasDia ventasRecarga = new DataVentasDia("Recargas");
            DataVentasDia ventasReservas = new DataVentasDia("Reservas");
            DataVentasDia ventasEnvios = new DataVentasDia("Envíos");
            DataVentasDia ventasEnviosMaritimos = new DataVentasDia("Envíos Marítimos");
            ventasRemesas.data.Add(0);
            ventasRecarga.data.Add(0);
            ventasReservas.data.Add(0);
            ventasEnvios.data.Add(0);
            ventasEnviosMaritimos.data.Add(0);
            while (datePrincipioMes.Date <= date.Date)
            {
                ventasRecarga.data.Add(this.recargas.Where(x => x.date.Date == datePrincipioMes.Date).Select(x => x.Import).Sum());
                ventasRemesas.data.Add(this.remesas.Where(x => x.Date.Date == datePrincipioMes.Date).Select(x => x.Amount).Sum());
                var auxorder = this.envios.Where(x => x.Date.Date == datePrincipioMes.Date);
                ventasEnvios.data.Add(auxorder.Where(x => x.AgencyId == agency.AgencyId).Sum(x => x.Amount) + auxorder.Where(x => x.agencyTransferida != null).Where(x => x.agencyTransferida.AgencyId == agency.AgencyId).Sum(x => x.costoMayorista + x.OtrosCostos));
                ventasEnviosMaritimos.data.Add(this.enviosMaritimos.Where(x => x.Date.Date == datePrincipioMes.Date).Select(x => x.Amount).Sum());
                ventasReservas.data.Add(this.reservas.Where(x => x.RegisterDate.Date == datePrincipioMes.Date).Select(x => x.Payment).Sum());
                datePrincipioMes = datePrincipioMes.AddDays(1);
            }
            aux.Add(ventasEnvios);
            aux.Add(ventasRecarga);
            aux.Add(ventasRemesas);
            aux.Add(ventasReservas);
            aux.Add(ventasEnviosMaritimos);
            return aux;
        }
    }

    public class DataVentasDia
    {
        public string nombreServicio { get; set; }
        public List<decimal> data { get; set; }
        public DataVentasDia(string nombreServicio)
        {
            this.nombreServicio = nombreServicio;
            data = new List<decimal>();
        }
    }
}
