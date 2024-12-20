using Agenciapp.Common.Contrains;
using Agenciapp.Service.IReportServices.Models;
using AgenciappHome.Models;
using GraphQL.Client.Abstractions.Utilities;
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
        private static Guid agencyDCubaId = Guid.Parse("4752B08A-7684-42B3-930D-FF86F496DF2F");
        public async static Task<string> GetReporteVentas(string rangeDate, databaseContext _context, User user, IWebHostEnvironment _env)
        {
            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {
                    //var role = AgenciaAuthorize.getRole(User, _context);
                    var aAgency = _context.Agency.Where(x => x.AgencyId == user.AgencyId);

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font fonttransferida = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.ITALIC, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    BaseColor colorcell = WebColors.GetRGBColor("#fffdc4");

                    Agency agency = aAgency.FirstOrDefault();
                    Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

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

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);
                    doc.Add(tableEncabezado);
                    doc.Add(Chunk.NEWLINE);

                    var auxDate = rangeDate.Split('-');
                    CultureInfo culture = new CultureInfo("es-US", true);
                    var dateIni = DateTime.Parse(auxDate[0], culture);
                    var dateFin = DateTime.Parse(auxDate[1], culture);

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
                    Paragraph parPaq = new Paragraph("Ventas por servicio del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);
                    /*
                     * 0: cash
                     * 1: zelle
                     * 2: cheque
                     * 3: credito o debito
                     * 4: transferencia bancaria
                     * 5: Web
                     * 6: Money Order
                     * 7: Crédito de Consumo
                     * */
                    Dictionary<string, decimal> ventastipopago = new Dictionary<string, decimal>();
                    Dictionary<string, int> canttipopago = new Dictionary<string, int>();
                    foreach (var item in _context.TipoPago)
                    {
                        ventastipopago.Add(item.Type, 0);
                        canttipopago.Add(item.Type, 0);
                    }
                    //decimal[] ventastipopago = new decimal[8];
                    //int[] canttipopago = new int[8];
                    decimal grantotal = 0;
                    decimal deudatotal = 0;
                    decimal totalpagado = 0;
                    #region // REMESAS
                    List<Remittance> remesas = await _context.Remittance
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.agencyTransferida)
                        .Where(x => (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date)
                        .Where(x => x.Status != Remittance.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    PdfPTable tblremesasData = new PdfPTable(columnWidths);
                    tblremesasData.WidthPercentage = 100;

                    PdfPCell cellremesas1 = new PdfPCell();
                    cellremesas1.Border = 1;
                    PdfPCell cellremesas2 = new PdfPCell();
                    cellremesas2.Border = 1;
                    PdfPCell cellremesas3 = new PdfPCell();
                    cellremesas3.Border = 1;
                    PdfPCell cellremesas4 = new PdfPCell();
                    cellremesas4.Border = 1;
                    PdfPCell cellremesas5 = new PdfPCell();
                    cellremesas5.Border = 1;
                    PdfPCell cellremesas6 = new PdfPCell();
                    cellremesas6.Border = 1;
                    PdfPCell cellremesas7 = new PdfPCell();
                    cellremesas7.Border = 1;

                    decimal refTotalRemesas = 0;
                    decimal refPagadpRemesas = 0;
                    decimal refDeudaRemesas = 0;
                    if (remesas.Count != 0)
                    {
                        doc.Add(new Phrase("Remesas", headFont));


                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);


                        foreach (Remittance remesa in remesas)
                        {
                            decimal reftotalEQ = remesa.Amount;
                            decimal refpagadoEQ = 0;
                            decimal refdeudaEQ = 0;
                            bool isbytransferencia = false;
                            if (remesa.agencyTransferida != null)
                            {
                                if (remesa.agencyTransferida.AgencyId == agency.AgencyId)
                                {
                                    isbytransferencia = true;
                                    reftotalEQ = remesa.costoMayorista + remesa.OtrosCostos;
                                    refpagadoEQ = 0;
                                    refdeudaEQ = reftotalEQ;
                                }
                            }
                            string pagos = "";
                            if (!isbytransferencia)
                            {
                                foreach (var item in remesa.Pagos)
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagadoEQ += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;
                                }
                                refdeudaEQ = reftotalEQ - refpagadoEQ;
                            }

                            grantotal += reftotalEQ;
                            totalpagado += refpagadoEQ;
                            deudatotal += refdeudaEQ;

                            refTotalRemesas += reftotalEQ;
                            refPagadpRemesas += refpagadoEQ;
                            refDeudaRemesas += refdeudaEQ;

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
                            }

                            cellremesas1.AddElement(new Phrase(isbytransferencia ? "T." + remesa.Number : remesa.Number, isbytransferencia ? fonttransferida : normalFont));
                            cellremesas2.AddElement(new Phrase(remesa.Client.Name + " " + remesa.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(remesa.Client.Phone != null ? remesa.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(remesa.User != null ? remesa.User.Name + " " + remesa.User.LastName : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refpagadoEQ.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(reftotalEQ.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refdeudaEQ.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(isbytransferencia ? "Pendiente" : pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadpRemesas.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalRemesas.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refDeudaRemesas.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));
                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }

                    #endregion

                    #region // Paquete Turistico
                    List<PaqueteTuristico> paquetesTuristicos = await _context.PaquetesTuristicos
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date)
                        .Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

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

                    decimal refTotalPaqueteTuristico = 0;
                    decimal refPagadoPaqueteTuristico = 0;
                    decimal refDeudaPaqueteTuristico = 0;
                    if (paquetesTuristicos.Count != 0)
                    {
                        doc.Add(new Phrase("Paquete Turístico", headFont));


                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);


                        foreach (PaqueteTuristico paqueteTuristico in paquetesTuristicos)
                        {
                            decimal reftotalEQ = paqueteTuristico.Amount;
                            decimal refpagadoEQ = 0;
                            decimal refdeudaEQ = 0;
                            bool isbytransferencia = false;

                            string pagos = "";
                            if (!isbytransferencia)
                            {
                                foreach (var item in paqueteTuristico.Pagos)
                                {
                                    pagos += item.tipoPago.Type + ", ";
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagadoEQ += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;
                                }
                                refdeudaEQ = reftotalEQ - refpagadoEQ;
                            }

                            grantotal += reftotalEQ;
                            totalpagado += refpagadoEQ;
                            deudatotal += refdeudaEQ;

                            refTotalPaqueteTuristico += reftotalEQ;
                            refPagadoPaqueteTuristico += refpagadoEQ;
                            refDeudaPaqueteTuristico += refdeudaEQ;

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
                            }

                            cellremesas1.AddElement(new Phrase(isbytransferencia ? "T." + paqueteTuristico.Number : paqueteTuristico.Number, isbytransferencia ? fonttransferida : normalFont));
                            cellremesas2.AddElement(new Phrase(paqueteTuristico.Client.Name + " " + paqueteTuristico.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(paqueteTuristico.Client.Phone != null ? paqueteTuristico.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(paqueteTuristico.User != null ? paqueteTuristico.User.Name + " " + paqueteTuristico.User.LastName : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refpagadoEQ.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(reftotalEQ.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refdeudaEQ.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(isbytransferencia ? "Pendiente" : pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoPaqueteTuristico.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalPaqueteTuristico.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refDeudaPaqueteTuristico.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));
                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }

                    #endregion

                    #region // ENVIOS MARITIMOS

                    float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1 };
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

                    List<EnvioMaritimo> enviosmaritimos = await _context.EnvioMaritimo
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date)
                        .Where(x => x.Status != EnvioMaritimo.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    decimal refTotalMaritimo = 0;
                    decimal refDeudaMaritimo = 0;
                    decimal refPagadoMaritimo = 0;
                    if (enviosmaritimos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Marítimos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);


                        foreach (EnvioMaritimo enviomaritimo in enviosmaritimos)
                        {
                            decimal reftotal = enviomaritimo.Amount;
                            decimal refpagado = 0;
                            decimal refdeuda = 0;

                            string pagos = "";
                            foreach (var item in enviomaritimo.RegistroPagos)
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refpagado += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;
                            }

                            refdeuda = reftotal - refpagado;

                            grantotal += reftotal;
                            totalpagado += refpagado;
                            deudatotal += refdeuda;

                            refTotalMaritimo += reftotal;
                            refPagadoMaritimo += refpagado;
                            refDeudaMaritimo += refdeuda;

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
                            }

                            cellremesas1.AddElement(new Phrase(enviomaritimo.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviomaritimo.Client.Name + " " + enviomaritimo.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(enviomaritimo.Client.Phone != null ? enviomaritimo.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(enviomaritimo.User != null ? $"{enviomaritimo.User.Name} {enviomaritimo.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refpagado.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(reftotal.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refdeuda.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoMaritimo.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalMaritimo.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refDeudaMaritimo.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS CARIBE

                    float[] columnWidthscaribe = { (float)1.5, 2, 2, 1, 1, 1, 1 };
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

                    List<EnvioCaribe> envioscaribe = await _context.EnvioCaribes
                        .Include(x => x.AgencyTransferida)
                        .Include(x => x.User)
                        .Include(x => x.RegistroPagos)
                        .ThenInclude(x => x.tipoPago)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.User.Username != "Manuel14" && (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.AgencyTransferidaId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date)
                        .Where(x => x.Status != EnvioCaribe.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    decimal refTotalEnvioCaribe = 0;
                    decimal refPagadoEnvioCaribe = 0;
                    decimal refDeudaEnvioCaribe = 0;
                    if (envioscaribe.Count != 0)
                    {

                        doc.Add(new Phrase("Envíos Caribe", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipos Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);


                        foreach (EnvioCaribe enviocaribe in envioscaribe)
                        {
                            decimal reftotalEC = enviocaribe.Amount;
                            decimal refpagadoEC = 0;
                            decimal refdeudaEC = 0;
                            bool isbytransferencia = false;
                            if (enviocaribe.AgencyTransferidaId != null)
                            {
                                if (enviocaribe.AgencyTransferidaId == agency.AgencyId)
                                {
                                    isbytransferencia = true;
                                    reftotalEC = enviocaribe.costo + enviocaribe.OtrosCostos;
                                    refpagadoEC = 0;
                                    refdeudaEC = reftotalEC;
                                }
                            }

                            string tipospagocaribe = "";
                            if (!isbytransferencia)
                            {
                                foreach (var item in enviocaribe.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagadoEC += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;
                                    tipospagocaribe += item.tipoPago.Type + ", ";
                                }
                                refdeudaEC = reftotalEC - refpagadoEC;

                            }

                            refTotalEnvioCaribe += reftotalEC;
                            refPagadoEnvioCaribe += refpagadoEC;
                            refDeudaEnvioCaribe += refdeudaEC;

                            grantotal += reftotalEC;
                            totalpagado += refpagadoEC;
                            deudatotal += refdeudaEC;

                            if (tipospagocaribe == "") { tipospagocaribe = "-"; }

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
                            }

                            cellremesas1.AddElement(new Phrase(isbytransferencia ? "T." + enviocaribe.Number : enviocaribe.Number, isbytransferencia ? fonttransferida : normalFont));
                            cellremesas2.AddElement(new Phrase(enviocaribe.Client.Name + " " + enviocaribe.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocaribe.Client.Phone != null ? enviocaribe.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(enviocaribe.User != null ? $"{enviocaribe.User.Name} {enviocaribe.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refpagadoEC.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(reftotalEC.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refdeudaEC.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(isbytransferencia ? "Pendiente" : tipospagocaribe, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoEnvioCaribe.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalEnvioCaribe.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refDeudaEnvioCaribe.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS CUBIQ

                    float[] columnWidthsCubiq = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthsCubiq);
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

                    List<OrderCubiq> envioscubiq = await _context.OrderCubiqs
                        .Include(x => x.agencyTransferida)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.User)
                        .Where(x => (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferidaId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date)
                        .Where(x => x.Status != OrderCubiq.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    decimal refTotalCubiq = 0;
                    decimal refPagadoCubiq = 0;
                    decimal refDeudaCubiq = 0;
                    if (envioscubiq.Count != 0)
                    {

                        doc.Add(new Phrase("Envíos Carga AM", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipos Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        foreach (OrderCubiq enviocubiq in envioscubiq)
                        {
                            decimal reftotal = enviocubiq.Amount;
                            decimal refpagado = 0;
                            decimal refdeuda = 0;
                            bool isbytransferencia = false;
                            if (enviocubiq.agencyTransferida != null)
                            {
                                if (enviocubiq.agencyTransferidaId == agency.AgencyId)
                                {
                                    isbytransferencia = true;
                                    reftotal = enviocubiq.Costo + enviocubiq.OtrosCostos;
                                    refpagado = 0;
                                    refdeuda = reftotal;
                                }
                            }

                            string pagos = "";
                            if (!isbytransferencia)
                            {
                                foreach (var item in enviocubiq.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo" && item.tipoPago.Type != "Cubiq")
                                        refpagado += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;
                                    pagos += item.tipoPago.Type + ", ";
                                }
                                refdeuda = reftotal - refpagado;
                            }

                            if (pagos == "") { pagos = "-"; }

                            refTotalCubiq += reftotal;
                            refPagadoCubiq += refpagado;
                            refDeudaCubiq += refdeuda;

                            grantotal += reftotal;
                            totalpagado += refpagado;
                            deudatotal += refdeuda;

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
                            }

                            cellremesas1.AddElement(new Phrase(isbytransferencia ? "T." + enviocubiq.Number : enviocubiq.Number, isbytransferencia ? fonttransferida : normalFont));
                            cellremesas2.AddElement(new Phrase(enviocubiq.Client.Name + " " + enviocubiq.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocubiq.Client != null ? enviocubiq.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(enviocubiq.User != null ? $"{enviocubiq.User.Name} {enviocubiq.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refpagado.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(reftotal.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refdeuda.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(isbytransferencia ? "Pendiente" : pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoCubiq.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalCubiq.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refDeudaCubiq.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // PASAPORTES

                    float[] columnWidthspasaportes = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthspasaportes);
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

                    List<Passport> pasaportes = await _context.Passport
                        .Include(x => x.Agency)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.AgencyTransferida)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Client).Where(x => (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId) && x.FechaSolicitud.Date >= dateIni.Date && x.FechaSolicitud.Date <= dateFin.Date)
                        .Where(x => x.Status != Passport.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    if (AgencyName.IsDistrictCuba(agency.AgencyId))
                    {
                        pasaportes = pasaportes.OrderBy(x => x.OrderNumber).ToList();
                    }
                    decimal refTotalPasaporte = 0;
                    decimal refPagadoPasaporte = 0;
                    decimal refDeudaPasaporte = 0;
                    if (pasaportes.Count != 0)
                    {

                        doc.Add(new Phrase("Pasaportes", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipos Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        foreach (Passport pasaporte in pasaportes)
                        {
                            decimal reftotal = pasaporte.Total;
                            decimal refpagado = 0;
                            decimal refdeuda = 0;
                            bool bytransferencia = false;
                            if (pasaporte.AgencyTransferidaId == agency.AgencyId)
                            {
                                bytransferencia = true;
                                reftotal = pasaporte.costo + pasaporte.OtrosCostos;
                                refpagado = 0;
                                refdeuda = reftotal;
                            }

                            string pagos = "";

                            if (!bytransferencia)
                            {
                                foreach (var item in pasaporte.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagado += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;
                                    pagos += item.tipoPago.Type + ", ";
                                }
                                refdeuda = reftotal - refpagado;
                            }


                            if (pagos == "") { pagos = "-"; }


                            refTotalPasaporte += reftotal;
                            refDeudaPasaporte += refdeuda;
                            refPagadoPasaporte += refpagado;

                            grantotal += reftotal;
                            totalpagado += refpagado;
                            deudatotal += refdeuda;

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
                            }

                            cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                            if (pasaporte.AgencyTransferida != null)
                            {
                                if (pasaporte.AgencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    cellremesas1.AddElement(new Phrase("T. " + pasaporte.Agency.Name, fonttransferida));

                                }
                            }
                            cellremesas2.AddElement(new Phrase(pasaporte.Client.Name + " " + pasaporte.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(pasaporte.Client.Phone != null ? pasaporte.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(pasaporte.User != null ? $"{pasaporte.User.Name} {pasaporte.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refpagado.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(reftotal.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refdeuda.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(bytransferencia ? "Pendiente" : pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoPasaporte.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalPasaporte.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refDeudaPasaporte.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS AEREOS

                    float[] columnWidthsaereo = { (float)1.5, 2, 2, 1, 1, 1, 1 };
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

                    List<Order> enviosaereos = await _context.Order
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Agency)
                        .Include(x => x.User)
                        .Include(x => x.agencyTransferida)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date && x.Type != "Remesas" && x.Type != "Combo")
                        .Where(x => x.Status != Order.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    decimal refTotalEnvioAereo = 0;
                    decimal refDeudaEnvioAereo = 0;
                    decimal refPagadoEnvioAereo = 0;
                    decimal refCreditoEnvioAereo = 0;
                    if (enviosaereos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Aéreos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        foreach (Order envioaereo in enviosaereos)
                        {
                            decimal refTotal = envioaereo.Amount;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;
                            decimal creditoConsumo = 0;

                            bool isbytransferencia = false;

                            if (envioaereo.agencyTransferida != null)
                            {
                                if (envioaereo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    isbytransferencia = true;
                                    refTotal = envioaereo.costoMayorista + envioaereo.OtrosCostos;
                                    refPagado = 0;
                                    refDeuda = refTotal;
                                }
                            }

                            string pagos = "";
                            if (!isbytransferencia)
                            {
                                foreach (var item in envioaereo.Pagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;

                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;
                                    pagos += item.tipoPago.Type + ", ";
                                }

                                refDeuda = refTotal - (refPagado + creditoConsumo);
                                refCreditoEnvioAereo += creditoConsumo;

                                refTotalEnvioAereo += refTotal;
                                refPagadoEnvioAereo += refPagado;
                                refDeudaEnvioAereo += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;
                            }
                            else
                            {
                                refTotalEnvioAereo += refTotal;
                                refPagadoEnvioAereo += refPagado;
                                refDeudaEnvioAereo += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;
                            }
                            if (pagos == "") { pagos = "-"; }




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
                            }

                            if (envioaereo.credito > 0)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(envioaereo.Number, normalFont));
                            if (envioaereo.agencyTransferida != null)
                            {
                                if (envioaereo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    cellremesas1.AddElement(new Phrase("T. " + envioaereo.Agency.Name, fonttransferida));

                                }
                            }
                            cellremesas2.AddElement(new Phrase(envioaereo.Client.Name + " " + envioaereo.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(envioaereo.Client.Phone != null ? envioaereo.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(envioaereo.User != null ? $"{envioaereo.User.Name} {envioaereo.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refPagado.ToString(), normalFont));
                            if (creditoConsumo > 0 && !isbytransferencia)
                                cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));

                            cellremesas5.AddElement(new Phrase(refTotal.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refDeuda.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(isbytransferencia ? "Pendiente" : pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoEnvioAereo.ToString(), headFont));
                        if (refCreditoEnvioAereo > 0)
                            cellremesas5.AddElement(new Phrase(refCreditoEnvioAereo.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refDeudaEnvioAereo.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // ENVIOS COMBOS

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


                    List<Order> envioscombos = await _context.Order
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Agency)
                        .Include(x => x.User)
                        .Include(x => x.Minorista)
                        .Include(x => x.agencyTransferida)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date && x.Type == "Combo")
                        .Where(x => x.Status != Order.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    decimal refTotalEnvioCombo = 0;
                    decimal refDeudaEnvioCombo = 0;
                    decimal refPagadoEnvioCombo = 0;
                    decimal refCreditoEnvioCombo = 0;
                    if (envioscombos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Combos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        foreach (Order enviocombo in envioscombos)
                        {
                            decimal refTotal = enviocombo.Amount;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;
                            decimal creditoConsumo = 0;

                            bool isbytransferencia = false;

                            if (enviocombo.agencyTransferida != null)
                            {
                                if (enviocombo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    isbytransferencia = true;
                                    refTotal = enviocombo.costoMayorista + enviocombo.OtrosCostos;
                                    refPagado = 0;
                                    refDeuda = refTotal;
                                }
                            }
                            else if (enviocombo.Minorista != null)
                            {
                                isbytransferencia = true;
                                refTotal = enviocombo.costoMayorista + enviocombo.OtrosCostos;
                                refPagado = 0;
                                refDeuda = refTotal;
                            }

                            string pagos = "";
                            if (!isbytransferencia)
                            {
                                foreach (var item in enviocombo.Pagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;

                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;
                                    pagos += item.tipoPago.Type + ", ";
                                }

                                refDeuda = refTotal - (refPagado + creditoConsumo);
                                refCreditoEnvioCombo += creditoConsumo;

                                refTotalEnvioCombo += refTotal;
                                refPagadoEnvioCombo += refPagado;
                                refDeudaEnvioCombo += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;
                            }
                            else
                            {
                                refTotalEnvioCombo += refTotal;
                                refPagadoEnvioCombo += refPagado;
                                refDeudaEnvioCombo += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;
                            }
                            if (pagos == "") { pagos = "-"; }

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
                            }

                            if (enviocombo.credito > 0)
                            {
                                cellremesas1.BackgroundColor = colorcell;
                                cellremesas2.BackgroundColor = colorcell;
                                cellremesas3.BackgroundColor = colorcell;
                                cellremesas4.BackgroundColor = colorcell;
                                cellremesas5.BackgroundColor = colorcell;
                                cellremesas6.BackgroundColor = colorcell;
                                cellremesas7.BackgroundColor = colorcell;
                            }

                            cellremesas1.AddElement(new Phrase(enviocombo.Number, normalFont));
                            if (enviocombo.agencyTransferida != null)
                            {
                                if (enviocombo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    cellremesas1.AddElement(new Phrase("T. " + enviocombo.Agency.Name, fonttransferida));

                                }
                            }
                            else if (enviocombo.Minorista != null)
                            {
                                cellremesas1.AddElement(new Phrase("T. " + enviocombo.Minorista.Name, fonttransferida));
                            }
                            cellremesas2.AddElement(new Phrase(enviocombo.Client.Name + " " + enviocombo.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocombo.Client.Phone != null ? enviocombo.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(enviocombo.User != null ? $"{enviocombo.User.Name} {enviocombo.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refPagado.ToString(), normalFont));
                            if (creditoConsumo > 0 && !isbytransferencia)
                                cellremesas4.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));

                            cellremesas5.AddElement(new Phrase(refTotal.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refDeuda.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(isbytransferencia ? "Pendiente" : pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoEnvioCombo.ToString(), headFont));
                        if (refCreditoEnvioCombo > 0)
                            cellremesas5.AddElement(new Phrase(refCreditoEnvioCombo.ToString(), headRedFont));
                        cellremesas6.AddElement(new Phrase(refDeudaEnvioCombo.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // BOLETOS  
                    float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, (float)1, (float)1, (float)1, (float)1 };
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
                    PdfPCell cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    List<Ticket> boletos = _context.Ticket
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => !x.ClientIsCarrier && x.PaqueteTuristicoId == null && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date)
                        .Where(x => x.State != Ticket.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToList();

                    decimal refTotalBoletos = 0;
                    decimal refDeudaBoletos = 0;
                    decimal refPagadoBoletos = 0;
                    decimal refCreditoBoletos = 0;
                    if (boletos.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Tipo", headFont));
                        cellremesas5.AddElement(new Phrase("Pagado", headFont));
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

                        foreach (Ticket boleto in boletos)
                        {
                            decimal refTotal = boleto.Total;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;
                            decimal creditoConsumo = 0;
                            string pagos = "";

                            foreach (var item in boleto.RegistroPagos)
                            {
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refPagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;

                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;
                                pagos += item.tipoPago.Type + ", ";
                            }
                            refDeuda = refTotal - (refPagado + creditoConsumo);
                            refCreditoBoletos += creditoConsumo;
                            grantotal += refTotal;
                            totalpagado += refPagado;
                            deudatotal += refDeuda;

                            refTotalBoletos += refTotal;
                            refPagadoBoletos += refPagado;
                            refDeudaBoletos += refDeuda;

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

                            if (creditoConsumo > 0)
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

                            cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.Client.Name + " " + boleto.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.Client.Phone != null ? boleto.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(boleto.User != null ? $"{boleto.User.Name} {boleto.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                            cellremesas5.AddElement(new Phrase(refPagado.ToString(), normalFont));
                            if (creditoConsumo > 0)
                            {
                                cellremesas5.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                            }
                            cellremesas6.AddElement(new Phrase(refTotal.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(refDeuda.ToString(), normalFont));
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
                        cellremesas4.AddElement(new Phrase("", normalFont));
                        cellremesas5.AddElement(new Phrase(refPagadoBoletos.ToString(), headFont));
                        if (refCreditoBoletos > 0)
                            cellremesas5.AddElement(new Phrase(refCreditoBoletos.ToString(), headRedFont));

                        cellremesas6.AddElement(new Phrase(refTotalBoletos.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refDeudaBoletos.ToString(), headFont));
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


                    #endregion

                    #region // BOLETOS CARRIER
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

                    List<Ticket> boletosCarrier = _context.Ticket
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Where(x => x.ClientIsCarrier && x.PaqueteTuristicoId == null && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date)
                        .Where(x => x.State != Remittance.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToList();

                    decimal refTotalBoletosCarrier = 0;
                    decimal refDeudaBoletosCarrier = 0;
                    decimal refPagadoBoletosCarrier = 0;
                    decimal refCreditoBoletosCarrier = 0;
                    if (boletosCarrier.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos Carrier", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Tipo", headFont));
                        cellremesas5.AddElement(new Phrase("Pagado", headFont));
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

                        foreach (Ticket boleto in boletosCarrier)
                        {
                            decimal refTotal = boleto.Total;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;
                            decimal creditoConsumo = 0;
                            string pagos = "";

                            foreach (var item in boleto.RegistroPagos)
                            {
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refPagado += item.valorPagado;
                                else
                                    creditoConsumo += item.valorPagado;

                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;
                                pagos += item.tipoPago.Type + ", ";
                            }
                            refDeuda = refTotal - (refPagado + creditoConsumo);
                            refCreditoBoletosCarrier += creditoConsumo;
                            grantotal += refTotal;
                            totalpagado += refPagado;
                            deudatotal += refDeuda;

                            refTotalBoletosCarrier += refTotal;
                            refPagadoBoletosCarrier += refPagado;
                            refDeudaBoletosCarrier += refDeuda;

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

                            if (creditoConsumo > 0)
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

                            cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.Client.Name + " " + boleto.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(boleto.Client.Phone != null ? boleto.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(boleto.User != null ? $"{boleto.User.Name} {boleto.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                            cellremesas5.AddElement(new Phrase(refPagado.ToString(), normalFont));
                            if (creditoConsumo > 0)
                            {
                                cellremesas5.AddElement(new Phrase(creditoConsumo.ToString(), normalRedFont));
                            }
                            cellremesas6.AddElement(new Phrase(refTotal.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(refDeuda.ToString(), normalFont));
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
                        cellremesas4.AddElement(new Phrase("", normalFont));
                        cellremesas5.AddElement(new Phrase(refPagadoBoletosCarrier.ToString(), headFont));
                        if (refCreditoBoletosCarrier > 0)
                            cellremesas5.AddElement(new Phrase(refCreditoBoletosCarrier.ToString(), headRedFont));

                        cellremesas6.AddElement(new Phrase(refTotalBoletosCarrier.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refDeudaBoletosCarrier.ToString(), headFont));
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
                    #endregion

                    #region // RECARGAS

                    float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1 };
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

                    List<Rechargue> recargas = await _context.Rechargue
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.Client).ThenInclude(x => x.Phone)
                        .Include(x => x.User)
                        .Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.date.Date >= dateIni.Date && x.date.Date <= dateFin.Date)
                        .Where(x => x.estado != Rechargue.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    decimal refTotalRecarga = 0;
                    decimal refPagadoRecarga = 0;
                    decimal refDeudaRecarga = 0;
                    if (recargas.Count != 0)
                    {
                        doc.Add(new Phrase("RECARGAS", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Pagado", headFont));
                        cellremesas5.AddElement(new Phrase("Total", headFont));
                        cellremesas6.AddElement(new Phrase("Debe", headFont));
                        cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);


                        foreach (Rechargue recarga in recargas)
                        {
                            decimal refTotal = recarga.Import;
                            decimal refPagado = 0;
                            decimal refDeuda = 0;



                            string pagos = "";
                            foreach (var item in recarga.RegistroPagos)
                            {
                                if (item.tipoPago.Type != "Crédito de Consumo")
                                    refPagado += item.valorPagado;
                                ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                canttipopago[item.tipoPago.Type] += 1;
                                pagos += item.tipoPago.Type + ", ";
                            }
                            refDeuda = refTotal - refPagado;

                            refTotalRecarga += refTotal;
                            refPagadoRecarga += refPagado;
                            refDeudaRecarga += refDeuda;

                            grantotal += refTotal;
                            totalpagado += refPagado;
                            deudatotal += refDeuda;

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
                            }

                            cellremesas1.AddElement(new Phrase(recarga.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(recarga.Client.Name + " " + recarga.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(recarga.Client.Phone != null ? recarga.Client.Phone.Number : "", normalFont));
                            cellremesas3.AddElement(new Phrase(recarga.User != null ? $"{recarga.User.Name} {recarga.User.LastName}" : "", normalFont));
                            cellremesas4.AddElement(new Phrase(refPagado.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(refTotal.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(refDeuda.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(pagos, normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
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

                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refPagadoRecarga.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalRecarga.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refDeudaRecarga.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        // Añado la tabla al documento
                        doc.Add(tblremesasData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region OTROS SERVICIOS
                    float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1 };

                    List<Servicio> servicios = await _context.Servicios
                        .Include(x => x.tipoServicio)
                        .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.cliente).ThenInclude(x => x.Phone)
                        .Include(x => x.User)
                        .Where(x => x.PaqueteTuristicoId == null && x.agency.AgencyId == aAgency.FirstOrDefault().AgencyId && x.fecha.Date >= dateIni.Date && x.fecha.Date <= dateFin.Date)
                        .Where(x => x.estado != Servicio.EstadoCancelado || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToListAsync();

                    //Hago una tabla para cada servicio
                    var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == agency.AgencyId);

                    decimal refTotalServicios = 0;
                    decimal refPagadoServicios = 0;
                    decimal refDeudaServicios = 0;
                    foreach (var tipo in tiposervicios)
                    {
                        var auxservicios = servicios.Where(x => x.tipoServicio == tipo).ToList();
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
                            doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("Pagado", headFont));
                            cellremesas5.AddElement(new Phrase("Total", headFont));
                            cellremesas6.AddElement(new Phrase("Debe", headFont));
                            cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);

                            decimal refTotalServiciosTbl = 0;
                            decimal refPagadoServiciosTbl = 0;
                            decimal refDeudaServiciosTbl = 0;
                            foreach (Servicio servicio in auxservicios)
                            {
                                decimal refTotal = servicio.importeTotal;
                                decimal refPagado = 0;
                                decimal refDeuda = 0;

                                string pagos = "";
                                foreach (var item in servicio.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    ventastipopago[item.tipoPago.Type] += item.valorPagado;
                                    canttipopago[item.tipoPago.Type] += 1;
                                    pagos += item.tipoPago.Type + ", ";
                                }
                                refDeuda = refTotal - refPagado;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;

                                refTotalServicios += refTotal;
                                refDeudaServicios += refDeuda;
                                refPagadoServicios += refPagado;

                                refTotalServiciosTbl += refTotal;
                                refDeudaServiciosTbl += refDeuda;
                                refPagadoServiciosTbl += refPagado;

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
                                }

                                cellremesas1.AddElement(new Phrase(servicio.numero, normalFont));
                                cellremesas2.AddElement(new Phrase(servicio.cliente.Name + " " + servicio.cliente.LastName, normalFont));
                                cellremesas2.AddElement(new Phrase(servicio.cliente.Phone != null ? servicio.cliente.Phone.Number : "", normalFont));
                                cellremesas3.AddElement(new Phrase(servicio.User != null ? $"{servicio.User.Name} {servicio.User.LastName}" : "", normalFont));
                                cellremesas4.AddElement(new Phrase(refPagado.ToString(), normalFont));
                                cellremesas5.AddElement(new Phrase(refTotal.ToString(), normalFont));
                                cellremesas6.AddElement(new Phrase(refDeuda.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(pagos, normalFont));

                                tblremesasData.AddCell(cellremesas1);
                                tblremesasData.AddCell(cellremesas2);
                                tblremesasData.AddCell(cellremesas3);
                                tblremesasData.AddCell(cellremesas4);
                                tblremesasData.AddCell(cellremesas5);
                                tblremesasData.AddCell(cellremesas6);
                                tblremesasData.AddCell(cellremesas7);
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

                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(refPagadoServiciosTbl.ToString(), headFont));
                            cellremesas5.AddElement(new Phrase(refTotalServiciosTbl.ToString(), headFont));
                            cellremesas6.AddElement(new Phrase(refDeudaServiciosTbl.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase("", normalFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);

                            // Añado la tabla al documento
                            doc.Add(tblremesasData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
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

                    foreach (var item in ventastipopago)
                    {
                        cellremesas1 = new PdfPCell();
                        cellremesas1.Border = 0;
                        cellremesas2 = new PdfPCell();
                        cellremesas2.Border = 0;
                        cellremesas3 = new PdfPCell();
                        cellremesas3.Border = 0;

                        cellremesas1.AddElement(new Phrase(item.Key, normalFont));
                        cellremesas2.AddElement(new Phrase(canttipopago[item.Key].ToString(), normalFont));
                        cellremesas3.AddElement(new Phrase(item.Value.ToString(), normalFont));
                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                    }

                    doc.Add(tblremesasData);
                    #endregion

                    #region Reporte por cantidad de trámites
                    float[] columnwhidts2 = { 5, 3 };
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
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalRemesas + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = paquetesTuristicos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PAQUETES TURÍSTICOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalPaqueteTuristico + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosmaritimos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalMaritimo + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = pasaportes.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + refTotalPasaporte + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }


                    auxcant = envioscaribe.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioCaribe + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscubiq.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalCubiq + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosaereos.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioAereo + " usd", normalFont));

                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscombos.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioCombo + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletos.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalBoletos + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = boletosCarrier.Count();
                    auxcanttoal += auxcant;
                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalBoletosCarrier + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = recargas.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalRecarga + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    foreach (var item in tiposervicios)
                    {
                        var auxservicio = servicios.Where(x => x.tipoServicio == item).ToList();
                        auxcant = auxservicio.Count();
                        auxcanttoal += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase(item.Nombre.ToUpper() + ": ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxservicio.Sum(x => x.importeTotal).ToString() + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                    }
                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);

                    cellleft.AddElement(new Phrase("Crédito de consumo", underLineFont));
                    cellleft.AddElement(Chunk.NEWLINE);
                    int auxcantTotalCredito = 0;

                    int auxcantCredito = enviosaereos.Where(x => x.credito > 0).Count();
                    auxcantTotalCredito += auxcantCredito;
                    if (auxcantCredito != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcantCredito + " Crédito de Consumo --$" + enviosaereos.Sum(x => x.credito) + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcantCredito = envioscombos.Where(x => x.credito > 0).Count();
                    auxcantTotalCredito += auxcantCredito;
                    if (auxcantCredito != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcantCredito + " Crédito de Consumo --$" + envioscombos.Sum(x => x.credito) + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcantCredito = boletos.Where(x => x.Credito > 0).Count();
                    auxcantTotalCredito += auxcantCredito;
                    if (auxcantCredito != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcantCredito + " Crédito --$" + boletos.Sum(x => x.Credito) + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Crédito de Consumo Total: ", headFont);
                    aux.AddSpecial(new Phrase(auxcantTotalCredito + " Crédito de Consumo", normalFont));
                    cellleft.AddElement(aux);
                    #endregion

                    #region CANCELADAS

                    var logs = _context.Logs
                    .Include(x => x.Order)
                    .Include(x => x.OrderCubic)
                    .Include(x => x.Passport)
                    .Include(x => x.EnvioMaritimo)
                    .Include(x => x.Rechargue)
                    .Include(x => x.Remittance)
                    .Include(x => x.Reserva)
                    .Include(x => x.EnvioCaribe)
                    .Include(x => x.Servicio).ThenInclude(x => x.tipoServicio)
                    .Include(x => x.User)
                    .Where(x => x.AgencyId == agency.AgencyId && x.Date.Date >= dateIni && x.Date.Date <= dateFin && x.Event == LogEvent.Cancelar)
                    //.Where(x => x.Type == LogType.Orden || x.Type == LogType.Cubiq || x.Type == LogType.Pasaporte || x.Type == LogType.Recarga || x.Type == LogType.Reserva || x.Type == LogType.Servicio || x.Type == LogType.Remesa)
                    .ToList();

                    decimal totalCancelaciones = 0;
                    decimal totalCancelaciones2 = 0;
                    PdfPTable tableCanceladas = null;
                    PdfPTable tableCanceladas2 = null;
                    if (dateIni.Month != dateFin.Month)
                    {
                        tableCanceladas = GetTableCancelations(logs, out totalCancelaciones);
                    }
                    else
                    {
                        var response = await GetTableCancelationsByMonth(logs, dateIni, dateFin);
                        totalCancelaciones = response.AmountThisMonth;
                        totalCancelaciones2 = response.AmountLastMonth;
                        tableCanceladas = response.TableThisMonth;
                        tableCanceladas2 = response.TableLastMonth;
                    }

                    #endregion


                    #region //GRAN TOTAL
                    doc.Add(Chunk.NEWLINE);
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph total = new Paragraph("Total: ", headFont2);
                    total.AddSpecial(new Phrase("$ " + grantotal.ToString("0.00"), normalFont2));
                    total.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(total);
                    Paragraph porpagar = new Paragraph("Pagado: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalpagado.ToString("0.00"), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);
                    var creditoTotal = refCreditoBoletos + refCreditoEnvioAereo;
                    if (creditoTotal > 0)
                    {
                        Paragraph credito = new Paragraph("Crédito: ", headFont2);
                        credito.AddSpecial(new Phrase("$ " + creditoTotal.ToString("0.00"), normalFont2));
                        credito.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(credito);
                    }

                    Paragraph deuda = new Paragraph("Pendiente a pago: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + deudatotal.ToString("0.00"), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);

                    if (dateIni.Month != dateFin.Month)
                    {
                        Paragraph cancelaciones = new Paragraph("Cancelaciones: ", headFont2);
                        cancelaciones.AddSpecial(new Phrase("-$ " + totalCancelaciones.ToString("0.00"), normalFont2));
                        cancelaciones.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(cancelaciones);
                        cellright.AddElement(Chunk.NEWLINE);

                    }
                    else
                    {
                        Paragraph cancelaciones = new Paragraph("* Cancelaciones: ", headFont2);
                        cancelaciones.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(cancelaciones);

                        cancelaciones = new Paragraph("Este Mes: ", headFont2);
                        cancelaciones.AddSpecial(new Phrase("-$ " + totalCancelaciones.ToString("0.00"), normalFont2));
                        cancelaciones.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(cancelaciones);

                        cancelaciones = new Paragraph("Meses Anteriores: ", headFont2);
                        cancelaciones.AddSpecial(new Phrase("-$ " + totalCancelaciones2.ToString("0.00"), normalFont2));
                        cancelaciones.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(cancelaciones);
                    }
                    #endregion

                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    if (tableCanceladas != null)
                    {
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(new Phrase("CANCELACIONES", underLineFont));
                        doc.Add(tableCanceladas);
                    }

                    if (tableCanceladas2 != null)
                    {
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(new Phrase("CANCELACIONES MESES ANTERIORES", underLineFont));
                        doc.Add(tableCanceladas2);
                    }

                    doc.Close();
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

        public async static Task<string> GetReporteVentasAdrianMyScooter(string rangeDate, databaseContext _context, User user, IWebHostEnvironment _env)
        {
            List<Guid> agenciesId = new List<Guid>() { AgencyName.AdrianMyScooterFlagler, AgencyName.AdrianMyScooter57Ave, AgencyName.AdrianMyScooter67Ave };

            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.Open();
                try
                {
                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font fonttransferida = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.ITALIC, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    BaseColor colorcell = WebColors.GetRGBColor("#fffdc4");

                    decimal summaryTotal = 0;
                    decimal summaryTotalPaid = 0;
                    decimal summaryTotalDebt = 0;
                    decimal summaryTotalCredit = 0;
                    decimal summaryTotalCancelations = 0;
                    decimal summaryTotalOrders = 0;

                    List<(int, ProductoBodega, Guid, decimal)> productosBodega = new List<(int, ProductoBodega, Guid, decimal)>();
                    List<Servicio> totalServicios = new List<Servicio>();
                    foreach (var agencyId in agenciesId)
                    {
                        var agency = _context.Agency.FirstOrDefault(x => x.AgencyId == agencyId);
                        var agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                        var agencyPhone = _context.Phone.Where(x => x.ReferenceId == agency.AgencyId).FirstOrDefault();

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
                            if (System.IO.File.Exists(filePathQR))
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

                        tableEncabezado.AddCell(cellAgency);
                        tableEncabezado.AddCell(celllogo);
                        doc.Add(tableEncabezado);
                        doc.Add(Chunk.NEWLINE);

                        var auxDate = rangeDate.Split('-');
                        CultureInfo culture = new CultureInfo("es-US", true);
                        var dateIni = DateTime.Parse(auxDate[0], culture);
                        var dateFin = DateTime.Parse(auxDate[1], culture);

                        doc.Add(Chunk.NEWLINE);
                        iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);

                        string texto = dateIni.Date.ToShortDateString() + " a " + dateFin.Date.ToShortDateString();
                        if (dateIni.Date == dateFin.Date) texto = dateIni.Date.ToShortDateString();

                        Paragraph parPaq = new Paragraph("Ventas por servicio del " + texto, line);
                        parPaq.Alignment = Element.ALIGN_CENTER;
                        doc.Add(parPaq);
                        doc.Add(Chunk.NEWLINE);

                        Dictionary<string, decimal> ventasTipoPago = new Dictionary<string, decimal>();
                        Dictionary<string, int> cantTipoPago = new Dictionary<string, int>();

                        Action<string, string, decimal> addVentaTipoPago = (tipo, reference, valor) =>
                        {
                            if (AgencyName.MiIslaServices == agency.AgencyId && tipo == "Zelle")
                                tipo = $"{tipo} {reference}";
                            if (ventasTipoPago.ContainsKey(tipo))
                                ventasTipoPago[tipo] += valor;
                            else
                                ventasTipoPago[tipo] = valor;

                            if (cantTipoPago.ContainsKey(tipo))
                                cantTipoPago[tipo]++;
                            else
                                cantTipoPago[tipo] = 1;
                        };

                        decimal grantotal = 0;
                        decimal deudatotal = 0;
                        decimal totalpagado = 0;
                        #region // REMESAS
                        List<Remittance> remesas = await _context.Remittance
                            .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.User)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Include(x => x.Agency).ThenInclude(x => x.Phone)
                            .Include(x => x.agencyTransferida)
                            .Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();

                        float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                        PdfPTable tblData = new PdfPTable(columnWidths)
                        {
                            WidthPercentage = 100
                        };

                        decimal refTotalRemesas = 0;
                        decimal refPagadpRemesas = 0;
                        decimal refDeudaRemesas = 0;
                        if (remesas.Count != 0)
                        {
                            doc.Add(new Phrase("Remesas", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                            .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (Remittance remesa in remesas)
                            {
                                decimal reftotalEQ = remesa.Amount;
                                decimal refpagadoEQ = 0;
                                decimal refdeudaEQ = 0;
                                bool isbytransferencia = false;
                                if (remesa.agencyTransferida != null)
                                {
                                    if (remesa.agencyTransferida.AgencyId == agency.AgencyId)
                                    {
                                        isbytransferencia = true;
                                        reftotalEQ = remesa.costoMayorista + remesa.OtrosCostos;
                                        refpagadoEQ = 0;
                                        refdeudaEQ = reftotalEQ;
                                    }
                                }
                                List<string> pagos = new List<string>();
                                if (!isbytransferencia)
                                {
                                    foreach (var item in remesa.Pagos)
                                    {
                                        if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                            pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                        else
                                            pagos.Add(item.tipoPago.Type);

                                        if (item.tipoPago.Type != "Crédito de Consumo")
                                            refpagadoEQ += item.valorPagado;

                                        addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                    }
                                    refdeudaEQ = reftotalEQ - refpagadoEQ;
                                }

                                grantotal += reftotalEQ;
                                totalpagado += refpagadoEQ;
                                deudatotal += refdeudaEQ;

                                refTotalRemesas += reftotalEQ;
                                refPagadpRemesas += refpagadoEQ;
                                refDeudaRemesas += refdeudaEQ;

                                var index = remesas.IndexOf(remesa);

                                AddValueToTable(tblData, isbytransferencia ? "T." + remesa.Number : remesa.Number, isbytransferencia ? fonttransferida : normalFont, index == 0 ? 1 : 0);

                                var valueCell = new List<(string, Font)>() { (remesa.Client.Name + " " + remesa.Client.LastName, normalFont),
                            (remesa.Client.Phone?.Number ?? string.Empty, normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                AddValueToTable(tblData, remesa.User != null ? remesa.User.Name + " " + remesa.User.LastName : string.Empty, normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refpagadoEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, reftotalEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refdeudaEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, refPagadpRemesas.ToString(), headFont, 0);
                            AddValueToTable(tblData, refTotalRemesas.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaRemesas.ToString(), headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }

                        #endregion

                        #region // Paquete Turistico
                        List<PaqueteTuristico> paquetesTuristicos = await _context.PaquetesTuristicos
                            .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.User)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA && x.Status != PaqueteTuristico.STATUS_INCOMPLETA && x.AgencyId == agency.AgencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();
                        tblData = new PdfPTable(columnWidths);
                        tblData.WidthPercentage = 100;

                        decimal refTotalPaqueteTuristico = 0;
                        decimal refPagadoPaqueteTuristico = 0;
                        decimal refDeudaPaqueteTuristico = 0;
                        if (paquetesTuristicos.Count != 0)
                        {
                            doc.Add(new Phrase("Paquete Turístico", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                            .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (PaqueteTuristico paqueteTuristico in paquetesTuristicos)
                            {
                                decimal reftotalEQ = paqueteTuristico.Amount;
                                decimal refpagadoEQ = 0;
                                decimal refdeudaEQ = 0;
                                bool isbytransferencia = false;

                                List<string> pagos = new List<string>();
                                if (!isbytransferencia)
                                {
                                    foreach (var item in paqueteTuristico.Pagos)
                                    {
                                        if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                            pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                        else
                                            pagos.Add(item.tipoPago.Type);

                                        if (item.tipoPago.Type != "Crédito de Consumo")
                                            refpagadoEQ += item.valorPagado;
                                        addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                    }
                                    refdeudaEQ = reftotalEQ - refpagadoEQ;
                                }

                                grantotal += reftotalEQ;
                                totalpagado += refpagadoEQ;
                                deudatotal += refdeudaEQ;

                                refTotalPaqueteTuristico += reftotalEQ;
                                refPagadoPaqueteTuristico += refpagadoEQ;
                                refDeudaPaqueteTuristico += refdeudaEQ;

                                var index = paquetesTuristicos.IndexOf(paqueteTuristico);

                                AddValueToTable(tblData, isbytransferencia ? "T." + paqueteTuristico.Number : paqueteTuristico.Number, isbytransferencia ? fonttransferida : normalFont, index == 0 ? 1 : 0);

                                var valueCell = new List<(string, Font)>() { (paqueteTuristico.Client.Name + " " + paqueteTuristico.Client.LastName, normalFont),
                            (paqueteTuristico.Client?.Phone?.Number ?? string.Empty, normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                AddValueToTable(tblData, paqueteTuristico.User != null ? paqueteTuristico.User.Name + " " + paqueteTuristico.User.LastName : string.Empty, normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refpagadoEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, reftotalEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refdeudaEQ.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, refPagadoPaqueteTuristico.ToString(), headFont, 0);
                            AddValueToTable(tblData, refTotalPaqueteTuristico.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaPaqueteTuristico.ToString(), headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }

                        #endregion

                        #region // ENVIOS MARITIMOS

                        float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthsmaritimo);
                        tblData.WidthPercentage = 100;

                        List<EnvioMaritimo> enviosmaritimos = await _context.EnvioMaritimo
                            .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.User)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Where(x => x.Status != "Cancelada" && x.AgencyId == agency.AgencyId && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();

                        decimal refTotalMaritimo = 0;
                        decimal refDeudaMaritimo = 0;
                        decimal refPagadoMaritimo = 0;
                        if (enviosmaritimos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Marítimos", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                           .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (EnvioMaritimo enviomaritimo in enviosmaritimos)
                            {
                                decimal reftotal = enviomaritimo.Amount;
                                decimal refpagado = 0;
                                decimal refdeuda = 0;

                                List<string> pagos = new List<string>();
                                foreach (var item in enviomaritimo.RegistroPagos)
                                {
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);

                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refpagado += item.valorPagado;
                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                }

                                refdeuda = reftotal - refpagado;

                                grantotal += reftotal;
                                totalpagado += refpagado;
                                deudatotal += refdeuda;

                                refTotalMaritimo += reftotal;
                                refPagadoMaritimo += refpagado;
                                refDeudaMaritimo += refdeuda;

                                var index = enviosmaritimos.IndexOf(enviomaritimo);

                                AddValueToTable(tblData, enviomaritimo.Number, normalFont, index == 0 ? 1 : 0);

                                var valueCell = new List<(string, Font)>() { (enviomaritimo.Client.Name + " " + enviomaritimo.Client.LastName, normalFont),
                            (enviomaritimo.Client?.Phone?.Number ?? string.Empty, normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                AddValueToTable(tblData, enviomaritimo.User != null ? $"{enviomaritimo.User.Name} {enviomaritimo.User.LastName}" : string.Empty, normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refpagado.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, reftotal.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refdeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            AddValueToTable(tblData, "Totales", normalFont, 0);
                            AddValueToTable(tblData, string.Empty, normalFont, 0);
                            AddValueToTable(tblData, string.Empty, normalFont, 0);
                            AddValueToTable(tblData, refPagadoMaritimo.ToString(), normalFont, 0);
                            AddValueToTable(tblData, refTotalMaritimo.ToString(), normalFont, 0);
                            AddValueToTable(tblData, refDeudaMaritimo.ToString(), normalFont, 0);
                            AddValueToTable(tblData, string.Empty, normalFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                        #endregion

                        #region // ENVIOS CARIBE

                        float[] columnWidthscaribe = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthsmaritimo);
                        tblData.WidthPercentage = 100;

                        List<EnvioCaribe> envioscaribe = await _context.EnvioCaribes
                            .Include(x => x.AgencyTransferida)
                            .Include(x => x.User)
                            .Include(x => x.RegistroPagos)
                            .ThenInclude(x => x.tipoPago)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();

                        decimal refTotalEnvioCaribe = 0;
                        decimal refPagadoEnvioCaribe = 0;
                        decimal refDeudaEnvioCaribe = 0;
                        if (envioscaribe.Count != 0)
                        {

                            doc.Add(new Phrase("Envíos Caribe", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                           .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (EnvioCaribe enviocaribe in envioscaribe)
                            {
                                decimal reftotalEC = enviocaribe.Amount;
                                decimal refpagadoEC = 0;
                                decimal refdeudaEC = 0;
                                bool isbytransferencia = false;
                                if (enviocaribe.AgencyTransferidaId != null)
                                {
                                    if (enviocaribe.AgencyTransferidaId == agency.AgencyId)
                                    {
                                        isbytransferencia = true;
                                        reftotalEC = enviocaribe.costo + enviocaribe.OtrosCostos;
                                        refpagadoEC = 0;
                                        refdeudaEC = reftotalEC;
                                    }
                                }

                                List<string> pagos = new List<string>();
                                if (!isbytransferencia)
                                {
                                    foreach (var item in enviocaribe.RegistroPagos)
                                    {
                                        if (item.tipoPago.Type != "Crédito de Consumo")
                                            refpagadoEC += item.valorPagado;
                                        addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);

                                        if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                            pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                        else
                                            pagos.Add(item.tipoPago.Type);
                                    }
                                    refdeudaEC = reftotalEC - refpagadoEC;

                                }

                                refTotalEnvioCaribe += reftotalEC;
                                refPagadoEnvioCaribe += refpagadoEC;
                                refDeudaEnvioCaribe += refdeudaEC;

                                grantotal += reftotalEC;
                                totalpagado += refpagadoEC;
                                deudatotal += refdeudaEC;

                                if (!pagos.Any()) { pagos.Add("-"); }

                                var index = envioscaribe.IndexOf(enviocaribe);

                                AddValueToTable(tblData, isbytransferencia ? "T." + enviocaribe.Number : enviocaribe.Number, isbytransferencia ? fonttransferida : normalFont, index == 0 ? 1 : 0);

                                var valueCell = new List<(string, Font)>() { (enviocaribe.Client.Name + " " + enviocaribe.Client.LastName, normalFont),
                            (enviocaribe.Client?.Phone?.Number ?? string.Empty, normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                AddValueToTable(tblData, enviocaribe.User != null ? $"{enviocaribe.User.Name} {enviocaribe.User.LastName}" : string.Empty, normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refpagadoEC.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, reftotalEC.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refdeudaEC.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            // Añado el total
                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, refPagadoEnvioCaribe.ToString(), headFont, 0);
                            AddValueToTable(tblData, refTotalEnvioCaribe.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaEnvioCaribe.ToString(), headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }


                        #endregion

                        #region // ENVIOS CUBIQ

                        float[] columnWidthsCubiq = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthsCubiq);
                        tblData.WidthPercentage = 100;

                        List<OrderCubiq> envioscubiq = await _context.OrderCubiqs
                            .Include(x => x.agencyTransferida)
                            .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Include(x => x.User)
                            .Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferidaId == agency.AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date).ToListAsync();
                        decimal refTotalCubiq = 0;
                        decimal refPagadoCubiq = 0;
                        decimal refDeudaCubiq = 0;
                        if (envioscubiq.Count != 0)
                        {

                            doc.Add(new Phrase("Envíos Carga", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                          .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (OrderCubiq enviocubiq in envioscubiq)
                            {
                                decimal reftotal = enviocubiq.Amount;
                                decimal refpagado = 0;
                                decimal refdeuda = 0;
                                bool isbytransferencia = false;
                                if (enviocubiq.agencyTransferida != null)
                                {
                                    if (enviocubiq.agencyTransferidaId == agency.AgencyId)
                                    {
                                        isbytransferencia = true;
                                        reftotal = enviocubiq.Costo + enviocubiq.OtrosCostos;
                                        refpagado = 0;
                                        refdeuda = reftotal;
                                    }
                                }

                                List<string> pagos = new List<string>();
                                if (!isbytransferencia)
                                {
                                    foreach (var item in enviocubiq.RegistroPagos)
                                    {
                                        if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                            pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                        else
                                            pagos.Add(item.tipoPago.Type);

                                        if (item.tipoPago.Type != "Crédito de Consumo" && item.tipoPago.Type != "Cubiq")
                                            refpagado += item.valorPagado;
                                        addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);

                                    }
                                    refdeuda = reftotal - refpagado;
                                }

                                if (!pagos.Any()) { pagos.Add("-"); }

                                refTotalCubiq += reftotal;
                                refPagadoCubiq += refpagado;
                                refDeudaCubiq += refdeuda;

                                grantotal += reftotal;
                                totalpagado += refpagado;
                                deudatotal += refdeuda;

                                var index = envioscubiq.IndexOf(enviocubiq);

                                AddValueToTable(tblData, isbytransferencia ? "T." + enviocubiq.Number : enviocubiq.Number, isbytransferencia ? fonttransferida : normalFont, index == 0 ? 1 : 0);

                                var valueCell = new List<(string, Font)>() { (enviocubiq.Client.Name + " " + enviocubiq.Client.LastName, normalFont),
                            (enviocubiq.Client != null ? enviocubiq.Client.Phone.Number : string.Empty, normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                AddValueToTable(tblData, enviocubiq.User != null ? $"{enviocubiq.User.Name} {enviocubiq.User.LastName}" : string.Empty, normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refpagado.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, reftotal.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refdeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            // Añado el total
                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, refPagadoCubiq.ToString(), headFont, 0);
                            AddValueToTable(tblData, refTotalCubiq.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaCubiq.ToString(), headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }

                        #endregion

                        #region // PASAPORTES

                        float[] columnWidthspasaportes = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthspasaportes);
                        tblData.WidthPercentage = 100;

                        List<Passport> pasaportes = await _context.Passport
                            .Include(x => x.Agency)
                            .Include(x => x.User)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Include(x => x.AgencyTransferida)
                            .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.Client)
                            .Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.AgencyTransferidaId == agency.AgencyId) && x.FechaSolicitud.Date >= dateIni.Date && x.FechaSolicitud.Date <= dateFin.Date)
                            .Where(x => !x.AppMovil || (x.AppMovil && x.Status != Passport.STATUS_REVIEW))
                            .ToListAsync();

                        if (AgencyName.IsDistrictCuba(agency.AgencyId))
                        {
                            pasaportes = pasaportes.OrderBy(x => x.OrderNumber).ToList();
                        }
                        decimal refTotalPasaporte = 0;
                        decimal refPagadoPasaporte = 0;
                        decimal refDeudaPasaporte = 0;
                        if (pasaportes.Count != 0)
                        {

                            doc.Add(new Phrase("Pasaportes", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                          .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (Passport pasaporte in pasaportes)
                            {
                                decimal reftotal = pasaporte.Total;
                                decimal refpagado = 0;
                                decimal refdeuda = 0;
                                bool bytransferencia = false;
                                if (pasaporte.AgencyTransferidaId == agency.AgencyId)
                                {
                                    bytransferencia = true;
                                    reftotal = pasaporte.costo + pasaporte.OtrosCostos;
                                    refpagado = 0;
                                    refdeuda = reftotal;
                                }

                                List<string> pagos = new List<string>();

                                if (!bytransferencia)
                                {
                                    foreach (var item in pasaporte.RegistroPagos)
                                    {
                                        if (item.tipoPago.Type != "Crédito de Consumo")
                                            refpagado += item.valorPagado;
                                        addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);

                                        if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                            pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                        else
                                            pagos.Add(item.tipoPago.Type);
                                    }
                                    refdeuda = reftotal - refpagado;
                                }

                                if (!pagos.Any()) { pagos.Add("-"); }

                                refTotalPasaporte += reftotal;
                                refDeudaPasaporte += refdeuda;
                                refPagadoPasaporte += refpagado;

                                grantotal += reftotal;
                                totalpagado += refpagado;
                                deudatotal += refdeuda;

                                var index = pasaportes.IndexOf(pasaporte);

                                var valueCell = new List<(string, Font)>() { (pasaporte.OrderNumber, normalFont) };
                                if (pasaporte.AgencyTransferida != null)
                                {
                                    if (pasaporte.AgencyTransferida.AgencyId == agency.AgencyId)
                                    {
                                        valueCell.Add(("T. " + pasaporte.Agency.Name, fonttransferida));
                                    }
                                }
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                valueCell = new List<(string, Font)>() { (pasaporte.Client.Name + " " + pasaporte.Client.LastName, normalFont),
                            (pasaporte.Client.Phone != null ? pasaporte.Client.Phone.Number : "", normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                AddValueToTable(tblData, pasaporte.User != null ? $"{pasaporte.User.Name} {pasaporte.User.LastName}" : "", normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refpagado.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, reftotal.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refdeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, bytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            // Añado el total
                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, refPagadoPasaporte.ToString(), headFont, 0);
                            AddValueToTable(tblData, refTotalPasaporte.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaPasaporte.ToString(), headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }


                        #endregion

                        #region // ENVIOS AEREOS

                        float[] columnWidthsaereo = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthsaereo);
                        tblData.WidthPercentage = 100;

                        List<Order> enviosaereos = await _context.Order
                            .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.Agency)
                            .Include(x => x.User)
                            .Include(x => x.Agency).ThenInclude(x => x.Phone)
                            .Include(x => x.agencyTransferida)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date && x.Type != "Remesas" && x.Type != "Combo").ToListAsync();
                        decimal refTotalEnvioAereo = 0;
                        decimal refDeudaEnvioAereo = 0;
                        decimal refPagadoEnvioAereo = 0;
                        decimal refCreditoEnvioAereo = 0;
                        if (enviosaereos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Aéreos", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                          .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (Order envioaereo in enviosaereos)
                            {
                                decimal refTotal = envioaereo.Amount;
                                decimal refPagado = 0;
                                decimal refDeuda = 0;
                                decimal creditoConsumo = 0;

                                bool isbytransferencia = false;

                                if (envioaereo.agencyTransferida != null)
                                {
                                    if (envioaereo.agencyTransferida.AgencyId == agency.AgencyId)
                                    {
                                        isbytransferencia = true;
                                        refTotal = envioaereo.costoMayorista;
                                        refPagado = 0;
                                        refDeuda = refTotal;
                                    }
                                }

                                List<string> pagos = new List<string>();
                                if (!isbytransferencia)
                                {
                                    foreach (var item in envioaereo.Pagos)
                                    {
                                        if (item.tipoPago.Type != "Crédito de Consumo")
                                            refPagado += item.valorPagado;
                                        else
                                            creditoConsumo += item.valorPagado;

                                        addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);

                                        if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                            pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                        else
                                            pagos.Add(item.tipoPago.Type);
                                    }

                                    refDeuda = refTotal - (refPagado + creditoConsumo);
                                    refCreditoEnvioAereo += creditoConsumo;

                                    refTotalEnvioAereo += refTotal;
                                    refPagadoEnvioAereo += refPagado;
                                    refDeudaEnvioAereo += refDeuda;

                                    grantotal += refTotal;
                                    totalpagado += refPagado;
                                    deudatotal += refDeuda;
                                }
                                else
                                {
                                    refTotalEnvioAereo += refTotal;
                                    refPagadoEnvioAereo += refPagado;
                                    refDeudaEnvioAereo += refDeuda;

                                    grantotal += refTotal;
                                    totalpagado += refPagado;
                                    deudatotal += refDeuda;
                                }
                                if (!pagos.Any()) { pagos.Add("-"); }

                                var index = enviosaereos.IndexOf(envioaereo);

                                List<(string, Font)> valueCell = new List<(string, Font)>() { (envioaereo.Number, normalFont) };
                                if (envioaereo.agencyTransferida != null)
                                {
                                    if (envioaereo.agencyTransferida.AgencyId == agency.AgencyId)
                                    {
                                        valueCell.Add(("T. " + envioaereo.Agency.Name, fonttransferida));
                                    }
                                }
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);

                                valueCell = new List<(string, Font)>() { (envioaereo.Client.Name + " " + envioaereo.Client.LastName, normalFont),
                                (isbytransferencia ? envioaereo.Agency.Phone?.Number ?? "" : envioaereo.Client.Phone?.Number ?? "", normalFont) };

                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                                AddValueToTable(tblData, envioaereo.User != null ? $"{envioaereo.User.Name} {envioaereo.User.LastName}" : "", normalFont, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);

                                valueCell = new List<(string, Font)>() { (refPagado.ToString(), normalFont) };
                                if (creditoConsumo > 0 && !isbytransferencia)
                                    valueCell.Add((creditoConsumo.ToString(), normalRedFont));

                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                                AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                                AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                                AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0, envioaereo.credito > 0 ? colorcell : null);
                            }

                            // Añado el total
                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, refPagadoEnvioAereo.ToString(), headFont, 0);

                            if (refCreditoEnvioAereo > 0)
                                AddValueToTable(tblData, refCreditoEnvioAereo.ToString(), headRedFont, 0);
                            else
                                AddValueToTable(tblData, "", headFont, 0);

                            AddValueToTable(tblData, refDeudaEnvioAereo.ToString(), headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                        #endregion

                        #region // ENVIOS COMBOS

                        tblData = new PdfPTable(columnWidthsaereo);
                        tblData.WidthPercentage = 100;

                        List<Order> envioscombos = await _context.Order
                            .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.Agency)
                            .Include(x => x.User)
                            .Include(x => x.Minorista)
                            .Include(x => x.agencyTransferida)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Where(x => x.Status != "Cancelada" && (x.AgencyId == agency.AgencyId || x.agencyTransferida.AgencyId == agency.AgencyId) && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date && x.Type == "Combo").ToListAsync();
                        decimal refTotalEnvioCombo = 0;
                        decimal refDeudaEnvioCombo = 0;
                        decimal refPagadoEnvioCombo = 0;
                        decimal refCreditoEnvioCombo = 0;
                        if (envioscombos.Count != 0)
                        {
                            doc.Add(new Phrase("Envíos Combos", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo Pago" }
                                .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (Order enviocombo in envioscombos)
                            {
                                decimal refTotal = enviocombo.Amount;
                                decimal refPagado = 0;
                                decimal refDeuda = 0;
                                decimal creditoConsumo = 0;

                                bool isbytransferencia = false;

                                if (enviocombo.agencyTransferida != null)
                                {
                                    if (enviocombo.agencyTransferida.AgencyId == agency.AgencyId)
                                    {
                                        isbytransferencia = true;
                                        refTotal = enviocombo.costoMayorista + enviocombo.OtrosCostos;
                                        refPagado = 0;
                                        refDeuda = refTotal;
                                    }
                                }
                                else if (enviocombo.Minorista != null)
                                {
                                    isbytransferencia = true;
                                    refTotal = enviocombo.costoMayorista + enviocombo.OtrosCostos;
                                    refPagado = 0;
                                    refDeuda = refTotal;
                                }

                                List<string> pagos = new List<string>();
                                if (!isbytransferencia)
                                {
                                    foreach (var item in enviocombo.Pagos)
                                    {
                                        if (item.tipoPago.Type != "Crédito de Consumo")
                                            refPagado += item.valorPagado;
                                        else
                                            creditoConsumo += item.valorPagado;

                                        addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                        if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                            pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                        else
                                            pagos.Add(item.tipoPago.Type);
                                    }

                                    refDeuda = refTotal - (refPagado + creditoConsumo);
                                    refCreditoEnvioCombo += creditoConsumo;

                                    refTotalEnvioCombo += refTotal;
                                    refPagadoEnvioCombo += refPagado;
                                    refDeudaEnvioCombo += refDeuda;

                                    grantotal += refTotal;
                                    totalpagado += refPagado;
                                    deudatotal += refDeuda;
                                }
                                else
                                {
                                    refTotalEnvioCombo += refTotal;
                                    refPagadoEnvioCombo += refPagado;
                                    refDeudaEnvioCombo += refDeuda;

                                    grantotal += refTotal;
                                    totalpagado += refPagado;
                                    deudatotal += refDeuda;
                                }
                                if (!pagos.Any()) { pagos.Add("-"); }

                                var index = envioscombos.IndexOf(enviocombo);

                                List<(string, Font)> valueCell = new List<(string, Font)>() { (enviocombo.Number, normalFont) };
                                if (enviocombo.agencyTransferida != null)
                                {
                                    if (enviocombo.agencyTransferida.AgencyId == agency.AgencyId)
                                    {
                                        valueCell.Add(("T. " + enviocombo.Agency.Name, fonttransferida));
                                    }
                                }
                                else if (enviocombo.Minorista != null)
                                {
                                    valueCell.Add(("T. " + enviocombo.Minorista.Name, fonttransferida));
                                }

                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);

                                valueCell = new List<(string, Font)>() { (enviocombo.Client.Name + " " + enviocombo.Client.LastName, normalFont),
                            (enviocombo.Client.Phone != null ? enviocombo.Client.Phone.Number : "", normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);

                                AddValueToTable(tblData, enviocombo.User != null ? $"{enviocombo.User.Name} {enviocombo.User.LastName}" : "", normalFont, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);

                                valueCell = new List<(string, Font)>() { (refPagado.ToString(), normalFont) };
                                if (creditoConsumo > 0 && !isbytransferencia)
                                    valueCell.Add((creditoConsumo.ToString(), normalRedFont));
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);

                                AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);
                                AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);
                                AddValueToTable(tblData, isbytransferencia ? "Pendiente" : String.Join(",", pagos), normalFont, index == 0 ? 1 : 0, enviocombo.credito > 0 ? colorcell : null);
                            }

                            // Añado el total
                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, refPagadoEnvioCombo.ToString(), headFont, 0);
                            if (refCreditoEnvioCombo > 0)
                                AddValueToTable(tblData, refCreditoEnvioCombo.ToString(), headFont, 0);
                            else
                                AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, refDeudaEnvioCombo.ToString(), headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                        #endregion

                        #region // BOLETOS  
                        float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, (float)1, (float)1, (float)1, (float)1 };
                        tblData = new PdfPTable(columnWidthboletos);
                        tblData.WidthPercentage = 100;

                        List<Ticket> boletos = _context.Ticket
                            .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.User)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Where(x => !x.ClientIsCarrier && x.PaqueteTuristicoId == null && x.State != "Cancelada" && x.AgencyId == agency.AgencyId && x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date).ToList();
                        decimal refTotalBoletos = 0;
                        decimal refDeudaBoletos = 0;
                        decimal refPagadoBoletos = 0;
                        decimal refCreditoBoletos = 0;
                        if (boletos.Count != 0)
                        {
                            doc.Add(new Phrase("Boletos", headFont));
                            new List<string>() { "No.", "Cliente", "Empleado", "Tipo", "Pagado", "Total", "Debe", "Tipo de Pago" }
                                .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (Ticket boleto in boletos)
                            {
                                decimal refTotal = boleto.Total;
                                decimal refPagado = 0;
                                decimal refDeuda = 0;
                                decimal creditoConsumo = 0;
                                List<string> pagos = new List<string>();

                                foreach (var item in boleto.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;

                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }
                                refDeuda = refTotal - (refPagado + creditoConsumo);
                                refCreditoBoletos += creditoConsumo;
                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;

                                refTotalBoletos += refTotal;
                                refPagadoBoletos += refPagado;
                                refDeudaBoletos += refDeuda;

                                var index = boletos.IndexOf(boleto);

                                AddValueToTable(tblData, boleto.ReservationNumber, normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                                var valueCell = new List<(string, Font)>() { (boleto.Client.Name + " " + boleto.Client.LastName, normalFont),
                            (boleto.Client.Phone != null ? boleto.Client.Phone.Number : "", normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                                AddValueToTable(tblData, boleto.User != null ? $"{boleto.User.Name} {boleto.User.LastName}" : "", normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                                AddValueToTable(tblData, boleto.type, normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                                valueCell = new List<(string, Font)>() { (refPagado.ToString(), normalFont) };
                                if (creditoConsumo > 0)
                                {
                                    valueCell.Add((creditoConsumo.ToString(), normalRedFont));
                                }
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                                AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                                AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                                AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                            }

                            // Añado el total
                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            var valueCell1 = new List<(string, Font)>() { (refPagadoBoletos.ToString(), headFont) };
                            if (refCreditoBoletos > 0)
                                valueCell1.Add((refCreditoBoletos.ToString(), headRedFont));

                            AddValueToTable(tblData, valueCell1, 0);
                            AddValueToTable(tblData, refTotalBoletos.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaBoletos.ToString(), headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }

                        #endregion

                        #region // BOLETOS CARRIER
                        tblData = new PdfPTable(columnWidthboletos);
                        tblData.WidthPercentage = 100;

                        List<Ticket> boletosCarrier = _context.Ticket
                            .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.User)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Where(x => x.ClientIsCarrier && x.PaqueteTuristicoId == null && x.State != "Cancelada" && x.AgencyId == agency.AgencyId && x.RegisterDate.Date >= dateIni.Date && x.RegisterDate.Date <= dateFin.Date).ToList();
                        decimal refTotalBoletosCarrier = 0;
                        decimal refDeudaBoletosCarrier = 0;
                        decimal refPagadoBoletosCarrier = 0;
                        decimal refCreditoBoletosCarrier = 0;
                        if (boletosCarrier.Count != 0)
                        {
                            doc.Add(new Phrase("Boletos Carrier", headFont));

                            doc.Add(new Phrase("Boletos", headFont));
                            new List<string>() { "No.", "Cliente", "Empleado", "Tipo", "Pagado", "Total", "Debe", "Tipo de Pago" }
                                .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (Ticket boleto in boletosCarrier)
                            {
                                decimal refTotal = boleto.Total;
                                decimal refPagado = 0;
                                decimal refDeuda = 0;
                                decimal creditoConsumo = 0;
                                List<string> pagos = new List<string>();

                                foreach (var item in boleto.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    else
                                        creditoConsumo += item.valorPagado;

                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }
                                refDeuda = refTotal - (refPagado + creditoConsumo);
                                refCreditoBoletosCarrier += creditoConsumo;
                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;

                                refTotalBoletosCarrier += refTotal;
                                refPagadoBoletosCarrier += refPagado;
                                refDeudaBoletosCarrier += refDeuda;

                                var index = boletosCarrier.IndexOf(boleto);

                                AddValueToTable(tblData, boleto.ReservationNumber, normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                                var valueCell = new List<(string, Font)>() { (boleto.Client.Name + " " + boleto.Client.LastName, normalFont),
                            (boleto.Client.Phone != null ? boleto.Client.Phone.Number : "", normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                                AddValueToTable(tblData, boleto.User != null ? $"{boleto.User.Name} {boleto.User.LastName}" : "", normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                                AddValueToTable(tblData, boleto.type, normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                                valueCell = new List<(string, Font)>() { (refPagado.ToString(), normalFont) };
                                if (creditoConsumo > 0)
                                {
                                    valueCell.Add((creditoConsumo.ToString(), normalRedFont));
                                }
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);

                                AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                                AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                                AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0, creditoConsumo > 0 ? colorcell : null);
                            }

                            // Añado el total

                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            var valueCell1 = new List<(string, Font)>() { (refPagadoBoletosCarrier.ToString(), headFont) };
                            if (refCreditoBoletosCarrier > 0)
                                valueCell1.Add((refCreditoBoletosCarrier.ToString(), headRedFont));
                            AddValueToTable(tblData, valueCell1, 0);

                            AddValueToTable(tblData, refTotalBoletosCarrier.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaBoletosCarrier.ToString(), headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                        #endregion

                        #region // RECARGAS

                        float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthsaereo);
                        tblData.WidthPercentage = 100;

                        List<Rechargue> recargas = await _context.Rechargue
                            .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Include(x => x.User)
                            .Where(x => x.estado != "Cancelada" && x.AgencyId == agency.AgencyId && x.date.Date >= dateIni.Date && x.date.Date <= dateFin.Date).ToListAsync();
                        decimal refTotalRecarga = 0;
                        decimal refPagadoRecarga = 0;
                        decimal refDeudaRecarga = 0;
                        if (recargas.Count != 0)
                        {
                            doc.Add(new Phrase("RECARGAS", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo de Pago" }
                                .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (Rechargue recarga in recargas)
                            {
                                decimal refTotal = recarga.Import;
                                decimal refPagado = 0;
                                decimal refDeuda = 0;

                                List<string> pagos = new List<string>();
                                foreach (var item in recarga.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }
                                refDeuda = refTotal - refPagado;

                                refTotalRecarga += refTotal;
                                refPagadoRecarga += refPagado;
                                refDeudaRecarga += refDeuda;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;

                                var index = recargas.IndexOf(recarga);

                                AddValueToTable(tblData, recarga.Number, normalFont, index == 0 ? 1 : 0);

                                var valueCell = new List<(string, Font)>() { (recarga.Client.Name + " " + recarga.Client.LastName, normalFont),
                            (recarga.Client.Phone != null ? recarga.Client.Phone.Number : "", normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                AddValueToTable(tblData, recarga.User != null ? $"{recarga.User.Name} {recarga.User.LastName}" : "", normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refPagado.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            // Añado el total

                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);
                            AddValueToTable(tblData, refPagadoRecarga.ToString(), headFont, 0);
                            AddValueToTable(tblData, refTotalRecarga.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaRecarga.ToString(), headFont, 0);
                            AddValueToTable(tblData, "", headFont, 0);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }


                        #endregion

                        #region OTROS SERVICIOS
                        float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1 };

                        DateTime initUtc = dateIni.Date.ToUniversalTime();
                        DateTime endUtc = dateFin.Date.AddDays(1).ToUniversalTime();
                        List<Servicio> servicios = await _context.Servicios
                            .Include(x => x.tipoServicio)
                            .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.cliente).ThenInclude(x => x.Phone)
                            .Include(x => x.User)
                            .Include(x => x.Products).ThenInclude(x => x.Product).ThenInclude(x => x.Categoria)
                            .Where(x => x.estado != "Cancelado" && x.PaqueteTuristicoId == null && x.agency.AgencyId == agency.AgencyId && x.fecha >= initUtc && x.fecha < endUtc).ToListAsync();
                        
                        totalServicios.AddRange(servicios);

                        //Hago una tabla para cada servicio
                        var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == agency.AgencyId);

                        decimal refTotalServicios = 0;
                        decimal refPagadoServicios = 0;
                        decimal refDeudaServicios = 0;
                        decimal refGastos = 0;
                        foreach (var tipo in tiposervicios)
                        {
                            var auxservicios = servicios.Where(x => x.tipoServicio == tipo).ToList();
                            if (tipo.Nombre.Equals("Gastos"))
                            {
                                refGastos += auxservicios.Sum(x => x.importeTotal);
                            }
                            else if (auxservicios.Count() != 0)
                            {
                                tblData = new PdfPTable(columnWidthsservicios);
                                tblData.WidthPercentage = 100;

                                doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                                new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo de Pago" }
                                    .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                                decimal refTotalServiciosTbl = 0;
                                decimal refPagadoServiciosTbl = 0;
                                decimal refDeudaServiciosTbl = 0;
                                foreach (Servicio servicio in auxservicios)
                                {
                                    if (servicio.Products != null && servicio.Products.Count > 0)
                                    {
                                        productosBodega.AddRange(servicio.Products.Where(x => x.Product != null).Select(x => (x.Cantidad, x.Product, (Guid)servicio.AgencyId, x.Price )));
                                    }

                                    decimal refTotal = servicio.importeTotal;
                                    decimal refPagado = 0;
                                    decimal refDeuda = 0;

                                    List<string> pagos = new List<string>();
                                    foreach (var item in servicio.RegistroPagos)
                                    {
                                        if (item.tipoPago.Type != "Crédito de Consumo")
                                            refPagado += item.valorPagado;
                                        addVentaTipoPago(item.tipoPago.Type, item.referecia, item.valorPagado);
                                        if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                            pagos.Add($"{item.tipoPago.Type} {item.referecia}");
                                        else
                                            pagos.Add(item.tipoPago.Type);
                                    }
                                    refDeuda = refTotal - refPagado;

                                    grantotal += refTotal;
                                    totalpagado += refPagado;
                                    deudatotal += refDeuda;

                                    refTotalServicios += refTotal;
                                    refDeudaServicios += refDeuda;
                                    refPagadoServicios += refPagado;

                                    refTotalServiciosTbl += refTotal;
                                    refDeudaServiciosTbl += refDeuda;
                                    refPagadoServiciosTbl += refPagado;

                                    var index = auxservicios.IndexOf(servicio);

                                    AddValueToTable(tblData, servicio.numero, normalFont, index == 0 ? 1 : 0);

                                    var valueCell = new List<(string, Font)>() { (servicio.cliente.Name + " " + servicio.cliente.LastName, normalFont),
                                    (servicio.cliente.Phone != null ? servicio.cliente.Phone.Number : "", normalFont)};
                                    AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);

                                    AddValueToTable(tblData, servicio.User != null ? $"{servicio.User.Name} {servicio.User.LastName}" : "", normalFont, index == 0 ? 1 : 0);
                                    AddValueToTable(tblData, refPagado.ToString(), normalFont, index == 0 ? 1 : 0);
                                    AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0);
                                    AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                                    AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                                }

                                // Añado el total

                                AddValueToTable(tblData, "Totales", headFont, 0);
                                AddValueToTable(tblData, "", headFont, 0);
                                AddValueToTable(tblData, "", headFont, 0);
                                AddValueToTable(tblData, refPagadoServiciosTbl.ToString(), headFont, 0);
                                AddValueToTable(tblData, refTotalServiciosTbl.ToString(), headFont, 0);
                                AddValueToTable(tblData, refDeudaServiciosTbl.ToString(), headFont, 0);
                                AddValueToTable(tblData, "", headFont, 0);

                                // Añado la tabla al documento
                                doc.Add(tblData);
                                doc.Add(Chunk.NEWLINE);
                                doc.Add(Chunk.NEWLINE);
                            }
                        }

                        #endregion

                        #region MERCADO
                        var mercados = await _context.Mercado
                            .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                            .Include(x => x.User)
                            .Include(x => x.Client).ThenInclude(x => x.Phone)
                            .Include(x => x.Agency).ThenInclude(x => x.Phone)
                            .Include(x => x.Productos).ThenInclude(x => x.Product).ThenInclude(x => x.Categoria)
                            .Where(x => x.Status != Mercado.STATUS_CANCELADA && x.AgencyId == agency.AgencyId
                            && x.Date.Date >= dateIni.Date && x.Date.Date <= dateFin.Date)
                            .ToListAsync();

                        float[] columnWidthsmercado = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                        tblData = new PdfPTable(columnWidthsmercado)
                        {
                            WidthPercentage = 100
                        };

                        decimal refTotalMercado = 0;
                        decimal refPagadoMercado = 0;
                        decimal refDeudaMercado = 0;

                        if (mercados.Count != 0)
                        {
                            doc.Add(new Phrase("MERCADO", headFont));

                            new List<string>() { "No.", "Cliente", "Empleado", "Pagado", "Total", "Debe", "Tipo de Pago" }
                                .ForEach(item => AddValueToTable(tblData, item, headFont, 1));

                            foreach (Mercado mercado in mercados)
                            {
                                productosBodega.AddRange(mercado.Productos.Where(x => x.Product != null).Select(x => (x.Cantidad, x.Product, mercado.AgencyId, x.Price)));

                                decimal refTotal = mercado.Amount;
                                decimal refPagado = 0;
                                decimal refDeuda = 0;

                                List<string> pagos = new List<string>();
                                foreach (var item in mercado.RegistroPagos)
                                {
                                    if (item.tipoPago.Type != "Crédito de Consumo")
                                        refPagado += item.valorPagado;
                                    addVentaTipoPago(item.tipoPago.Type, item.nota, item.valorPagado);
                                    if (agency.AgencyId == AgencyName.MiIslaServices && item.tipoPago.Type == "Zelle")
                                        pagos.Add($"{item.tipoPago.Type} {item.nota}");
                                    else
                                        pagos.Add(item.tipoPago.Type);
                                }
                                refDeuda = refTotal - refPagado;

                                grantotal += refTotal;
                                totalpagado += refPagado;
                                deudatotal += refDeuda;

                                refTotalMercado += refTotal;
                                refDeudaMercado += refDeuda;
                                refPagadoMercado += refPagado;

                                var index = mercados.IndexOf(mercado);

                                AddValueToTable(tblData, mercado.Number, normalFont, index == 0 ? 1 : 0);

                                var valueCell = new List<(string, Font)>() { (mercado.Client.Name + " " + mercado.Client.LastName, normalFont),
                            (mercado.Client.Phone != null ? mercado.Client.Phone.Number : "", normalFont)};
                                AddValueToTable(tblData, valueCell, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, mercado.User != null ? $"{mercado.User.Name} {mercado.User.LastName}" : "", normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refPagado.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refTotal.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, refDeuda.ToString(), normalFont, index == 0 ? 1 : 0);
                                AddValueToTable(tblData, String.Join(",", pagos), normalFont, index == 0 ? 1 : 0);
                            }

                            AddValueToTable(tblData, "Totales", headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);
                            AddValueToTable(tblData, refPagadoMercado.ToString(), headFont, 0);
                            AddValueToTable(tblData, refTotalMercado.ToString(), headFont, 0);
                            AddValueToTable(tblData, refDeudaMercado.ToString(), headFont, 0);
                            AddValueToTable(tblData, string.Empty, headFont, 0);

                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                        #endregion

                        #region // VENTAS POR TIPO DE PAGO

                        doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                        float[] columnWidthstipopago = { 3, 2, 2 };
                        tblData = new PdfPTable(columnWidthstipopago);
                        tblData.WidthPercentage = 100;

                        AddValueToTable(tblData, "Tipo de Pago", headFont, 1);
                        AddValueToTable(tblData, "Cantidad", headFont, 1);
                        AddValueToTable(tblData, "Importe", headFont, 1);

                        foreach (var item in ventasTipoPago)
                        {
                            AddValueToTable(tblData, item.Key, normalFont, 0);
                            AddValueToTable(tblData, cantTipoPago[item.Key].ToString(), normalFont, 0);
                            AddValueToTable(tblData, item.Value.ToString(), normalFont, 0);
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
                        int totalOrders = 0;


                        int auxcant = remesas.Count();
                        totalOrders += auxcant;
                        if (auxcant != 0)
                        {
                            aux = new Phrase("REMESAS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalRemesas + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                        auxcant = paquetesTuristicos.Count();
                        totalOrders += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("PAQUETES TURÍSTICOS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalPaqueteTuristico + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        auxcant = enviosmaritimos.Count();
                        totalOrders += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalMaritimo + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        auxcant = pasaportes.Count();
                        totalOrders += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("PASAPORTES: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + refTotalPasaporte + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }


                        auxcant = envioscaribe.Count();
                        totalOrders += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioCaribe + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        auxcant = envioscubiq.Count();
                        totalOrders += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalCubiq + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        auxcant = enviosaereos.Count();
                        totalOrders += auxcant;
                        if (auxcant != 0)
                        {
                            aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioAereo + " usd " + $"({enviosaereos.Sum(x => x.CantLb + x.CantLbMedicina)} Lb)", normalFont));

                            cellleft.AddElement(aux);
                        }

                        auxcant = envioscombos.Count();
                        totalOrders += auxcant;
                        if (auxcant != 0)
                        {
                            aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalEnvioCombo + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        auxcant = boletos.Count();
                        totalOrders += auxcant;
                        if (auxcant != 0)
                        {
                            aux = new Phrase("BOLETOS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalBoletos + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        auxcant = boletosCarrier.Count();
                        totalOrders += auxcant;
                        if (auxcant != 0)
                        {
                            aux = new Phrase("BOLETOS CARRIER: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalBoletosCarrier + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        auxcant = recargas.Count();
                        totalOrders += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("RECARGAS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalRecarga + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        foreach (var item in tiposervicios)
                        {
                            var auxservicio = servicios.Where(x => x.tipoServicio == item).ToList();
                            auxcant = auxservicio.Count();
                            totalOrders += auxcant;

                            if (auxcant != 0)
                            {
                                aux = new Phrase(item.Nombre.ToUpper() + ": ", headFont);
                                aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxservicio.Sum(x => x.importeTotal).ToString() + " usd", normalFont));
                                cellleft.AddElement(aux);
                            }
                        }

                        auxcant = mercados.Count();
                        totalOrders += auxcant;
                        summaryTotalOrders += totalOrders;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("Mercado: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + refTotalMercado + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        cellleft.AddElement(Chunk.NEWLINE);
                        aux = new Phrase("Total de Órdenes: ", headFont);
                        aux.AddSpecial(new Phrase(totalOrders + " Órdenes", normalFont));
                        cellleft.AddElement(aux);
                        cellleft.AddElement(Chunk.NEWLINE);

                        cellleft.AddElement(new Phrase("Crédito de consumo", underLineFont));
                        cellleft.AddElement(Chunk.NEWLINE);
                        int auxcantTotalCredito = 0;

                        int auxcantCredito = enviosaereos.Where(x => x.credito > 0).Count();
                        auxcantTotalCredito += auxcantCredito;
                        if (auxcantCredito != 0)
                        {
                            aux = new Phrase("ENVÍOS AÉREOS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcantCredito + " Crédito de Consumo --$" + enviosaereos.Sum(x => x.credito) + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                        auxcantCredito = envioscombos.Where(x => x.credito > 0).Count();
                        auxcantTotalCredito += auxcantCredito;
                        if (auxcantCredito != 0)
                        {
                            aux = new Phrase("ENVÍOS COMBOS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcantCredito + " Crédito de Consumo --$" + envioscombos.Sum(x => x.credito) + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                        auxcantCredito = boletos.Where(x => x.Credito > 0).Count();
                        auxcantTotalCredito += auxcantCredito;
                        if (auxcantCredito != 0)
                        {
                            aux = new Phrase("BOLETOS: ", headFont);
                            aux.AddSpecial(new Phrase(auxcantCredito + " Crédito --$" + boletos.Sum(x => x.Credito) + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }

                        cellleft.AddElement(Chunk.NEWLINE);
                        aux = new Phrase("Crédito de Consumo Total: ", headFont);
                        aux.AddSpecial(new Phrase(auxcantTotalCredito + " Crédito de Consumo", normalFont));
                        cellleft.AddElement(aux);
                        #endregion

                        #region CANCELADAS
                        var logs = await _context.Logs
                        .Include(x => x.Order)
                        .Include(x => x.OrderCubic)
                        .Include(x => x.Passport)
                        .Include(x => x.EnvioMaritimo)
                        .Include(x => x.Rechargue)
                        .Include(x => x.Remittance)
                        .Include(x => x.Reserva)
                        .Include(x => x.EnvioCaribe)
                        .Include(x => x.Servicio).ThenInclude(x => x.tipoServicio)
                        .Include(x => x.Mercado)
                        .Include(x => x.User)
                        .Where(x => x.AgencyId == agency.AgencyId && x.Date.Date >= dateIni && x.Date.Date <= dateFin && x.Event == LogEvent.Cancelar)
                        //.Where(x => x.Type == LogType.Orden || x.Type == LogType.Cubiq || x.Type == LogType.Pasaporte || x.Type == LogType.Recarga || x.Type == LogType.Reserva || x.Type == LogType.Servicio || x.Type == LogType.Remesa)
                        .ToListAsync();

                        decimal totalCancelaciones = logs.Sum(x => decimal.Parse(x.Precio));

                        doc.Add(Chunk.NEWLINE);
                        bool tieneCanceladas = false;
                        if (logs.Any())
                        {
                            tieneCanceladas = true;
                            float[] columnWidthscanceladas = { 1, 1, 1, 1, 1 };
                            tblData = new PdfPTable(columnWidthscanceladas);
                            tblData.WidthPercentage = 100;

                            AddValueToTable(tblData, "No. Orden", headFont, 1);
                            AddValueToTable(tblData, "Fecha", headFont, 1);
                            AddValueToTable(tblData, "Tipo Trámite", headFont, 1);
                            AddValueToTable(tblData, "Empleado", headFont, 1);
                            AddValueToTable(tblData, "Monto", headFont, 1);
                            AddCancelationTable(normalFont, logs, tblData);
                        }

                        #endregion

                        #region //GRAN TOTAL
                        Paragraph linetotal = new Paragraph("______________________", normalFont);
                        linetotal.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(linetotal);
                        Paragraph total = new Paragraph("Total: ", headFont2);
                        total.AddSpecial(new Phrase("$ " + grantotal.ToString("0.00"), normalFont2));
                        total.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(total);
                        Paragraph porpagar = new Paragraph("Pagado: ", headFont2);
                        porpagar.AddSpecial(new Phrase("$ " + totalpagado.ToString("0.00"), normalFont2));
                        porpagar.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(porpagar);

                        if (refGastos > 0)
                        {
                            Paragraph pgastos = new Paragraph("Gastos: ", headFont2);
                            pgastos.AddSpecial(new Phrase("$ " + refGastos.ToString("0.00"), normalFont2));
                            pgastos.Alignment = Element.ALIGN_RIGHT;
                            cellright.AddElement(pgastos);
                            totalpagado -= refGastos;
                            porpagar = new Paragraph("Total Pagado: ", headFont2);
                            porpagar.AddSpecial(new Phrase("$ " + totalpagado.ToString("0.00"), normalFont2));
                            porpagar.Alignment = Element.ALIGN_RIGHT;
                            cellright.AddElement(porpagar);
                        }

                        var creditoTotal = refCreditoBoletos + refCreditoEnvioAereo;
                        if (creditoTotal > 0)
                        {
                            Paragraph credito = new Paragraph("Crédito: ", headFont2);
                            credito.AddSpecial(new Phrase("$ " + creditoTotal.ToString("0.00"), normalFont2));
                            credito.Alignment = Element.ALIGN_RIGHT;
                            cellright.AddElement(credito);
                        }

                        Paragraph deuda = new Paragraph("Pendiente a pago: ", headFont2);
                        deuda.AddSpecial(new Phrase("$ " + deudatotal.ToString("0.00"), normalFont2));
                        deuda.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(deuda);

                        Paragraph cancelaciones = new Paragraph("Cancelaciones: ", headFont2);
                        cancelaciones.AddSpecial(new Phrase("-- $ " + totalCancelaciones.ToString("0.00"), normalFont2));
                        cancelaciones.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(cancelaciones);
                        cellright.AddElement(Chunk.NEWLINE);

                        summaryTotal += grantotal;
                        summaryTotalPaid += totalpagado;
                        summaryTotalDebt += deudatotal;
                        summaryTotalCancelations += totalCancelaciones;
                        summaryTotalCredit += creditoTotal;

                        #endregion

                        tableEnd.AddCell(cellleft);
                        tableEnd.AddCell(cellright);
                        doc.Add(tableEnd);
                        if (tieneCanceladas)
                        {
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(new Phrase("CANCELACIONES", underLineFont));
                            doc.Add(tblData);
                        }

                        // new page
                        doc.NewPage();
                    }

                    #region Summary
                    doc.Add(new Phrase("RESUMEN", headFont));
                    float[] columnWidthssummary = { 1, 1 };
                    var tbl = new PdfPTable(columnWidthssummary);
                    var cell1 = new PdfPCell()
                    {
                        BorderWidth = 0
                    };

                    var cell2 = new PdfPCell()
                    {
                        BorderWidth = 0
                    };

                    Paragraph p = new Paragraph("______________________", normalFont);
                    p.Alignment = Element.ALIGN_RIGHT;
                    cell2.AddElement(p);
                    p = new Paragraph("Total Órdenes: ", headFont2);
                    p.AddSpecial(new Phrase(summaryTotalOrders.ToString(), normalFont2));
                    p.Alignment = Element.ALIGN_RIGHT;
                    cell2.AddElement(p);
                    p = new Paragraph("Total: ", headFont2);
                    p.AddSpecial(new Phrase("$ " + summaryTotal.ToString("0.00"), normalFont2));
                    p.Alignment = Element.ALIGN_RIGHT;
                    cell2.AddElement(p);
                    p = new Paragraph("Pagado: ", headFont2);
                    p.AddSpecial(new Phrase("$ " + summaryTotalPaid.ToString("0.00"), normalFont2));
                    p.Alignment = Element.ALIGN_RIGHT;
                    cell2.AddElement(p);
                    if (summaryTotalCredit > 0)
                    {
                        p = new Paragraph("Crédito: ", headFont2);
                        p.AddSpecial(new Phrase("$ " + summaryTotalCredit.ToString("0.00"), normalFont2));
                        p.Alignment = Element.ALIGN_RIGHT;
                        cell2.AddElement(p);
                    }

                    p = new Paragraph("Pendiente a pago: ", headFont2);
                    p.AddSpecial(new Phrase("$ " + summaryTotalDebt.ToString("0.00"), normalFont2));
                    p.Alignment = Element.ALIGN_RIGHT;
                    cell2.AddElement(p);

                    p = new Paragraph("Cancelaciones: ", headFont2);
                    p.AddSpecial(new Phrase("-- $ " + summaryTotalCancelations.ToString("0.00"), normalFont2));
                    p.Alignment = Element.ALIGN_RIGHT;
                    cell2.AddElement(p);
                    cell2.AddElement(Chunk.NEWLINE);

                    tbl.AddCell(cell1);
                    tbl.AddCell(cell2);
                    doc.Add(tbl);
                    #endregion

                    // reporte de productos Adrian May Scooter
                    if (productosBodega.Count > 0)
                    {

                        doc.Add(Chunk.NEWLINE);
                        doc.Add(new Phrase("PRODUCTOS", underLineFont));
                        doc.Add(Chunk.NEWLINE);
                        BuildTableProduct(_context, doc, headFont, normalFont, productosBodega, totalServicios);

                    }

                    doc.Close();
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

        #region PRIVATE METHODS
        private static void BuildTableProduct(databaseContext _context, Document doc, Font headFont, Font normalFont, IEnumerable<(int, ProductoBodega, Guid, decimal)> productos, List<Servicio> servicios)
        {
            // producto, bodega, cantidad, disponibilidad, venta, costo, precio-inventario
            List<ItemProductReportSale> reportByBodega = new List<ItemProductReportSale>();

            Guid bodegaAlmacen = Guid.Parse("4a1851a0-cfd3-4529-bec0-f9a71f6561e0");
            var disponibilidadAlmacen = _context.BodegaProductos
                .Include(x => x.Bodega)
                .Include(x => x.Producto).ThenInclude(x => x.Categoria)
                .Where(x => x.Bodega.Id == bodegaAlmacen);

            foreach (var item in disponibilidadAlmacen.GroupBy(x => x.IdProducto))
            {
                var bodega = item.First().Bodega;
                var product = item.First().Producto;
                reportByBodega.Add(new ItemProductReportSale
                {
                    AgencyId = bodega.idAgency,
                    Name = product.Nombre,
                    Bodega = bodega.Nombre,
                    Availability = item.Sum(x => (int)x.Cantidad),
                    QtySale = 0,
                    Category = product.Categoria.Nombre,
                    Cost = product.PrecioCompraReferencial ?? 0,
                    PriceRef = product.PrecioVentaReferencial ?? 0,
                    SumSale = decimal.Zero,
                    ProductId = product.IdProducto
                });
            }

            foreach (var item in productos.GroupBy(x => new { x.Item2.IdProducto, x.Item3 }))
            {
                var product = item.First().Item2;
                var bodegaProducto = _context.BodegaProductos.Include(x => x.Bodega).Where(x => x.IdProducto == product.IdProducto && x.Bodega.idAgency == item.Key.Item3 && x.Bodega.Id != bodegaAlmacen);
                foreach (var bodega in bodegaProducto)
                {
                    string nombre = product.Nombre;
                    string bodegaName = bodega?.Bodega?.Nombre ?? "";
                    int cantidad = item.Sum(x => x.Item1); // Si es almacen no se cuenta la cantidad
                    int disponibilidad = bodega?.Cantidad != null ? (int)bodega.Cantidad : 0;
                    decimal venta = item.Sum(x => x.Item1 * x.Item4);

                    var exist = reportByBodega.FirstOrDefault(x => x.Name == nombre && x.Bodega == bodegaName && x.AgencyId == item.Key.Item3);
                    if (exist != default)
                    {
                        exist.QtySale += cantidad;
                        exist.SumSale += venta;
                    }
                    else reportByBodega.Add(new ItemProductReportSale
                    {
                        AgencyId = item.Key.Item3,
                        Name = nombre,
                        Bodega = bodegaName,
                        Availability = disponibilidad,
                        QtySale = cantidad,
                        Category = product.Categoria.Nombre,
                        Cost = product.PrecioCompraReferencial ?? 0,
                        PriceRef = product.PrecioVentaReferencial ?? 0,
                        SumSale = venta,
                        ProductId = product.IdProducto
                    });
                }
            }

            foreach (var byAgency in reportByBodega.GroupBy(x => x.AgencyId))
            {
                var agency = _context.Agency.Find(byAgency.Key);
                var motos = byAgency.Where(x => x.Category == "Motos");
                var piezas = byAgency.Where(x => x.Category != "Motos");
                var serviciosAux = servicios.Where(x => x.AgencyId == byAgency.Key);

                doc.Add(Chunk.NEWLINE);
                doc.Add(new Phrase(agency.Name.ToUpper(), headFont));
                foreach (var byBodega in motos.GroupBy(x => x.Bodega))
                {
                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase(byBodega.Key.ToUpperCase(), headFont));

                    float[] columnWidthsProductos = { 2, 1, 1, 1, 1, 1 };
                    var tblData = new PdfPTable(columnWidthsProductos);
                    tblData.WidthPercentage = 100;

                    AddValueToTable(tblData, "Moto", headFont, 1);
                    AddValueToTable(tblData, "Vendido", headFont, 1);
                    AddValueToTable(tblData, "Disponibilidad", headFont, 1);
                    AddValueToTable(tblData, "Costo", headFont, 1);
                    AddValueToTable(tblData, "Precio Venta Referido", headFont, 1);
                    AddValueToTable(tblData, "Sumatoria Venta Real", headFont, 1);

                    foreach (var moto in byBodega)
                    {
                        AddValueToTable(tblData, moto.Name, normalFont, 1);
                        AddValueToTable(tblData, moto.QtySale.ToString(), normalFont, 1);
                        AddValueToTable(tblData, moto.Availability.ToString(), normalFont, 1);
                        AddValueToTable(tblData, moto.Cost.ToString(), normalFont, 1);
                        AddValueToTable(tblData, moto.PriceRef.ToString(), normalFont, 1);
                        AddValueToTable(tblData, moto.SumSale.ToString(), normalFont, 1);
                    }

                    AddValueToTable(tblData, "Totales", headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.QtySale).ToString(), headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.Availability).ToString(), headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.Cost).ToString(), headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.PriceRef).ToString(), headFont, 1);
                    AddValueToTable(tblData, byBodega.Sum(x => x.SumSale).ToString(), headFont, 1);

                    doc.Add(tblData);
                }

                if (piezas.Any() || serviciosAux.Any())
                {
                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase("PIEZAS Y SERVICIOS", headFont));

                    float[] columnWidthsProductos = { 2, 1, 1, 1, 1, 1 };
                    var tblData = new PdfPTable(columnWidthsProductos);
                    tblData.WidthPercentage = 100;

                    AddValueToTable(tblData, "Producto", headFont, 1);
                    AddValueToTable(tblData, "Vendido", headFont, 1);
                    AddValueToTable(tblData, "Disponibilidad", headFont, 1);
                    AddValueToTable(tblData, "Costo", headFont, 1);
                    AddValueToTable(tblData, "Precio Venta Referido", headFont, 1);
                    AddValueToTable(tblData, "Sumatoria Venta Real", headFont, 1);

                    int totalQtySale = 0;
                    int totalAvailability = 0;
                    decimal totalCost = 0;
                    decimal totalPriceRef = 0;
                    decimal totalSumSale = 0;
                    foreach (var pieza in piezas)
                    {
                        totalQtySale += pieza.QtySale;
                        totalAvailability += pieza.Availability;
                        totalCost += pieza.Cost;
                        totalPriceRef += pieza.PriceRef;
                        totalSumSale += pieza.SumSale;

                        AddValueToTable(tblData, pieza.Name, normalFont, 1);
                        AddValueToTable(tblData, pieza.QtySale.ToString(), normalFont, 1);
                        AddValueToTable(tblData, pieza.Availability.ToString(), normalFont, 1);
                        AddValueToTable(tblData, pieza.Cost.ToString(), normalFont, 1);
                        AddValueToTable(tblData, pieza.PriceRef.ToString(), normalFont, 1);
                        AddValueToTable(tblData, pieza.SumSale.ToString(), normalFont, 1);
                    }

                    // agregar servicios a tabla de piezas
                    foreach (var servicio in serviciosAux.GroupBy(x => x.tipoServicio))
                    {
                        totalQtySale += servicio.Count();
                        totalCost += 1;
                        totalPriceRef += servicio.Key.Price;
                        totalSumSale += servicio.Sum(x => x.importeTotal);

                        AddValueToTable(tblData, servicio.Key.Nombre, normalFont, 1);
                        AddValueToTable(tblData, servicio.Count().ToString(), normalFont, 1);
                        AddValueToTable(tblData, "", normalFont, 1);
                        AddValueToTable(tblData, "1", normalFont, 1);
                        AddValueToTable(tblData, servicio.Key.Price.ToString(), normalFont, 1);
                        AddValueToTable(tblData, servicio.Sum(x => x.importeTotal).ToString(), normalFont, 1);
                    }

                    AddValueToTable(tblData, "Totales", headFont, 1);
                    AddValueToTable(tblData, totalQtySale.ToString(), headFont, 1);
                    AddValueToTable(tblData, totalAvailability.ToString(), headFont, 1);
                    AddValueToTable(tblData, totalCost.ToString(), headFont, 1);
                    AddValueToTable(tblData, totalPriceRef.ToString(), headFont, 1);
                    AddValueToTable(tblData, totalSumSale.ToString(), headFont, 1);

                    doc.Add(tblData);
                }
            }
        }


        private static PdfPTable GetTableCancelations(List<Log> logs, out decimal totalCancelaciones)
        {
            iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            totalCancelaciones = logs.Sum(x => decimal.Parse(x.Precio));
            if (logs.Any())
            {
                float[] columnWidthscanceladas = { 1, 1, 1, 1, 1 };
                PdfPTable tbl = new PdfPTable(columnWidthscanceladas);
                tbl.WidthPercentage = 100;
                PdfPCell cell1 = new PdfPCell();
                cell1.Border = 1;
                PdfPCell cell2 = new PdfPCell();
                cell2.Border = 1;
                PdfPCell cell3 = new PdfPCell();
                cell3.Border = 1;
                PdfPCell cell4 = new PdfPCell();
                cell4.Border = 1;
                PdfPCell cell5 = new PdfPCell();
                cell5.Border = 1;

                cell1.AddElement(new Phrase("No. Orden", headFont));
                cell2.AddElement(new Phrase("Fecha", headFont));
                cell3.AddElement(new Phrase("Tipo Trámite", headFont));
                cell4.AddElement(new Phrase("Empleado", headFont));
                cell5.AddElement(new Phrase("Monto", headFont));

                tbl.AddCell(cell1);
                tbl.AddCell(cell2);
                tbl.AddCell(cell3);
                tbl.AddCell(cell4);
                tbl.AddCell(cell5);
                AddCancelationTable(normalFont, logs, tbl);

                return tbl;
            }

            return null;
        }

        private static void AddCancelationTable(Font normalFont, List<Log> logs, PdfPTable tblremesasData)
        {
            foreach (var item in logs.OrderByDescending(x => x.Date))
            {
                string number = "";
                decimal? amount = 0;
                string serviceName = "";
                DateTime? date = null;
                switch (item.Type)
                {
                    case LogType.Reserva:
                        number = item.Reserva?.ReservationNumber;
                        serviceName = "Reserva";
                        date = item.Reserva?.RegisterDate;
                        amount = item.Reserva?.Total;
                        break;
                    case LogType.Combo:
                        number = item.Order?.Number;
                        serviceName = "Combo";
                        date = item.Order?.Date;
                        amount = item.Order?.Amount;
                        break;
                    case LogType.EnvioCaribe:
                        number = item.EnvioCaribe?.Number;
                        serviceName = "Envio Caribe";
                        date = item.EnvioCaribe?.Date;
                        amount = item.EnvioCaribe?.Amount;
                        break;
                    case LogType.Cubiq:
                        number = item.OrderCubic?.Number;
                        serviceName = "Cubiq";
                        date = item.OrderCubic?.Date;
                        amount = item.OrderCubic?.Amount;
                        break;
                    case LogType.EnvioMaritimo:
                        number = item.EnvioMaritimo?.Number;
                        serviceName = "Marítimo";
                        date = item.EnvioMaritimo?.Date;
                        amount = item.EnvioMaritimo?.Amount;
                        break;
                    case LogType.Orden:
                        number = item.Order?.Number;
                        serviceName = "Order";
                        date = item.Order?.Date;
                        amount = item.Order?.Amount;
                        break;
                    case LogType.Pasaporte:
                        number = item.Passport?.OrderNumber;
                        serviceName = "Pasaporte";
                        date = item.Passport?.FechaSolicitud;
                        amount = item.Passport?.Total;
                        break;
                    case LogType.Recarga:
                        number = item.Rechargue?.Number;
                        serviceName = "Recarga";
                        date = item.Rechargue?.date;
                        amount = item.Rechargue?.Import;
                        break;
                    case LogType.Remesa:
                        number = item.Remittance?.Number;
                        serviceName = "Remesa";
                        date = item.Remittance?.Date;
                        amount = item.Remittance?.Amount;
                        break;
                    case LogType.Servicio:
                        number = item.Servicio?.numero;
                        serviceName = item.Servicio?.tipoServicio?.Nombre;
                        date = item.Servicio?.fecha;
                        amount = item.Servicio?.importeTotal;
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrEmpty(number))
                    continue;

                var cellremesas1 = new PdfPCell();
                cellremesas1.Border = 1;
                var cellremesas2 = new PdfPCell();
                cellremesas2.Border = 1;
                var cellremesas3 = new PdfPCell();
                cellremesas3.Border = 1;
                var cellremesas4 = new PdfPCell();
                cellremesas4.Border = 1;
                var cellremesas5 = new PdfPCell();
                cellremesas5.Border = 1;

                cellremesas1.AddElement(new Phrase(number, normalFont));
                cellremesas2.AddElement(new Phrase(date != null ? ((DateTime)date).ToShortDateString() : "", normalFont));
                cellremesas3.AddElement(new Phrase(serviceName, normalFont));
                cellremesas4.AddElement(new Phrase(item.User?.FullName, normalFont));
                cellremesas5.AddElement(new Phrase((amount is null ? 0 : (decimal)amount).ToString("0.00"), normalFont));
                tblremesasData.AddCell(cellremesas1);
                tblremesasData.AddCell(cellremesas2);
                tblremesasData.AddCell(cellremesas3);
                tblremesasData.AddCell(cellremesas4);
                tblremesasData.AddCell(cellremesas5);
            }
        }

        private static async Task<AuxCancelationModel> GetTableCancelationsByMonth(List<Log> logs, DateTime init, DateTime end)
        {
            iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            float[] columnWidthscanceladas = { 1, 1, 1, 1, 1 };
            PdfPTable tableThisMonth = new PdfPTable(columnWidthscanceladas);
            tableThisMonth.WidthPercentage = 100;
            PdfPCell cell1 = new PdfPCell();
            cell1.Border = 1;
            PdfPCell cell2 = new PdfPCell();
            cell2.Border = 1;
            PdfPCell cell3 = new PdfPCell();
            cell3.Border = 1;
            PdfPCell cell4 = new PdfPCell();
            cell4.Border = 1;
            PdfPCell cell5 = new PdfPCell();
            cell5.Border = 1;

            cell1.AddElement(new Phrase("No. Orden", headFont));
            cell2.AddElement(new Phrase("Fecha", headFont));
            cell3.AddElement(new Phrase("Tipo Trámite", headFont));
            cell4.AddElement(new Phrase("Empleado", headFont));
            cell5.AddElement(new Phrase("Monto", headFont));

            tableThisMonth.AddCell(cell1);
            tableThisMonth.AddCell(cell2);
            tableThisMonth.AddCell(cell3);
            tableThisMonth.AddCell(cell4);
            tableThisMonth.AddCell(cell5);

            PdfPTable tableLastMonth = new PdfPTable(columnWidthscanceladas);
            tableLastMonth.WidthPercentage = 100;
            tableLastMonth.AddCell(cell1);
            tableLastMonth.AddCell(cell2);
            tableLastMonth.AddCell(cell3);
            tableLastMonth.AddCell(cell4);
            tableLastMonth.AddCell(cell5);

            int qtyThisMonth = 0;
            int qtyLastMonth = 0;
            decimal amountThisMonth = 0;
            decimal amountLastMonth = 0;
            foreach (var item in logs)
            {
                string number = "";
                decimal? amount = 0;
                string serviceName = "";
                DateTime? date = null;
                switch (item.Type)
                {
                    case LogType.Reserva:
                        number = item.Reserva?.ReservationNumber;
                        serviceName = "Reserva";
                        date = item.Reserva?.RegisterDate;
                        amount = item.Reserva?.Total;
                        break;
                    case LogType.Combo:
                        number = item.Order?.Number;
                        serviceName = "Combo";
                        date = item.Order?.Date;
                        amount = item.Order?.Amount;
                        break;
                    case LogType.EnvioCaribe:
                        number = item.EnvioCaribe?.Number;
                        serviceName = "Envio Caribe";
                        date = item.EnvioCaribe?.Date;
                        amount = item.EnvioCaribe?.Amount;
                        break;
                    case LogType.Cubiq:
                        number = item.OrderCubic?.Number;
                        serviceName = "Cubiq";
                        date = item.OrderCubic?.Date;
                        amount = item.OrderCubic?.Amount;
                        break;
                    case LogType.EnvioMaritimo:
                        number = item.EnvioMaritimo?.Number;
                        serviceName = "Marítimo";
                        date = item.EnvioMaritimo?.Date;
                        amount = item.EnvioMaritimo?.Amount;
                        break;
                    case LogType.Orden:
                        number = item.Order?.Number;
                        serviceName = "Order";
                        date = item.Order?.Date;
                        amount = item.Order?.Amount;
                        break;
                    case LogType.Pasaporte:
                        number = item.Passport?.OrderNumber;
                        serviceName = "Pasaporte";
                        date = item.Passport?.FechaSolicitud;
                        amount = item.Passport?.Total;
                        break;
                    case LogType.Recarga:
                        number = item.Rechargue?.Number;
                        serviceName = "Recarga";
                        date = item.Rechargue?.date;
                        amount = item.Rechargue?.Import;
                        break;
                    case LogType.Remesa:
                        number = item.Remittance?.Number;
                        serviceName = "Remesa";
                        date = item.Remittance?.Date;
                        amount = item.Remittance?.Amount;
                        break;
                    case LogType.Servicio:
                        number = item.Servicio?.numero;
                        serviceName = item.Servicio?.tipoServicio?.Nombre;
                        date = item.Servicio?.fecha;
                        amount = item.Servicio?.importeTotal;
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrEmpty(number))
                    continue;

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

                cell1.AddElement(new Phrase(number, normalFont));
                cell2.AddElement(new Phrase(date != null ? ((DateTime)date).ToShortDateString() : "", normalFont));
                cell3.AddElement(new Phrase(serviceName, normalFont));
                cell4.AddElement(new Phrase(item.User?.FullName, normalFont));
                cell5.AddElement(new Phrase((amount is null ? 0 : (decimal)amount).ToString("0.00"), normalFont));

                if (date != null)
                {
                    if (((DateTime)date).Month == init.Month && ((DateTime)date).Year == init.Year)
                    {
                        tableThisMonth.AddCell(cell1);
                        tableThisMonth.AddCell(cell2);
                        tableThisMonth.AddCell(cell3);
                        tableThisMonth.AddCell(cell4);
                        tableThisMonth.AddCell(cell5);
                        amountThisMonth += amount is null ? 0 : (decimal)amount;
                        qtyThisMonth++;
                    }
                    else
                    {
                        tableLastMonth.AddCell(cell1);
                        tableLastMonth.AddCell(cell2);
                        tableLastMonth.AddCell(cell3);
                        tableLastMonth.AddCell(cell4);
                        tableLastMonth.AddCell(cell5);
                        amountLastMonth += amount is null ? 0 : (decimal)amount;
                        qtyLastMonth++;
                    }
                }
            }

            if (qtyLastMonth == 0)
                tableLastMonth = null;
            if (qtyThisMonth == 0)
                tableThisMonth = null;

            return new AuxCancelationModel
            {
                TableThisMonth = tableThisMonth,
                TableLastMonth = tableLastMonth,
                QtyThisMonth = qtyThisMonth,
                QtyLastMonth = qtyLastMonth,
                AmountThisMonth = amountThisMonth,
                AmountLastMonth = amountLastMonth
            };
        }

        private static void AddValueToTable(PdfPTable table, string value, Font font, int border = 0, BaseColor backgroundColor = null)
        {
            PdfPCell cell = new PdfPCell
            {
                Border = border,
            };
            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }
            cell.AddElement(new Phrase(value, font));
            table.AddCell(cell);
        }

        private static void AddValueToTable(PdfPTable table, List<(string value, Font font)> values, int border = 0, BaseColor backgroundColor = null)
        {
            PdfPCell cell = new PdfPCell
            {
                Border = border,
            };
            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }
            foreach (var item in values)
            {
                cell.AddElement(new Phrase(item.value, item.font));
            }
            table.AddCell(cell);
        }


        private class AuxCancelationModel
        {
            public PdfPTable TableThisMonth { get; set; }
            public PdfPTable TableLastMonth { get; set; }
            public int QtyThisMonth { get; set; }
            public int QtyLastMonth { get; set; }
            public decimal AmountThisMonth { get; set; }
            public decimal AmountLastMonth { get; set; }
        }
        #endregion
    }
}
