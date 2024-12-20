using Agenciapp.Common.Contrains;
using AgenciappHome.Models;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.IReportServices.Reports
{
    public static partial class Reporte
    {
        public static async Task<string> GetReporteVentasEmpleadoRapid(string rangeDate, User aUser, databaseContext _context, IHostingEnvironment _env)
        {
            Serilog.Log.Information("Inicio de Reporte");
            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {
                    var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId);

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    BaseColor colorcell = WebColors.GetRGBColor("#fdb4b4");

                    Agency agency = await aAgency.FirstOrDefaultAsync();
                    Address agencyAddress = await _context.Address.FirstOrDefaultAsync(a => a.ReferenceId == agency.AgencyId);
                    Phone agencyPhone = await _context.Phone.FirstOrDefaultAsync(p => p.ReferenceId == agency.AgencyId);

                    var auxDate = rangeDate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateIni = DateTime.Parse(auxDate[0], culture);
                    var dateFin = DateTime.Parse(auxDate[1], culture);

                    //Datos
                    var remesas = await _context.Remittance
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.Status != Remittance.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { registroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Date, x.Amount, x.UserId, x.Number, ClientName = x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var paquetesTuristicos = await _context.PaquetesTuristicos
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { registroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Date, x.Amount, x.UserId, x.Number, ClientName = x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var enviosmaritimos = await _context
                        .EnvioMaritimo
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.Status != EnvioMaritimo.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number, x.UserId, x.Date })
                        .ToListAsync();

                    var pasaportes = await _context.Passport
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => !x.AppMovil || (x.AppMovil && x.Status != Passport.STATUS_REVIEW))
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.FechaSolicitud.Date >= dateIni.Date && x.FechaSolicitud.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.Status != Passport.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.UserId, x.OrderNumber, x.Total, x.FechaSolicitud, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var envioscaribe = await _context.EnvioCaribes
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.User.Username != "Manuel14" && x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.ToLocalTime().Date >= dateIni.Date && x.Date.ToLocalTime().Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.Status != EnvioCaribe.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Number, x.Date, x.UserId, x.Amount, x.User.FullName, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var enviosaereos = await _context.Order
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Type != "Remesas" && x.Type != "Combo")
                        .AsNoTracking().Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.Status != Order.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), Bag = x.Bag.Select(y => new { y.BagItems, }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var envioscombos = await _context.Order
                        .AsNoTracking().Include(x => x.Bag).ThenInclude(x => x.BagItems)
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.Minorista)
                        .AsNoTracking().Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Type == "Combo")
                        .AsNoTracking().Where(x => (x.Pagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .AsNoTracking().Where(x => x.Minorista == null)
                        .Where(x => x.Status != Order.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.Pagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), Bag = x.Bag.Select(y => new { y.BagItems, }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var boletos = await _context.Ticket
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.PaqueteTuristicoId == null && x.AgencyId == agency.AgencyId && !x.ClientIsCarrier)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.State != Ticket.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.type, x.ReservationNumber, x.RegisterDate, x.UserId, x.Total, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var boletosCarrier = await _context.Ticket
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.PaqueteTuristicoId == null && x.AgencyId == agency.AgencyId && x.ClientIsCarrier)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.State != Ticket.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.type, x.ReservationNumber, x.RegisterDate, x.UserId, x.Total, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var recargas = await _context.Rechargue
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.date.Date >= dateIni.Date && x.date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.estado != Rechargue.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { x.Number, RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.date, x.UserId, x.Client.FullData, ClientPhone = x.Client.Phone.Number, x.Import })
                        .ToListAsync();

                    var envioscubiq = await _context.OrderCubiqs
                        .AsNoTracking().Include(x => x.Client).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.Status != OrderCubiq.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.Date, x.UserId, x.Number, x.Amount, x.Client.FullData, ClientPhone = x.Client.Phone.Number })
                        .ToListAsync();

                    var servicios = await _context.Servicios
                        .AsNoTracking().Include(x => x.cliente).ThenInclude(x => x.Phone)
                        .AsNoTracking().Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .AsNoTracking().Where(x => x.PaqueteTuristicoId == null && x.agency.AgencyId == aAgency.FirstOrDefault().AgencyId)
                        .AsNoTracking().Where(x => (x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId)) || (x.fecha.ToLocalTime().Date >= dateIni.Date && x.fecha.ToLocalTime().Date <= dateFin.Date) && x.UserId == aUser.UserId)
                        .Where(x => x.estado != Remittance.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .Select(x => new { RegistroPagos = x.RegistroPagos.Select(y => new { y.valorPagado, y.tipoPago, y.date, y.UserId }), x.tipoServicio, x.numero, x.importeTotal, x.fecha, x.cliente.FullData, ClientPhone = x.cliente.Phone.Number, x.UserId })
                        .ToListAsync();

                    Serilog.Log.Information("Datos obtenidos");

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        celllogo.AddElement(imagelogo);
                    }


                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    Phrase empl = new Phrase("Empleado: ", headFont);
                    empl.AddSpecial(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                    cellAgency.AddElement(empl);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = "";
                    if (dateIni.Date == dateFin.Date)
                    {
                        texto = dateIni.Date.ToShortDateString();
                    }
                    else
                    {
                        texto = dateIni.Date.ToShortDateString() + " a " + dateFin.Date.ToShortDateString();
                    }
                    Paragraph parPaq = new Paragraph("Liquidación del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);

                    Dictionary<string, decimal> ventastipopago = new Dictionary<string, decimal>();
                    Dictionary<string, int> canttipopago = new Dictionary<string, int>();
                    foreach (var item in _context.TipoPago)
                    {
                        ventastipopago.Add(item.Type, 0);
                        canttipopago.Add(item.Type, 0);
                    }

                    decimal tramitesTotal = 0;
                    decimal tramitesDeuda = 0;
                    decimal tramitesTotalPagado = 0;
                    decimal tramitesTotalCredito = 0;
                    decimal tramitesPagado = 0;
                    decimal tramitesCredito = 0;
                    decimal totalitems = 0;

                    PdfPTable tblremesasData;
                    float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    PdfPCell cellremesas1;
                    PdfPCell cellremesas2;
                    PdfPCell cellremesas3;
                    PdfPCell cellremesas4;
                    PdfPCell cellremesas5;
                    PdfPCell cellremesas6;
                    PdfPCell cellremesas7;
                    PdfPCell cellremesas8;
                    PdfPCell cellremesas9;


                    #region // REMESAS

                    decimal refRemesasPagado = 0;

                    if (remesas.Any())
                    {

                        tblremesasData = new PdfPTable(columnWidths);
                        tblremesasData.WidthPercentage = 100;

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refRemesasTotal = 0;
                        decimal refRemesasTotalPagado = 0;
                        decimal refRemesasDebe = 0;
                        decimal refRemesasCredito = 0;
                        decimal refRemesasTotalCredito = 0;

                        if (remesas.Count != 0)
                        {
                            doc.Add(new Phrase("Remesas", headFont));
                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            foreach (var remesa in remesas)
                            {
                                decimal pagado = 0;
                                decimal totalPagado = remesa.registroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = remesa.registroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal total = remesa.Amount;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in remesa.registroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > remesa.Date.Date || item.UserId != remesa.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > remesa.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refRemesasPagado += pagado;
                                refRemesasCredito += creditoConsumo;
                                tramitesPagado += totalPagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                                {
                                    refRemesasTotalCredito += totalCredito;
                                    refRemesasTotalPagado += totalPagado;
                                    refRemesasTotal += total;
                                    refRemesasDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesDeuda += debe;
                                    tramitesTotalCredito += tramitesTotalCredito;
                                }

                                var index = remesas.IndexOf(remesa);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(remesa.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(remesa.ClientName, normalFont));
                                cellremesas2.AddElement(new Phrase(remesa.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }
                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                            if (refRemesasCredito > 0)
                                cellremesas4.AddElement(new Phrase(refRemesasCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refRemesasTotalPagado.ToString(), headFont));
                            if (refRemesasTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refRemesasTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refRemesasTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refRemesasDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }


                    #endregion

                    #region // Paquete Turístico

                    decimal refPaqueteTuristicoPagado = 0;

                    if (paquetesTuristicos.Any())
                    {

                        tblremesasData = new PdfPTable(columnWidths);
                        tblremesasData.WidthPercentage = 100;

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refPaqueteTuristicoTotal = 0;
                        decimal refPaqueteTuristicoTotalPagado = 0;
                        decimal refPaqueteTuristicoDebe = 0;
                        decimal refPaqueteTuristicoCredito = 0;
                        decimal refPaqueteTuristicoTotalCredito = 0;

                        if (paquetesTuristicos.Count != 0)
                        {
                            doc.Add(new Phrase("Paquete Turístico", headFont));
                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            foreach (var paqueteTuristico in paquetesTuristicos)
                            {
                                decimal pagado = 0;
                                decimal totalPagado = paqueteTuristico.registroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = paqueteTuristico.registroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal total = paqueteTuristico.Amount;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in paqueteTuristico.registroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > paqueteTuristico.Date.Date || item.UserId != paqueteTuristico.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > paqueteTuristico.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refPaqueteTuristicoPagado += pagado;
                                refPaqueteTuristicoCredito += creditoConsumo;
                                tramitesPagado += totalPagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                                {
                                    refPaqueteTuristicoTotalCredito += totalCredito;
                                    refPaqueteTuristicoTotalPagado += totalPagado;
                                    refPaqueteTuristicoTotal += total;
                                    refPaqueteTuristicoDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesDeuda += debe;
                                    tramitesTotalCredito += tramitesTotalCredito;
                                }

                                var index = paquetesTuristicos.IndexOf(paqueteTuristico);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(paqueteTuristico.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(paqueteTuristico.ClientName, normalFont));
                                cellremesas2.AddElement(new Phrase(paqueteTuristico.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }
                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                            if (refPaqueteTuristicoCredito > 0)
                                cellremesas4.AddElement(new Phrase(refPaqueteTuristicoCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refPaqueteTuristicoTotalPagado.ToString(), headFont));
                            if (refPaqueteTuristicoTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refPaqueteTuristicoTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refPaqueteTuristicoTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refPaqueteTuristicoDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }


                    #endregion

                    #region // ENVIOS MARITIMOS

                    float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    decimal refPagadoEnvioM = 0;

                    if (enviosmaritimos.Any())
                    {
                        tblremesasData = new PdfPTable(columnWidthsmaritimo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refTotalEnvioM = 0;
                        decimal refCreditoEnvioM = 0;
                        decimal refTotalPagadoEnvioM = 0;
                        decimal refTotalCreditoEnvioM = 0;
                        decimal refDebeEnvioM = 0;
                        if (enviosmaritimos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Marítimos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var enviomaritimo in enviosmaritimos)
                            {
                                decimal total = enviomaritimo.Amount;
                                decimal Totalpagado = enviomaritimo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal TotalCredito = enviomaritimo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                bool diffDate = false;
                                bool colorearCell = false;
                                string tipoPagos = "";
                                foreach (var item in enviomaritimo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tipoPagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviomaritimo.Date.Date || item.UserId != enviomaritimo.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviomaritimo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - (Totalpagado + TotalCredito);

                                refPagadoEnvioM += pagado;
                                refCreditoEnvioM += creditoConsumo;
                                tramitesPagado += pagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate)
                                {
                                    refTotalEnvioM += total;
                                    refTotalPagadoEnvioM += Totalpagado;
                                    refDebeEnvioM += debe;
                                    refTotalCreditoEnvioM += TotalCredito;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += Totalpagado;
                                    tramitesTotalCredito += TotalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = enviosmaritimos.IndexOf(enviomaritimo);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviomaritimo.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviomaritimo.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(enviomaritimo.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                                if (TotalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(tipoPagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refPagadoEnvioM.ToString(), headFont));
                            if (refCreditoEnvioM > 0)
                                cellremesas4.AddElement(new Phrase(refCreditoEnvioM.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refTotalPagadoEnvioM.ToString(), headFont));
                            if (refTotalCreditoEnvioM > 0)
                                cellremesas5.AddElement(new Phrase(refTotalCreditoEnvioM.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refTotalEnvioM.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refDebeEnvioM.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // Pasaportes

                    decimal refPassportPagado = 0;

                    if (pasaportes.Any())
                    {
                        float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthspasaporte);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        if (AgencyName.IsDistrictCuba(agency.AgencyId))
                        {
                            pasaportes = pasaportes.OrderByDescending(x => x.OrderNumber).ToList();
                        }

                        decimal refPassportTotal = 0;
                        decimal refPassportCredito = 0;
                        decimal refPassportTotalPagado = 0;
                        decimal refPassportTotalCredito = 0;
                        decimal refPassportDebe = 0;
                        if (pasaportes.Count != 0)
                        {
                            doc.Add(new Phrase("Pasaportes", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var pasaporte in pasaportes)
                            {
                                decimal total = pasaporte.Total;
                                decimal totalPagado = pasaporte.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = pasaporte.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string pagos = "";
                                foreach (var item in pasaporte.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date || item.UserId != pasaporte.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refPassportPagado += pagado;
                                refPassportCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refPassportTotal += total;
                                    refPassportTotalPagado += totalPagado;
                                    refPassportTotalCredito += totalCredito;
                                    refPassportDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = pasaportes.IndexOf(pasaporte);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(pasaporte.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(pasaporte.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refPassportPagado.ToString(), headFont));
                            if (refPassportCredito > 0)
                                cellremesas4.AddElement(new Phrase(refPassportCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refPassportTotalPagado.ToString(), headFont));
                            if (refPassportTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refPassportTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refPassportTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refPassportDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS CARIBE
                    decimal refEnvioCaribePagado = 0;
                    if (envioscaribe.Any())
                    {
                        float[] columnWidthscaribe = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthscaribe);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;


                        decimal refEnvioCaribeTotal = 0;
                        decimal refEnvioCaribeCredito = 0;
                        decimal refEnvioCaribeTotalPagado = 0;
                        decimal refEnvioCaribeTotalCredito = 0;
                        decimal refEnvioCaribeDebe = 0;
                        if (envioscaribe.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Caribe", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var enviocaribe in envioscaribe)
                            {
                                decimal total = enviocaribe.Amount;
                                decimal Totalpagado = enviocaribe.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal TotalCredito = enviocaribe.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;

                                string tipopagosenviocaribe = "";
                                bool paintRow = false;
                                bool diffDate = false;
                                foreach (var item in enviocaribe.RegistroPagos.Where(y => y.date.ToLocalTime() >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tipopagosenviocaribe += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviocaribe.Date.Date || item.UserId != enviocaribe.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviocaribe.Date.Date)
                                            diffDate = true;
                                        paintRow = true;
                                    }
                                }

                                debe = total - (Totalpagado + TotalCredito);
                                refEnvioCaribePagado += pagado;
                                refEnvioCaribeCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;

                                if (!diffDate)
                                {
                                    refEnvioCaribeTotal += total;
                                    refEnvioCaribeTotalPagado += Totalpagado;
                                    refEnvioCaribeTotalCredito += TotalCredito;
                                    refEnvioCaribeDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += Totalpagado;
                                    tramitesTotalCredito += TotalCredito;
                                    tramitesDeuda += debe;
                                }



                                var index = envioscaribe.IndexOf(enviocaribe);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (paintRow)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviocaribe.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocaribe.FullName, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocaribe.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                                if (TotalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(tipopagosenviocaribe, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                            if (refEnvioCaribeCredito > 0)
                                cellremesas4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                            cellremesas5.AddElement(new Phrase(refEnvioCaribeCredito.ToString(), headRedFont));
                            if (refEnvioCaribeTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnvioCaribeTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnvioCaribeTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refEnvioCaribeDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS AEREOS
                    decimal refEnviosAereosPagado = 0;
                    if (enviosaereos.Any())
                    {
                        float[] columnWidthsaereo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthsaereo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refEnviosAereosTotal = 0;
                        decimal refEnviosAereosCredito = 0;
                        decimal refEnviosAereosTotalPagado = 0;
                        decimal refEnviosAereosTotalCredito = 0;
                        decimal refEnviosAereosDebe = 0;
                        if (enviosaereos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Aéreos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var envioaereo in enviosaereos)
                            {
                                decimal total = envioaereo.Amount;
                                decimal totalPagado = envioaereo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = envioaereo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var item in envioaereo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tiposdepagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > envioaereo.Date.Date || item.UserId != envioaereo.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > envioaereo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosAereosPagado += pagado;
                                refEnviosAereosCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosAereosTotal += total;
                                    refEnviosAereosTotalPagado += totalPagado;
                                    refEnviosAereosTotalCredito += totalCredito;
                                    refEnviosAereosDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                var index = enviosaereos.IndexOf(envioaereo);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;

                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(envioaereo.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(envioaereo.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(envioaereo.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refEnviosAereosPagado.ToString(), headFont));
                            if (refEnviosAereosCredito > 0)
                                cellremesas4.AddElement(new Phrase(refEnviosAereosCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refEnviosAereosTotalPagado.ToString(), headFont));
                            if (refEnviosAereosTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnviosAereosTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnviosAereosTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refEnviosAereosDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS COMBOS
                    decimal refEnviosCombosPagado = 0;

                    if (envioscombos.Any())
                    {
                        float[] columnWidthsCombo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthsCombo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;


                        decimal refEnviosCombosTotal = 0;
                        decimal refEnviosCombosCredito = 0;
                        decimal refEnviosCombosTotalPagado = 0;
                        decimal refEnviosCombosTotalCredito = 0;
                        decimal refEnviosCombosDebe = 0;
                        if (envioscombos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Combos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Cant", headFont));
                            cellremesas5.AddElement(new Phrase("Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas7.AddElement(new Phrase("Total", headFont));
                            cellremesas8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);


                            foreach (var enviocombo in envioscombos)
                            {
                                decimal total = enviocombo.Amount;
                                decimal totalPagado = enviocombo.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = enviocombo.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var item in enviocombo.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tiposdepagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviocombo.Date.Date || item.UserId != enviocombo.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviocombo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosCombosPagado += pagado;
                                refEnviosCombosCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosCombosTotal += total;
                                    refEnviosCombosTotalPagado += totalPagado;
                                    refEnviosCombosTotalCredito += totalCredito;
                                    refEnviosCombosDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                var index = envioscombos.IndexOf(enviocombo);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviocombo.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocombo.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocombo.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                var items = enviocombo.Bag.Select(x => x.BagItems);
                                int cantitems = 0;
                                foreach (var item in items)
                                {
                                    cantitems += item.Count();
                                }
                                totalitems += cantitems;
                                cellremesas4.AddElement(new Phrase(cantitems.ToString(), normalFont));
                                cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                                tblremesasData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(totalitems.ToString(), headFont));
                            cellremesas5.AddElement(new Phrase(refEnviosCombosPagado.ToString(), headFont));
                            if (refEnviosCombosCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnviosCombosCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnviosCombosTotalPagado.ToString(), headFont));
                            if (refEnviosCombosTotalCredito > 0)
                                cellremesas6.AddElement(new Phrase(refEnviosCombosTotalCredito.ToString(), headRedFont));
                            cellremesas7.AddElement(new Phrase(refEnviosCombosTotal.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase(refEnviosCombosDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // BOLETOS  
                    decimal refBoletoPagado = 0;

                    if (boletos.Any())
                    {
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthboletos);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;

                        decimal refBoletoTotal = 0;
                        decimal refBoletoCredito = 0;
                        decimal refBoletoTotalPagado = 0;
                        decimal refBoletoTotalCredito = 0;
                        decimal refBoletoDebe = 0;
                        if (boletos.Count != 0)
                        {
                            doc.Add(new Phrase("Boletos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Tipo", headFont));
                            cellremesas5.AddElement(new Phrase("Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas7.AddElement(new Phrase("Total", headFont));
                            cellremesas8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);


                            foreach (var boleto in boletos)
                            {
                                decimal total = boleto.Total;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = boleto.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = boleto.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in boleto.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date || item.UserId != boleto.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;
                                refBoletoPagado += pagado;
                                refBoletoCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refBoletoTotal += total;
                                    refBoletoTotalPagado += totalPagado;
                                    refBoletoTotalCredito += totalCredito;
                                    refBoletoDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = boletos.IndexOf(boleto);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                                cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                                tblremesasData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase("", normalFont));
                            cellremesas5.AddElement(new Phrase(refBoletoPagado.ToString(), headFont));
                            if (refBoletoCredito > 0)
                                cellremesas5.AddElement(new Phrase(refBoletoCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refBoletoTotalPagado.ToString(), headFont));
                            if (refBoletoTotalCredito > 0)
                                cellremesas6.AddElement(new Phrase(refBoletoTotalCredito.ToString(), headRedFont));
                            cellremesas7.AddElement(new Phrase(refBoletoTotal.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase(refBoletoDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // BOLETOS CARRIER 
                    decimal refBoletoCarrierPagado = 0;

                    if (boletosCarrier.Any())
                    {
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthboletos);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;


                        decimal refBoletoCarrierTotal = 0;
                        decimal refBoletoCarrierCredito = 0;
                        decimal refBoletoCarrierTotalPagado = 0;
                        decimal refBoletoCarrierTotalCredito = 0;
                        decimal refBoletoCarrierDebe = 0;
                        if (boletosCarrier.Count != 0)
                        {
                            doc.Add(new Phrase("Boletos Carrier", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Tipo", headFont));
                            cellremesas5.AddElement(new Phrase("Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas7.AddElement(new Phrase("Total", headFont));
                            cellremesas8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);


                            foreach (var boleto in boletosCarrier)
                            {
                                decimal total = boleto.Total;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = boleto.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = boleto.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in boleto.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date || item.UserId != boleto.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;
                                refBoletoCarrierPagado += pagado;
                                refBoletoCarrierCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refBoletoCarrierTotal += total;
                                    refBoletoCarrierTotalPagado += totalPagado;
                                    refBoletoCarrierTotalCredito += totalCredito;
                                    refBoletoCarrierDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = boletosCarrier.IndexOf(boleto);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                                cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                                tblremesasData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase("", normalFont));
                            cellremesas5.AddElement(new Phrase(refBoletoCarrierPagado.ToString(), headFont));
                            if (refBoletoCarrierCredito > 0)
                                cellremesas5.AddElement(new Phrase(refBoletoCarrierCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refBoletoCarrierTotalPagado.ToString(), headFont));
                            if (refBoletoCarrierTotalCredito > 0)
                                cellremesas6.AddElement(new Phrase(refBoletoCarrierTotalCredito.ToString(), headRedFont));
                            cellremesas7.AddElement(new Phrase(refBoletoCarrierTotal.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase(refBoletoCarrierDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // RECARGAS
                    decimal refRecharguePagado = 0;

                    if (recargas.Any())
                    {
                        float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthsrecarga);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refRechargueTotal = 0;
                        decimal refRechargueCredito = 0;
                        decimal refRechargueTotalPagado = 0;
                        decimal refRechargueTotalCredito = 0;
                        decimal refRechargueDebe = 0;
                        if (recargas.Count != 0)
                        {
                            doc.Add(new Phrase("RECARGAS", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var recarga in recargas)
                            {
                                decimal total = recarga.Import;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = recarga.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = recarga.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string pagos = "";
                                foreach (var item in recarga.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > recarga.date.Date || item.UserId != recarga.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > recarga.date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado + totalCredito;
                                refRecharguePagado += pagado;
                                refRechargueCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;

                                if (!diffDate)
                                {
                                    refRechargueTotal += total;
                                    refRechargueTotalPagado += totalPagado;
                                    refRechargueTotalCredito += totalCredito;
                                    refRechargueDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                var index = recargas.IndexOf(recarga);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(recarga.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(recarga.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(recarga.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refRecharguePagado.ToString(), headFont));
                            if (refRechargueCredito > 0)
                                cellremesas4.AddElement(new Phrase(refRechargueCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refRechargueTotalPagado.ToString(), headFont));
                            if (refRechargueTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refRechargueTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refRechargueTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refRechargueDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS CUBIQ
                    decimal refEnviosCubiqPagado = 0;

                    if (envioscubiq.Any())
                    {
                        float[] columnWidthCubiq = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthCubiq);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refEnviosCubiqTotal = 0;
                        decimal refEnviosCubiqCredito = 0;
                        decimal refEnviosCubiqTotalPagado = 0;
                        decimal refEnviosCubiqTotalCredito = 0;
                        decimal refEnviosCubiqDebe = 0;
                        if (envioscubiq.Count() != 0)
                        {
                            doc.Add(new Phrase("Envíos Carga AM", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var enviocubiq in envioscubiq)
                            {
                                decimal total = enviocubiq.Amount;
                                decimal totalPagado = enviocubiq.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo" && x.tipoPago.Type != "Cubiq").Sum(x => x.valorPagado);
                                decimal totalCredito = enviocubiq.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var item in enviocubiq.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                {
                                    tiposdepagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo" && item.tipoPago.Type != "Cubiq")
                                        pagado += item.valorPagado;
                                    else if (item.tipoPago.Type == "Crédito de Consumo")
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviocubiq.Date.Date || item.UserId != enviocubiq.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviocubiq.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosCubiqPagado += pagado;
                                refEnviosCubiqCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosCubiqTotal += total;
                                    refEnviosCubiqTotalPagado += totalPagado;
                                    refEnviosCubiqTotalCredito += totalCredito;
                                    refEnviosCubiqDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                var index = envioscubiq.IndexOf(enviocubiq);
                                if (index == 0)
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;

                                }
                                else
                                {
                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 0;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 0;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 0;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 0;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 0;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 0;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviocubiq.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocubiq.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocubiq.ClientPhone, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refEnviosCubiqPagado.ToString(), headFont));
                            if (refEnviosCubiqCredito > 0)
                                cellremesas4.AddElement(new Phrase(refEnviosCubiqCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refEnviosCubiqTotalPagado.ToString(), headFont));
                            if (refEnviosCubiqTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnviosCubiqTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnviosCubiqTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refEnviosCubiqDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region OTROS SERVICIOS
                    float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };

                    var tiposervicios = _context.TipoServicios
                            .Where(x => x.agency.AgencyId == agency.AgencyId);
                    Dictionary<string, decimal> creditoServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> creditoTotalServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> pagadoServicios = new Dictionary<string, decimal>();

                    if (servicios.Any())
                    {
                        //Hago una tabla para cada servicio
                        foreach (var tipo in tiposervicios)
                        {

                            var auxservicios = servicios.Where(x => x.tipoServicio.TipoServicioId == tipo.TipoServicioId).ToList();
                            if (auxservicios.Count() != 0)
                            {
                                tblremesasData = new PdfPTable(columnWidthsservicios);
                                tblremesasData.WidthPercentage = 100;
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                                doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                                cellremesas1.AddElement(new Phrase("No.", headFont));
                                cellremesas2.AddElement(new Phrase("Cliente", headFont));
                                cellremesas3.AddElement(new Phrase("Empleado", headFont));
                                cellremesas4.AddElement(new Phrase("Pagado", headFont));
                                cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                                cellremesas6.AddElement(new Phrase("Total", headFont));
                                cellremesas7.AddElement(new Phrase("Debe", headFont));
                                cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);

                                creditoServicios.Add(tipo.Nombre, 0);
                                creditoTotalServicios.Add(tipo.Nombre.ToString(), 0);
                                pagadoServicios.Add(tipo.Nombre, 0);
                                decimal refServicioTotal = 0;
                                decimal refServicioPagado = 0;
                                decimal refServicioTotalPagado = 0;
                                decimal refServicioDebe = 0;
                                foreach (var servicio in auxservicios)
                                {
                                    decimal total = servicio.importeTotal;
                                    decimal totalPagado = servicio.RegistroPagos.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                    decimal totalCredito = servicio.RegistroPagos.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                    decimal pagado = 0;
                                    decimal credito = 0;
                                    decimal debe = 0;
                                    string pagos = "";
                                    bool diffDate = false;
                                    bool colorearCell = false;
                                    foreach (var item in servicio.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date && y.UserId == aUser.UserId))
                                    {
                                        pagos += item.tipoPago.Type + ", ";
                                        if (item.tipoPago.Type == "Crédito de Consumo")
                                            credito += item.valorPagado;
                                        else
                                            pagado += item.valorPagado;
                                        ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                        canttipopago[item.tipoPago.Type] += 1;

                                        //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                        if (item.date.ToLocalTime().Date > servicio.fecha.Date || item.UserId != servicio.UserId)
                                        {
                                            if (item.date.ToLocalTime().Date > servicio.fecha.Date)
                                                diffDate = true;
                                            colorearCell = true;
                                        }
                                    }
                                    debe = total - totalPagado - totalCredito;
                                    creditoServicios[tipo.Nombre] += credito;
                                    pagadoServicios[tipo.Nombre] += pagado;
                                    tramitesPagado += pagado;
                                    tramitesCredito += credito;
                                    if (!diffDate)
                                    {
                                        refServicioTotal += total;
                                        refServicioPagado += pagado;
                                        refServicioTotalPagado += totalPagado;
                                        creditoTotalServicios[tipo.Nombre.ToString()] += totalCredito;
                                        refServicioDebe += debe;

                                        tramitesTotal += total;
                                        tramitesTotalPagado += totalPagado;
                                        tramitesTotalCredito += totalCredito;
                                        tramitesDeuda += debe;
                                    }

                                    var index = auxservicios.IndexOf(servicio);
                                    if (index == 0)
                                    {
                                        cellremesas1 = new PdfPCell();
                                        cellremesas1.Border = 1;
                                        cellremesas2 = new PdfPCell();
                                        cellremesas2.Border = 1;
                                        cellremesas3 = new PdfPCell();
                                        cellremesas3.Border = 1;
                                        cellremesas4 = new PdfPCell();
                                        cellremesas4.Border = 1;
                                        cellremesas5 = new PdfPCell();
                                        cellremesas5.Border = 1;
                                        cellremesas6 = new PdfPCell();
                                        cellremesas6.Border = 1;
                                        cellremesas7 = new PdfPCell();
                                        cellremesas7.Border = 1;
                                        cellremesas8 = new PdfPCell();
                                        cellremesas8.Border = 1;
                                    }
                                    else
                                    {
                                        cellremesas1 = new PdfPCell();
                                        cellremesas1.Border = 0;
                                        cellremesas2 = new PdfPCell();
                                        cellremesas2.Border = 0;
                                        cellremesas3 = new PdfPCell();
                                        cellremesas3.Border = 0;
                                        cellremesas4 = new PdfPCell();
                                        cellremesas4.Border = 0;
                                        cellremesas5 = new PdfPCell();
                                        cellremesas5.Border = 0;
                                        cellremesas6 = new PdfPCell();
                                        cellremesas6.Border = 0;
                                        cellremesas7 = new PdfPCell();
                                        cellremesas7.Border = 0;
                                        cellremesas8 = new PdfPCell();
                                        cellremesas8.Border = 0;
                                    }
                                    if (colorearCell)
                                    {
                                        cellremesas1.BackgroundColor = colorcell;
                                        cellremesas2.BackgroundColor = colorcell;
                                        cellremesas3.BackgroundColor = colorcell;
                                        cellremesas4.BackgroundColor = colorcell;
                                        cellremesas5.BackgroundColor = colorcell;
                                        cellremesas6.BackgroundColor = colorcell;
                                        cellremesas7.BackgroundColor = colorcell;
                                        cellremesas8.BackgroundColor = colorcell;
                                    }
                                    cellremesas1.AddElement(new Phrase(servicio.numero, normalFont));
                                    cellremesas2.AddElement(new Phrase(servicio.FullData, normalFont));
                                    cellremesas2.AddElement(new Phrase(servicio.ClientPhone, normalFont));
                                    cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                    cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                    if (credito > 0)
                                        cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                    cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                    if (totalCredito > 0 && !diffDate)
                                        cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                    cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                    cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                    cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                    tblremesasData.AddCell(cellremesas1);
                                    tblremesasData.AddCell(cellremesas2);
                                    tblremesasData.AddCell(cellremesas3);
                                    tblremesasData.AddCell(cellremesas4);
                                    tblremesasData.AddCell(cellremesas5);
                                    tblremesasData.AddCell(cellremesas6);
                                    tblremesasData.AddCell(cellremesas7);
                                    tblremesasData.AddCell(cellremesas8);
                                }

                                // Añado el total
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;

                                cellremesas1.AddElement(new Phrase("Totales", headFont));
                                cellremesas2.AddElement(new Phrase("", normalFont));
                                cellremesas3.AddElement(new Phrase("", normalFont));
                                cellremesas4.AddElement(new Phrase(refServicioPagado.ToString(), headFont));
                                if (creditoServicios[tipo.Nombre] > 0)
                                    cellremesas4.AddElement(new Phrase(creditoServicios[tipo.Nombre].ToString(), headRedFont));
                                cellremesas5.AddElement(new Phrase(refServicioTotalPagado.ToString(), headFont));
                                if (creditoTotalServicios[tipo.Nombre.ToString()] > 0)
                                    cellremesas5.AddElement(new Phrase(creditoTotalServicios[tipo.Nombre.ToString()].ToString(), headRedFont));
                                cellremesas6.AddElement(new Phrase(refServicioTotal.ToString(), headFont));
                                cellremesas7.AddElement(new Phrase(refServicioDebe.ToString(), headFont));
                                cellremesas8.AddElement(new Phrase("", normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);

                                // Añado la tabla al documento
                                doc.Add(tblremesasData);
                                doc.Add(Chunk.NEWLINE);
                                doc.Add(Chunk.NEWLINE);
                            }
                        }

                    }

                    #endregion

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    tblremesasData = new PdfPTable(columnWidthstipopago);
                    tblremesasData.WidthPercentage = 100;

                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;

                    cellremesas1.AddElement(new Phrase("Tipo de Pago", headFont));
                    cellremesas2.AddElement(new Phrase("Cantidad", headFont));
                    cellremesas3.AddElement(new Phrase("Importe", headFont));
                    tblremesasData.AddCell(cellremesas1);
                    tblremesasData.AddCell(cellremesas2);
                    tblremesasData.AddCell(cellremesas3);
                    foreach (var item in _context.TipoPago)
                    {

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;

                        cellremesas1.AddElement(new Phrase(item.Type, normalFont));
                        cellremesas2.AddElement(new Phrase(canttipopago[item.Type].ToString(), normalFont));
                        cellremesas3.AddElement(new Phrase(ventastipopago[item.Type].ToString(), normalFont));
                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                    }

                    doc.Add(tblremesasData);
                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 2 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    doc.Add(Chunk.NEWLINE);
                    cellleft.AddElement(new Phrase("RESUMEN DE TRÁMITES", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    //Remesas: 15 ordenes -- $360.00 usd
                    Phrase aux;
                    int auxcanttoal = 0;
                    int auxcant = remesas.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("REMESAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRemesasPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = paquetesTuristicos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PAQUETES TURÍSTICOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPaqueteTuristicoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosmaritimos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPagadoEnvioM.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = pasaportes.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + refPassportPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscaribe.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnvioCaribePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosaereos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosAereosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscubiq.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCubiqPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscombos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCombosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletosCarrier.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoCarrierPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = recargas.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRecharguePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    foreach (var item in tiposervicios)
                    {
                        var auxservicio = servicios.Where(x => x.tipoServicio.TipoServicioId == item.TipoServicioId).ToList();
                        auxcant = auxservicio.Count();
                        auxcanttoal += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase(item.Nombre.ToUpper() + ": ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + pagadoServicios[item.Nombre.ToString()].ToString() + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                    }
                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);
                    #endregion

                    #region //GRAN TOTAL

                    Paragraph pPagado = new Paragraph("Pagado Total: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesPagado.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    pPagado = new Paragraph("Crédito: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesCredito.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph totalp = new Paragraph("Total Ventas Día: ", headFont2);
                    totalp.AddSpecial(new Phrase("$ " + tramitesTotal.ToString(), normalFont2));
                    totalp.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(totalp);
                    Paragraph deuda = new Paragraph("Pendiente a pago: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + tramitesDeuda.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);
                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);
                    doc.Close();
                    Serilog.Log.Information("Fin de reporte");
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        public static async Task<string> GetReporteVentasEmpleado(string rangeDate, User aUser, databaseContext _context, IHostingEnvironment _env)
        {
            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {
                    var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId);

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    BaseColor colorcell = WebColors.GetRGBColor("#fdb4b4");


                    Agency agency = aAgency.FirstOrDefault();
                    Address agencyAddress = _context.Address.FirstOrDefault(a => a.ReferenceId == agency.AgencyId);
                    Phone agencyPhone = _context.Phone.FirstOrDefault(p => p.ReferenceId == agency.AgencyId);

                    var auxDate = rangeDate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateInit = DateTime.Parse(auxDate[0], culture);
                    var dateEnd = DateTime.Parse(auxDate[1], culture);

                    //Datos
                    var dateInitUtc = dateInit.Date.ToUniversalTime();
                    var dateEndUtc = dateEnd.Date.AddDays(1).ToUniversalTime();

                    List<RegistroPago> registrosPago = await _context.RegistroPagos
                        .Include(x => x.tipoPago)
                    .Where(x => x.UserId == aUser.UserId && x.date >= dateInitUtc && x.date <= dateEndUtc && x.tipoPago.Type != "Cubiq" && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                   .ToListAsync();

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        if (File.Exists(filePathQR))
                        {
                            iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                            imagelogo.ScaleAbsolute(75, 75);
                            celllogo.AddElement(imagelogo);
                        }
                    }

                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    Phrase empl = new Phrase("Empleado: ", headFont);
                    empl.AddSpecial(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                    cellAgency.AddElement(empl);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = "";
                    if (dateInit.Date == dateEnd.Date)
                    {
                        texto = dateInit.Date.ToShortDateString();
                    }
                    else
                    {
                        texto = dateInit.Date.ToShortDateString() + " a " + dateEnd.Date.ToShortDateString();
                    }
                    Paragraph parPaq = new Paragraph("Liquidación del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);

                    Dictionary<string, decimal> ventastipopago = new Dictionary<string, decimal>();
                    Dictionary<string, int> canttipopago = new Dictionary<string, int>();
                    foreach (var item in _context.TipoPago)
                    {
                        ventastipopago.Add(item.Type, 0);
                        canttipopago.Add(item.Type, 0);
                    }

                    decimal cantLibPaquete = 0;
                    decimal cantLibMedicina = 0;
                    decimal cantLbCargaMisc = 0;
                    decimal cantLbCargaDurad = 0;

                    var productos = new List<dynamic>();

                    decimal tramitesTotal = 0;
                    decimal tramitesDeuda = 0;
                    decimal tramitesTotalPagado = 0;
                    decimal tramitesTotalCredito = 0;
                    decimal tramitesPagado = 0;
                    decimal tramitesCredito = 0;
                    decimal totalitems = 0;
                    decimal tramitesGastos = 0;
                    decimal tramitesTotalGastos = 0;

                    PdfPTable tblremesasData;
                    float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    PdfPCell cellremesas1;
                    PdfPCell cellremesas2;
                    PdfPCell cellremesas3;
                    PdfPCell cellremesas4;
                    PdfPCell cellremesas5;
                    PdfPCell cellremesas6;
                    PdfPCell cellremesas7;
                    PdfPCell cellremesas8;
                    PdfPCell cellremesas9;

                    #region // REMESAS

                    decimal refRemesasPagado = 0;
                    int countRemesas = 0;
                    var paysByRemitances = registrosPago.Where(x => x.RemittanceId != null).GroupBy(x => x.RemittanceId).ToList();

                    if (paysByRemitances.Any())
                    {
                        tblremesasData = new PdfPTable(columnWidths);
                        tblremesasData.WidthPercentage = 100;

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refRemesasTotal = 0;
                        decimal refRemesasTotalPagado = 0;
                        decimal refRemesasDebe = 0;
                        decimal refRemesasCredito = 0;
                        decimal refRemesasTotalCredito = 0;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        foreach (var paysRemitance in paysByRemitances)
                        {
                            Remittance remesa = _context.Remittance
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .FirstOrDefault(x => x.RemittanceId == paysRemitance.Key);

                            if (remesa.Status == Remittance.STATUS_CANCELADA)
                                continue;

                            countRemesas++;
                            decimal pagado = 0;
                            decimal totalPagado = paysRemitance.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = paysRemitance.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal total = remesa.Amount;
                            decimal debe = 0;
                            decimal creditoConsumo = 0;

                            string pagos = "";
                            bool colorearCell = false;
                            bool diffDate = false;
                            foreach (var item in paysRemitance.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > remesa.Date.Date || item.UserId != remesa.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > remesa.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refRemesasPagado += pagado;
                            refRemesasCredito += creditoConsumo;
                            tramitesPagado += totalPagado;
                            tramitesCredito += creditoConsumo;

                            if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                            {
                                refRemesasTotalCredito += totalCredito;
                                refRemesasTotalPagado += totalPagado;
                                refRemesasTotal += total;
                                refRemesasDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesDeuda += debe;
                                tramitesTotalCredito += tramitesTotalCredito;
                            }

                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;

                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(remesa.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(remesa.Client?.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(remesa.Client?.Phone?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (creditoConsumo > 0)
                                cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                        if (refRemesasCredito > 0)
                            cellremesas4.AddElement(new Phrase(refRemesasCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refRemesasTotalPagado.ToString(), headFont));
                        if (refRemesasTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refRemesasTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refRemesasTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refRemesasDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        if (countRemesas > 0)
                        {
                            doc.Add(new Phrase("Remesas", headFont));
                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // Paquete Turístico
                    int countPaqueteTuristico = 0;
                    decimal refPaqueteTuristicoPagado = 0;
                    var paysByPaqueteTuristico = registrosPago.Where(x => x.PaqueteTuristicoId != null).GroupBy(x => x.PaqueteTuristicoId).ToList();

                    if (paysByPaqueteTuristico.Any())
                    {
                        tblremesasData = new PdfPTable(columnWidths);
                        tblremesasData.WidthPercentage = 100;

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refPaqueteTuristicoTotal = 0;
                        decimal refPaqueteTuristicoTotalPagado = 0;
                        decimal refPaqueteTuristicoDebe = 0;
                        decimal refPaqueteTuristicoCredito = 0;
                        decimal refPaqueteTuristicoTotalCredito = 0;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        foreach (var paysPaqueteTuristico in paysByPaqueteTuristico)
                        {
                            PaqueteTuristico paqueteTuristico = await _context.PaquetesTuristicos
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .FirstOrDefaultAsync(x => x.PaqueteId == paysPaqueteTuristico.Key);

                            if (paqueteTuristico.Status == PaqueteTuristico.STATUS_CANCELADA)
                                continue;

                            countPaqueteTuristico++;
                            decimal pagado = 0;
                            decimal totalPagado = paysPaqueteTuristico.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = paysPaqueteTuristico.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal total = paqueteTuristico.Amount;
                            decimal debe = 0;
                            decimal creditoConsumo = 0;

                            string pagos = "";
                            bool colorearCell = false;
                            bool diffDate = false;
                            foreach (var item in paysPaqueteTuristico.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > paqueteTuristico.Date.Date || item.UserId != paqueteTuristico.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > paqueteTuristico.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refPaqueteTuristicoPagado += pagado;
                            refPaqueteTuristicoCredito += creditoConsumo;
                            tramitesPagado += totalPagado;
                            tramitesCredito += creditoConsumo;

                            if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                            {
                                refPaqueteTuristicoTotalCredito += totalCredito;
                                refPaqueteTuristicoTotalPagado += totalPagado;
                                refPaqueteTuristicoTotal += total;
                                refPaqueteTuristicoDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesDeuda += debe;
                                tramitesTotalCredito += tramitesTotalCredito;
                            }

                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;

                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(paqueteTuristico.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(paqueteTuristico.Client?.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(paqueteTuristico.Client?.Phone?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (creditoConsumo > 0)
                                cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }
                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                        if (refPaqueteTuristicoCredito > 0)
                            cellremesas4.AddElement(new Phrase(refPaqueteTuristicoCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refPaqueteTuristicoTotalPagado.ToString(), headFont));
                        if (refPaqueteTuristicoTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refPaqueteTuristicoTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refPaqueteTuristicoTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refPaqueteTuristicoDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        if (countPaqueteTuristico > 0)
                        {
                            doc.Add(new Phrase("Paquete Turístico", headFont));
                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }


                    #endregion

                    #region // ENVIOS MARITIMOS
                    int countMaritimo = 0;
                    float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    decimal refPagadoEnvioM = 0;
                    var paysByMaritimos = registrosPago.Where(x => x.EnvioMaritimoId != null).GroupBy(x => x.EnvioMaritimoId).ToList();

                    if (paysByMaritimos.Any())
                    {
                        tblremesasData = new PdfPTable(columnWidthsmaritimo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refTotalEnvioM = 0;
                        decimal refCreditoEnvioM = 0;
                        decimal refTotalPagadoEnvioM = 0;
                        decimal refTotalCreditoEnvioM = 0;
                        decimal refDebeEnvioM = 0;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        foreach (var paysMaritimo in paysByMaritimos)
                        {
                            EnvioMaritimo enviomaritimo = await _context.EnvioMaritimo
                                .Include(x => x.Client).ThenInclude(x => x.Phone)
                                .Include(x => x.products).ThenInclude(x => x.Product).ThenInclude(x => x.ProductoBodega)
                                .FirstOrDefaultAsync(x => x.Id == paysMaritimo.Key);

                            if (enviomaritimo.Status == EnvioMaritimo.STATUS_CANCELADA)
                                continue;

                            productos.AddRange(enviomaritimo.products.Where(x => x.Product.ProductoBodega != null).Select(x => new {Product = x.Product.ProductoBodega, Qty = x.cantidad }));

                            countMaritimo++;
                            decimal total = enviomaritimo.Amount;
                            decimal Totalpagado = paysMaritimo.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal TotalCredito = paysMaritimo.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal debe = 0;
                            decimal creditoConsumo = 0;

                            bool diffDate = false;
                            bool colorearCell = false;
                            string tipoPagos = "";
                            foreach (var item in paysMaritimo.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                            {
                                tipoPagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > enviomaritimo.Date.Date || item.UserId != enviomaritimo.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > enviomaritimo.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - (Totalpagado + TotalCredito);

                            refPagadoEnvioM += pagado;
                            refCreditoEnvioM += creditoConsumo;
                            tramitesPagado += pagado;
                            tramitesCredito += creditoConsumo;

                            if (!diffDate)
                            {
                                refTotalEnvioM += total;
                                refTotalPagadoEnvioM += Totalpagado;
                                refDebeEnvioM += debe;
                                refTotalCreditoEnvioM += TotalCredito;

                                tramitesTotal += total;
                                tramitesTotalPagado += Totalpagado;
                                tramitesTotalCredito += TotalCredito;
                                tramitesDeuda += debe;
                            }

                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;

                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(enviomaritimo.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviomaritimo.Client?.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(enviomaritimo.Client?.Phone?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (creditoConsumo > 0)
                                cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                            if (TotalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tipoPagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoEnvioM.ToString(), headFont));
                        if (refCreditoEnvioM > 0)
                            cellremesas4.AddElement(new Phrase(refCreditoEnvioM.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refTotalPagadoEnvioM.ToString(), headFont));
                        if (refTotalCreditoEnvioM > 0)
                            cellremesas5.AddElement(new Phrase(refTotalCreditoEnvioM.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refTotalEnvioM.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refDebeEnvioM.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        if (countMaritimo > 0)
                        {
                            doc.Add(new Phrase("Envíos Marítimos", headFont));
                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // Pasaportes
                    int countPasaporte = 0;
                    decimal refPassportPagado = 0;
                    var paysByPasaportes = registrosPago.Where(x => x.PassportId != null).GroupBy(x => x.PassportId).ToList();
                    var passportsId = paysByPasaportes.Select(x => x.Key).ToList();
                    var pasaportes = await _context.Passport.Include(x => x.Client).ThenInclude(x => x.Phone).Where(x => x.Status != Passport.STATUS_CANCELADA && passportsId.Contains(x.PassportId)).ToListAsync();

                    if (pasaportes.Any())
                    {
                        float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthspasaporte);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        if (AgencyName.IsDistrictCuba(agency.AgencyId))
                        {
                            pasaportes = pasaportes.OrderByDescending(x => x.OrderNumber).ToList();
                        }

                        decimal refPassportTotal = 0;
                        decimal refPassportCredito = 0;
                        decimal refPassportTotalPagado = 0;
                        decimal refPassportTotalCredito = 0;
                        decimal refPassportDebe = 0;
                        if (pasaportes.Count != 0)
                        {
                            doc.Add(new Phrase("Pasaportes", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("Total", headFont));
                            cellremesas7.AddElement(new Phrase("Debe", headFont));
                            cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);


                            foreach (var pasaporte in pasaportes)
                            {
                                var paysPassport = paysByPasaportes.FirstOrDefault(x => x.Key == pasaporte.PassportId);
                                countPasaporte++;
                                decimal total = pasaporte.Total;
                                decimal totalPagado = paysPassport.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = paysPassport.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string pagos = "";
                                foreach (var item in paysPassport.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date || item.UserId != pasaporte.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refPassportPagado += pagado;
                                refPassportCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refPassportTotal += total;
                                    refPassportTotalPagado += totalPagado;
                                    refPassportTotalCredito += totalCredito;
                                    refPassportDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;

                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(pasaporte.Client?.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(pasaporte.Client?.Phone?.Number, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refPassportPagado.ToString(), headFont));
                            if (refPassportCredito > 0)
                                cellremesas4.AddElement(new Phrase(refPassportCredito.ToString(), headRedFont));
                            cellremesas5.AddElement(new Phrase(refPassportTotalPagado.ToString(), headFont));
                            if (refPassportTotalCredito > 0)
                                cellremesas5.AddElement(new Phrase(refPassportTotalCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refPassportTotal.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase(refPassportDebe.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS CARIBE
                    int countEnvioCaribe = 0;
                    decimal refEnvioCaribePagado = 0;
                    var paysByEnviosCaribe = registrosPago.Where(x => x.EnvioCaribeId != null).GroupBy(x => x.EnvioCaribeId).ToList();

                    if (paysByEnviosCaribe.Any())
                    {
                        float[] columnWidthscaribe = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthscaribe);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;


                        decimal refEnvioCaribeTotal = 0;
                        decimal refEnvioCaribeCredito = 0;
                        decimal refEnvioCaribeTotalPagado = 0;
                        decimal refEnvioCaribeTotalCredito = 0;
                        decimal refEnvioCaribeDebe = 0;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var paysEnvioCaribe in paysByEnviosCaribe)
                        {
                            EnvioCaribe enviocaribe = await _context.EnvioCaribes
                                .Include(x => x.Client).ThenInclude(x => x.Phone)
                                .FirstOrDefaultAsync(x => x.EnvioCaribeId == paysEnvioCaribe.Key);
                            if (enviocaribe.Status == EnvioCaribe.STATUS_CANCELADA)
                                continue;

                            countEnvioCaribe++;
                            decimal total = enviocaribe.Amount;
                            decimal Totalpagado = paysEnvioCaribe.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal TotalCredito = paysEnvioCaribe.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal debe = 0;

                            string tipopagosenviocaribe = "";
                            bool paintRow = false;
                            bool diffDate = false;
                            foreach (var item in enviocaribe.RegistroPagos.Where(y => y.date.ToLocalTime() >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                            {
                                tipopagosenviocaribe += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > enviocaribe.Date.Date || item.UserId != enviocaribe.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > enviocaribe.Date.Date)
                                        diffDate = true;
                                    paintRow = true;
                                }
                            }

                            debe = total - (Totalpagado + TotalCredito);
                            refEnvioCaribePagado += pagado;
                            refEnvioCaribeCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;

                            if (!diffDate)
                            {
                                refEnvioCaribeTotal += total;
                                refEnvioCaribeTotalPagado += Totalpagado;
                                refEnvioCaribeTotalCredito += TotalCredito;
                                refEnvioCaribeDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += Totalpagado;
                                tramitesTotalCredito += TotalCredito;
                                tramitesDeuda += debe;
                            }

                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;

                            if (paintRow)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(enviocaribe.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocaribe.Client?.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocaribe.Client?.Phone?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                            if (TotalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tipopagosenviocaribe, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                        if (refEnvioCaribeCredito > 0)
                            cellremesas4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refEnvioCaribeCredito.ToString(), headRedFont));
                        if (refEnvioCaribeTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refEnvioCaribeTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refEnvioCaribeTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refEnvioCaribeDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        if (countEnvioCaribe > 0)
                        {
                            doc.Add(new Phrase("Envíos Caribe", headFont));

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS AEREOS
                    int countEnvioAereo = 0;
                    decimal refEnviosAereosPagado = 0;
                    var paysByOrders = registrosPago.Where(x => x.OrderId != null).GroupBy(x => x.OrderId).ToList();
                    List<Order> combos = new List<Order>();
                    if (paysByOrders.Any())
                    {
                        float[] columnWidthsaereo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthsaereo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refEnviosAereosTotal = 0;
                        decimal refEnviosAereosCredito = 0;
                        decimal refEnviosAereosTotalPagado = 0;
                        decimal refEnviosAereosTotalCredito = 0;
                        decimal refEnviosAereosDebe = 0;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var paysOrder in paysByOrders)
                        {
                            Order envioaereo = await _context.Order
                                .Include(x => x.Bag).ThenInclude(x => x.BagItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductoBodega)
                                .Include(x => x.Client).ThenInclude(x => x.Phone)
                                .Include(x => x.Minorista)
                                .FirstOrDefaultAsync(x => x.OrderId == paysOrder.Key);

                            if (envioaereo.Status == Order.STATUS_CANCELADA)
                                continue;
                            else if (envioaereo.Type == "Combo")
                            {
                                combos.Add(envioaereo);
                                continue;
                            }

                            productos.AddRange(envioaereo.Bag.SelectMany(y => y.BagItems.Where(z => z.Product.ProductoBodega != null).Select(z => new { Product = z.Product.ProductoBodega, z.Qty })));
                            cantLibPaquete += envioaereo.CantLb;
                            cantLibMedicina += envioaereo.CantLbMedicina;

                            countEnvioAereo++;
                            decimal total = envioaereo.Amount;
                            decimal totalPagado = paysOrder.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = paysOrder.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal debe = 0;
                            bool colorearCell = false;
                            bool diffDate = false;
                            string tiposdepagos = "";
                            foreach (var item in paysOrder.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                            {
                                tiposdepagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > envioaereo.Date.Date || item.UserId != envioaereo.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > envioaereo.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refEnviosAereosPagado += pagado;
                            refEnviosAereosCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refEnviosAereosTotal += total;
                                refEnviosAereosTotalPagado += totalPagado;
                                refEnviosAereosTotalCredito += totalCredito;
                                refEnviosAereosDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            if (tiposdepagos == "")
                            {
                                tiposdepagos = "-";
                            }

                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;

                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(envioaereo.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(envioaereo.Client?.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(envioaereo.Client?.Phone?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tiposdepagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refEnviosAereosPagado.ToString(), headFont));
                        if (refEnviosAereosCredito > 0)
                            cellremesas4.AddElement(new Phrase(refEnviosAereosCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refEnviosAereosTotalPagado.ToString(), headFont));
                        if (refEnviosAereosTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refEnviosAereosTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refEnviosAereosTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refEnviosAereosDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        if (countEnvioAereo > 0)
                        {
                            doc.Add(new Phrase("Envíos Aéreos", headFont));
                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS COMBOS
                    int countCombos = 0;
                    decimal refEnviosCombosPagado = 0;
                    combos = combos.Where(x => x.Minorista == null).ToList();
                    if (combos.Any())
                    {
                        float[] columnWidthsCombo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthsCombo);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;


                        decimal refEnviosCombosTotal = 0;
                        decimal refEnviosCombosCredito = 0;
                        decimal refEnviosCombosTotalPagado = 0;
                        decimal refEnviosCombosTotalCredito = 0;
                        decimal refEnviosCombosDebe = 0;
                        if (combos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Combos", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Cant", headFont));
                            cellremesas5.AddElement(new Phrase("Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas7.AddElement(new Phrase("Total", headFont));
                            cellremesas8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);


                            foreach (var enviocombo in combos)
                            {
                                countCombos++;
                                var paysCombo = paysByOrders.FirstOrDefault(x => x.Key == enviocombo.OrderId);
                                decimal total = enviocombo.Amount;
                                decimal totalPagado = paysCombo.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = paysCombo.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var item in paysCombo.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                                {
                                    tiposdepagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > enviocombo.Date.Date || item.UserId != enviocombo.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > enviocombo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosCombosPagado += pagado;
                                refEnviosCombosCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosCombosTotal += total;
                                    refEnviosCombosTotalPagado += totalPagado;
                                    refEnviosCombosTotalCredito += totalCredito;
                                    refEnviosCombosDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                                cellremesas9 = new PdfPCell();
                                cellremesas9.Border = 1;

                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(enviocombo.Number, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocombo.Client?.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(enviocombo.Client?.Phone?.Number, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                var items = enviocombo.Bag.Select(x => x.BagItems);
                                int cantitems = 0;
                                foreach (var item in items)
                                {
                                    cantitems += item.Count();
                                }
                                totalitems += cantitems;
                                cellremesas4.AddElement(new Phrase(cantitems.ToString(), normalFont));
                                cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                                tblremesasData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(totalitems.ToString(), headFont));
                            cellremesas5.AddElement(new Phrase(refEnviosCombosPagado.ToString(), headFont));
                            if (refEnviosCombosCredito > 0)
                                cellremesas5.AddElement(new Phrase(refEnviosCombosCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refEnviosCombosTotalPagado.ToString(), headFont));
                            if (refEnviosCombosTotalCredito > 0)
                                cellremesas6.AddElement(new Phrase(refEnviosCombosTotalCredito.ToString(), headRedFont));
                            cellremesas7.AddElement(new Phrase(refEnviosCombosTotal.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase(refEnviosCombosDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // BOLETOS  
                    int countBoletos = 0;
                    decimal refBoletoPagado = 0;
                    var paysByTickets = registrosPago.Where(x => x.TicketId != null).GroupBy(x => x.TicketId).ToList();
                    List<Ticket> boletosCarrier = new List<Ticket>();
                    if (paysByTickets.Any())
                    {
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthboletos);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;

                        decimal refBoletoTotal = 0;
                        decimal refBoletoCredito = 0;
                        decimal refBoletoTotalPagado = 0;
                        decimal refBoletoTotalCredito = 0;
                        decimal refBoletoDebe = 0;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Tipo", headFont));
                        cellremesas5.AddElement(new Phrase("Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas7.AddElement(new Phrase("Total", headFont));
                        cellremesas8.AddElement(new Phrase("Debe", headFont));
                        cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        tblremesasData.AddCell(cellremesas9);


                        foreach (var paysTicket in paysByTickets)
                        {
                            Ticket boleto = await _context.Ticket
                                .Include(x => x.Client).ThenInclude(x => x.Phone)
                                .FirstOrDefaultAsync(x => x.TicketId == paysTicket.Key);
                            if (boleto.State == Ticket.STATUS_CANCELADA || boleto.PaqueteTuristicoId != null)
                                continue;
                            else if (boleto.ClientIsCarrier)
                            {
                                boletosCarrier.Add(boleto);
                                continue;
                            }
                            countBoletos++;
                            decimal total = boleto.Total;
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal totalPagado = paysTicket.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = paysTicket.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal debe = 0;
                            string pagos = "";
                            bool colorearCell = false;
                            bool diffDate = false;
                            foreach (var item in paysTicket.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date || item.UserId != boleto.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;
                            refBoletoPagado += pagado;
                            refBoletoCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refBoletoTotal += total;
                                refBoletoTotalPagado += totalPagado;
                                refBoletoTotalCredito += totalCredito;
                                refBoletoDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 1;

                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                                cellremesas9.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.Client?.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.Client?.Phone?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                            cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas9.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase("", normalFont));
                        cellremesas5.AddElement(new Phrase(refBoletoPagado.ToString(), headFont));
                        if (refBoletoCredito > 0)
                            cellremesas5.AddElement(new Phrase(refBoletoCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refBoletoTotalPagado.ToString(), headFont));
                        if (refBoletoTotalCredito > 0)
                            cellremesas6.AddElement(new Phrase(refBoletoTotalCredito.ToString(), headRedFont));
                        cellremesas7.AddElement(new Phrase(refBoletoTotal.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase(refBoletoDebe.ToString(), headFont));
                        cellremesas9.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);
                        tblremesasData.AddCell(cellremesas9);

                        if (countBoletos > 0)
                        {
                            doc.Add(new Phrase("Boletos", headFont));
                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // BOLETOS CARRIER 
                    decimal refBoletoCarrierPagado = 0;
                    int countBoletosCarrier = 0;
                    if (boletosCarrier.Any())
                    {
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };

                        tblremesasData = new PdfPTable(columnWidthboletos);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;


                        decimal refBoletoCarrierTotal = 0;
                        decimal refBoletoCarrierCredito = 0;
                        decimal refBoletoCarrierTotalPagado = 0;
                        decimal refBoletoCarrierTotalCredito = 0;
                        decimal refBoletoCarrierDebe = 0;
                        if (boletosCarrier.Count != 0)
                        {
                            doc.Add(new Phrase("Boletos Carrier", headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Tipo", headFont));
                            cellremesas5.AddElement(new Phrase("Pagado", headFont));
                            cellremesas6.AddElement(new Phrase("T. Pagado", headFont));
                            cellremesas7.AddElement(new Phrase("Total", headFont));
                            cellremesas8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);


                            foreach (var boleto in boletosCarrier)
                            {
                                var paysTicket = paysByTickets.FirstOrDefault(x => x.Key == boleto.TicketId);
                                countBoletosCarrier++;
                                decimal total = boleto.Total;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = paysTicket.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = paysTicket.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var item in paysTicket.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        pagado += item.valorPagado;
                                    else
                                        credito += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date || item.UserId != boleto.UserId)
                                    {
                                        if (item.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;
                                refBoletoCarrierPagado += pagado;
                                refBoletoCarrierCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refBoletoCarrierTotal += total;
                                    refBoletoCarrierTotalPagado += totalPagado;
                                    refBoletoCarrierTotalCredito += totalCredito;
                                    refBoletoCarrierDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                                cellremesas9 = new PdfPCell();
                                cellremesas9.Border = 1;

                                if (colorearCell)
                                {
                                    cellremesas1.BackgroundColor = colorcell;
                                    cellremesas2.BackgroundColor = colorcell;
                                    cellremesas3.BackgroundColor = colorcell;
                                    cellremesas4.BackgroundColor = colorcell;
                                    cellremesas5.BackgroundColor = colorcell;
                                    cellremesas6.BackgroundColor = colorcell;
                                    cellremesas7.BackgroundColor = colorcell;
                                    cellremesas8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.Client?.FullData, normalFont));
                                cellremesas2.AddElement(new Phrase(boleto.Client?.Phone?.Number, normalFont));
                                cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                                cellremesas5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cellremesas5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cellremesas6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cellremesas6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cellremesas7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);
                                tblremesasData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 0;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 0;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 0;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 0;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 0;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 0;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase("", normalFont));
                            cellremesas5.AddElement(new Phrase(refBoletoCarrierPagado.ToString(), headFont));
                            if (refBoletoCarrierCredito > 0)
                                cellremesas5.AddElement(new Phrase(refBoletoCarrierCredito.ToString(), headRedFont));
                            cellremesas6.AddElement(new Phrase(refBoletoCarrierTotalPagado.ToString(), headFont));
                            if (refBoletoCarrierTotalCredito > 0)
                                cellremesas6.AddElement(new Phrase(refBoletoCarrierTotalCredito.ToString(), headRedFont));
                            cellremesas7.AddElement(new Phrase(refBoletoCarrierTotal.ToString(), headFont));
                            cellremesas8.AddElement(new Phrase(refBoletoCarrierDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                            tblremesasData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // RECARGAS
                    decimal refRecharguePagado = 0;
                    int countRecargas = 0;
                    var paysByRecharges = registrosPago.Where(x => x.RechargueId != null).GroupBy(x => x.RechargueId).ToList();

                    if (paysByRecharges.Any())
                    {
                        float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthsrecarga);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refRechargueTotal = 0;
                        decimal refRechargueCredito = 0;
                        decimal refRechargueTotalPagado = 0;
                        decimal refRechargueTotalCredito = 0;
                        decimal refRechargueDebe = 0;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var paysRechargue in paysByRecharges)
                        {
                            Rechargue recarga = await _context.Rechargue
                                .Include(x => x.Client).ThenInclude(x => x.Phone)
                                .FirstOrDefaultAsync(x => x.RechargueId == paysRechargue.Key);

                            if (recarga.estado == Rechargue.STATUS_CANCELADA)
                                continue;

                            countRecargas++;
                            decimal total = recarga.Import;
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal totalPagado = paysRechargue.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal totalCredito = paysRechargue.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal debe = 0;
                            bool colorearCell = false;
                            bool diffDate = false;
                            string pagos = "";
                            foreach (var item in paysRechargue.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    pagado += item.valorPagado;
                                else
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > recarga.date.Date || item.UserId != recarga.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > recarga.date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado + totalCredito;
                            refRecharguePagado += pagado;
                            refRechargueCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;

                            if (!diffDate)
                            {
                                refRechargueTotal += total;
                                refRechargueTotalPagado += totalPagado;
                                refRechargueTotalCredito += totalCredito;
                                refRechargueDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;

                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(recarga.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(recarga.Client?.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(recarga.Client?.Phone?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refRecharguePagado.ToString(), headFont));
                        if (refRechargueCredito > 0)
                            cellremesas4.AddElement(new Phrase(refRechargueCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refRechargueTotalPagado.ToString(), headFont));
                        if (refRechargueTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refRechargueTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refRechargueTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refRechargueDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        if (countRecargas > 0)
                        {
                            doc.Add(new Phrase("RECARGAS", headFont));
                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS CUBIQ
                    decimal refEnviosCubiqPagado = 0;
                    var paysByCubiqs = registrosPago.Where(x => x.OrderCubiqId != null).GroupBy(x => x.OrderCubiqId).ToList();
                    int countCubics = 0;
                    if (paysByCubiqs.Any())
                    {
                        float[] columnWidthCubiq = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblremesasData = new PdfPTable(columnWidthCubiq);
                        tblremesasData.WidthPercentage = 100;
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 1;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 1;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 1;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 1;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 1;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 1;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 1;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 1;

                        decimal refEnviosCubiqTotal = 0;
                        decimal refEnviosCubiqCredito = 0;
                        decimal refEnviosCubiqTotalPagado = 0;
                        decimal refEnviosCubiqTotalCredito = 0;
                        decimal refEnviosCubiqDebe = 0;

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                        cellremesas6.AddElement(new Phrase("Total", headFont));
                        cellremesas7.AddElement(new Phrase("Debe", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (var paysCubiq in paysByCubiqs)
                        {
                            var enviocubiq = await _context.OrderCubiqs
                                .Include(x => x.Paquetes)
                                .Include(x => x.Client).ThenInclude(x => x.Phone)
                                .FirstOrDefaultAsync(x => x.OrderCubiqId == paysCubiq.Key);

                            if (enviocubiq.Status == OrderCubiq.STATUS_CANCELADA)
                                continue;

                            cantLbCargaMisc += enviocubiq.Paquetes.Where(x => x.Type == Domain.Enums.CubiqPackageType.Miscelaneo).Sum(x => x.PesoLb);
                            cantLbCargaDurad += enviocubiq.Paquetes.Where(x => x.Type != Domain.Enums.CubiqPackageType.Miscelaneo).Sum(x => x.PesoLb);

                            countCubics++;
                            decimal total = enviocubiq.Amount;
                            decimal totalPagado = paysCubiq.Where(x => x.tipoPago.Type != "Crédito de Consumo" && x.tipoPago.Type != "Cubiq").Sum(x => x.valorPagado);
                            decimal totalCredito = paysCubiq.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                            decimal pagado = 0;
                            decimal credito = 0;
                            decimal debe = 0;
                            bool colorearCell = false;
                            bool diffDate = false;
                            string tiposdepagos = "";
                            foreach (var item in enviocubiq.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                            {
                                tiposdepagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo" && item.tipoPago.Type != "Cubiq")
                                    pagado += item.valorPagado;
                                else if (item.tipoPago.Type == "Crédito de Consumo")
                                    credito += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;

                                //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                if (item.date.ToLocalTime().Date > enviocubiq.Date.Date || item.UserId != enviocubiq.UserId)
                                {
                                    if (item.date.ToLocalTime().Date > enviocubiq.Date.Date)
                                        diffDate = true;
                                    colorearCell = true;
                                }
                            }

                            debe = total - totalPagado - totalCredito;

                            refEnviosCubiqPagado += pagado;
                            refEnviosCubiqCredito += credito;
                            tramitesPagado += pagado;
                            tramitesCredito += credito;
                            if (!diffDate)
                            {
                                refEnviosCubiqTotal += total;
                                refEnviosCubiqTotalPagado += totalPagado;
                                refEnviosCubiqTotalCredito += totalCredito;
                                refEnviosCubiqDebe += debe;

                                tramitesTotal += total;
                                tramitesTotalPagado += totalPagado;
                                tramitesTotalCredito += totalCredito;
                                tramitesDeuda += debe;
                            }

                            if (tiposdepagos == "")
                            {
                                tiposdepagos = "-";
                            }

                            cellremesas1 = new PdfPCell();
                            cellremesas1.Border = 1;
                            cellremesas2 = new PdfPCell();
                            cellremesas2.Border = 1;
                            cellremesas3 = new PdfPCell();
                            cellremesas3.Border = 1;
                            cellremesas4 = new PdfPCell();
                            cellremesas4.Border = 1;
                            cellremesas5 = new PdfPCell();
                            cellremesas5.Border = 1;
                            cellremesas6 = new PdfPCell();
                            cellremesas6.Border = 1;
                            cellremesas7 = new PdfPCell();
                            cellremesas7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;

                            if (colorearCell)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                                cellremesas8.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(enviocubiq.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocubiq.Client?.FullData, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocubiq.Client?.Phone?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                            cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                            if (credito > 0)
                                cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                            cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                            if (totalCredito > 0 && !diffDate)
                                cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                            cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tiposdepagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;
                        cellremesas4 = new PdfPCell();
                        cellremesas4.Border = 0;
                        cellremesas5 = new PdfPCell();
                        cellremesas5.Border = 0;
                        cellremesas6 = new PdfPCell();
                        cellremesas6.Border = 0;
                        cellremesas7 = new PdfPCell();
                        cellremesas7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refEnviosCubiqPagado.ToString(), headFont));
                        if (refEnviosCubiqCredito > 0)
                            cellremesas4.AddElement(new Phrase(refEnviosCubiqCredito.ToString(), headRedFont));
                        cellremesas5.AddElement(new Phrase(refEnviosCubiqTotalPagado.ToString(), headFont));
                        if (refEnviosCubiqTotalCredito > 0)
                            cellremesas5.AddElement(new Phrase(refEnviosCubiqTotalCredito.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refEnviosCubiqTotal.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refEnviosCubiqDebe.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        if (countCubics > 0)
                        {
                            doc.Add(new Phrase("Envíos Carga AM", headFont));
                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region OTROS SERVICIOS
                    float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    var paysByServicios = registrosPago.Where(x => x.ServicioId != null).GroupBy(x => x.ServicioId).ToList();
                    var idsServicios = paysByServicios.Select(x => x.Key);
                    List<Servicio> servicios = await _context.Servicios
                        .Include(x => x.cliente).ThenInclude(x => x.Phone)
                        .Include(x => x.tipoServicio)
                        .Include(x => x.SubServicio)
                        .Where(x => x.estado != Servicio.EstadoCancelado && idsServicios.Contains(x.ServicioId)).ToListAsync();

                    var tiposervicios = _context.TipoServicios
                            .Where(x => x.agency.AgencyId == agency.AgencyId);
                    Dictionary<string, decimal> creditoServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> creditoTotalServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> pagadoServicios = new Dictionary<string, decimal>();

                    if (servicios.Any())
                    {
                        //Hago una tabla para cada servicio
                        foreach (var tipo in tiposervicios)
                        {

                            var auxservicios = servicios.Where(x => x.tipoServicio.TipoServicioId == tipo.TipoServicioId).ToList();
                            if (auxservicios.Count() != 0)
                            {
                                tblremesasData = new PdfPTable(columnWidthsservicios);
                                tblremesasData.WidthPercentage = 100;
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 1;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 1;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 1;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 1;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 1;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 1;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                                doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                                cellremesas1.AddElement(new Phrase("No.", headFont));
                                cellremesas2.AddElement(new Phrase("Cliente", headFont));
                                cellremesas3.AddElement(new Phrase("Empleado", headFont));
                                cellremesas4.AddElement(new Phrase("Pagado", headFont));
                                cellremesas5.AddElement(new Phrase("T. Pagado", headFont));
                                cellremesas6.AddElement(new Phrase("Total", headFont));
                                cellremesas7.AddElement(new Phrase("Debe", headFont));
                                cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);

                                creditoServicios.Add(tipo.Nombre, 0);
                                creditoTotalServicios.Add(tipo.Nombre.ToString(), 0);
                                pagadoServicios.Add(tipo.Nombre, 0);
                                decimal refServicioTotal = 0;
                                decimal refServicioPagado = 0;
                                decimal refServicioTotalPagado = 0;
                                decimal refServicioDebe = 0;
                                foreach (var servicio in auxservicios)
                                {
                                    var paysServicio = paysByServicios.FirstOrDefault(x => x.Key == servicio.ServicioId);
                                    decimal total = servicio.importeTotal;
                                    decimal totalPagado = paysServicio.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                    decimal totalCredito = paysServicio.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                    decimal pagado = 0;
                                    decimal credito = 0;
                                    decimal debe = 0;
                                    string pagos = "";
                                    bool diffDate = false;
                                    bool colorearCell = false;
                                    foreach (var item in paysServicio.Where(y => y.date.ToLocalTime().Date >= dateInit.Date && y.date.ToLocalTime().Date <= dateEnd.Date && y.UserId == aUser.UserId))
                                    {
                                        pagos += item.tipoPago.Type + ", ";
                                        if (item.tipoPago.Type == "Crédito de Consumo")
                                            credito += item.valorPagado;
                                        else
                                            pagado += item.valorPagado;
                                        ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                        canttipopago[item.tipoPago.Type] += 1;

                                        //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                        if (item.date.ToLocalTime().Date > servicio.fecha.Date || item.UserId != servicio.UserId)
                                        {
                                            if (item.date.ToLocalTime().Date > servicio.fecha.Date)
                                                diffDate = true;
                                            colorearCell = true;
                                        }
                                    }
                                    debe = total - totalPagado - totalCredito;
                                    creditoServicios[tipo.Nombre] += credito;
                                    pagadoServicios[tipo.Nombre] += pagado;

                                    if (servicio.tipoServicio.Nombre == "Gastos")
                                    {
                                        tramitesGastos += pagado;
                                        if (!diffDate)
                                        {
                                            tramitesTotalGastos += totalPagado;
                                        }
                                    }

                                    tramitesPagado += pagado;
                                    tramitesCredito += credito;

                                    if (!diffDate)
                                    {
                                        refServicioTotal += total;
                                        refServicioPagado += pagado;
                                        refServicioTotalPagado += totalPagado;
                                        creditoTotalServicios[tipo.Nombre.ToString()] += totalCredito;
                                        refServicioDebe += debe;

                                        tramitesTotal += total;
                                        tramitesTotalPagado += totalPagado;
                                        tramitesTotalCredito += totalCredito;
                                        tramitesDeuda += debe;
                                    }

                                    cellremesas1 = new PdfPCell();
                                    cellremesas1.Border = 1;
                                    cellremesas2 = new PdfPCell();
                                    cellremesas2.Border = 1;
                                    cellremesas3 = new PdfPCell();
                                    cellremesas3.Border = 1;
                                    cellremesas4 = new PdfPCell();
                                    cellremesas4.Border = 1;
                                    cellremesas5 = new PdfPCell();
                                    cellremesas5.Border = 1;
                                    cellremesas6 = new PdfPCell();
                                    cellremesas6.Border = 1;
                                    cellremesas7 = new PdfPCell();
                                    cellremesas7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;

                                    if (colorearCell)
                                    {
                                        cellremesas1.BackgroundColor = colorcell;
                                        cellremesas2.BackgroundColor = colorcell;
                                        cellremesas3.BackgroundColor = colorcell;
                                        cellremesas4.BackgroundColor = colorcell;
                                        cellremesas5.BackgroundColor = colorcell;
                                        cellremesas6.BackgroundColor = colorcell;
                                        cellremesas7.BackgroundColor = colorcell;
                                        cellremesas8.BackgroundColor = colorcell;
                                    }
                                    cellremesas1.AddElement(new Phrase(servicio.numero, normalFont));
                                    cellremesas2.AddElement(new Phrase(servicio.cliente?.FullData, normalFont));
                                    cellremesas2.AddElement(new Phrase(servicio.cliente?.Phone?.Number, normalFont));
                                    cellremesas3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                    cellremesas4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                    if (credito > 0)
                                        cellremesas4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                    cellremesas5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                    if (totalCredito > 0 && !diffDate)
                                        cellremesas5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                    cellremesas6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                    cellremesas7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                    cellremesas8.AddElement(new Phrase(pagos, normalFont));

                                    tblremesasData.AddCell(cellremesas1);
                                    tblremesasData.AddCell(cellremesas2);
                                    tblremesasData.AddCell(cellremesas3);
                                    tblremesasData.AddCell(cellremesas4);
                                    tblremesasData.AddCell(cellremesas5);
                                    tblremesasData.AddCell(cellremesas6);
                                    tblremesasData.AddCell(cellremesas7);
                                    tblremesasData.AddCell(cellremesas8);
                                }

                                // Añado el total
                                cellremesas1 = new PdfPCell();
                                cellremesas1.Border = 0;
                                cellremesas2 = new PdfPCell();
                                cellremesas2.Border = 0;
                                cellremesas3 = new PdfPCell();
                                cellremesas3.Border = 0;
                                cellremesas4 = new PdfPCell();
                                cellremesas4.Border = 0;
                                cellremesas5 = new PdfPCell();
                                cellremesas5.Border = 0;
                                cellremesas6 = new PdfPCell();
                                cellremesas6.Border = 0;
                                cellremesas7 = new PdfPCell();
                                cellremesas7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;

                                cellremesas1.AddElement(new Phrase("Totales", headFont));
                                cellremesas2.AddElement(new Phrase("", normalFont));
                                cellremesas3.AddElement(new Phrase("", normalFont));
                                cellremesas4.AddElement(new Phrase(refServicioPagado.ToString(), headFont));
                                if (creditoServicios[tipo.Nombre] > 0)
                                    cellremesas4.AddElement(new Phrase(creditoServicios[tipo.Nombre].ToString(), headRedFont));
                                cellremesas5.AddElement(new Phrase(refServicioTotalPagado.ToString(), headFont));
                                if (creditoTotalServicios[tipo.Nombre.ToString()] > 0)
                                    cellremesas5.AddElement(new Phrase(creditoTotalServicios[tipo.Nombre.ToString()].ToString(), headRedFont));
                                cellremesas6.AddElement(new Phrase(refServicioTotal.ToString(), headFont));
                                cellremesas7.AddElement(new Phrase(refServicioDebe.ToString(), headFont));
                                cellremesas8.AddElement(new Phrase("", normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
                                tblremesasData.AddCell(cellremesas8);

                                // Añado la tabla al documento
                                doc.Add(tblremesasData);
                                doc.Add(Chunk.NEWLINE);
                                doc.Add(Chunk.NEWLINE);
                            }
                        }

                    }

                    #endregion

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    tblremesasData = new PdfPTable(columnWidthstipopago);
                    tblremesasData.WidthPercentage = 100;

                    cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;

                    cellremesas1.AddElement(new Phrase("Tipo de Pago", headFont));
                    cellremesas2.AddElement(new Phrase("Cantidad", headFont));
                    cellremesas3.AddElement(new Phrase("Importe", headFont));
                    tblremesasData.AddCell(cellremesas1);
                    tblremesasData.AddCell(cellremesas2);
                    tblremesasData.AddCell(cellremesas3);
                    foreach (var item in _context.TipoPago)
                    {

                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;

                        cellremesas1.AddElement(new Phrase(item.Type, normalFont));
                        cellremesas2.AddElement(new Phrase(canttipopago[item.Type].ToString(), normalFont));
                        cellremesas3.AddElement(new Phrase(ventastipopago[item.Type].ToString(), normalFont));
                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                    }

                    doc.Add(tblremesasData);
                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 2 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    doc.Add(Chunk.NEWLINE);
                    cellleft.AddElement(new Phrase("RESUMEN DE TRÁMITES", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    //Remesas: 15 ordenes -- $360.00 usd
                    Phrase aux;
                    int auxcanttoal = 0;
                    int auxcant = countRemesas;
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("REMESAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRemesasPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = countPaqueteTuristico;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PAQUETES TURÍSTICOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPaqueteTuristicoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countMaritimo;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPagadoEnvioM.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countPasaporte;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + refPassportPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countEnvioCaribe;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnvioCaribePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countEnvioAereo;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosAereosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countCubics;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCubiqPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countCombos;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCombosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countBoletos;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countBoletosCarrier;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoCarrierPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = countRecargas;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRecharguePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    foreach (var item in tiposervicios)
                    {
                        var items = servicios.Where(x => x.tipoServicio.TipoServicioId == item.TipoServicioId).GroupBy(x => x.SubServicio).ToList();
                        foreach (var auxservicio in items)
                        {
                            auxcant = auxservicio.Count();
                            auxcanttoal += auxcant;

                            if (auxcant != 0)
                            {
                                string serviceName = auxservicio.Key != null ? $"{item.Nombre} {auxservicio.Key.Nombre}" : $"{item.Nombre}";
                                aux = new Phrase(serviceName.ToUpper() + ": ", headFont);
                                aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + pagadoServicios[item.Nombre.ToString()].ToString() + " usd", normalFont));
                                cellleft.AddElement(aux);
                            }
                        }
                       
                    }
                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);
                    #endregion

                    #region //GRAN TOTAL

                    Paragraph pPagado = new Paragraph("Pagado Total: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesPagado.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    pPagado = new Paragraph("Crédito: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesCredito.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    if (tramitesTotalGastos > 0)
                    {
                        pPagado = new Paragraph("Gastos: ", headFont2);
                        pPagado.AddSpecial(new Phrase("$ " + tramitesTotalGastos.ToString(), normalFont2));
                        pPagado.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(pPagado);
                    }
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph totalp = new Paragraph("Total Ventas Día: ", headFont2);
                    totalp.AddSpecial(new Phrase("$ " + tramitesTotal.ToString(), normalFont2));
                    totalp.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(totalp);
                    Paragraph deuda = new Paragraph("Pendiente a pago: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + tramitesDeuda.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);
                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    doc.Add(Chunk.NEWLINE);

                    // Paquete
                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase("Cantidad de libras", headFont));
                    PdfPTable tbl = new PdfPTable(new float[] { 2, 1, 1 });
                    tbl.WidthPercentage = 100;

                    new List<string> { "Empleado", "Paquete", "Medicina" }
                    .ForEach(x => { tbl.AddCell(new PdfPCell(new Phrase(x, headFont))); });

                    tbl.AddCell(new PdfPCell(new Phrase($"{aUser.Name} {aUser.LastName}", headFont)));
                    tbl.AddCell(new PdfPCell(new Phrase(cantLibPaquete.ToString("0.00") + " lb", headFont)));
                    tbl.AddCell(new PdfPCell(new Phrase(cantLibMedicina.ToString("0.00") + " lb", headFont)));
                    doc.Add(tbl);

                    doc.Add(Chunk.NEWLINE);

                    // Carga AM
                    if(cantLbCargaMisc > 0 || cantLbCargaDurad > 0)
                    {
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(new Phrase("Cantidad de libras Carga AM", headFont));
                        tbl = new PdfPTable(new float[] { 2, 1, 1 });
                        tbl.WidthPercentage = 100;

                        new List<string> { "Empleado", "Miscelaneo", "Duradero" }
                        .ForEach(x => { tbl.AddCell(new PdfPCell(new Phrase(x, headFont))); });

                        tbl.AddCell(new PdfPCell(new Phrase($"{aUser.Name} {aUser.LastName}", headFont)));
                        tbl.AddCell(new PdfPCell(new Phrase(cantLbCargaMisc.ToString("0.00") + " lb", headFont)));
                        tbl.AddCell(new PdfPCell(new Phrase(cantLbCargaDurad.ToString("0.00") + " lb", headFont)));
                        doc.Add(tbl);

                        doc.Add(Chunk.NEWLINE);
                    }

                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase("Productos tienda", headFont));
                    tbl = new PdfPTable(new float[] { 1, 1,1 });
                    tbl.WidthPercentage = 100;

                    new List<string> { "Cantidad", "Nombre", "Precio" }
                     .ForEach(x => { tbl.AddCell(new PdfPCell(new Phrase(x, headFont))); });
                    foreach (var item in productos.GroupBy(x => x.Product))
                    {
                        ProductoBodega product = item.Key;
                        int qty = item.Sum(x => x.Qty);
                        decimal price = ((decimal)product.PrecioVentaReferencial) * qty;
                        tbl.AddCell(new PdfPCell(new Phrase(product.Nombre, headFont)));
                        tbl.AddCell(new PdfPCell(new Phrase($"{qty}", headFont)));
                        tbl.AddCell(new PdfPCell(new Phrase(price.ToString("0.00"), headFont)));
                    }

                    doc.Add(tbl);

                    doc.Close();
                    Serilog.Log.Information("Fin de reporte");
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        public static async Task<string> GetReporteVentasEmpleadoToday(string rangeDate, User aUser, databaseContext _context, IHostingEnvironment _env)
        {
            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {
                    var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId);

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    BaseColor colorcell = WebColors.GetRGBColor("#fdb4b4");


                    Agency agency = aAgency.FirstOrDefault();
                    Address agencyAddress = _context.Address.FirstOrDefault(a => a.ReferenceId == agency.AgencyId);
                    Phone agencyPhone = _context.Phone.FirstOrDefault(p => p.ReferenceId == agency.AgencyId);

                    var auxDate = rangeDate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateIni = DateTime.Parse(auxDate[0], culture);
                    var dateFin = DateTime.Parse(auxDate[1], culture);

                    int qtyRemesas = 0;
                    int qtyPaquetesTuristicos = 0;
                    int qtyEnviosMaritimos = 0;
                    int qtyPasaportes = 0;
                    int qtyEnviosCaribe = 0;
                    int qtyEnviosAereos = 0;
                    int qtyEnviosCubiq = 0;
                    int qtyEnviosCombos = 0;
                    int qtyBoletos = 0;
                    int qtyBoletosCarrier = 0;
                    int qtyRecargas = 0;
                    int qtyMercado = 0;

                    var init = dateIni.Date.ToUniversalTime();
                    var end = dateFin.Date.AddDays(1).ToUniversalTime();

                    var pays = await _context.RegistroPagosToday
                        .Include(x => x.tipoPago)
                        .Include(x => x.EnvioCaribe).ThenInclude(x => x.Client)
                        .Include(x => x.EnvioMaritimo).ThenInclude(x => x.Client)
                        .Include(x => x.EnvioMaritimo).ThenInclude(x => x.products).ThenInclude(x => x.Product).ThenInclude(x => x.ProductoBodega)
                        .Include(x => x.Order).ThenInclude(x => x.Client)
                        .Include(x => x.Order).ThenInclude(x => x.Bag).ThenInclude(x => x.BagItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductoBodega)
                        .Include(x => x.OrderCubiq).ThenInclude(x => x.Client)
                        .Include(x => x.OrderCubiq).ThenInclude(x => x.Paquetes)
                        .Include(x => x.PaqueteTuristico).ThenInclude(x => x.Client)
                        .Include(x => x.Passport).ThenInclude(x => x.Client)
                        .Include(x => x.Rechargue).ThenInclude(x => x.Client)
                        .Include(x => x.Remittance).ThenInclude(x => x.Client)
                        .Include(x => x.Servicio).ThenInclude(x => x.cliente)
                        .Include(x => x.Servicio).ThenInclude(x => x.tipoServicio)
                        .Include(x => x.Servicio).ThenInclude(x => x.SubServicio)
                        .Include(x => x.Ticket).ThenInclude(x => x.Client)
                        .Include(x => x.Mercado).ThenInclude(x => x.Client)
                        .Include(x => x.Mercado).ThenInclude(x => x.Productos).ThenInclude(x => x.Product)
                        .Where(x => x.UserId == aUser.UserId && x.date >= init && x.date < end)
                        .ToListAsync();

                    // Logo de la agencia
                    float[] columnWidths1 = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths1);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        if (File.Exists(filePathQR))
                        {
                            iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                            imagelogo.ScaleAbsolute(75, 75);
                            celllogo.AddElement(imagelogo);
                        }
                    }

                    cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cellAgency.AddElement(telefono);

                    Phrase fax = new Phrase("Fecha: ", headFont);
                    fax.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cellAgency.AddElement(fax);

                    Phrase empl = new Phrase("Empleado: ", headFont);
                    empl.AddSpecial(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                    cellAgency.AddElement(empl);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    doc.Add(Chunk.NEWLINE);
                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    string texto = "";
                    if (dateIni.Date == dateFin.Date)
                    {
                        texto = dateIni.Date.ToShortDateString();
                    }
                    else
                    {
                        texto = dateIni.Date.ToShortDateString() + " a " + dateFin.Date.ToShortDateString();
                    }
                    Paragraph parPaq = new Paragraph("Liquidación del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);

                    Dictionary<string, decimal> ventastipopago = new Dictionary<string, decimal>();
                    Dictionary<string, int> canttipopago = new Dictionary<string, int>();
                    foreach (var item in _context.TipoPago)
                    {
                        ventastipopago.Add(item.Type, 0);
                        canttipopago.Add(item.Type, 0);
                    }

                    var productos = new List<dynamic>();

                    decimal cantLibPaquete = 0;
                    decimal cantLibMedicina = 0;
                    decimal cantLbCargaMisc = 0;
                    decimal cantLbCargaDurad = 0;

                    decimal tramitesTotal = 0;
                    decimal tramitesDeuda = 0;
                    decimal tramitesTotalPagado = 0;
                    decimal tramitesTotalCredito = 0;
                    decimal tramitesPagado = 0;
                    decimal tramitesCredito = 0;
                    decimal totalitems = 0;
                    decimal tramitesGastos = 0;
                    decimal tramitesTotalGastos = 0;

                    PdfPTable tblData;
                    float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    PdfPCell cell1;
                    PdfPCell cell2;
                    PdfPCell cell3;
                    PdfPCell cell4;
                    PdfPCell cell5;
                    PdfPCell cell6;
                    PdfPCell cell7;
                    PdfPCell cell8;
                    PdfPCell cellremesas9;


                    #region // REMESAS

                    decimal refRemesasPagado = 0;
                    var paysByRemittance = pays.Where(x => x.Remittance != null && x.Remittance.Status != Remittance.STATUS_CANCELADA).GroupBy(x => x.Remittance);
                    if (paysByRemittance.Any())
                    {

                        tblData = new PdfPTable(columnWidths);
                        tblData.WidthPercentage = 100;

                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;

                        decimal refRemesasTotal = 0;
                        decimal refRemesasTotalPagado = 0;
                        decimal refRemesasDebe = 0;
                        decimal refRemesasCredito = 0;
                        decimal refRemesasTotalCredito = 0;

                        if (paysByRemittance.Count() != 0)
                        {
                            doc.Add(new Phrase("Remesas", headFont));
                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            foreach (var item in paysByRemittance)
                            {
                                Remittance remesa = item.Key;

                                qtyRemesas++;

                                decimal pagado = 0;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal total = remesa.Amount;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var pay in item)
                                {
                                    pagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        creditoConsumo += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > remesa.Date.Date || pay.UserId != remesa.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > remesa.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refRemesasPagado += pagado;
                                refRemesasCredito += creditoConsumo;
                                tramitesPagado += totalPagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                                {
                                    refRemesasTotalCredito += totalCredito;
                                    refRemesasTotalPagado += totalPagado;
                                    refRemesasTotal += total;
                                    refRemesasDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesDeuda += debe;
                                    tramitesTotalCredito += tramitesTotalCredito;
                                }

                                //var index = PaysByRemittance.IndexOf(item);
                                var index = 0;
                                if (index == 0)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(remesa.Number, normalFont));
                                cell2.AddElement(new Phrase(remesa.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(remesa.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cell4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(pagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }
                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                            if (refRemesasCredito > 0)
                                cell4.AddElement(new Phrase(refRemesasCredito.ToString(), headRedFont));
                            cell5.AddElement(new Phrase(refRemesasTotalPagado.ToString(), headFont));
                            if (refRemesasTotalCredito > 0)
                                cell5.AddElement(new Phrase(refRemesasTotalCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refRemesasTotal.ToString(), headFont));
                            cell7.AddElement(new Phrase(refRemesasDebe.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // Paquete Turístico

                    decimal refPaqueteTuristicoPagado = 0;
                    var paysByPaqueteTuristico = pays.Where(x => x.PaqueteTuristico != null && x.PaqueteTuristico.Status != PaqueteTuristico.STATUS_CANCELADA).GroupBy(x => x.PaqueteTuristico);
                    if (paysByPaqueteTuristico.Any())
                    {

                        tblData = new PdfPTable(columnWidths);
                        tblData.WidthPercentage = 100;

                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;

                        decimal refPaqueteTuristicoTotal = 0;
                        decimal refPaqueteTuristicoTotalPagado = 0;
                        decimal refPaqueteTuristicoDebe = 0;
                        decimal refPaqueteTuristicoCredito = 0;
                        decimal refPaqueteTuristicoTotalCredito = 0;

                        if (paysByPaqueteTuristico.Count() != 0)
                        {
                            doc.Add(new Phrase("Paquete Turístico", headFont));
                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            foreach (var item in paysByPaqueteTuristico)
                            {
                                var paqueteTuristico = item.Key;

                                qtyPaquetesTuristicos++;

                                decimal pagado = 0;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal total = paqueteTuristico.Amount;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var pay in item)
                                {
                                    pagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        creditoConsumo += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > paqueteTuristico.Date.Date || pay.UserId != paqueteTuristico.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > paqueteTuristico.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refPaqueteTuristicoPagado += pagado;
                                refPaqueteTuristicoCredito += creditoConsumo;
                                tramitesPagado += totalPagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate) //Si el tramite no es diferente la fecha del pago a la fecha de creada
                                {
                                    refPaqueteTuristicoTotalCredito += totalCredito;
                                    refPaqueteTuristicoTotalPagado += totalPagado;
                                    refPaqueteTuristicoTotal += total;
                                    refPaqueteTuristicoDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesDeuda += debe;
                                    tramitesTotalCredito += tramitesTotalCredito;
                                }

                                //var index = paquetesTuristicos.IndexOf(paqueteTuristico);
                                var index = 0;
                                if (index == 0)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(paqueteTuristico.Number, normalFont));
                                cell2.AddElement(new Phrase(paqueteTuristico.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(paqueteTuristico.Client.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cell4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(pagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }
                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refRemesasPagado.ToString(), headFont));
                            if (refPaqueteTuristicoCredito > 0)
                                cell4.AddElement(new Phrase(refPaqueteTuristicoCredito.ToString(), headRedFont));
                            cell5.AddElement(new Phrase(refPaqueteTuristicoTotalPagado.ToString(), headFont));
                            if (refPaqueteTuristicoTotalCredito > 0)
                                cell5.AddElement(new Phrase(refPaqueteTuristicoTotalCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refPaqueteTuristicoTotal.ToString(), headFont));
                            cell7.AddElement(new Phrase(refPaqueteTuristicoDebe.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS MARITIMOS

                    float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    decimal refPagadoEnvioM = 0;
                    var paysByEnvioMaritimo = pays.Where(x => x.EnvioMaritimo != null && x.EnvioMaritimo.Status != EnvioMaritimo.STATUS_CANCELADA).GroupBy(x => x.EnvioMaritimo);
                    if (paysByEnvioMaritimo.Any())
                    {
                        tblData = new PdfPTable(columnWidthsmaritimo);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;

                        decimal refTotalEnvioM = 0;
                        decimal refCreditoEnvioM = 0;
                        decimal refTotalPagadoEnvioM = 0;
                        decimal refTotalCreditoEnvioM = 0;
                        decimal refDebeEnvioM = 0;
                        if (paysByEnvioMaritimo.Count() != 0)
                        {
                            doc.Add(new Phrase("Envíos Marítimos", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);


                            foreach (var item in paysByEnvioMaritimo)
                            {
                                var enviomaritimo = item.Key;
                                productos.AddRange(enviomaritimo.products.Where(x => x.Product.ProductoBodega != null).Select(x => new { Product = x.Product.ProductoBodega, Qty = x.cantidad, Id = x.Product.ProductoBodega.IdProducto }));
                                qtyEnviosMaritimos++;

                                decimal total = enviomaritimo.Amount;
                                decimal Totalpagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal TotalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal debe = 0;
                                decimal creditoConsumo = 0;

                                bool diffDate = false;
                                bool colorearCell = false;
                                string tipoPagos = "";
                                foreach (var pay in item)
                                {
                                    tipoPagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        creditoConsumo += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > enviomaritimo.Date.Date || pay.UserId != enviomaritimo.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > enviomaritimo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - (Totalpagado + TotalCredito);

                                refPagadoEnvioM += pagado;
                                refCreditoEnvioM += creditoConsumo;
                                tramitesPagado += pagado;
                                tramitesCredito += creditoConsumo;

                                if (!diffDate)
                                {
                                    refTotalEnvioM += total;
                                    refTotalPagadoEnvioM += Totalpagado;
                                    refDebeEnvioM += debe;
                                    refTotalCreditoEnvioM += TotalCredito;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += Totalpagado;
                                    tramitesTotalCredito += TotalCredito;
                                    tramitesDeuda += debe;
                                }

                                //var index = enviosmaritimos.IndexOf(enviomaritimo);
                                var index = 0;
                                if (index == 0)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(enviomaritimo.Number, normalFont));
                                cell2.AddElement(new Phrase(enviomaritimo.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(enviomaritimo.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (creditoConsumo > 0)
                                    cell4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                                if (TotalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(tipoPagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refPagadoEnvioM.ToString(), headFont));
                            if (refCreditoEnvioM > 0)
                                cell4.AddElement(new Phrase(refCreditoEnvioM.ToString(), headRedFont));
                            cell5.AddElement(new Phrase(refTotalPagadoEnvioM.ToString(), headFont));
                            if (refTotalCreditoEnvioM > 0)
                                cell5.AddElement(new Phrase(refTotalCreditoEnvioM.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refTotalEnvioM.ToString(), headFont));
                            cell7.AddElement(new Phrase(refDebeEnvioM.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // Pasaportes

                    decimal refPassportPagado = 0;
                    var paysByPassport = pays.Where(x => x.Passport != null && x.Passport.Status != Passport.STATUS_CANCELADA).GroupBy(x => x.Passport);
                    if (paysByPassport.Any())
                    {
                        float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };

                        tblData = new PdfPTable(columnWidthspasaporte);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;

                        if (agency.AgencyId == agencyDCubaId || agency.AgencyId == Guid.Parse("4E50B1EF-0EC1-4383-BCEE-3098744CDDD0"))
                        {
                            paysByPassport = paysByPassport.OrderByDescending(x => x.Key.OrderNumber).ToList();
                        }

                        decimal refPassportTotal = 0;
                        decimal refPassportCredito = 0;
                        decimal refPassportTotalPagado = 0;
                        decimal refPassportTotalCredito = 0;
                        decimal refPassportDebe = 0;
                        if (paysByPassport.Count() != 0)
                        {
                            doc.Add(new Phrase("Pasaportes", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);


                            foreach (var item in paysByPassport)
                            {
                                var pasaporte = item.Key;

                                qtyPasaportes++;

                                decimal total = pasaporte.Total;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string pagos = "";
                                foreach (var pay in item)
                                {
                                    pagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date || pay.UserId != pasaporte.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > pasaporte.FechaSolicitud.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refPassportPagado += pagado;
                                refPassportCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refPassportTotal += total;
                                    refPassportTotalPagado += totalPagado;
                                    refPassportTotalCredito += totalCredito;
                                    refPassportDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                //var index = pasaportes.IndexOf(pasaporte);
                                var index = 0;
                                if (index == 0)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                cell2.AddElement(new Phrase(pasaporte.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(pasaporte.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(pagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refPassportPagado.ToString(), headFont));
                            if (refPassportCredito > 0)
                                cell4.AddElement(new Phrase(refPassportCredito.ToString(), headRedFont));
                            cell5.AddElement(new Phrase(refPassportTotalPagado.ToString(), headFont));
                            if (refPassportTotalCredito > 0)
                                cell5.AddElement(new Phrase(refPassportTotalCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refPassportTotal.ToString(), headFont));
                            cell7.AddElement(new Phrase(refPassportDebe.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS CARIBE
                    decimal refEnvioCaribePagado = 0;
                    var paysByEnviosCaribe = pays.Where(x => x.EnvioCaribe != null && x.EnvioCaribe.Status != EnvioCaribe.STATUS_CANCELADA).GroupBy(x => x.EnvioCaribe);
                    if (paysByEnviosCaribe.Any())
                    {
                        float[] columnWidthscaribe = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthscaribe);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;


                        decimal refEnvioCaribeTotal = 0;
                        decimal refEnvioCaribeCredito = 0;
                        decimal refEnvioCaribeTotalPagado = 0;
                        decimal refEnvioCaribeTotalCredito = 0;
                        decimal refEnvioCaribeDebe = 0;
                        if (paysByEnviosCaribe.Count() != 0)
                        {
                            doc.Add(new Phrase("Envíos Caribe", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);


                            foreach (var item in paysByEnviosCaribe)
                            {
                                var enviocaribe = item.Key;

                                qtyEnviosCaribe++;

                                decimal total = enviocaribe.Amount;
                                decimal Totalpagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal TotalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;

                                string tipopagosenviocaribe = "";
                                bool paintRow = false;
                                bool diffDate = false;
                                foreach (var pay in item)
                                {
                                    tipopagosenviocaribe += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > enviocaribe.Date.Date || pay.UserId != enviocaribe.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > enviocaribe.Date.Date)
                                            diffDate = true;
                                        paintRow = true;
                                    }
                                }

                                debe = total - (Totalpagado + TotalCredito);
                                refEnvioCaribePagado += pagado;
                                refEnvioCaribeCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;

                                if (!diffDate)
                                {
                                    refEnvioCaribeTotal += total;
                                    refEnvioCaribeTotalPagado += Totalpagado;
                                    refEnvioCaribeTotalCredito += TotalCredito;
                                    refEnvioCaribeDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += Totalpagado;
                                    tramitesTotalCredito += TotalCredito;
                                    tramitesDeuda += debe;
                                }



                                //var index = envioscaribe.IndexOf(enviocaribe);
                                var index = 0;
                                if (index == 0)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                }
                                if (paintRow)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(enviocaribe.Number, normalFont));
                                cell2.AddElement(new Phrase(enviocaribe.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(enviocaribe.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : Totalpagado.ToString(), normalFont));
                                if (TotalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(TotalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(tipopagosenviocaribe, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                            if (refEnvioCaribeCredito > 0)
                                cell4.AddElement(new Phrase(refEnvioCaribePagado.ToString(), headFont));
                            cell5.AddElement(new Phrase(refEnvioCaribeCredito.ToString(), headRedFont));
                            if (refEnvioCaribeTotalCredito > 0)
                                cell5.AddElement(new Phrase(refEnvioCaribeTotalCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refEnvioCaribeTotal.ToString(), headFont));
                            cell7.AddElement(new Phrase(refEnvioCaribeDebe.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS AEREOS
                    decimal refEnviosAereosPagado = 0;
                    var paysByEnviosAereos = pays.Where(x => x.Order != null && x.Order.Status != Order.STATUS_CANCELADA && x.Order.Type != "Combo").GroupBy(x => x.Order);
                    if (paysByEnviosAereos.Any())
                    {
                        float[] columnWidthsaereo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthsaereo);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;

                        decimal refEnviosAereosTotal = 0;
                        decimal refEnviosAereosCredito = 0;
                        decimal refEnviosAereosTotalPagado = 0;
                        decimal refEnviosAereosTotalCredito = 0;
                        decimal refEnviosAereosDebe = 0;
                        if (paysByEnviosAereos.Count() != 0)
                        {
                            doc.Add(new Phrase("Envíos Aéreos", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);


                            foreach (var item in paysByEnviosAereos)
                            {
                                var envioaereo = item.Key;

                                productos.AddRange(envioaereo.Bag.SelectMany(y => y.BagItems.Where(z => z.Product.ProductoBodega != null).Select(z => new { Product = z.Product.ProductoBodega, z.Qty, Id = z.Product.ProductoBodega.IdProducto})));

                                cantLibPaquete += envioaereo.CantLb;
                                cantLibMedicina += envioaereo.CantLbMedicina;

                                qtyEnviosAereos++;

                                decimal total = envioaereo.Amount;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var pay in item)
                                {
                                    tiposdepagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > envioaereo.Date.Date || pay.UserId != envioaereo.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > envioaereo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosAereosPagado += pagado;
                                refEnviosAereosCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosAereosTotal += total;
                                    refEnviosAereosTotalPagado += totalPagado;
                                    refEnviosAereosTotalCredito += totalCredito;
                                    refEnviosAereosDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                //var index = enviosaereos.IndexOf(envioaereo);
                                cell1 = new PdfPCell();
                                cell1.Border = 1;
                                cell2 = new PdfPCell();
                                cell2.Border = 1;
                                cell3 = new PdfPCell();
                                cell3.Border = 1;
                                cell4 = new PdfPCell();
                                cell4.Border = 1;
                                cell5 = new PdfPCell();
                                cell5.Border = 1;
                                cell6 = new PdfPCell();
                                cell6.Border = 1;
                                cell7 = new PdfPCell();
                                cell7.Border = 1;
                                cell8 = new PdfPCell();
                                cell8.Border = 1;

                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(envioaereo.Number, normalFont));
                                cell2.AddElement(new Phrase(envioaereo.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(envioaereo.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refEnviosAereosPagado.ToString(), headFont));
                            if (refEnviosAereosCredito > 0)
                                cell4.AddElement(new Phrase(refEnviosAereosCredito.ToString(), headRedFont));
                            cell5.AddElement(new Phrase(refEnviosAereosTotalPagado.ToString(), headFont));
                            if (refEnviosAereosTotalCredito > 0)
                                cell5.AddElement(new Phrase(refEnviosAereosTotalCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refEnviosAereosTotal.ToString(), headFont));
                            cell7.AddElement(new Phrase(refEnviosAereosDebe.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS COMBOS
                    decimal refEnviosCombosPagado = 0;
                    paysByEnviosAereos = pays.Where(x => x.Order != null && x.Order.Status != Order.STATUS_CANCELADA && x.Order.Type == "Combo").GroupBy(x => x.Order);

                    if (paysByEnviosAereos.Any())
                    {
                        float[] columnWidthsCombo = { (float)1.5, 2, 2, 1, 1, 1, 1, 1, 1 };

                        tblData = new PdfPTable(columnWidthsCombo);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;


                        decimal refEnviosCombosTotal = 0;
                        decimal refEnviosCombosCredito = 0;
                        decimal refEnviosCombosTotalPagado = 0;
                        decimal refEnviosCombosTotalCredito = 0;
                        decimal refEnviosCombosDebe = 0;
                        if (paysByEnviosAereos.Count() != 0)
                        {
                            doc.Add(new Phrase("Envíos Combos", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Cant", headFont));
                            cell5.AddElement(new Phrase("Pagado", headFont));
                            cell6.AddElement(new Phrase("T. Pagado", headFont));
                            cell7.AddElement(new Phrase("Total", headFont));
                            cell8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);
                            tblData.AddCell(cellremesas9);


                            foreach (var item in paysByEnviosAereos)
                            {
                                var enviocombo = item.Key;

                                if (enviocombo == null)
                                    continue;

                                qtyEnviosCombos++;

                                decimal total = enviocombo.Amount;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var pay in item)
                                {
                                    tiposdepagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > enviocombo.Date.Date || pay.UserId != enviocombo.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > enviocombo.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosCombosPagado += pagado;
                                refEnviosCombosCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosCombosTotal += total;
                                    refEnviosCombosTotalPagado += totalPagado;
                                    refEnviosCombosTotalCredito += totalCredito;
                                    refEnviosCombosDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                //var index = envioscombos.IndexOf(enviocombo);
                                if (true)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(enviocombo.Number, normalFont));
                                cell2.AddElement(new Phrase(enviocombo.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(enviocombo.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                var items = enviocombo.Bag.Select(x => x.BagItems);
                                int cantitems = 0;
                                foreach (var bagItem in items)
                                {
                                    cantitems += bagItem.Count();
                                }
                                totalitems += cantitems;
                                cell4.AddElement(new Phrase(cantitems.ToString(), normalFont));
                                cell5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                                tblData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(totalitems.ToString(), headFont));
                            cell5.AddElement(new Phrase(refEnviosCombosPagado.ToString(), headFont));
                            if (refEnviosCombosCredito > 0)
                                cell5.AddElement(new Phrase(refEnviosCombosCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refEnviosCombosTotalPagado.ToString(), headFont));
                            if (refEnviosCombosTotalCredito > 0)
                                cell6.AddElement(new Phrase(refEnviosCombosTotalCredito.ToString(), headRedFont));
                            cell7.AddElement(new Phrase(refEnviosCombosTotal.ToString(), headFont));
                            cell8.AddElement(new Phrase(refEnviosCombosDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);
                            tblData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // BOLETOS  
                    decimal refBoletoPagado = 0;
                    var paysByTicket = pays.Where(x => x.Ticket != null && !x.Ticket.ClientIsCarrier && x.Ticket.State != Ticket.STATUS_CANCELADA).GroupBy(x => x.Ticket);
                    if (paysByTicket.Any())
                    {
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthboletos);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;

                        decimal refBoletoTotal = 0;
                        decimal refBoletoCredito = 0;
                        decimal refBoletoTotalPagado = 0;
                        decimal refBoletoTotalCredito = 0;
                        decimal refBoletoDebe = 0;
                        if (paysByTicket.Count() != 0)
                        {
                            doc.Add(new Phrase("Boletos", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Tipo", headFont));
                            cell5.AddElement(new Phrase("Pagado", headFont));
                            cell6.AddElement(new Phrase("T. Pagado", headFont));
                            cell7.AddElement(new Phrase("Total", headFont));
                            cell8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);
                            tblData.AddCell(cellremesas9);


                            foreach (var item in paysByTicket)
                            {
                                var boleto = item.Key;

                                qtyBoletos++;

                                decimal total = boleto.Total;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var pay in item)
                                {
                                    pagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > boleto.RegisterDate.Date || pay.UserId != boleto.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;
                                refBoletoPagado += pagado;
                                refBoletoCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refBoletoTotal += total;
                                    refBoletoTotalPagado += totalPagado;
                                    refBoletoTotalCredito += totalCredito;
                                    refBoletoDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                //var index = boletos.IndexOf(boleto);
                                if (true)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                                cell2.AddElement(new Phrase(boleto.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(boleto.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(boleto.type, normalFont));
                                cell5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(pagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                                tblData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase("", normalFont));
                            cell5.AddElement(new Phrase(refBoletoPagado.ToString(), headFont));
                            if (refBoletoCredito > 0)
                                cell5.AddElement(new Phrase(refBoletoCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refBoletoTotalPagado.ToString(), headFont));
                            if (refBoletoTotalCredito > 0)
                                cell6.AddElement(new Phrase(refBoletoTotalCredito.ToString(), headRedFont));
                            cell7.AddElement(new Phrase(refBoletoTotal.ToString(), headFont));
                            cell8.AddElement(new Phrase(refBoletoDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);
                            tblData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // BOLETOS CARRIER 
                    decimal refBoletoCarrierPagado = 0;
                    paysByTicket = pays.Where(x => x.Ticket != null && x.Ticket.ClientIsCarrier && x.Ticket.State != Ticket.STATUS_CANCELADA).GroupBy(x => x.Ticket);
                    if (paysByTicket.Any())
                    {
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, 1, 1, 1, 1, 1 };

                        tblData = new PdfPTable(columnWidthboletos);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;
                        cellremesas9 = new PdfPCell();
                        cellremesas9.Border = 1;


                        decimal refBoletoCarrierTotal = 0;
                        decimal refBoletoCarrierCredito = 0;
                        decimal refBoletoCarrierTotalPagado = 0;
                        decimal refBoletoCarrierTotalCredito = 0;
                        decimal refBoletoCarrierDebe = 0;
                        if (paysByTicket.Count() != 0)
                        {
                            doc.Add(new Phrase("Boletos Carrier", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Tipo", headFont));
                            cell5.AddElement(new Phrase("Pagado", headFont));
                            cell6.AddElement(new Phrase("T. Pagado", headFont));
                            cell7.AddElement(new Phrase("Total", headFont));
                            cell8.AddElement(new Phrase("Debe", headFont));
                            cellremesas9.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);
                            tblData.AddCell(cellremesas9);


                            foreach (var item in paysByTicket)
                            {
                                var boleto = item.Key;

                                if (boleto == null)
                                    continue;

                                qtyBoletosCarrier++;

                                decimal total = boleto.Total;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                string pagos = "";
                                bool colorearCell = false;
                                bool diffDate = false;
                                foreach (var pay in item)
                                {
                                    pagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > boleto.RegisterDate.Date || pay.UserId != boleto.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > boleto.RegisterDate.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;
                                refBoletoCarrierPagado += pagado;
                                refBoletoCarrierCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refBoletoCarrierTotal += total;
                                    refBoletoCarrierTotalPagado += totalPagado;
                                    refBoletoCarrierTotalCredito += totalCredito;
                                    refBoletoCarrierDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                //var index = boletosCarrier.IndexOf(boleto);
                                if (true)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                    cellremesas9.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                                cell2.AddElement(new Phrase(boleto.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(boleto.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(boleto.type, normalFont));
                                cell5.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell5.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell6.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell8.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(pagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                                tblData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase("", normalFont));
                            cell5.AddElement(new Phrase(refBoletoCarrierPagado.ToString(), headFont));
                            if (refBoletoCarrierCredito > 0)
                                cell5.AddElement(new Phrase(refBoletoCarrierCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refBoletoCarrierTotalPagado.ToString(), headFont));
                            if (refBoletoCarrierTotalCredito > 0)
                                cell6.AddElement(new Phrase(refBoletoCarrierTotalCredito.ToString(), headRedFont));
                            cell7.AddElement(new Phrase(refBoletoCarrierTotal.ToString(), headFont));
                            cell8.AddElement(new Phrase(refBoletoCarrierDebe.ToString(), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);
                            tblData.AddCell(cellremesas9);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // RECARGAS
                    decimal refRecharguePagado = 0;
                    var paysByRechargues = pays.Where(x => x.Rechargue != null && x.Rechargue.estado != Rechargue.STATUS_CANCELADA).GroupBy(x => x.Rechargue);
                    if (paysByRechargues.Any())
                    {
                        float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthsrecarga);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;

                        decimal refRechargueTotal = 0;
                        decimal refRechargueCredito = 0;
                        decimal refRechargueTotalPagado = 0;
                        decimal refRechargueTotalCredito = 0;
                        decimal refRechargueDebe = 0;
                        if (paysByRechargues.Count() != 0)
                        {
                            doc.Add(new Phrase("RECARGAS", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);


                            foreach (var item in paysByRechargues)
                            {
                                var recarga = item.Key;

                                if (recarga == null)
                                    continue;

                                qtyRecargas++;

                                decimal total = recarga.Import;
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string pagos = "";
                                foreach (var pay in item)
                                {
                                    pagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > recarga.date.Date || pay.UserId != recarga.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > recarga.date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado + totalCredito;
                                refRecharguePagado += pagado;
                                refRechargueCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;

                                if (!diffDate)
                                {
                                    refRechargueTotal += total;
                                    refRechargueTotalPagado += totalPagado;
                                    refRechargueTotalCredito += totalCredito;
                                    refRechargueDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                //var index = recargas.IndexOf(recarga);
                                if (true)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;
                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(recarga.Number, normalFont));
                                cell2.AddElement(new Phrase(recarga.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(recarga.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(pagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refRecharguePagado.ToString(), headFont));
                            if (refRechargueCredito > 0)
                                cell4.AddElement(new Phrase(refRechargueCredito.ToString(), headRedFont));
                            cell5.AddElement(new Phrase(refRechargueTotalPagado.ToString(), headFont));
                            if (refRechargueTotalCredito > 0)
                                cell5.AddElement(new Phrase(refRechargueTotalCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refRechargueTotal.ToString(), headFont));
                            cell7.AddElement(new Phrase(refRechargueDebe.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region // ENVIOS CUBIQ
                    decimal refEnviosCubiqPagado = 0;
                    var paysByCubiq = pays
                        .Where(x => x.OrderCubiq != null && x.OrderCubiq.Status != OrderCubiq.STATUS_CANCELADA)
                        .GroupBy(x => x.OrderCubiq);
                    if (paysByCubiq.Any())
                    {
                        float[] columnWidthCubiq = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthCubiq);
                        tblData.WidthPercentage = 100;
                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;

                        decimal refEnviosCubiqTotal = 0;
                        decimal refEnviosCubiqCredito = 0;
                        decimal refEnviosCubiqTotalPagado = 0;
                        decimal refEnviosCubiqTotalCredito = 0;
                        decimal refEnviosCubiqDebe = 0;
                        if (paysByCubiq.Count() != 0)
                        {
                            doc.Add(new Phrase("Envíos Carga AM", headFont));

                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);


                            foreach (var item in paysByCubiq)
                            {
                                var enviocubiq = item.Key;

                                cantLbCargaMisc += enviocubiq.Paquetes.Where(x => x.Type == Domain.Enums.CubiqPackageType.Miscelaneo).Sum(x => x.PesoLb);
                                cantLbCargaDurad += enviocubiq.Paquetes.Where(x => x.Type != Domain.Enums.CubiqPackageType.Miscelaneo).Sum(x => x.PesoLb);


                                if (enviocubiq == null)
                                    continue;

                                qtyEnviosCubiq++;

                                decimal total = enviocubiq.Amount;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo" && x.tipoPago.Type != "Cubiq").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                bool colorearCell = false;
                                bool diffDate = false;
                                string tiposdepagos = "";
                                foreach (var pay in item)
                                {
                                    tiposdepagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo" && pay.tipoPago.Type != "Cubiq")
                                        pagado += pay.valorPagado;
                                    else if (pay.tipoPago.Type == "Crédito de Consumo")
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > enviocubiq.Date.Date || pay.UserId != enviocubiq.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > enviocubiq.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }

                                debe = total - totalPagado - totalCredito;

                                refEnviosCubiqPagado += pagado;
                                refEnviosCubiqCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refEnviosCubiqTotal += total;
                                    refEnviosCubiqTotalPagado += totalPagado;
                                    refEnviosCubiqTotalCredito += totalCredito;
                                    refEnviosCubiqDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                if (tiposdepagos == "")
                                {
                                    tiposdepagos = "-";
                                }

                                //var index = envioscubiq.IndexOf(enviocubiq);
                                if (true)
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 1;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 1;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 1;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 1;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 1;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 1;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 1;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 1;

                                }
                                else
                                {
                                    cell1 = new PdfPCell();
                                    cell1.Border = 0;
                                    cell2 = new PdfPCell();
                                    cell2.Border = 0;
                                    cell3 = new PdfPCell();
                                    cell3.Border = 0;
                                    cell4 = new PdfPCell();
                                    cell4.Border = 0;
                                    cell5 = new PdfPCell();
                                    cell5.Border = 0;
                                    cell6 = new PdfPCell();
                                    cell6.Border = 0;
                                    cell7 = new PdfPCell();
                                    cell7.Border = 0;
                                    cell8 = new PdfPCell();
                                    cell8.Border = 0;
                                }
                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(enviocubiq.Number, normalFont));
                                cell2.AddElement(new Phrase(enviocubiq.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(enviocubiq.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(tiposdepagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refEnviosCubiqPagado.ToString(), headFont));
                            if (refEnviosCubiqCredito > 0)
                                cell4.AddElement(new Phrase(refEnviosCubiqCredito.ToString(), headRedFont));
                            cell5.AddElement(new Phrase(refEnviosCubiqTotalPagado.ToString(), headFont));
                            if (refEnviosCubiqTotalCredito > 0)
                                cell5.AddElement(new Phrase(refEnviosCubiqTotalCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refEnviosCubiqTotal.ToString(), headFont));
                            cell7.AddElement(new Phrase(refEnviosCubiqDebe.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    #region OTROS SERVICIOS
                    float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };

                    var tiposervicios = _context.TipoServicios
                            .Where(x => x.agency.AgencyId == agency.AgencyId);
                    Dictionary<string, decimal> creditoServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> creditoTotalServicios = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> pagadoServicios = new Dictionary<string, decimal>();
                    var paysByServices = pays.Where(x => x.Servicio != null && x.Servicio.estado != Servicio.EstadoCancelado).GroupBy(x => x.Servicio);
                    if (paysByServices.Any())
                    {
                        //Hago una tabla para cada servicio
                        foreach (var tipo in tiposervicios)
                        {
                            var auxservicios = paysByServices.Where(x => x.Key.tipoServicio.TipoServicioId == tipo.TipoServicioId).ToList();
                            if (auxservicios.Count() != 0)
                            {
                                tblData = new PdfPTable(columnWidthsservicios);
                                tblData.WidthPercentage = 100;
                                cell1 = new PdfPCell();
                                cell1.Border = 1;
                                cell2 = new PdfPCell();
                                cell2.Border = 1;
                                cell3 = new PdfPCell();
                                cell3.Border = 1;
                                cell4 = new PdfPCell();
                                cell4.Border = 1;
                                cell5 = new PdfPCell();
                                cell5.Border = 1;
                                cell6 = new PdfPCell();
                                cell6.Border = 1;
                                cell7 = new PdfPCell();
                                cell7.Border = 1;
                                cell8 = new PdfPCell();
                                cell8.Border = 1;
                                doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                                cell1.AddElement(new Phrase("No.", headFont));
                                cell2.AddElement(new Phrase("Cliente", headFont));
                                cell3.AddElement(new Phrase("Empleado", headFont));
                                cell4.AddElement(new Phrase("Pagado", headFont));
                                cell5.AddElement(new Phrase("T. Pagado", headFont));
                                cell6.AddElement(new Phrase("Total", headFont));
                                cell7.AddElement(new Phrase("Debe", headFont));
                                cell8.AddElement(new Phrase("Tipo Pago", headFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);

                                creditoServicios.Add(tipo.Nombre, 0);
                                creditoTotalServicios.Add(tipo.Nombre.ToString(), 0);
                                pagadoServicios.Add(tipo.Nombre, 0);
                                decimal refServicioTotal = 0;
                                decimal refServicioPagado = 0;
                                decimal refServicioTotalPagado = 0;
                                decimal refServicioDebe = 0;
                                foreach (var servicio in auxservicios)
                                {
                                    decimal total = servicio.Key.importeTotal;
                                    decimal totalPagado = servicio.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                    decimal totalCredito = servicio.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                    decimal pagado = 0;
                                    decimal credito = 0;
                                    decimal debe = 0;
                                    string pagos = "";
                                    bool diffDate = false;
                                    bool colorearCell = false;

                                    foreach (var pay in servicio)
                                    {
                                        pagos += pay.tipoPago.Type + ", ";
                                        if (pay.tipoPago.Type == "Crédito de Consumo")
                                            credito += pay.valorPagado;
                                        else
                                            pagado += pay.valorPagado;
                                        ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                        canttipopago[pay.tipoPago.Type] += 1;

                                        //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                        if (pay.date.ToLocalTime().Date > servicio.Key.fecha.Date || pay.UserId != servicio.Key.UserId)
                                        {
                                            if (pay.date.ToLocalTime().Date > servicio.Key.fecha.Date)
                                                diffDate = true;
                                            colorearCell = true;
                                        }
                                    }
                                    debe = total - totalPagado - totalCredito;
                                    creditoServicios[tipo.Nombre] += credito;
                                    pagadoServicios[tipo.Nombre] += pagado;

                                    if (servicio.Key.tipoServicio.Nombre == "Gastos")
                                    {
                                        tramitesGastos += pagado;
                                        if (!diffDate)
                                        {
                                            tramitesTotalGastos += totalPagado;
                                        }
                                    }

                                    tramitesPagado += pagado;
                                    tramitesCredito += credito;

                                    if (!diffDate)
                                    {
                                        refServicioTotal += total;
                                        refServicioPagado += pagado;
                                        refServicioTotalPagado += totalPagado;
                                        creditoTotalServicios[tipo.Nombre.ToString()] += totalCredito;
                                        refServicioDebe += debe;

                                        tramitesTotal += total;
                                        tramitesTotalPagado += totalPagado;
                                        tramitesTotalCredito += totalCredito;
                                        tramitesDeuda += debe;
                                    }

                                    var index = auxservicios.IndexOf(servicio);
                                    if (index == 0)
                                    {
                                        cell1 = new PdfPCell();
                                        cell1.Border = 1;
                                        cell2 = new PdfPCell();
                                        cell2.Border = 1;
                                        cell3 = new PdfPCell();
                                        cell3.Border = 1;
                                        cell4 = new PdfPCell();
                                        cell4.Border = 1;
                                        cell5 = new PdfPCell();
                                        cell5.Border = 1;
                                        cell6 = new PdfPCell();
                                        cell6.Border = 1;
                                        cell7 = new PdfPCell();
                                        cell7.Border = 1;
                                        cell8 = new PdfPCell();
                                        cell8.Border = 1;
                                    }
                                    else
                                    {
                                        cell1 = new PdfPCell();
                                        cell1.Border = 0;
                                        cell2 = new PdfPCell();
                                        cell2.Border = 0;
                                        cell3 = new PdfPCell();
                                        cell3.Border = 0;
                                        cell4 = new PdfPCell();
                                        cell4.Border = 0;
                                        cell5 = new PdfPCell();
                                        cell5.Border = 0;
                                        cell6 = new PdfPCell();
                                        cell6.Border = 0;
                                        cell7 = new PdfPCell();
                                        cell7.Border = 0;
                                        cell8 = new PdfPCell();
                                        cell8.Border = 0;
                                    }
                                    if (colorearCell)
                                    {
                                        cell1.BackgroundColor = colorcell;
                                        cell2.BackgroundColor = colorcell;
                                        cell3.BackgroundColor = colorcell;
                                        cell4.BackgroundColor = colorcell;
                                        cell5.BackgroundColor = colorcell;
                                        cell6.BackgroundColor = colorcell;
                                        cell7.BackgroundColor = colorcell;
                                        cell8.BackgroundColor = colorcell;
                                    }
                                    cell1.AddElement(new Phrase(servicio.Key.numero, normalFont));
                                    cell2.AddElement(new Phrase(servicio.Key.cliente?.FullData, normalFont));
                                    cell2.AddElement(new Phrase(servicio.Key.cliente?.Phone?.Number, normalFont));
                                    cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                    cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                    if (credito > 0)
                                        cell4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                    cell5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                    if (totalCredito > 0 && !diffDate)
                                        cell5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                    cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                    cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                    cell8.AddElement(new Phrase(pagos, normalFont));

                                    tblData.AddCell(cell1);
                                    tblData.AddCell(cell2);
                                    tblData.AddCell(cell3);
                                    tblData.AddCell(cell4);
                                    tblData.AddCell(cell5);
                                    tblData.AddCell(cell6);
                                    tblData.AddCell(cell7);
                                    tblData.AddCell(cell8);
                                }

                                // Añado el total
                                cell1 = new PdfPCell();
                                cell1.Border = 0;
                                cell2 = new PdfPCell();
                                cell2.Border = 0;
                                cell3 = new PdfPCell();
                                cell3.Border = 0;
                                cell4 = new PdfPCell();
                                cell4.Border = 0;
                                cell5 = new PdfPCell();
                                cell5.Border = 0;
                                cell6 = new PdfPCell();
                                cell6.Border = 0;
                                cell7 = new PdfPCell();
                                cell7.Border = 0;
                                cell8 = new PdfPCell();
                                cell8.Border = 0;

                                cell1.AddElement(new Phrase("Totales", headFont));
                                cell2.AddElement(new Phrase("", normalFont));
                                cell3.AddElement(new Phrase("", normalFont));
                                cell4.AddElement(new Phrase(refServicioPagado.ToString(), headFont));
                                if (creditoServicios[tipo.Nombre] > 0)
                                    cell4.AddElement(new Phrase(creditoServicios[tipo.Nombre].ToString(), headRedFont));
                                cell5.AddElement(new Phrase(refServicioTotalPagado.ToString(), headFont));
                                if (creditoTotalServicios[tipo.Nombre.ToString()] > 0)
                                    cell5.AddElement(new Phrase(creditoTotalServicios[tipo.Nombre.ToString()].ToString(), headRedFont));
                                cell6.AddElement(new Phrase(refServicioTotal.ToString(), headFont));
                                cell7.AddElement(new Phrase(refServicioDebe.ToString(), headFont));
                                cell8.AddElement(new Phrase("", normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);

                                // Añado la tabla al documento
                                doc.Add(tblData);
                                doc.Add(Chunk.NEWLINE);
                                doc.Add(Chunk.NEWLINE);
                            }
                        }

                    }

                    #endregion

                    #region MERCADO
                    decimal refMercadoPagado = 0;
                    var paysByMercado = pays.Where(x => x.Mercado != null && x.Mercado.Status != Mercado.STATUS_CANCELADA).GroupBy(x => x.Mercado);
                    if (paysByMercado.Any())
                    {
                        tblData = new PdfPTable(columnWidths)
                        {
                            WidthPercentage = 100
                        };

                        cell1 = new PdfPCell();
                        cell1.Border = 1;
                        cell2 = new PdfPCell();
                        cell2.Border = 1;
                        cell3 = new PdfPCell();
                        cell3.Border = 1;
                        cell4 = new PdfPCell();
                        cell4.Border = 1;
                        cell5 = new PdfPCell();
                        cell5.Border = 1;
                        cell6 = new PdfPCell();
                        cell6.Border = 1;
                        cell7 = new PdfPCell();
                        cell7.Border = 1;
                        cell8 = new PdfPCell();
                        cell8.Border = 1;

                        decimal refMercadoTotal = 0;
                        decimal refMercadoTotalPagado = 0;
                        decimal refMercadoDebe = 0;
                        decimal refMercadoCredito = 0;
                        decimal refMercadoTotalCredito = 0;

                        if (paysByMercado.Count() != 0)
                        {
                            doc.Add(new Phrase("Mercado", headFont));
                            cell1.AddElement(new Phrase("No.", headFont));
                            cell2.AddElement(new Phrase("Cliente", headFont));
                            cell3.AddElement(new Phrase("Empleado", headFont));
                            cell4.AddElement(new Phrase("Pagado", headFont));
                            cell5.AddElement(new Phrase("T. Pagado", headFont));
                            cell6.AddElement(new Phrase("Total", headFont));
                            cell7.AddElement(new Phrase("Debe", headFont));
                            cell8.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            foreach (var item in paysByMercado)
                            {
                                var mercado = item.Key;
                                if (mercado == null)
                                    continue;

                                qtyMercado++;

                                decimal total = mercado.Amount;
                                decimal totalPagado = item.Where(x => x.tipoPago.Type != "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal totalCredito = item.Where(x => x.tipoPago.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                                decimal pagado = 0;
                                decimal credito = 0;
                                decimal debe = 0;
                                string pagos = "";
                                bool diffDate = false;
                                bool colorearCell = false;
                                foreach (var pay in item)
                                {
                                    pagos += pay.tipoPago.Type + ", ";
                                    if (pay.tipoPago.Type != "Crédito de Consumo")
                                        pagado += pay.valorPagado;
                                    else
                                        credito += pay.valorPagado;
                                    ventastipopago[pay.tipoPago.Type] += pay.valorPagado;
                                    canttipopago[pay.tipoPago.Type] += 1;

                                    //Si se realizo un pago un dia diferente al que se creo el tramite u otro usuario realizo un pago 
                                    if (pay.date.ToLocalTime().Date > mercado.Date.Date || pay.UserId != mercado.UserId)
                                    {
                                        if (pay.date.ToLocalTime().Date > mercado.Date.Date)
                                            diffDate = true;
                                        colorearCell = true;
                                    }
                                }
                                debe = total - totalPagado - totalCredito;

                                refMercadoPagado += pagado;
                                refMercadoCredito += credito;
                                tramitesPagado += pagado;
                                tramitesCredito += credito;
                                if (!diffDate)
                                {
                                    refMercadoTotal += total;
                                    refMercadoTotalPagado += totalPagado;
                                    refMercadoTotalCredito += totalCredito;
                                    refMercadoDebe += debe;

                                    tramitesTotal += total;
                                    tramitesTotalPagado += totalPagado;
                                    tramitesTotalCredito += totalCredito;
                                    tramitesDeuda += debe;
                                }

                                cell1 = new PdfPCell();
                                cell1.Border = 1;
                                cell2 = new PdfPCell();
                                cell2.Border = 1;
                                cell3 = new PdfPCell();
                                cell3.Border = 1;
                                cell4 = new PdfPCell();
                                cell4.Border = 1;
                                cell5 = new PdfPCell();
                                cell5.Border = 1;
                                cell6 = new PdfPCell();
                                cell6.Border = 1;
                                cell7 = new PdfPCell();
                                cell7.Border = 1;
                                cell8 = new PdfPCell();
                                cell8.Border = 1;

                                if (colorearCell)
                                {
                                    cell1.BackgroundColor = colorcell;
                                    cell2.BackgroundColor = colorcell;
                                    cell3.BackgroundColor = colorcell;
                                    cell4.BackgroundColor = colorcell;
                                    cell5.BackgroundColor = colorcell;
                                    cell6.BackgroundColor = colorcell;
                                    cell7.BackgroundColor = colorcell;
                                    cell8.BackgroundColor = colorcell;
                                }

                                cell1.AddElement(new Phrase(mercado.Number, normalFont));
                                cell2.AddElement(new Phrase(mercado.Client?.FullData, normalFont));
                                cell2.AddElement(new Phrase(mercado.Client?.Phone?.Number, normalFont));
                                cell3.AddElement(new Phrase(aUser.Name + " " + aUser.LastName, normalFont));
                                cell4.AddElement(new Phrase(pagado.ToString(), normalFont));
                                if (credito > 0)
                                    cell4.AddElement(new Phrase(credito.ToString(), normalRedFont));
                                cell5.AddElement(new Phrase(diffDate ? "-" : totalPagado.ToString(), normalFont));
                                if (totalCredito > 0 && !diffDate)
                                    cell5.AddElement(new Phrase(totalCredito.ToString(), normalRedFont));
                                cell6.AddElement(new Phrase(diffDate ? "-" : total.ToString(), normalFont));
                                cell7.AddElement(new Phrase(diffDate ? "-" : debe.ToString(), normalFont));
                                cell8.AddElement(new Phrase(pagos, normalFont));

                                tblData.AddCell(cell1);
                                tblData.AddCell(cell2);
                                tblData.AddCell(cell3);
                                tblData.AddCell(cell4);
                                tblData.AddCell(cell5);
                                tblData.AddCell(cell6);
                                tblData.AddCell(cell7);
                                tblData.AddCell(cell8);
                            }

                            // Añado el total
                            cell1 = new PdfPCell();
                            cell1.Border = 0;
                            cell2 = new PdfPCell();
                            cell2.Border = 0;
                            cell3 = new PdfPCell();
                            cell3.Border = 0;
                            cell4 = new PdfPCell();
                            cell4.Border = 0;
                            cell5 = new PdfPCell();
                            cell5.Border = 0;
                            cell6 = new PdfPCell();
                            cell6.Border = 0;
                            cell7 = new PdfPCell();
                            cell7.Border = 0;
                            cell8 = new PdfPCell();
                            cell8.Border = 0;

                            cell1.AddElement(new Phrase("Totales", headFont));
                            cell2.AddElement(new Phrase("", normalFont));
                            cell3.AddElement(new Phrase("", normalFont));
                            cell4.AddElement(new Phrase(refMercadoPagado.ToString(), headFont));
                            if (refMercadoCredito > 0)
                                cell4.AddElement(new Phrase(refMercadoCredito.ToString(), headRedFont));
                            cell5.AddElement(new Phrase(refMercadoTotalPagado.ToString(), headFont));
                            if (refMercadoTotalCredito > 0)
                                cell5.AddElement(new Phrase(refMercadoTotalCredito.ToString(), headRedFont));
                            cell6.AddElement(new Phrase(refMercadoTotal.ToString(), headFont));
                            cell7.AddElement(new Phrase(refMercadoDebe.ToString(), headFont));
                            cell8.AddElement(new Phrase("", normalFont));

                            tblData.AddCell(cell1);
                            tblData.AddCell(cell2);
                            tblData.AddCell(cell3);
                            tblData.AddCell(cell4);
                            tblData.AddCell(cell5);
                            tblData.AddCell(cell6);
                            tblData.AddCell(cell7);
                            tblData.AddCell(cell8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);

                        }

                    }
                    #endregion

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    tblData = new PdfPTable(columnWidthstipopago);
                    tblData.WidthPercentage = 100;

                    cell1 = new PdfPCell();
                    cell1.Border = 1;
                    cell2 = new PdfPCell();
                    cell2.Border = 1;
                    cell3 = new PdfPCell();
                    cell3.Border = 1;

                    cell1.AddElement(new Phrase("Tipo de Pago", headFont));
                    cell2.AddElement(new Phrase("Cantidad", headFont));
                    cell3.AddElement(new Phrase("Importe", headFont));
                    tblData.AddCell(cell1);
                    tblData.AddCell(cell2);
                    tblData.AddCell(cell3);
                    foreach (var item in _context.TipoPago)
                    {

                        cell1 = new PdfPCell();
                        cell1.Border = 0;
                        cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell3 = new PdfPCell();
                        cell3.Border = 0;

                        cell1.AddElement(new Phrase(item.Type, normalFont));
                        cell2.AddElement(new Phrase(canttipopago[item.Type].ToString(), normalFont));
                        cell3.AddElement(new Phrase(ventastipopago[item.Type].ToString(), normalFont));
                        tblData.AddCell(cell1);
                        tblData.AddCell(cell2);
                        tblData.AddCell(cell3);
                    }

                    doc.Add(tblData);
                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 2 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    doc.Add(Chunk.NEWLINE);
                    cellleft.AddElement(new Phrase("RESUMEN DE TRÁMITES", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    //Remesas: 15 ordenes -- $360.00 usd
                    Phrase aux;
                    int auxcanttoal = 0;
                    int auxcant = qtyRemesas;
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("REMESAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRemesasPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = qtyPaquetesTuristicos;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PAQUETES TURÍSTICOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPaqueteTuristicoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyEnviosMaritimos;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refPagadoEnvioM.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyPasaportes;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + refPassportPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyEnviosCaribe;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnvioCaribePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyEnviosAereos;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosAereosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyEnviosCubiq;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCubiqPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyEnviosCombos;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refEnviosCombosPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyBoletos;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyBoletosCarrier;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refBoletoCarrierPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = qtyRecargas;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refRecharguePagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    foreach (var item in tiposervicios)
                    {
                        var items = paysByServices.Where(x => x.Key.tipoServicio.TipoServicioId == item.TipoServicioId).GroupBy(x => x.Key.SubServicio).ToList();
                        foreach (var auxservicio in items)
                        {
                            auxcant = auxservicio.Count();
                            auxcanttoal += auxcant;

                            if (auxcant != 0)
                            {
                                string serviceName = auxservicio.Key != null ? $"{item.Nombre} {auxservicio.Key.Nombre}" : $"{item.Nombre}";
                                aux = new Phrase(serviceName.ToUpper() + ": ", headFont);
                                aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + pagadoServicios[item.Nombre.ToString()].ToString() + " usd", normalFont));
                                cellleft.AddElement(aux);
                            }
                        }

                    }

                    cellleft.AddElement(Chunk.NEWLINE);

                    auxcant = qtyMercado;
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("MERCADO: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refMercadoPagado.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);
                    #endregion

                    #region //GRAN TOTAL

                    Paragraph pPagado = new Paragraph("Pagado Total: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesPagado.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    pPagado = new Paragraph("Crédito: ", headFont2);
                    pPagado.AddSpecial(new Phrase("$ " + tramitesCredito.ToString(), normalFont2));
                    pPagado.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(pPagado);
                    if(tramitesTotalGastos > 0)
                    {
                        pPagado = new Paragraph("Gastos: ", headFont2);
                        pPagado.AddSpecial(new Phrase("$ " + tramitesTotalGastos.ToString(), normalFont2));
                        pPagado.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(pPagado);
                    }
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph totalp = new Paragraph("Total Ventas Día: ", headFont2);
                    totalp.AddSpecial(new Phrase("$ " + tramitesTotal.ToString(), normalFont2));
                    totalp.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(totalp);
                    Paragraph deuda = new Paragraph("Pendiente a pago: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + tramitesDeuda.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);
                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    doc.Add(Chunk.NEWLINE);

                    // Paquetes
                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase("Cantidad de libras", headFont));
                    PdfPTable tbl = new PdfPTable(new float[] { 2, 1, 1 });
                    tbl.WidthPercentage = 100;

                    new List<string> { "Empleado", "Paquete", "Medicina" }
                    .ForEach(x => { tbl.AddCell(new PdfPCell(new Phrase(x, headFont))); });

                    tbl.AddCell(new PdfPCell(new Phrase($"{aUser.Name} {aUser.LastName}", headFont)));
                    tbl.AddCell(new PdfPCell(new Phrase(cantLibPaquete.ToString("0.00") + " lb", headFont)));
                    tbl.AddCell(new PdfPCell(new Phrase(cantLibMedicina.ToString("0.00") + " lb", headFont)));
                    doc.Add(tbl);

                    doc.Add(Chunk.NEWLINE);

                    // Carga AM
                    if (cantLbCargaMisc > 0 || cantLbCargaDurad > 0)
                    {
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(new Phrase("Cantidad de libras Carga AM", headFont));
                        tbl = new PdfPTable(new float[] { 2, 1, 1 });
                        tbl.WidthPercentage = 100;

                        new List<string> { "Empleado", "Miscelaneo", "Duradero" }
                        .ForEach(x => { tbl.AddCell(new PdfPCell(new Phrase(x, headFont))); });

                        tbl.AddCell(new PdfPCell(new Phrase($"{aUser.Name} {aUser.LastName}", headFont)));
                        tbl.AddCell(new PdfPCell(new Phrase(cantLbCargaMisc.ToString("0.00") + " lb", headFont)));
                        tbl.AddCell(new PdfPCell(new Phrase(cantLbCargaDurad.ToString("0.00") + " lb", headFont)));
                        doc.Add(tbl);

                        doc.Add(Chunk.NEWLINE);
                    }

                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase("Productos tienda", headFont));
                    tbl = new PdfPTable(new float[] { 1, 1, 1 });
                    tbl.WidthPercentage = 100;

                    new List<string> { "Cantidad", "Nombre", "Precio" }
                    .ForEach(x => { tbl.AddCell(new PdfPCell(new Phrase(x, headFont))); });
                    foreach (var item in productos.GroupBy(x => x.Product))
                    {
                        ProductoBodega product = item.Key;
                        int qty = item.Sum(x => x.Qty);
                        decimal price = ((decimal)product.PrecioVentaReferencial) * qty;
                        tbl.AddCell(new PdfPCell(new Phrase(product.Nombre, headFont)));
                        tbl.AddCell(new PdfPCell(new Phrase($"{qty}", headFont)));
                        tbl.AddCell(new PdfPCell(new Phrase(price.ToString("0.00"), headFont)));
                    }
                    

                    doc.Add(tbl);


                    doc.Close();
                    Serilog.Log.Information("Fin de reporte");
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    doc.Close();
                    pdf.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }
    }
}
