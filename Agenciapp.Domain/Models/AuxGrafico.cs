using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class AuxGrafico
    {
        private databaseContext _context;

        public IQueryable<Ticket> reservas;
        public IQueryable<Order> orders;
        public IQueryable<Remittance> remesas;
        public IQueryable<Rechargue> recarga;
        public IQueryable<EnvioMaritimo> envioMaritimo;
        public IQueryable<EnvioCaribe> envioCaribes;
        public IQueryable<Passport> pasaporte;
        private int mayor;


        public AuxGrafico(Guid idAgency, databaseContext _context)
        {
            this._context = _context;
            mayor = 0;
            reservas = _context.Ticket.Where(x => x.AgencyId == idAgency && x.State != "Cancelada");
            orders = _context.Order.Where(x =>x.Status != "Cancelada" && x.AgencyId == idAgency && x.Type != "Remesas");
            remesas = _context.Remittance.Where(x =>x.Status != "Cancelada" && x.AgencyId == idAgency);
            recarga = _context.Rechargue.Where(x =>x.estado != "Cancelada" && x.AgencyId == idAgency);
            envioMaritimo = _context.EnvioMaritimo.Where(x =>x.Status != "Cancelada" && x.AgencyId == idAgency);
            envioCaribes = _context.EnvioCaribes.Where(x =>x.Status != "Cancelada" && x.AgencyId == idAgency);
            pasaporte = _context.Passport.Where(x =>x.Status != "Cancelada" && x.AgencyId == idAgency);

        }

        public int getCantReservaMes(int mes)
        {
            int cant = reservas.Where(x => x.RegisterDate.Month == mes && x.RegisterDate.Year == DateTime.Now.Year).Count();
            if (cant > mayor )
            {
                mayor = cant;
            }
            return cant;
        }

        public int getCantOrderMes(int mes)
        {
            int cant = orders.Where(x => x.Date.Month == mes && x.Date.Year == DateTime.Now.Year).Count();

            if (cant > mayor)
            {
                mayor = cant;
            }
            return cant;
        }

        public int getCantEnviosMaritimosMes(int mes)
        {
            int cant = envioMaritimo.Where(x => x.Date.Month == mes && x.Date.Year == DateTime.Now.Year).Count();
             cant += envioCaribes.Where(x => x.Date.Month == mes && x.Date.Year == DateTime.Now.Year).Count();

            if (cant > mayor)
            {
                mayor = cant;
            }
            return cant;
        }

        public int getCantpasaporteMes(int mes)
        {
            int cant = pasaporte.Where(x => x.FechaSolicitud.Month == mes && x.FechaSolicitud.Year == DateTime.Now.Year).Count();
            cant += pasaporte.Where(x => x.FechaSolicitud.Month == mes && x.FechaSolicitud.Year == DateTime.Now.Year).Count();

            if (cant > mayor)
            {
                mayor = cant;
            }
            return cant;
        }

        public int getCantRemesasMes(int mes)
        {
            int cant = remesas.Where(x => x.Date.Month == mes && x.Date.Year == DateTime.Now.Year).Count();
            if (cant > mayor)
            {
                mayor = cant;
            }
            return cant;
        }

        public int getCantRecargasMes(int mes)
        {
            int cant = recarga.Where(x => x.date.Month == mes && x.date.Year == DateTime.Now.Year).Count();
            if (cant > mayor)
            {
                mayor = cant;
            }
            return cant;
        }

        public int getMayorCantidad()
        {
            if (this.mayor == 0)
            {
                return 10;
            }
            if (this.mayor % 10 != 0)
            {
                int aux = this.mayor % 10;
                int val = this.mayor + (10 - aux);
                return val;
            }

            return this.mayor;
        }

    }
}
