using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxTransaccionesPorDia
    {
        private databaseContext _context;
        private Agency agency;
        List<Remittance> remesas;
        List<Ticket> reservas;
        List<Order> envios;
        List<Order> combos;
        List<Rechargue> recargas;
        List<EnvioMaritimo> enviosMaritimos;
        List<EnvioCaribe> enviosCaribe;
        List<Servicio> servicios;
        List<Passport> passports;
        List<OrderCubiq> cubiq;

        public AuxTransaccionesPorDia(Agency agency, databaseContext _context)
        {
            this.agency = agency;
            this._context =_context;
            this.remesas = this._context.Remittance.Where(x => x.Status != "Cancelada" && x.AgencyId == agency.AgencyId).ToList();
            this.envios = this._context.Order.Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Type != "Remesas" && x.Type != "Combo").ToList();
            this.combos = this._context.Order.Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Type == "Combo").ToList();
            this.enviosMaritimos = this._context.EnvioMaritimo.Where(x =>x.Status != "Cancelada" && x.AgencyId == agency.AgencyId).ToList();
            this.enviosCaribe = this._context.EnvioCaribes.Where(x =>x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId)).ToList();
            this.reservas = this._context.Ticket.Where(x => x.State != "Cancelada" && x.AgencyId == agency.AgencyId).ToList();
            this.recargas = this._context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == agency.AgencyId).ToList();
            this.passports = this._context.Passport.Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId)).ToList();
            this.cubiq = this._context.OrderCubiqs.Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId)).ToList();
        }

        public List<DataTransaccionesDia> getDataMes()
        {
            List<DataTransaccionesDia> aux = new List<DataTransaccionesDia>();
            var date = DateTime.Now;
            var datePrincipioMes = Convert.ToDateTime(date.Month + "/01/" + date.Year);

            DataTransaccionesDia cantidadRemesas = new DataTransaccionesDia("Remesas");
            DataTransaccionesDia cantidadRecarga = new DataTransaccionesDia("Recargas");
            DataTransaccionesDia cantidadReservas = new DataTransaccionesDia("Reservas");
            DataTransaccionesDia cantidadEnvios = new DataTransaccionesDia("Envíos");
            DataTransaccionesDia cantidadCombos = new DataTransaccionesDia("Combos");
            DataTransaccionesDia cantidadEnviosMaritimos = new DataTransaccionesDia("Envíos Marítimos");
            DataTransaccionesDia cantidadPasaportes = new DataTransaccionesDia("Pasaportes");
            DataTransaccionesDia cantidadCubiq = new DataTransaccionesDia("Cubiq");
            cantidadRemesas.data.Add(0);
            cantidadRecarga.data.Add(0);
            cantidadReservas.data.Add(0);
            cantidadEnvios.data.Add(0);
            cantidadCombos.data.Add(0);
            cantidadEnviosMaritimos.data.Add(0);
            cantidadPasaportes.data.Add(0);
            cantidadCubiq.data.Add(0);
            while (datePrincipioMes.Date <= date.Date)
            {
                cantidadRecarga.data.Add(this.recargas.Where(x => x.date.Date == datePrincipioMes.Date).Count());
                cantidadRemesas.data.Add(this.remesas.Where(x => x.Date.Date == datePrincipioMes.Date).Count());
                cantidadEnvios.data.Add(this.envios.Where(x => x.Date.Date == datePrincipioMes.Date).Count());
                cantidadCombos.data.Add(this.combos.Where(x => x.Date.Date == datePrincipioMes.Date).Count());
                cantidadEnviosMaritimos.data.Add(this.enviosMaritimos.Where(x => x.Date.Date == datePrincipioMes.Date).Count() + this.enviosCaribe.Where(x => x.Date.ToLocalTime().Date == datePrincipioMes.Date).Count());
                cantidadPasaportes.data.Add(this.passports.Where(x => x.FechaSolicitud.Date == datePrincipioMes.Date).Count());
                cantidadReservas.data.Add(this.reservas.Where(x => x.RegisterDate.Date == datePrincipioMes.Date).Count());
                cantidadCubiq.data.Add(this.cubiq.Where(x => x.Date.Date == datePrincipioMes.Date).Count());
                datePrincipioMes = datePrincipioMes.AddDays(1);
            }
            aux.Add(cantidadEnvios);
            aux.Add(cantidadCombos);
            aux.Add(cantidadRecarga);
            aux.Add(cantidadRemesas);
            aux.Add(cantidadReservas);
            aux.Add(cantidadEnviosMaritimos);
            aux.Add(cantidadPasaportes);
            aux.Add(cantidadCubiq);
            return aux;
        }
    }

    public class DataTransaccionesDia
    {
        public string nombreServicio { get; set; }
        public List<decimal> data { get; set; }
        public DataTransaccionesDia(string nombreServicio)
        {
            this.nombreServicio = nombreServicio;
            data = new List<decimal>();
        }
    }
}
