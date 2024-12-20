using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxVentasEmpleado
    {
        private databaseContext _context;
        private List<TramiteEmpleado> tramites;
        public AuxVentasEmpleado(Agency agency, databaseContext _context)
        {
            this._context = _context;
            this.tramites = this._context.TramiteEmpleado.Where(x => x.IdAgency == agency.AgencyId).ToList();
        }

        public List<dataVenta> getReporte(DateTime? inicio, DateTime? fin)
        {
            List<dataVenta> response = new List<dataVenta>();
            IEnumerable<IGrouping<Guid,TramiteEmpleado>> aux;
            if (inicio == null && fin != null)
            {
                aux = tramites.Where(x => x.fecha.ToLocalTime().Date <= fin.Value.Date).GroupBy(x => x.IdEmpleado);
            }
            else if(fin == null && inicio != null)
            {
                aux = tramites.Where(x => x.fecha.ToLocalTime().Date >= inicio.Value.Date).GroupBy(x => x.IdEmpleado);
            }
            else if( inicio != null && fin != null)
            {
                aux = tramites.Where(x => x.fecha.ToLocalTime().Date <= fin.Value.Date && x.fecha.ToLocalTime().Date >= inicio.Value.Date).GroupBy(x => x.IdEmpleado);
            }
            else
            {
                aux = tramites.GroupBy(x => x.IdEmpleado);
            }
            foreach (var item in aux)
            {
                User empleado = _context.User.Find(item.Key);
                decimal importe = 0;
                int cantidad = 0;
                var dict = new Dictionary<string, (decimal, decimal)>();
                foreach (var tramite in item)
                {
                    if (tramite.tipoTramite == TramiteEmpleado.tipo_ENVIO)
                    {
                        Order envio = _context.Order.Include(y => y.Bag).ThenInclude(y => y.BagItems).FirstOrDefault(y => y.OrderId == tramite.IdTramite);
                        if (envio.Status == "Cancelada")
                            continue;
                        importe += envio.Amount;
                        var x = ((decimal)0, (decimal)0);
                        int cantaux = 0;
                        switch (envio.Type)
                        {
                            case "Combo":
                                foreach (var bag in envio.Bag)
                                {
                                    cantaux += bag.BagItems.Count;
                                }

                                x = dict.GetValueOrDefault("Combo");
                                x.Item1 += cantaux;
                                x.Item2 += envio.Amount;
                                dict["Combo"] = x;
                                break;
                            case "Tienda":
                                cantaux++;
                                x = dict.GetValueOrDefault("Tienda");
                                x.Item1 += 1;
                                x.Item2 += envio.Amount;
                                dict["Tienda"] = x;
                                break;
                            default:
                                cantaux++;
                                x = dict.GetValueOrDefault("Paquete");
                                x.Item1 += 1;
                                x.Item2 += envio.Amount;
                                dict["Paquete"] = x;
                                break;
                        }                        
                        cantidad += cantaux;
                    }
                    if (tramite.tipoTramite == TramiteEmpleado.tipo_ENVIOMARITIMO)
                    {
                        EnvioMaritimo envio = _context.EnvioMaritimo.Find(tramite.IdTramite);
                        if (envio.Status == "Cancelada")
                            continue;
                        importe += envio.Amount;
                        var x = ((decimal)0, (decimal)0);
                        x = dict.GetValueOrDefault("Marítimo");
                        x.Item1 += 1;
                        x.Item2 += envio.Amount;
                        dict["Marítimo"] = x;
                        cantidad++;
                    }
                    if (tramite.tipoTramite == TramiteEmpleado.tipo_RECARGA)
                    {
                        Rechargue recarga = _context.Rechargue.Find(tramite.IdTramite);
                        if (recarga.estado == "Cancelada")
                            continue;
                        importe += recarga.Import;

                        var x = ((decimal)0, (decimal)0);
                        x = dict.GetValueOrDefault("Recarga");
                        x.Item1 += 1;
                        x.Item2 += recarga.Import;
                        dict["Recarga"] = x;
                        cantidad++;
                    }
                    if (tramite.tipoTramite == TramiteEmpleado.tipo_REMESA)
                    {
                        Remittance remesa = _context.Remittance.Find(tramite.IdTramite);
                        if (remesa.Status == "Cancelada")
                            continue;
                        importe += remesa.Amount;

                        var x = ((decimal)0, (decimal)0);
                        x = dict.GetValueOrDefault("Remesa");
                        x.Item1 += 1;
                        x.Item2 += remesa.Amount;
                        dict["Remesa"] = x;

                        cantidad++;
                    }
                    if (tramite.tipoTramite == TramiteEmpleado.tipo_RESERVA)
                    {
                        Ticket reserva = _context.Ticket.Find(tramite.IdTramite);
                        if (reserva.State == "Cancelada")
                            continue;
                        importe += reserva.Total;

                        var x = ((decimal)0, (decimal)0);
                        x = dict.GetValueOrDefault("Reserva");
                        x.Item1 += 1;
                        x.Item2 += reserva.Total;
                        dict["Reserva"] = x;

                        cantidad++;
                    }
                    if (tramite.tipoTramite == TramiteEmpleado.tipo_PASSPORT)
                    {
                        Passport passport = _context.Passport.Find(tramite.IdTramite);
                        if (passport.Status == Passport.STATUS_CANCELADA)
                            continue;
                        importe += passport.Total;

                        var x = ((decimal)0, (decimal)0);
                        x = dict.GetValueOrDefault("Passport");
                        x.Item1 += 1;
                        x.Item2 += passport.Total;
                        dict["Passport"] = x;

                        cantidad++;
                    }
                }
                response.Add(new dataVenta(empleado, importe, cantidad, dict));
            }

            return response;
        }
    }

    public class dataVenta
    {
        public User empleado { get; set; }
        public decimal importe { get; set; }
        public int cantVentas { get; set; }

        public Dictionary<string,(decimal, decimal)> PorTipo { get; set; }

        public dataVenta(User empleado, decimal importe, int cantVentas, Dictionary<string,(decimal,decimal)> dict)
        {
            this.cantVentas = cantVentas;
            this.importe = importe;
            this.empleado = empleado;
            PorTipo = dict;
        }
    }
}
