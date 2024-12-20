using Agenciapp.Service.IReportServices;
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
        public async static Task<AuxVentasxServicio> GetVentasxServicioRapid(User user, databaseContext _context, IReportService _reportService)
        {
            var aAgency = _context.Agency.Find(user.AgencyId);

            AuxVentasxServicio response = new AuxVentasxServicio();

            //**** Ventas por servicio. ****
            var datehoy = DateTime.Now.Date;
            DateTime dateayer = datehoy.AddDays(-1);
            DateTime dateayer2 = datehoy.AddDays((datehoy.Day - 1) * -1);


            //Utilidad por servicio
            decimal importeAuto1Utilidad = 0;
            decimal importePasaje1Utilidad = 0;
            decimal importeHotel1Utilidad = 0;
            decimal importeAislamiento1Utilidad = 0;
            //Venta por servicio
            decimal importeAuto1 = 0;
            decimal importePasaje1 = 0;
            decimal importeHotel1 = 0;
            decimal importeAislamiento1 = 0;
            var ticket = _context.Ticket
                .Where(x => x.PaqueteTuristicoId == null && x.AgencyId == aAgency.AgencyId && x.State != "Cancelada");
            var ticketaux = ticket.Where(x => x.RegisterDate.Date >= datehoy);
            foreach (var item in ticketaux)
            {
                if (item.type == "pasaje")
                {
                    importePasaje1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importePasaje1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
                if (item.type == "auto")
                {
                    importeAuto1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAuto1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
                if (item.type == "hotel")
                {
                    importeHotel1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeHotel1Utilidad += item.Total - item.Cost - item.Charges;
                    }


                }
                if (item.type == "aislamiento")
                {
                    importeAislamiento1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAislamiento1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
            }

            var auxorder = _context.Order.Include(x => x.agencyTransferida).Include(x => x.TipoPago).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type != "Remesas" && x.Type != "Combo" && x.Date.Date >= datehoy);
            decimal importeEnvios1 = auxorder.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxorder.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista);
            decimal importeEnvios1Utilidad = auxorder.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costoMayorista) + auxorder.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId && x.credito == 0).Sum(x => x.costoMayorista - x.costoDeProveedor - x.OtrosCostos);

            var auxcombos = _context.Order.Include(x => x.agencyTransferida).Include(x => x.Minorista).Include(x => x.TipoPago).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type == "Combo" && x.Date.Date >= datehoy);
            decimal importeCombos1 = auxcombos.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount) + auxcombos.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeCombos1Utilidad = auxcombos.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount - x.costoMayorista - x.OtrosCostos) + auxcombos.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista - x.costoDeProveedor);

            var auxRemesas = _context.Remittance.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= datehoy);
            decimal importeRemesas1 = auxRemesas.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxRemesas.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeRemesas1Utilidad = auxRemesas.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - (x.costoMayorista + x.costoporDespacho + aAgency.remesa_entregaCuba)) + auxRemesas.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - x.ValorPagado - aAgency.remesa_entregaCuba);

            var auxPaqueteTuristico = _context.PaquetesTuristicos.Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= datehoy);
            decimal importePaqueteTuristico1 = auxPaqueteTuristico.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount);
            decimal importePaqueteTuristico1Utilidad = auxPaqueteTuristico.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.Costo - x.OtrosCostos);

            var auxrecarga = _context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.date.Date >= datehoy);
            decimal importeRecarga1 = auxrecarga.Sum(x => x.Import);
            decimal importeRecarga1Utilidad = auxrecarga.Sum(x => x.Import - x.costoMayorista);

            var auxenviomatitimo = _context.EnvioMaritimo.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date.Date >= datehoy);
            decimal importeEnvioMaritimo1 = auxenviomatitimo.Sum(x => x.Amount);
            decimal importeEnvioMaritimo1Utilidad = auxenviomatitimo.Sum(x => x.Amount - x.costoMayorista);

            var auxpasaporte = _context.Passport.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.FechaSolicitud.Date >= datehoy.Date);
            decimal importepasaporte = auxpasaporte.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Total) + auxpasaporte.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
            decimal importePasaporteUtilidad = (await _reportService.UtilityByService(aAgency.AgencyId, STipo.Passport, datehoy, datehoy)).Value.Sum(x => x.Utility);

            var auxcubiq = _context.OrderCubiqs.Include(x => x.Paquetes).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= datehoy);
            decimal importecubiq = 0;
            decimal importecubiqUtilidad = 0;
            foreach (var item in auxcubiq)
            {
                if (item.AgencyId == aAgency.AgencyId)
                {
                    importecubiq += item.Amount;
                    if (aAgency.Name == "Cubiq LLC")
                    {
                        importecubiqUtilidad += item.Amount - item.Costo - item.OtrosCostos;
                    }
                    else
                    {
                        importecubiqUtilidad += item.Amount - item.costoMayorista - item.OtrosCostos;
                    }
                }
                else if (item.agencyTransferida.AgencyId == aAgency.AgencyId)
                {
                    importecubiq += item.costoMayorista + item.OtrosCostos;
                    importecubiqUtilidad += item.costoMayorista - item.Paquetes.Sum(y => y.CostoCubiq);
                }
            }

            var auxmercado = _context.Mercado.Where(x => x.Status != Mercado.STATUS_CANCELADA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= datehoy);
            decimal importeMercado = auxmercado.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount);
            decimal importeMercadoUtilidad = auxmercado.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.Cargos);

            AuxVentasPorServicio ventasxservicio1 = new AuxVentasPorServicio();
            if (importeAuto1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto1)); };
            if (importePasaje1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje1)); };
            if (importeHotel1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel1)); };
            if (importeAislamiento1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento1)); };
            if (importeEnvios1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios1)); };
            if (importeCombos1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos1)); };
            if (importeRemesas1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas1)); };
            if (importePaqueteTuristico1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico1)); };
            if (importeRecarga1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga1)); };
            if (importeEnvioMaritimo1 != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo1));
            if (importepasaporte != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporte));
            if (importecubiq != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiq));
            if (importeMercado != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Mercado", importeMercado));

            AuxVentasPorServicio ventasxservicio1Utilidad = new AuxVentasPorServicio();
            if (importeAuto1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto1Utilidad));
            if (importePasaje1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje1Utilidad));
            if (importeHotel1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel1Utilidad));
            if (importeAislamiento1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento1Utilidad));
            if (importeEnvios1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios1Utilidad));
            if (importeCombos1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos1Utilidad));
            if (importeRemesas1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas1Utilidad));
            if (importePaqueteTuristico1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico1Utilidad));
            if (importeRecarga1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga1Utilidad));
            if (importeEnvioMaritimo1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo1Utilidad));
            if (importePasaporteUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaporte", importePasaporteUtilidad));
            if (importecubiqUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiqUtilidad));
            if (importeMercadoUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Mercado", importeMercadoUtilidad));

            if (aAgency.Name == "Rapid Multiservice")
            {
                var auxenviocaribe = _context.EnvioCaribes.Include(x => x.Contact).ThenInclude(x => x.Address).Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.Date.Date >= datehoy);
                decimal importeenviocaribe1 = auxenviocaribe.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxenviocaribe.Where(x => x.AgencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
                decimal importeenviocaribe1Utilidad = auxenviocaribe.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costo - x.OtrosCostos) + GetCostoCaribe(auxenviocaribe.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).ToList(), _context);
                if (importeenviocaribe1 != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe1));
                if (importeenviocaribe1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe1Utilidad));

            }

            var servicios = _context.Servicios.Include(x => x.tipoServicio).Where(x => x.PaqueteTuristicoId == null && x.estado != "Cancelado" && x.agency.AgencyId == aAgency.AgencyId && x.fecha.ToLocalTime().Date >= datehoy);
            var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == aAgency.AgencyId);
            foreach (var item in tiposervicios)
            {
                //Utilidad por servicio
                decimal sumUtilidad = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal - x.costoMayorista - x.CostoXServicio);
                //Ventas por servicio
                decimal sum = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal);

                if (sum != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>(item.Nombre, sum));
                if (sumUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>(item.Nombre, sumUtilidad));
            }

            response.ventasHoy = ventasxservicio1.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalVentasHoy = ventasxservicio1.getTotal();

            response.UtilidadHoy = ventasxservicio1Utilidad.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalUtilidadHoy = ventasxservicio1Utilidad.getTotal();

            decimal importeAuto2 = 0;
            decimal importeAuto2Utilidad = 0;

            decimal importePasaje2 = 0;
            decimal importePasaje2Utilidad = 0;

            decimal importeHotel2 = 0;
            decimal importeHotel2Utilidad = 0;

            decimal importeAislamiento2 = 0;
            decimal importeAislamiento2Utilidad = 0;
            ticketaux = ticket.Where(x => x.PaqueteTuristicoId == null && x.RegisterDate.Date.Date >= dateayer2)
                .Where(x => x.RegistroEstados.Where(y => y.Date.Date <= datehoy.Date).OrderByDescending(y => y.Date).FirstOrDefault() == null ||
                         (x.RegistroEstados.Where(y => y.Date.Date <= datehoy.Date).OrderByDescending(y => y.Date).FirstOrDefault() != null &&
                            x.RegistroEstados.Where(y => y.Date.Date <= datehoy.Date).OrderByDescending(y => y.Date).FirstOrDefault().Estado != Ticket.STATUS_CANCELADA));
            foreach (var item in ticketaux)
            {
                if (item.type == "pasaje")
                {
                    importePasaje2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importePasaje2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "auto")
                {
                    importeAuto2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAuto2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "hotel")
                {
                    importeHotel2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeHotel2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "aislamiento")
                {
                    importeAislamiento2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAislamiento2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
            }

            var auxenvios2 = _context.Order.Include(x => x.agencyTransferida).Include(x => x.TipoPago).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type != "Remesas" && x.Type != "Combo" && x.Date.Date >= dateayer2);
            decimal importeEnvios2 = auxenvios2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxenvios2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista);
            decimal importeEnvios2Utilidad = auxenvios2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costoMayorista) + auxenvios2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - x.costoDeProveedor - x.OtrosCostos);

            var auxcombos2 = _context.Order.Include(x => x.agencyTransferida).Include(x => x.Minorista).Include(x => x.TipoPago).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type == "Combo" && x.Date.Date.Date >= dateayer2);
            decimal importeCombos2 = auxcombos2.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount) + auxcombos2.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeCombos2Utilidad = auxcombos2.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount - x.costoMayorista - x.OtrosCostos) + auxcombos2.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista - x.costoDeProveedor);


            var auxremesa2 = _context.Remittance.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
            decimal importeRemesas2 = auxremesa2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxremesa2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeRemesas2Utilidad = auxremesa2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - (x.costoMayorista + x.costoporDespacho + aAgency.remesa_entregaCuba)) + auxremesa2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - aAgency.remesa_entregaCuba);

            var auxPaqueteTuristico2 = _context.PaquetesTuristicos.Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= dateayer2);
            decimal importePaqueteTuristico2 = auxPaqueteTuristico2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount);
            decimal importePaqueteTuristico2Utilidad = auxPaqueteTuristico2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.Costo - x.OtrosCostos);

            var auxrecarga2 = _context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.date.Date >= dateayer2);
            decimal importeRecarga2 = auxrecarga2.Sum(x => x.Import);
            decimal importeRecarga2Utilidad = auxrecarga2.Sum(x => x.Import - x.costoMayorista);

            var auxenviomaritimo2 = _context.EnvioMaritimo.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date.Date >= dateayer2);
            decimal importeEnvioMaritimo = auxenviomaritimo2.Sum(x => x.Amount);
            decimal importeEnvioMaritimo2Utilidad = auxenviomaritimo2.Sum(x => x.Amount - x.costoMayorista);

            var auxpasaporte2 = _context.Passport.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.FechaSolicitud.Date.Date >= dateayer2);
            decimal importepasaporte2 = auxpasaporte2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Total) + auxpasaporte2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
            decimal importepasaporteUtilidad2 = (await _reportService.UtilityByService(aAgency.AgencyId, STipo.Passport, dateayer2, datehoy)).Value.Sum(x => x.Utility);

            var auxcubiq2 = _context.OrderCubiqs.Include(x => x.Paquetes).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
            decimal importecubiq2 = 0;
            decimal importecubiqUtilidad2 = 0;
            foreach (var item in auxcubiq2)
            {
                if (item.AgencyId == aAgency.AgencyId)
                {
                    importecubiq2 += item.Amount;
                    if (aAgency.Name == "Cubiq LLC")
                    {
                        importecubiqUtilidad2 += item.Amount - item.Costo - item.OtrosCostos;
                    }
                    else
                    {
                        importecubiqUtilidad2 += item.Amount - item.costoMayorista - item.OtrosCostos;
                    }
                }
                else if (item.agencyTransferida.AgencyId == aAgency.AgencyId)
                {
                    importecubiq2 += item.costoMayorista + item.OtrosCostos;
                    importecubiqUtilidad2 += item.costoMayorista - item.Paquetes.Sum(y => y.CostoCubiq);
                }

            }

            var auxmercado2 = _context.Mercado.Where(x => x.Status != Mercado.STATUS_CANCELADA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= dateayer2);
            decimal importeMercado2 = auxmercado2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount);
            decimal importeMercadoUtilidad2 = auxmercado2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.Cargos);

            AuxVentasPorServicio ventasxservicio2 = new AuxVentasPorServicio();
            if (importeAuto2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto2));
            if (importePasaje2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje2));
            if (importeHotel2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel2));
            if (importeAislamiento2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento2));
            if (importeEnvios2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios2));
            if (importeCombos2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos2));
            if (importeRemesas2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas2));
            if (importePaqueteTuristico2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico2));
            if (importeRecarga2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga2));
            if (importeEnvioMaritimo != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo));
            if (importepasaporte2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporte2));
            if (importecubiq2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiq2));
            if (importeMercado2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Mercado", importeMercado2));

            AuxVentasPorServicio ventasxservicio2Utilidad = new AuxVentasPorServicio();
            if (importeAuto2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto2Utilidad));
            if (importePasaje2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje2Utilidad));
            if (importeHotel2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel2Utilidad));
            if (importeAislamiento2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento2Utilidad));
            if (importeEnvios2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios2Utilidad));
            if (importeCombos2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos2Utilidad));
            if (importeRemesas2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas2Utilidad));
            if (importePaqueteTuristico2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico2Utilidad));
            if (importeRecarga2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga2Utilidad));
            if (importeEnvioMaritimo2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo2Utilidad));
            if (importepasaporteUtilidad2 != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporteUtilidad2));
            if (importecubiqUtilidad2 != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiqUtilidad2));
            if (importeMercadoUtilidad2 != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Mercado", importeMercadoUtilidad2));
            if (aAgency.Name == "Rapid Multiservice")
            {
                var auxenviocaribe2 = _context.EnvioCaribes.Include(x => x.Contact).ThenInclude(x => x.Address).Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
                decimal importeenviocaribe = auxenviocaribe2.Where(x => x.AgencyId == user.AgencyId).Sum(x => x.Amount) + auxenviocaribe2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
                decimal importeenviocaribe2Utilidad = auxenviocaribe2.Where(x => x.AgencyId == user.AgencyId).Sum(x => x.Amount - x.costo - x.OtrosCostos) + GetCostoCaribe(auxenviocaribe2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).ToList(), _context);
                if (importeenviocaribe != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe));
                if (importeenviocaribe2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe2Utilidad));
            }

            servicios = _context.Servicios.Include(x => x.tipoServicio).Where(x => x.PaqueteTuristicoId == null && x.estado != "Cancelado" && x.agency.AgencyId == aAgency.AgencyId && x.fecha.ToLocalTime().Date >= dateayer2);
            foreach (var item in tiposervicios)
            {
                decimal sum = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal);
                decimal sumUtilidad = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal - x.costoMayorista - x.CostoXServicio);
                if (sum != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>(item.Nombre, sum));
                if (sumUtilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>(item.Nombre, sumUtilidad));
            }
            response.ventasAyer = ventasxservicio2.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalVentasAyer = ventasxservicio2.getTotal();
            response.UtilidadAyer = ventasxservicio2Utilidad.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalUtilidadAyer = ventasxservicio2Utilidad.getTotal();

            //Ventas por empleado Hoy
            AuxVentasPorEmpleado ventasxempleado = new AuxVentasPorEmpleado();
            var ventasEmpleado = _context.RegistroPagos
                .Include(x => x.User)
                .Include(x => x.tipoPago)
                .Include(x => x.EnvioCaribe)
                .Include(x => x.EnvioMaritimo)
                .Include(x => x.Order).ThenInclude(x => x.Minorista)
                .Include(x => x.OrderCubiq)
                .Include(x => x.Passport)
                .Include(x => x.Rechargue)
                .Include(x => x.Servicio)
                .Include(x => x.Remittance)
                .Include(x => x.PaqueteTuristico)
                .Include(x => x.Ticket)
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date.ToLocalTime().Date >= datehoy && x.tipoPago.Type != "Crédito de Consumo" && x.tipoPago.Type != "Cubiq" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.Remittance.Status != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada" && x.Mercado.Status != Mercado.STATUS_CANCELADA)
                .Where(x => x.Order.Minorista == null && x.Ticket.PaqueteTuristicoId == null && x.Servicio.PaqueteTuristicoId == null)
                .Select(x => new { x.User, x.valorPagado })
                .GroupBy(x => x.User);

            foreach (var t in ventasEmpleado)
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionHoy = ventasxempleado.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionHoy = ventasxempleado.getTotal();

            //Ventas por empleado Ayer
            AuxVentasPorEmpleado ventasxempleado2 = new AuxVentasPorEmpleado();
            var ventasEmpleado2 = _context.RegistroPagos
                .Include(x => x.User)
                .Include(x => x.tipoPago)
                .Include(x => x.EnvioCaribe)
                .Include(x => x.EnvioMaritimo)
                .Include(x => x.Order).ThenInclude(x => x.Minorista)
                .Include(x => x.OrderCubiq)
                .Include(x => x.Passport)
                .Include(x => x.Rechargue)
                .Include(x => x.Remittance)
                .Include(x => x.PaqueteTuristico)
                .Include(x => x.Servicio)
                .Include(x => x.Ticket)
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date.ToLocalTime().Date < datehoy && x.date.ToLocalTime().Date >= dateayer && x.tipoPago.Type != "Crédito de Consumo" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                .Where(x => (x.EnvioCaribe.Status != "Cancelada" || ((DateTime)x.EnvioCaribe.CanceledDate).Date > datehoy) 
                && (x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA || ((DateTime)x.PaqueteTuristico.CanceledDate).Date > datehoy) 
                && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA 
                && (x.EnvioMaritimo.Status != "Cancelada" || ((DateTime)x.EnvioMaritimo.CanceledDate).Date > datehoy) 
                && (x.Order.Status != "Cancelada" || ((DateTime)x.Order.CanceledDate).Date > datehoy) 
                && (x.OrderCubiq.Status != "Cancelada" || ((DateTime)x.OrderCubiq.CanceledDate).Date > datehoy) 
                && (x.Passport.Status != "Cancelada" || ((DateTime)x.Passport.CanceledDate).Date > datehoy) 
                && (x.Rechargue.estado != "Cancelada" || ((DateTime)x.Rechargue.CanceledDate).Date > datehoy) 
                && (x.Servicio.estado != "Cancelado" || ((DateTime)x.Servicio.CanceledDate).Date > datehoy) 
                && (x.Ticket.State != "Cancelada" || ((DateTime)x.Ticket.CanceledDate).Date > datehoy)
                && (x.Mercado.Status != Mercado.STATUS_CANCELADA || ((DateTime)x.Mercado.CanceledDate).Date > datehoy))
                .Where(x => x.Order.Minorista == null)
                .Select(x => new { x.User, x.valorPagado })
                .GroupBy(x => x.User);

            foreach (var t in ventasEmpleado2)
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado2.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionAyer = ventasxempleado2.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionAyer = ventasxempleado2.getTotal();

            return response;
        }

        public async static Task<AuxVentasxServicio> GetVentasxServicio(User user, databaseContext _context, IReportService _reportService)
        {
            var aAgency = _context.Agency.Find(user.AgencyId);

            AuxVentasxServicio response = new AuxVentasxServicio();

            //**** Ventas por servicio. ****
            var datehoy = DateTime.Now.Date;
            DateTime dateayer = datehoy.AddDays(-1);
            DateTime dateayer2 = datehoy.AddDays((datehoy.Day - 1) * -1);


            //Utilidad por servicio
            decimal importeAuto1Utilidad = 0;
            decimal importePasaje1Utilidad = 0;
            decimal importeHotel1Utilidad = 0;
            decimal importeAislamiento1Utilidad = 0;
            //Venta por servicio
            decimal importeAuto1 = 0;
            decimal importePasaje1 = 0;
            decimal importeHotel1 = 0;
            decimal importeAislamiento1 = 0;

            var ticket = _context.Ticket
                .Where(x => x.PaqueteTuristicoId == null && x.AgencyId == aAgency.AgencyId && x.State != "Cancelada");

            var ticketaux = ticket.Where(x => x.RegisterDate.Date >= datehoy);

            foreach (var item in ticketaux)
            {
                if (item.type == "pasaje")
                {
                    importePasaje1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importePasaje1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
                if (item.type == "auto")
                {
                    importeAuto1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAuto1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
                if (item.type == "hotel")
                {
                    importeHotel1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeHotel1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
                if (item.type == "aislamiento")
                {
                    importeAislamiento1 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAislamiento1Utilidad += item.Total - item.Cost - item.Charges;
                    }
                }
            }

            var auxorder = _context.Order.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type != "Remesas" && x.Type != "Combo" && x.Date.Date >= datehoy);
            decimal importeEnvios1 = auxorder.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxorder.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista);
            decimal importeEnvios1Utilidad = auxorder.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costoMayorista - x.ProductsShipping) + auxorder.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId && x.credito == 0).Sum(x => x.costoMayorista - x.costoDeProveedor - x.OtrosCostos);

            var auxcombos = _context.Order.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type == "Combo" && x.Date.Date >= datehoy);
            decimal importeCombos1 = auxcombos.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount) + auxcombos.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeCombos1Utilidad = auxcombos.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount - x.costoMayorista - x.OtrosCostos - x.ProductsShipping) + auxcombos.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista - x.costoDeProveedor);

            var auxRemesas = _context.Remittance.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= datehoy);
            decimal importeRemesas1 = auxRemesas.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxRemesas.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeRemesas1Utilidad = auxRemesas.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - (x.costoMayorista + x.costoporDespacho + aAgency.remesa_entregaCuba)) + auxRemesas.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - x.ValorPagado - aAgency.remesa_entregaCuba);

            var auxPaqueteTuristico = _context.PaquetesTuristicos.Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= datehoy);
            decimal importePaqueteTuristico1 = auxPaqueteTuristico.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount);
            decimal importePaqueteTuristico1Utilidad = auxPaqueteTuristico.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.Costo - x.OtrosCostos);

            var auxrecarga = _context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.date.Date >= datehoy);
            decimal importeRecarga1 = auxrecarga.Sum(x => x.Import);
            decimal importeRecarga1Utilidad = auxrecarga.Sum(x => x.Import - x.costoMayorista);

            var auxenviomatitimo = _context.EnvioMaritimo.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date.Date >= datehoy);
            decimal importeEnvioMaritimo1 = auxenviomatitimo.Sum(x => x.Amount);
            decimal importeEnvioMaritimo1Utilidad = auxenviomatitimo.Sum(x => x.Amount - x.costoMayorista);

            var auxpasaporte = _context.Passport.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.FechaSolicitud.Date >= datehoy.Date);
            decimal importepasaporte = auxpasaporte.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Total) + auxpasaporte.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
            decimal importePasaporteUtilidad = (await _reportService.UtilityByService(aAgency.AgencyId, STipo.Passport, datehoy, datehoy)).Value.Sum(x => x.Utility);

            var auxcubiq = _context.OrderCubiqs.Include(x => x.Paquetes).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= datehoy);
            decimal importecubiq = 0;
            decimal importecubiqUtilidad = (await _reportService.UtilityByService(aAgency.AgencyId, STipo.Cubiq, datehoy, datehoy)).Value.Sum(x => x.Utility);
            foreach (var item in auxcubiq)
            {
                if (item.AgencyId == aAgency.AgencyId)
                {
                    importecubiq += item.Amount;
                }
                else if (item.agencyTransferida.AgencyId == aAgency.AgencyId)
                {
                    importecubiq += item.costoMayorista + item.OtrosCostos;
                }
            }

            var auxmercado = _context.Mercado.Where(x => x.Status != Mercado.STATUS_CANCELADA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= datehoy);
            decimal importeMercado = auxmercado.Sum(x => x.Amount);
            decimal importeMercadoUtilidad = auxmercado.Sum(x => x.Amount - x.Cargos);

            AuxVentasPorServicio ventasxservicio1 = new AuxVentasPorServicio();
            if (importeAuto1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto1)); };
            if (importePasaje1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje1)); };
            if (importeHotel1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel1)); };
            if (importeAislamiento1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento1)); };
            if (importeEnvios1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios1)); };
            if (importeCombos1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos1)); };
            if (importeRemesas1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas1)); };
            if (importePaqueteTuristico1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico1)); };
            if (importeRecarga1 != 0) { ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga1)); };
            if (importeEnvioMaritimo1 != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo1));
            if (importepasaporte != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporte));
            if (importecubiq != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiq));
            if (importeMercado != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Mercado", importeMercado));

            AuxVentasPorServicio ventasxservicio1Utilidad = new AuxVentasPorServicio();
            if (importeAuto1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto1Utilidad));
            if (importePasaje1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje1Utilidad));
            if (importeHotel1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel1Utilidad));
            if (importeAislamiento1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento1Utilidad));
            if (importeEnvios1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios1Utilidad));
            if (importeCombos1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos1Utilidad));
            if (importeRemesas1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas1Utilidad));
            if (importePaqueteTuristico1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico1Utilidad));
            if (importeRecarga1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga1Utilidad));
            if (importeEnvioMaritimo1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo1Utilidad));
            if (importePasaporteUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaporte", importePasaporteUtilidad));
            if (importecubiqUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiqUtilidad));
            if (importeMercadoUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Mercado", importeMercadoUtilidad));
            
            if (aAgency.Name == "Rapid Multiservice")
            {
                var auxenviocaribe = _context.EnvioCaribes
                    .Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.Date.Date >= datehoy);
                decimal importeenviocaribe1 = auxenviocaribe.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxenviocaribe.Where(x => x.AgencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
                decimal importeenviocaribe1Utilidad = auxenviocaribe.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costo - x.OtrosCostos) + GetCostoCaribe(auxenviocaribe.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).ToList(),_context);
                if (importeenviocaribe1 != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe1));
                if (importeenviocaribe1Utilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe1Utilidad));

            }

            var servicios = _context.Servicios.Where(x => x.PaqueteTuristicoId == null && x.estado != "Cancelado" && x.agency.AgencyId == aAgency.AgencyId && x.fecha >= datehoy.ToUniversalTime());
            var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == aAgency.AgencyId);
            foreach (var item in tiposervicios)
            {
                //Utilidad por servicio
                decimal sumUtilidad = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal - x.costoMayorista - x.CostoXServicio);
                //Ventas por servicio
                decimal sum = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal);

                if (sum != 0) ventasxservicio1.servicios.Add(new ParAux<string, decimal>(item.Nombre, sum));
                if (sumUtilidad != 0) ventasxservicio1Utilidad.servicios.Add(new ParAux<string, decimal>(item.Nombre, sumUtilidad));
            }

            response.ventasHoy = ventasxservicio1.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalVentasHoy = ventasxservicio1.getTotal();

            response.UtilidadHoy = ventasxservicio1Utilidad.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalUtilidadHoy = ventasxservicio1Utilidad.getTotal();

            decimal importeAuto2 = 0;
            decimal importeAuto2Utilidad = 0;

            decimal importePasaje2 = 0;
            decimal importePasaje2Utilidad = 0;

            decimal importeHotel2 = 0;
            decimal importeHotel2Utilidad = 0;

            decimal importeAislamiento2 = 0;
            decimal importeAislamiento2Utilidad = 0;
            ticketaux = ticket.Where(x => x.PaqueteTuristicoId == null && x.RegisterDate.Date.Date >= dateayer2);
            foreach (var item in ticketaux)
            {
                if (item.type == "pasaje")
                {
                    importePasaje2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importePasaje2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "auto")
                {
                    importeAuto2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAuto2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "hotel")
                {
                    importeHotel2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeHotel2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
                if (item.type == "aislamiento")
                {
                    importeAislamiento2 += item.Total;
                    if (!item.ClientIsCarrier)
                    {
                        importeAislamiento2Utilidad += item.Total - item.Cost - item.Charges;
                    }

                }
            }

            var auxenvios2 = _context.Order.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type != "Remesas" && x.Type != "Combo" && x.Date.Date >= dateayer2);
            decimal importeEnvios2 = auxenvios2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxenvios2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista);
            decimal importeEnvios2Utilidad = auxenvios2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.costoMayorista - x.ProductsShipping) + auxenvios2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - x.costoDeProveedor - x.OtrosCostos);

            var auxcombos2 = _context.Order.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Type == "Combo" && x.Date.Date.Date >= dateayer2);
            decimal importeCombos2 = auxcombos2.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount) + auxcombos2.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeCombos2Utilidad = auxcombos2.Where(x => x.AgencyId == aAgency.AgencyId && x.Minorista == null).Sum(x => x.Amount - x.costoMayorista - x.OtrosCostos - x.ProductsShipping) + auxcombos2.Where(x => (x.agencyTransferida.AgencyId == aAgency.AgencyId) || x.Minorista != null).Sum(x => x.costoMayorista - x.costoDeProveedor);


            var auxremesa2 = _context.Remittance.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
            decimal importeRemesas2 = auxremesa2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount) + auxremesa2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista + x.OtrosCostos);
            decimal importeRemesas2Utilidad = auxremesa2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - (x.costoMayorista + x.costoporDespacho + aAgency.remesa_entregaCuba)) + auxremesa2.Where(x => x.agencyTransferida.AgencyId == aAgency.AgencyId).Sum(x => x.costoMayorista - aAgency.remesa_entregaCuba);

            var auxPaqueteTuristico2 = _context.PaquetesTuristicos.Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= dateayer2);
            decimal importePaqueteTuristico2 = auxPaqueteTuristico2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount);
            decimal importePaqueteTuristico2Utilidad = auxPaqueteTuristico2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Amount - x.Costo - x.OtrosCostos);

            var auxrecarga2 = _context.Rechargue.Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.date.Date >= dateayer2);
            decimal importeRecarga2 = auxrecarga2.Sum(x => x.Import);
            decimal importeRecarga2Utilidad = auxrecarga2.Sum(x => x.Import - x.costoMayorista);

            var auxenviomaritimo2 = _context.EnvioMaritimo.Where(x => x.Status != "Cancelada" && x.AgencyId == aAgency.AgencyId && x.Date.Date >= dateayer2);
            decimal importeEnvioMaritimo = auxenviomaritimo2.Sum(x => x.Amount);
            decimal importeEnvioMaritimo2Utilidad = auxenviomaritimo2.Sum(x => x.Amount - x.costoMayorista);

            var auxpasaporte2 = _context.Passport.Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.FechaSolicitud.Date.Date >= dateayer2);
            decimal importepasaporte2 = auxpasaporte2.Where(x => x.AgencyId == aAgency.AgencyId).Sum(x => x.Total) + auxpasaporte2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
            decimal importepasaporteUtilidad2 = (await _reportService.UtilityByService(aAgency.AgencyId, STipo.Passport, dateayer2, datehoy)).Value.Sum(x => x.Utility);

            var auxcubiq2 = _context.OrderCubiqs.Include(x => x.Paquetes).Include(x => x.agencyTransferida).Where(x => x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.agencyTransferida.AgencyId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
            decimal importecubiq2 = 0;
            decimal importecubiqUtilidad2 = (await _reportService.UtilityByService(aAgency.AgencyId, STipo.Cubiq, dateayer2, datehoy)).Value.Sum(x => x.Utility);
            foreach (var item in auxcubiq2)
            {
                if (item.AgencyId == aAgency.AgencyId)
                {
                    importecubiq2 += item.Amount;
                }
                else if (item.agencyTransferida.AgencyId == aAgency.AgencyId)
                {
                    importecubiq2 += item.costoMayorista + item.OtrosCostos;
                }
            }

            var auxmercado2 = _context.Mercado.Where(x => x.Status != Mercado.STATUS_CANCELADA && x.AgencyId == aAgency.AgencyId && x.Date.Date >= dateayer2);
            decimal importeMercado2 = auxmercado2.Sum(x => x.Amount);
            decimal importeMercadoUtilidad2 = auxmercado2.Sum(x => x.Amount - x.Cargos);

            AuxVentasPorServicio ventasxservicio2 = new AuxVentasPorServicio();
            if (importeAuto2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto2));
            if (importePasaje2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje2));
            if (importeHotel2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel2));
            if (importeAislamiento2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento2));
            if (importeEnvios2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios2));
            if (importeCombos2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos2));
            if (importeRemesas2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas2));
            if (importePaqueteTuristico2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico2));
            if (importeRecarga2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga2));
            if (importeEnvioMaritimo != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo));
            if (importepasaporte2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporte2));
            if (importecubiq2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiq2));
            if (importeMercado2 != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Mercado", importeMercado2));

            AuxVentasPorServicio ventasxservicio2Utilidad = new AuxVentasPorServicio();
            if (importeAuto2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Auto", importeAuto2Utilidad));
            if (importePasaje2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaje", importePasaje2Utilidad));
            if (importeHotel2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Hotel/Vacaciones", importeHotel2Utilidad));
            if (importeAislamiento2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Aislamiento", importeAislamiento2Utilidad));
            if (importeEnvios2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envíos", importeEnvios2Utilidad));
            if (importeCombos2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Combos", importeCombos2Utilidad));
            if (importeRemesas2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Remesas", importeRemesas2Utilidad));
            if (importePaqueteTuristico2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Paquete Turístico", importePaqueteTuristico2Utilidad));
            if (importeRecarga2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Recarga", importeRecarga2Utilidad));
            if (importeEnvioMaritimo2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Marítimo", importeEnvioMaritimo2Utilidad));
            if (importepasaporteUtilidad2 != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Pasaporte", importepasaporteUtilidad2));
            if (importecubiqUtilidad2 != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Carga AM", importecubiqUtilidad2));
            if (importeMercadoUtilidad2 != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Mercado", importeMercadoUtilidad2));
            if (aAgency.Name == "Rapid Multiservice")
            {
                var auxenviocaribe2 = _context.EnvioCaribes.Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == aAgency.AgencyId || x.AgencyTransferidaId == aAgency.AgencyId) && x.Date.Date >= dateayer2);
                decimal importeenviocaribe = auxenviocaribe2.Where(x => x.AgencyId == user.AgencyId).Sum(x => x.Amount) + auxenviocaribe2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).Sum(x => x.costo + x.OtrosCostos);
                decimal importeenviocaribe2Utilidad = auxenviocaribe2.Where(x => x.AgencyId == user.AgencyId).Sum(x => x.Amount - x.costo - x.OtrosCostos) + GetCostoCaribe(auxenviocaribe2.Where(x => x.AgencyTransferidaId == aAgency.AgencyId).ToList(), _context);
                if (importeenviocaribe != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe));
                if (importeenviocaribe2Utilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>("Envío Caribe", importeenviocaribe2Utilidad));
            }

            servicios = _context.Servicios.Where(x => x.PaqueteTuristicoId == null && x.estado != "Cancelado" && x.agency.AgencyId == aAgency.AgencyId && x.fecha >= dateayer2.ToUniversalTime());
            foreach (var item in tiposervicios)
            {
                decimal sum = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal);
                decimal sumUtilidad = servicios.Where(x => x.tipoServicio == item).Sum(x => x.importeTotal - x.costoMayorista - x.CostoXServicio);
                if (sum != 0) ventasxservicio2.servicios.Add(new ParAux<string, decimal>(item.Nombre, sum));
                if (sumUtilidad != 0) ventasxservicio2Utilidad.servicios.Add(new ParAux<string, decimal>(item.Nombre, sumUtilidad));
            }
            response.ventasAyer = ventasxservicio2.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalVentasAyer = ventasxservicio2.getTotal();
            response.UtilidadAyer = ventasxservicio2Utilidad.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalUtilidadAyer = ventasxservicio2Utilidad.getTotal();

            //Ventas por empleado Hoy
            AuxVentasPorEmpleado ventasxempleado = new AuxVentasPorEmpleado();
            var dateHoyUtc = datehoy.Date.ToUniversalTime();
            var dateAyerUtc = dateayer.Date.ToUniversalTime();
            var ventasEmpleado = _context.RegistroPagosToday
                .AsNoTracking()
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date >= dateHoyUtc && x.tipoPago.Type != "Crédito de Consumo" && x.tipoPago.Type != "Cubiq" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.Remittance.Status != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada" && x.Mercado.Status != Mercado.STATUS_CANCELADA)
                .Where(x => x.Order.Minorista == null && x.Ticket.PaqueteTuristicoId == null && x.Servicio.PaqueteTuristicoId == null)
                .Select(x => new { x.User, x.valorPagado }).ToList()
                .GroupBy(x => x.User);

            foreach (var t in ventasEmpleado)
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionHoy = ventasxempleado.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionHoy = ventasxempleado.getTotal();

            //Ventas por empleado Ayer
            AuxVentasPorEmpleado ventasxempleado2 = new AuxVentasPorEmpleado();
            var ventasEmpleado2 = _context.RegistroPagos
                .AsNoTracking()
                .Where(x => x.AgencyId == aAgency.AgencyId && x.date < dateHoyUtc && x.date >= dateAyerUtc && x.tipoPago.Type != "Crédito de Consumo" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                .Where(x => x.EnvioCaribe.Status != "Cancelada" && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.EnvioMaritimo.Status != "Cancelada" && x.Order.Status != "Cancelada" && x.OrderCubiq.Status != "Cancelada" && x.Passport.Status != "Cancelada" && x.Rechargue.estado != "Cancelada" && x.Servicio.estado != "Cancelado" && x.Ticket.State != "Cancelada" && x.Mercado.Status != Mercado.STATUS_CANCELADA)
                .Where(x => x.Order.Minorista == null)
                .Select(x => new { x.User, x.valorPagado }).ToList()
                .GroupBy(x => x.User);

            foreach (var t in ventasEmpleado2)
            {
                string empleado = t.Key.Name + " " + t.Key.LastName;
                Guid idempleado = t.Key.UserId;
                decimal venta = t.Sum(x => x.valorPagado);
                ventasxempleado2.servicios.Add(new ParAux<ParAux<Guid, string>, decimal>(new ParAux<Guid, string>(idempleado, empleado), venta));
            }
            response.liquidacionAyer = ventasxempleado2.servicios.OrderByDescending(x => x.elem2).ToList();
            response.totalLiquidacionAyer = ventasxempleado2.getTotal();

            return response;
        }

        public static decimal GetCostoCaribe(List<EnvioCaribe> envioscaribe,databaseContext _context)
        {
            decimal costo = 0;
            //obtengo el mayorista 
            Agency agency = _context.Agency.FirstOrDefault(x => x.Name == "Rapid Multiservice");
            if (agency != null)
            {
                var mayorista = _context.Wholesalers.Include(x => x.tipoServicioHabana).Include(x => x.tipoServicioRestoProv).FirstOrDefault(x => x.EsVisible && x.AgencyId == agency.AgencyId && x.Category.category == "Maritimo-Aereo");
                if (mayorista != null)
                {
                    foreach (var item in envioscaribe)
                    {
                        List<TipoServicioMayorista> servicios = new List<TipoServicioMayorista>();
                        if (item.Contact.Address.City == "La Habana")
                        {
                            servicios = mayorista.tipoServicioHabana;
                        }
                        else
                        {
                            servicios = mayorista.tipoServicioRestoProv;
                        }
                        if (servicios != null)
                        {
                            if (item.servicio == "Correo-Aereo")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                costo += item.costo - ((item.PesoTotal * servicio.costoAereo) + item.OtrosCostos);
                            }
                            else if (item.servicio == "Correo-Maritimo")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                costo += item.costo - ((item.PesoTotal * servicio.costoMaritimo) + item.OtrosCostos);
                            }
                            else if (item.servicio == "Aerovaradero- Recogida")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Aereo_Varadero);
                                costo += item.costo - ((item.PesoTotal * servicio.costoAereo) + item.OtrosCostos);
                            }
                            else if (item.servicio == "Maritimo-Palco Almacen")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                costo += item.costo - ((item.PesoTotal * servicio.costoMaritimo) + item.OtrosCostos);
                            }
                            else if (item.servicio == "Palco ENTREGA A DOMICILIO")
                            {
                                var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                costo += item.costo - ((item.PesoTotal * servicio.costoAereo) + item.OtrosCostos);
                            }
                        }
                    }
                }

            }

            return costo;
        }

    }
}
