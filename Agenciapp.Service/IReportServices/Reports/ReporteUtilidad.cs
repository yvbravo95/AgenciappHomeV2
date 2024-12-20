using Agenciapp.Common.Class;
using Agenciapp.Service.IReportServices;
using Agenciapp.Service.IReportServices.Models;
using AgenciappHome.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Common.Contrains;

namespace Agenciapp.Service.IReportServices.Reports
{
    public static partial class Reporte
    {
        public async static Task<object> GetReporteUtilidadRapid(string strdate, User aUser, databaseContext _context, IWebHostEnvironment _env, IReportService _reportService, bool onlyClientsAgency = false)
        {
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId);

            var auxDate = strdate.Split('-');
            CultureInfo culture = new CultureInfo("es-US", true);
            var dateIni = DateTime.Parse(auxDate[0], culture).Date;
            var dateFin = DateTime.Parse(auxDate[1], culture).Date;


            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.SetPageSize(PageSize.A4);
                doc.Open();
                try
                {

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font fonttransferida = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.ITALIC, BaseColor.RED);


                    Agency agency = aAgency.FirstOrDefault();
                    var agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    var tiposPago = _context.TipoPago.ToList();
                    tiposPago.Add(new TipoPago
                    {
                        TipoPagoId = Guid.Empty,
                        Type = "Pendiente"
                    });

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
                    Paragraph parPaq = new Paragraph("Utilidad por servicio del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);
                    /*
                     * 0: cash
                     * 1: zelle
                     * 2: cheque
                     * 3: credito o debito
                     * 4: Transferencia Bancaria
                     * 5: Web
                     * 6: Money Order
                     * */
                    decimal[] ventastipopago = new decimal[7];
                    int[] canttipopago = new int[7];
                    decimal totalcosto = 0;
                    decimal totalprecio = 0;
                    decimal totalunitario = 0;
                    Dictionary<string, List<AuxReportTipoPagoDCuba>> reporteTiposPago = new Dictionary<string, List<AuxReportTipoPagoDCuba>>();

                    #region // REMESAS
                    var remesas = (await _reportService.UtilityByServiceRapid(agency.AgencyId, STipo.Remesa, dateIni, dateFin)).Value;
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
                    decimal totalremesas = 0;

                    if (remesas.Any())
                    {
                        doc.Add(new Phrase("Remesas", headFont));


                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas5.AddElement(new Phrase("Costo", headFont));
                        cellremesas6.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        foreach (UtilityModel remesa in remesas)
                        {

                            string pagos = "";
                            foreach (var item in remesa.Pays)
                            {
                                pagos += item.TipoPago + ", ";
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

                            cellremesas1.AddElement(new Phrase(remesa.OrderNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(remesa.Client.FullName, normalFont));
                            cellremesas2.AddElement(new Phrase(remesa.Client.PhoneNumber, normalFont));
                            cellremesas3.AddElement(new Phrase(remesa.Employee.FullName, normalFont));
                            cellremesas4.AddElement(new Phrase(remesa.SalePrice.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(remesa.Cost.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(remesa.Utility.ToString(), normalFont));
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
                        decimal refTotalcostoR = remesas.Sum(x => x.Cost);
                        decimal refTotalunitarioR = remesas.Sum(x => x.Utility);
                        decimal refTotalprecioR = remesas.Sum(x => x.SalePrice);
                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refTotalprecioR.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalcostoR.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refTotalunitarioR.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));
                        totalremesas = refTotalunitarioR;
                        totalunitario += refTotalunitarioR;
                        totalcosto += refTotalcostoR;
                        totalprecio += refTotalprecioR;
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

                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = remesas.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "REMESAS",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["REMESAS"] = tipoPagoDCuba;
                    }

                    #endregion

                    #region // PAQUETE TURISTICO
                    var paquetesTuristicos = (await _reportService.UtilityByServiceRapid(agency.AgencyId, STipo.PTuristico, dateIni, dateFin)).Value;
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
                    decimal totalPaquetesTuristicos = 0;

                    if (paquetesTuristicos.Any())
                    {
                        doc.Add(new Phrase("Paquete Turístico", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas5.AddElement(new Phrase("Costo", headFont));
                        cellremesas6.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);

                        foreach (UtilityModel paqueteTuristico in paquetesTuristicos)
                        {

                            string pagos = "";
                            foreach (var item in paqueteTuristico.Pays)
                            {
                                pagos += item.TipoPago + ", ";
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

                            cellremesas1.AddElement(new Phrase(paqueteTuristico.OrderNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(paqueteTuristico.Client.FullName, normalFont));
                            cellremesas2.AddElement(new Phrase(paqueteTuristico.Client.PhoneNumber, normalFont));
                            cellremesas3.AddElement(new Phrase(paqueteTuristico.Employee.FullName, normalFont));
                            cellremesas4.AddElement(new Phrase(paqueteTuristico.SalePrice.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(paqueteTuristico.Cost.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(paqueteTuristico.Utility.ToString(), normalFont));
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
                        decimal refTotalcostoR = paquetesTuristicos.Sum(x => x.Cost);
                        decimal refTotalunitarioR = paquetesTuristicos.Sum(x => x.Utility);
                        decimal refTotalprecioR = paquetesTuristicos.Sum(x => x.SalePrice);
                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(refTotalprecioR.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalcostoR.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(refTotalunitarioR.ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));
                        totalPaquetesTuristicos = refTotalunitarioR;
                        totalunitario += refTotalunitarioR;
                        totalcosto += refTotalcostoR;
                        totalprecio += refTotalprecioR;
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

                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = paquetesTuristicos.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "PAQUETE TURISTICO",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["PAQUETE TURISTICO"] = tipoPagoDCuba;
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

                    List<EnvioMaritimo> enviosmaritimos = _context.EnvioMaritimo
                        .Include(x => x.TipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Client)
                        .Include(x => x.Contact).Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.Date.Date >= dateIni && x.Date.Date <= dateFin)
                        .Where(x => x.Status != EnvioMaritimo.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToList();
                    if (enviosmaritimos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Marítimos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas5.AddElement(new Phrase("Costo", headFont));
                        cellremesas6.AddElement(new Phrase("Utilidad", headFont));
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
                            totalcosto += enviomaritimo.costoMayorista;
                            totalunitario += enviomaritimo.Amount - enviomaritimo.costoMayorista;
                            totalprecio += enviomaritimo.Amount;


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
                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == enviomaritimo.Client.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellremesas1.AddElement(new Phrase(enviomaritimo.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviomaritimo.Client.Name + " " + enviomaritimo.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(phoneClient.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(enviomaritimo.User != null ? enviomaritimo.User.Name + " " + enviomaritimo.User.LastName : "-", normalFont));
                            cellremesas4.AddElement(new Phrase(enviomaritimo.Amount.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(enviomaritimo.costoMayorista.ToString(), normalFont));
                            decimal auxutilidad = enviomaritimo.Amount - enviomaritimo.costoMayorista;
                            cellremesas6.AddElement(new Phrase(auxutilidad.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(enviomaritimo.TipoPago.Type, normalFont));

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
                        cellremesas4.AddElement(new Phrase(enviosmaritimos.Sum(x => x.Amount).ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(enviosmaritimos.Sum(x => x.costoMayorista).ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(enviosmaritimos.Sum(x => x.Amount - x.costoMayorista).ToString(), headFont));
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

                    #region // ENVIOS PASAPORTE
                    PdfPCell cellremesas8 = new PdfPCell();
                    PdfPCell cellremesas9 = new PdfPCell();
                    float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    var passports = (await _reportService.UtilityByServiceRapid(agency.AgencyId, STipo.Passport, dateIni, dateFin)).Value;
                    decimal utilidadPassport = 0;
                    decimal costoConsular = 0; //Para districtCuba
                    if (passports.Any())
                    {
                        if (AgencyName.IsDistrictCuba(agency.AgencyId))
                        {
                            if (onlyClientsAgency)
                                passports = passports.Where(x => !x.ByTransferencia).ToList();

                            float[] columnWidthspasaporte2 = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                            doc.Add(new Phrase("Pasaportes", headFont));

                            tblremesasData = new PdfPTable(columnWidthspasaporte2);
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

                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("S. Consular", headFont));
                            cellremesas5.AddElement(new Phrase("P. Venta", headFont));
                            cellremesas6.AddElement(new Phrase("Costo Consular", headFont));
                            cellremesas7.AddElement(new Phrase("Utilidad", headFont));
                            cellremesas8.AddElement(new Phrase("Tipos Pagos", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            foreach (var pasaporte in passports)
                            {

                                string pagos = "";
                                if (!pasaporte.ByTransferencia)
                                    foreach (var item in pasaporte.Pays)
                                    {
                                        pagos += item.TipoPago + ", ";
                                    }

                                var index = passports.IndexOf(pasaporte);
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
                                if (pasaporte.ByTransferencia)
                                {
                                    cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                    cellremesas1.AddElement(new Phrase("T. " + pasaporte.TransferredAgencyName, fonttransferida));
                                }
                                else
                                {
                                    cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                }
                                cellremesas2.AddElement(new Phrase(pasaporte.Client.FullName, normalFont));
                                cellremesas2.AddElement(new Phrase(pasaporte.Client.PhoneNumber, normalFont));
                                cellremesas3.AddElement(new Phrase(pasaporte.Employee.FullName, normalFont));
                                string servConsular = "";
                                switch (pasaporte.ServicioConsular)
                                {
                                    case ServicioConsular.None:
                                        break;
                                    case ServicioConsular.PrimerVez:
                                    case ServicioConsular.PrimerVez2:
                                        servConsular = "1 Vez";
                                        break;
                                    case ServicioConsular.Prorroga1:
                                        servConsular = "Prorro1";
                                        break;
                                    case ServicioConsular.Prorroga2:
                                        servConsular = "Prorro2";
                                        break;
                                    case ServicioConsular.Renovacion:
                                        servConsular = pasaporte.ServicioConsular.GetDescription();
                                        break;
                                    case ServicioConsular.HE11:
                                        servConsular = pasaporte.ServicioConsular.GetDescription();
                                        break;
                                    default:
                                        servConsular = pasaporte.ServicioConsular.GetDescription();
                                        break;
                                }

                                cellremesas4.AddElement(new Phrase(servConsular, normalFont));
                                cellremesas5.AddElement(new Phrase(pasaporte.SalePrice.ToString(), normalFont));
                                cellremesas6.AddElement(new Phrase(pasaporte.Cost.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(pasaporte.Utility.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pasaporte.ByTransferencia ? "Pendiente" : pagos, normalFont));

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

                            decimal reftotalprecios = passports.Sum(x => x.SalePrice);
                            decimal reftotalcosto = passports.Sum(x => x.Cost);
                            decimal reftotalutilidad = passports.Sum(x => x.Utility);
                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase("", headFont));
                            cellremesas5.AddElement(new Phrase(reftotalprecios.ToString("0.00"), headFont));
                            cellremesas6.AddElement(new Phrase(reftotalcosto.ToString("0.00"), headFont));
                            cellremesas7.AddElement(new Phrase(reftotalutilidad.ToString("0.00"), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));
                            utilidadPassport += reftotalutilidad;
                            totalunitario += reftotalutilidad;
                            totalcosto += reftotalcosto;
                            costoConsular += reftotalcosto;
                            totalprecio += reftotalprecios;
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
                            List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                            var passportDCuba = passports.OrderBy(x => x.OrderNumber).GroupBy(x => x.ServicioConsular);
                            foreach (var PassportServConsular in passportDCuba)
                            {
                                foreach (var item in tiposPago)
                                {
                                    var items = PassportServConsular.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                                    if (items.Any())
                                    {
                                        tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                        {
                                            TipoPago = item,
                                            ServiceType = PassportServConsular.Key.GetDescription().ToUpper(),
                                            Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                            Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                            Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                            Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                        });
                                    }
                                }
                            }
                            reporteTiposPago["PASAPORTES"] = tipoPagoDCuba;
                        }
                        else
                        {
                            doc.Add(new Phrase("Pasaportes", headFont));
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
                            cellremesas1.AddElement(new Phrase("No.", headFont));
                            cellremesas2.AddElement(new Phrase("Cliente", headFont));
                            cellremesas3.AddElement(new Phrase("Empleado", headFont));
                            cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                            if (agency.AgencyId == agencyDCubaId)
                            {
                                cellremesas5.AddElement(new Phrase("Costo Consular", headFont));
                                cellremesas6.AddElement(new Phrase("S. Consular", headFont));
                            }
                            else
                            {
                                cellremesas5.AddElement(new Phrase("Costo", headFont));
                                cellremesas6.AddElement(new Phrase("C. Servicio", headFont));
                            }
                            cellremesas7.AddElement(new Phrase("Utilidad", headFont));
                            cellremesas8.AddElement(new Phrase("Tipos Pagos", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);
                            tblremesasData.AddCell(cellremesas8);

                            foreach (var pasaporte in passports)
                            {

                                string pagos = "";
                                if (!pasaporte.ByTransferencia)
                                    foreach (var item in pasaporte.Pays)
                                    {
                                        pagos += item.TipoPago + ", ";
                                    }

                                var index = passports.IndexOf(pasaporte);
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
                                if (pasaporte.ByTransferencia)
                                {
                                    cellremesas1.AddElement(new Phrase("T. " + pasaporte.TransferredAgencyName, fonttransferida));
                                }
                                else
                                {
                                    cellremesas1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                }
                                cellremesas2.AddElement(new Phrase(pasaporte.Client.FullName, normalFont));
                                cellremesas2.AddElement(new Phrase(pasaporte.Client.PhoneNumber, normalFont));
                                cellremesas3.AddElement(new Phrase(pasaporte.Employee.FullName, normalFont));
                                cellremesas4.AddElement(new Phrase(pasaporte.SalePrice.ToString(), normalFont));
                                if (agency.AgencyId == agencyDCubaId)
                                {
                                    cellremesas5.AddElement(new Phrase(pasaporte.Cost.ToString(), normalFont));
                                    string servConsular = "";
                                    switch (pasaporte.ServicioConsular)
                                    {
                                        case ServicioConsular.None:
                                            break;
                                        case ServicioConsular.PrimerVez:
                                        case ServicioConsular.PrimerVez2:
                                            servConsular = "1 Vez";
                                            break;
                                        case ServicioConsular.Prorroga1:
                                            servConsular = "Prorro1";
                                            break;
                                        case ServicioConsular.Prorroga2:
                                            servConsular = "Prorro2";
                                            break;
                                        case ServicioConsular.Renovacion:
                                            servConsular = pasaporte.ServicioConsular.GetDescription();
                                            break;
                                        case ServicioConsular.HE11:
                                            servConsular = pasaporte.ServicioConsular.GetDescription();
                                            break;
                                        default:
                                            servConsular = pasaporte.ServicioConsular.GetDescription();
                                            break;
                                    }
                                    cellremesas6.AddElement(new Phrase(servConsular, normalFont));
                                }
                                else
                                {
                                    cellremesas5.AddElement(new Phrase(pasaporte.Cost.ToString(), normalFont));
                                    cellremesas6.AddElement(new Phrase(pasaporte.CServicio.ToString(), normalFont));
                                }

                                cellremesas7.AddElement(new Phrase(pasaporte.Utility.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pasaporte.ByTransferencia ? "Pendiente" : pagos, normalFont));

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

                            decimal reftotalprecios = passports.Sum(x => x.SalePrice);
                            decimal reftotalcosto = passports.Sum(x => x.Cost);
                            decimal reftotalutilidad = passports.Sum(x => x.Utility);
                            cellremesas1.AddElement(new Phrase("Totales", headFont));
                            cellremesas2.AddElement(new Phrase("", normalFont));
                            cellremesas3.AddElement(new Phrase("", normalFont));
                            cellremesas4.AddElement(new Phrase(reftotalprecios.ToString("0.00"), headFont));
                            cellremesas5.AddElement(new Phrase(reftotalcosto.ToString("0.00"), headFont));
                            cellremesas6.AddElement(new Phrase(passports.Sum(x => x.CServicio).ToString("0.00"), headFont));
                            cellremesas7.AddElement(new Phrase(reftotalutilidad.ToString("0.00"), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));
                            utilidadPassport = reftotalutilidad;
                            totalunitario += reftotalutilidad;
                            costoConsular += reftotalcosto;
                            totalcosto += reftotalcosto;
                            totalprecio += reftotalprecios;
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

                    cellremesas8 = new PdfPCell();
                    #region // ENVIOS CARIBE

                    float[] columnWidthscaribe = { 2, (float)1.7, (float)1.7, 1, 1, 1, 1, 1 };
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

                    List<EnvioCaribe> envioscaribe = _context.EnvioCaribes
                        .Include(x => x.AgencyTransferida)
                        .Include(x => x.User)
                        .Include(x => x.Contact).ThenInclude(x => x.Address).Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago).Include(x => x.Client).Include(x => x.Contact).Where(x => x.User.Username != "Manuel14" && (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.AgencyTransferidaId == agency.AgencyId) && x.Date.Date >= dateIni && x.Date.Date <= dateFin)
                        .Where(x => x.Status != EnvioCaribe.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToList();
                    decimal totalenviocaribe = 0;
                    if (envioscaribe.Count != 0)
                    {
                        //Auxiliar para obtener el costo que aplica el mayorista Caribe a Rapid
                        var rapid = _context.Agency.FirstOrDefault(x => x.Name == "Rapid Multiservice");
                        var mayorista = _context.Wholesalers.Include(x => x.tipoServicioHabana).Include(x => x.tipoServicioRestoProv).FirstOrDefault(x => x.EsVisible && x.AgencyId == rapid.AgencyId && x.Category.category == "Maritimo-Aereo");

                        doc.Add(new Phrase("Envíos Caribe", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas5.AddElement(new Phrase("Costo", headFont));
                        cellremesas6.AddElement(new Phrase("C. Servicio", headFont));
                        cellremesas7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        decimal refTotalcostoEC = 0;
                        decimal refTotalunitarioEC = 0;
                        decimal refTotalprecioEC = 0;
                        foreach (EnvioCaribe enviocaribe in envioscaribe)
                        {
                            decimal refcostoEC = enviocaribe.costo + enviocaribe.OtrosCostos;
                            decimal refunitarioEC = enviocaribe.Amount - (enviocaribe.costo + enviocaribe.OtrosCostos);
                            decimal refprecioEC = enviocaribe.Amount;

                            bool isbytransferencia = false;
                            if (enviocaribe.AgencyTransferida != null)
                            {
                                if (enviocaribe.AgencyTransferida.AgencyId == agency.AgencyId)
                                {
                                    isbytransferencia = true;
                                    refcostoEC = 0;
                                    refunitarioEC = 0;
                                    refprecioEC = 0;

                                    if (mayorista != null)
                                    {
                                        refprecioEC = enviocaribe.costo;
                                        List<TipoServicioMayorista> serv = new List<TipoServicioMayorista>();
                                        if (enviocaribe.Contact.Address.City == "La Habana")
                                        {
                                            serv = mayorista.tipoServicioHabana;
                                        }
                                        else
                                        {
                                            serv = mayorista.tipoServicioRestoProv;
                                        }

                                        if (serv != null)
                                        {
                                            if (enviocaribe.servicio == "Correo-Aereo")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoAereo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                            else if (enviocaribe.servicio == "Correo-Maritimo")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoMaritimo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                            else if (enviocaribe.servicio == "Aerovaradero- Recogida")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Aereo_Varadero);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoAereo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                            else if (enviocaribe.servicio == "Maritimo-Palco Almacen")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoMaritimo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                            else if (enviocaribe.servicio == "Palco ENTREGA A DOMICILIO")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoAereo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                        }
                                    }
                                }
                            }

                            refTotalcostoEC += refcostoEC;
                            refTotalprecioEC += refprecioEC;
                            refTotalunitarioEC += refunitarioEC;

                            totalcosto += refcostoEC;
                            totalprecio += refprecioEC;

                            string tipospagoec = "";

                            if (!isbytransferencia)
                            {
                                foreach (var item in enviocaribe.RegistroPagos)
                                {
                                    tipospagoec += item.tipoPago.Type + ", ";
                                }
                            }

                            if (tipospagoec == "")
                            {
                                tipospagoec = "-";
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


                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == enviocaribe.Client.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellremesas1.AddElement(new Phrase(enviocaribe.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocaribe.Client.Name + " " + enviocaribe.Client.LastName, normalFont));
                            cellremesas3.AddElement(new Phrase(enviocaribe.User != null ? enviocaribe.User.Name + " " + enviocaribe.User.LastName : "-", normalFont));
                            cellremesas4.AddElement(new Phrase(refprecioEC.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase((refcostoEC - enviocaribe.OtrosCostos).ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(enviocaribe.OtrosCostos.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(refunitarioEC.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tipospagoec, normalFont));

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
                        cellremesas4.AddElement(new Phrase((refTotalprecioEC).ToString(), headFont));
                        cellremesas5.AddElement(new Phrase((refTotalcostoEC - envioscaribe.Sum(x => x.OtrosCostos)).ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(envioscaribe.Sum(x => x.OtrosCostos).ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refTotalunitarioEC.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalenviocaribe = refTotalunitarioEC;
                        totalunitario += refTotalunitarioEC;
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

                    #region // ENVIOS CUBIQ

                    float[] columnWidthscubiq = { 2, (float)1.7, (float)1.7, 1, 1, 1, 1, 1 };
                    tblremesasData = new PdfPTable(columnWidthscubiq);
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

                    var envioscubiq = (await _reportService.UtilityByServiceRapid(agency.AgencyId, STipo.Cubiq, dateIni, dateFin)).Value;

                    decimal totalenviocubiq = 0;
                    if (envioscubiq.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos CARGA AM", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas5.AddElement(new Phrase("Costo", headFont));
                        cellremesas6.AddElement(new Phrase("C. Servicio", headFont));
                        cellremesas7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        decimal refTotalcostoCubiq = 0;
                        decimal refTotalunitarioCubiq = 0;
                        decimal refTotalprecioCubiq = 0;
                        foreach (var envioCubiq in envioscubiq)
                        {
                            decimal refcostoCubiq = envioCubiq.Cost;
                            decimal refunitarioCubiq = envioCubiq.Utility;
                            decimal refprecioCubiq = envioCubiq.SalePrice;

                            refTotalcostoCubiq += refcostoCubiq;
                            refTotalprecioCubiq += refprecioCubiq;
                            refTotalunitarioCubiq += refunitarioCubiq;

                            totalcosto += refcostoCubiq;
                            totalprecio += refprecioCubiq;

                            string tipospagoCubiq = "";

                            if (!envioCubiq.ByTransferencia)
                            {
                                foreach (var item in envioCubiq.Pays)
                                {
                                    tipospagoCubiq += item.TipoPago + ", ";
                                }
                            }

                            if (tipospagoCubiq == "")
                            {
                                tipospagoCubiq = "-";
                            }


                            var index = envioscubiq.IndexOf(envioCubiq);
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


                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == envioCubiq.Client.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellremesas1.AddElement(new Phrase(envioCubiq.OrderNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(envioCubiq.Client.FullName, normalFont));
                            cellremesas3.AddElement(new Phrase(envioCubiq.Employee != null ? envioCubiq.Employee.FullName : "-", normalFont));
                            cellremesas4.AddElement(new Phrase(refprecioCubiq.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(refcostoCubiq.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(envioCubiq.CServicio.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(refunitarioCubiq.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tipospagoCubiq, normalFont));

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
                        cellremesas4.AddElement(new Phrase(refTotalprecioCubiq.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(refTotalcostoCubiq.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(envioscubiq.Sum(x => x.CServicio).ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(refTotalunitarioCubiq.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalenviocubiq = refTotalunitarioCubiq;
                        totalunitario += refTotalunitarioCubiq;
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
                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = envioscubiq.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "CARGA AM",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["CUBIQ"] = tipoPagoDCuba;
                    }
                    #endregion

                    #region // ENVIOS AEREOS

                    float[] columnWidthsaereo = { (float)2, (float)1.7, (float)1.7, 1, 1, 1, 1, 1 };
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


                    decimal auxtotalutilidad = 0;
                    List<Order> enviosaereos = _context.Order
                        .Include(x => x.Pagos).ThenInclude(x => x.tipoPago)
                        .Include(x => x.User)
                        .Include(x => x.Agency).Include(x => x.agencyTransferida)
                        .Include(x => x.TipoPago).Include(x => x.Client).Include(x => x.Contact).Where(x => (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId) && x.Date.Date >= dateIni && x.Date.Date <= dateFin && x.Type != "Remesas" && x.Type != "Combo")
                        .Where(x => x.Status != Order.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToList();
                    if (enviosaereos.Count != 0)
                    {
                        doc.Add(new Phrase("ENVÍOS", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas5.AddElement(new Phrase("Costo", headFont));
                        cellremesas6.AddElement(new Phrase("C. Service", headFont));
                        cellremesas7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);

                        decimal reftotalcosto = 0;
                        decimal reftotalutilidad = 0;
                        decimal reftotalprecio = 0;
                        foreach (Order envioaereo in enviosaereos)
                        {
                            decimal refcosto = envioaereo.costoMayorista + envioaereo.costoProductosBodega;
                            decimal refutilidad = envioaereo.Amount - envioaereo.costoMayorista - envioaereo.costoProductosBodega;
                            decimal refprecio = envioaereo.Amount;
                            if (envioaereo.agencyTransferida != null)
                            {
                                if (envioaereo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    refcosto = envioaereo.costoDeProveedor + envioaereo.OtrosCostos;
                                    refprecio = envioaereo.costoMayorista;
                                    refutilidad = refprecio - refcosto;
                                }
                            }

                            bool usedcredito = false;
                            string tiposdepago = "";
                            foreach (var item in envioaereo.Pagos)
                            {
                                tiposdepago += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type == "Crédito de Consumo")
                                {
                                    usedcredito = true;
                                }
                            }
                            if (tiposdepago == "")
                            {
                                tiposdepago = "-";
                            }
                            if (usedcredito)
                            {
                                refutilidad = 0;
                            }
                            reftotalcosto += refcosto;
                            reftotalprecio += refprecio;
                            reftotalutilidad += refutilidad;
                            totalcosto += refcosto;
                            totalprecio += refprecio;

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


                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == envioaereo.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellremesas1.AddElement(new Phrase(envioaereo.Number, normalFont));
                            if (envioaereo.agencyTransferida != null)
                            {
                                if (envioaereo.agencyTransferida.AgencyId == aAgency.FirstOrDefault().AgencyId)
                                {
                                    cellremesas1.AddElement(new Phrase("T. " + envioaereo.Agency.Name, fonttransferida));
                                }
                            }
                            cellremesas2.AddElement(new Phrase(envioaereo.Client.Name + " " + envioaereo.Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(phoneClient.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(envioaereo.User != null ? envioaereo.User.Name + " " + envioaereo.User.LastName : "-", normalFont));
                            cellremesas4.AddElement(new Phrase(refprecio.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(refcosto.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase(envioaereo.OtrosCostos.ToString(), normalFont)); //Fee Agencia
                            cellremesas7.AddElement(new Phrase(refutilidad.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tiposdepago, normalFont));

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
                        cellremesas4.AddElement(new Phrase(reftotalprecio.ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(reftotalcosto.ToString(), headFont));
                        cellremesas6.AddElement(new Phrase((enviosaereos.Sum(x => x.OtrosCostos)).ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(reftotalutilidad.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalunitario += reftotalutilidad;
                        auxtotalutilidad = reftotalutilidad;

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

                    #region // ENVIOS COMBO
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


                    decimal auxtotalutilidadCombo = 0;
                    var envioscombos = (await _reportService.UtilityByServiceRapid(agency.AgencyId, STipo.Combo, dateIni, dateFin)).Value;
                    if (envioscombos.Count != 0)
                    {
                        doc.Add(new Phrase("COMBOS", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas5.AddElement(new Phrase("Costo", headFont));
                        cellremesas6.AddElement(new Phrase("C. Service", headFont));
                        cellremesas7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                        tblremesasData.AddCell(cellremesas6);
                        tblremesasData.AddCell(cellremesas7);
                        tblremesasData.AddCell(cellremesas8);


                        foreach (UtilityModel enviocombo in envioscombos)
                        {

                            bool usedcredito = false;
                            string tiposdepago = "";
                            foreach (var item in enviocombo.Pays)
                            {
                                tiposdepago += item.TipoPago + ", ";
                                if (item.TipoPago == "Crédito de Consumo")
                                {
                                    usedcredito = true;
                                }
                            }
                            if (tiposdepago == "")
                            {
                                tiposdepago = "-";
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
                            cellremesas1.AddElement(new Phrase(enviocombo.OrderNumber, normalFont));
                            if (enviocombo.TransferredAgencyName != "")
                            {
                                cellremesas1.AddElement(new Phrase("T. " + enviocombo.TransferredAgencyName, fonttransferida));
                            }
                            cellremesas2.AddElement(new Phrase(enviocombo.Client.FullName, normalFont));
                            cellremesas2.AddElement(new Phrase(enviocombo.Client.PhoneNumber, normalFont));
                            cellremesas3.AddElement(new Phrase(enviocombo.Employee.FullName, normalFont));
                            cellremesas4.AddElement(new Phrase(enviocombo.SalePrice.ToString("0.00"), normalFont));
                            cellremesas5.AddElement(new Phrase(enviocombo.Cost.ToString("0.00"), normalFont));
                            cellremesas6.AddElement(new Phrase(enviocombo.CServicio.ToString("0.00"), normalFont)); //Fee Agencia
                            cellremesas7.AddElement(new Phrase(enviocombo.Utility.ToString("0.00"), normalFont));
                            cellremesas8.AddElement(new Phrase(tiposdepago, normalFont));

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
                        decimal reftotalcosto = envioscombos.Sum(x => x.Cost);
                        decimal reftotalutilidad = envioscombos.Sum(x => x.Utility);
                        decimal reftotalprecio = envioscombos.Sum(x => x.SalePrice);
                        decimal refTotalOtrosCosto = envioscombos.Sum(x => x.CServicio);
                        cellremesas1.AddElement(new Phrase("Totales", headFont));
                        cellremesas2.AddElement(new Phrase("", normalFont));
                        cellremesas3.AddElement(new Phrase("", normalFont));
                        cellremesas4.AddElement(new Phrase(reftotalprecio.ToString("0.00"), headFont));
                        cellremesas5.AddElement(new Phrase(reftotalcosto.ToString("0.00"), headFont));
                        cellremesas6.AddElement(new Phrase(refTotalOtrosCosto.ToString("0.00"), headFont));
                        cellremesas7.AddElement(new Phrase(reftotalutilidad.ToString("0.00"), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalunitario += reftotalutilidad;
                        totalcosto += reftotalcosto;
                        totalprecio += reftotalprecio;
                        auxtotalutilidadCombo = reftotalutilidad;

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
                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = envioscombos.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "COMBOS",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["COMBOS"] = tipoPagoDCuba;
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
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    List<Ticket> boletos = _context.Ticket.Include(x => x.User).Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.PaqueteTuristicoId == null && !x.ClientIsCarrier && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.RegisterDate.Date >= dateIni && x.RegisterDate.Date <= dateFin)
                        .Where(x => x.State != Remittance.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToList();
                    if (boletos.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Tipo", headFont));
                        cellremesas5.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas6.AddElement(new Phrase("Costo", headFont));
                        cellremesas7.AddElement(new Phrase("Utilidad", headFont));
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
                            totalcosto += boleto.Cost + boleto.Charges;
                            totalprecio += boleto.Total;

                            string pagos = "";

                            foreach (var item in boleto.RegistroPagos)
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type == "Cash")
                                {
                                    ventastipopago[0] += item.valorPagado;
                                    canttipopago[0] += 1;
                                }
                                else if (item.tipoPago.Type == "Zelle")
                                {
                                    ventastipopago[1] += item.valorPagado;
                                    canttipopago[1] += 1;
                                }
                                else if (item.tipoPago.Type == "Cheque")
                                {
                                    ventastipopago[2] += item.valorPagado;
                                    canttipopago[2] += 1;
                                }
                                else if (item.tipoPago.Type == "Crédito o Débito")
                                {
                                    ventastipopago[3] += item.valorPagado;
                                    canttipopago[3] += 1;
                                }
                                else if (item.tipoPago.Type == "Transferencia Bancaria")
                                {
                                    ventastipopago[4] += item.valorPagado;
                                    canttipopago[4] += 1;
                                }
                                else if (item.tipoPago.Type == "Web")
                                {
                                    ventastipopago[5] += item.valorPagado;
                                    canttipopago[5] += 1;
                                }
                                else if (item.tipoPago.Type == "Money Order")
                                {
                                    ventastipopago[6] += item.valorPagado;
                                    canttipopago[6] += 1;
                                }
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


                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == boleto.ClientId && x.Type == "Móvil").FirstOrDefault();
                            var Client = _context.Client.Find(boleto.ClientId);
                            cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(Client.Name + " " + Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(phoneClient.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(boleto.User != null ? boleto.User.Name + " " + boleto.User.LastName : "-", normalFont));
                            cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                            cellremesas5.AddElement(new Phrase(boleto.Total.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase((boleto.Cost + boleto.Charges).ToString(), normalFont));
                            decimal auxutilidad = boleto.Total - boleto.Cost - boleto.Charges;
                            cellremesas7.AddElement(new Phrase(auxutilidad.ToString(), normalFont));
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
                        cellremesas5.AddElement(new Phrase(boletos.Sum(x => x.Total).ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(boletos.Sum(x => x.Cost + x.Charges).ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(boletos.Sum(x => x.Total - x.Cost - x.Charges).ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalunitario += boletos.Sum(x => x.Total - x.Cost - x.Charges);

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

                    List<Ticket> boletosCarrier = _context.Ticket.Include(x => x.User).Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                        .Where(x => x.PaqueteTuristicoId == null && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.RegisterDate.Date >= dateIni && x.RegisterDate.Date <= dateFin && x.ClientIsCarrier)
                        .Where(x => x.State != PaqueteTuristico.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToList();
                    if (boletosCarrier.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos Carrier", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("Tipo", headFont));
                        cellremesas5.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas6.AddElement(new Phrase("Costo", headFont));
                        cellremesas7.AddElement(new Phrase("Utilidad", headFont));
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
                            //totalcosto += boleto.Cost + boleto.Charges;
                            //totalprecio += boleto.Total;

                            string pagos = "";

                            foreach (var item in boleto.RegistroPagos)
                            {
                                pagos += item.tipoPago.Type + ", ";
                                if (item.tipoPago.Type == "Cash")
                                {
                                    ventastipopago[0] += item.valorPagado;
                                    canttipopago[0] += 1;
                                }
                                else if (item.tipoPago.Type == "Zelle")
                                {
                                    ventastipopago[1] += item.valorPagado;
                                    canttipopago[1] += 1;
                                }
                                else if (item.tipoPago.Type == "Cheque")
                                {
                                    ventastipopago[2] += item.valorPagado;
                                    canttipopago[2] += 1;
                                }
                                else if (item.tipoPago.Type == "Crédito o Débito")
                                {
                                    ventastipopago[3] += item.valorPagado;
                                    canttipopago[3] += 1;
                                }
                                else if (item.tipoPago.Type == "Transferencia Bancaria")
                                {
                                    ventastipopago[4] += item.valorPagado;
                                    canttipopago[4] += 1;
                                }
                                else if (item.tipoPago.Type == "Web")
                                {
                                    ventastipopago[5] += item.valorPagado;
                                    canttipopago[5] += 1;
                                }
                                else if (item.tipoPago.Type == "Money Order")
                                {
                                    ventastipopago[6] += item.valorPagado;
                                    canttipopago[6] += 1;
                                }
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


                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == boleto.ClientId && x.Type == "Móvil").FirstOrDefault();
                            var Client = _context.Client.Find(boleto.ClientId);
                            cellremesas1.AddElement(new Phrase(boleto.ReservationNumber, normalFont));
                            cellremesas2.AddElement(new Phrase(Client.Name + " " + Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(phoneClient.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(boleto.User != null ? boleto.User.Name + " " + boleto.User.LastName : "-", normalFont));
                            cellremesas4.AddElement(new Phrase(boleto.type, normalFont));
                            cellremesas5.AddElement(new Phrase(boleto.Total.ToString(), normalFont));
                            cellremesas6.AddElement(new Phrase((boleto.Cost + boleto.Charges).ToString(), normalFont));
                            decimal auxutilidad = boleto.Total - boleto.Cost - boleto.Charges;
                            cellremesas7.AddElement(new Phrase(auxutilidad.ToString(), normalFont));
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
                        cellremesas5.AddElement(new Phrase(boletosCarrier.Sum(x => x.Total).ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(boletosCarrier.Sum(x => x.Cost + x.Charges).ToString(), headFont));
                        cellremesas7.AddElement(new Phrase(boletosCarrier.Sum(x => x.Total - x.Cost - x.Charges).ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        //totalunitario += boletosCarrier.Sum(x => x.Total - x.Cost - x.Charges);
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

                    List<Rechargue> recargas = _context.Rechargue.Include(x => x.User).Include(x => x.tipoPago).Include(x => x.Client).Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.date.Date >= dateIni && x.date.Date <= dateFin)
                        .Where(x => x.estado != Rechargue.STATUS_CANCELADA || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                        .ToList();
                    if (recargas.Count != 0)
                    {
                        doc.Add(new Phrase("RECARGAS", headFont));

                        cellremesas1.AddElement(new Phrase("No.", headFont));
                        cellremesas2.AddElement(new Phrase("Cliente", headFont));
                        cellremesas3.AddElement(new Phrase("Empleado", headFont));
                        cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                        cellremesas5.AddElement(new Phrase("Costo", headFont));
                        cellremesas6.AddElement(new Phrase("Utilidad", headFont));
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
                            totalcosto += recarga.costoMayorista;
                            totalprecio += recarga.Import;

                            if (recarga.tipoPago.Type == "Cash")
                            {
                                ventastipopago[0] += recarga.Import - recarga.costoMayorista;
                                canttipopago[0] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Zelle")
                            {
                                ventastipopago[1] += recarga.Import - recarga.costoMayorista;
                                canttipopago[1] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Cheque")
                            {
                                ventastipopago[2] += recarga.Import - recarga.costoMayorista;
                                canttipopago[2] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Crédito o Débito")
                            {
                                ventastipopago[3] += recarga.Import - recarga.costoMayorista;
                                canttipopago[3] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Transferencia Bancaria")
                            {
                                ventastipopago[4] += recarga.Import - recarga.costoMayorista;
                                canttipopago[4] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Web")
                            {
                                ventastipopago[5] += recarga.Import - recarga.costoMayorista;
                                canttipopago[5] += 1;
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


                            var Client = _context.Client.Find(recarga.ClientId);
                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == recarga.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellremesas1.AddElement(new Phrase(recarga.Number, normalFont));
                            cellremesas2.AddElement(new Phrase(Client.Name + " " + Client.LastName, normalFont));
                            cellremesas2.AddElement(new Phrase(phoneClient?.Number, normalFont));
                            cellremesas3.AddElement(new Phrase(recarga.User != null ? recarga.User.Name + " " + recarga.User.LastName : "-", normalFont));
                            cellremesas4.AddElement(new Phrase(recarga.Import.ToString(), normalFont));
                            cellremesas5.AddElement(new Phrase(recarga.costoMayorista.ToString(), normalFont));
                            decimal auxutilidad = recarga.Import - recarga.costoMayorista;
                            cellremesas6.AddElement(new Phrase(auxutilidad.ToString(), normalFont));
                            cellremesas7.AddElement(new Phrase(recarga.tipoPago.Type, normalFont));

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
                        cellremesas4.AddElement(new Phrase(recargas.Sum(x => x.Import).ToString(), headFont));
                        cellremesas5.AddElement(new Phrase(recargas.Sum(x => x.costoMayorista).ToString(), headFont));
                        cellremesas6.AddElement(new Phrase(recargas.Sum(x => x.Import - x.costoMayorista).ToString(), headFont));
                        cellremesas7.AddElement(new Phrase("", normalFont));
                        totalunitario += recargas.Sum(x => x.Import - x.costoMayorista);
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


                    List<Servicio> servicios = _context.Servicios
                    .Include(x => x.User)
                    .Include(x => x.tipoServicio)
                    .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                    .Include(x => x.cliente)
                    .Where(x => x.PaqueteTuristicoId == null && x.agency.AgencyId == aAgency.FirstOrDefault().AgencyId && x.fecha.ToLocalTime().Date >= dateIni && x.fecha.ToLocalTime().Date <= dateFin)
                    .Where(x => x.estado != Servicio.EstadoCancelado || ((DateTime)x.CanceledDate).Date > dateFin.Date)
                    .ToList();
                    //Hago una tabla para cada servicio
                    var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == agency.AgencyId);
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
                            cellremesas4.AddElement(new Phrase("P. Venta", headFont));
                            cellremesas5.AddElement(new Phrase("Costo", headFont));
                            cellremesas6.AddElement(new Phrase("Utilidad", headFont));
                            cellremesas7.AddElement(new Phrase("Tipo Pago", headFont));

                            tblremesasData.AddCell(cellremesas1);
                            tblremesasData.AddCell(cellremesas2);
                            tblremesasData.AddCell(cellremesas3);
                            tblremesasData.AddCell(cellremesas4);
                            tblremesasData.AddCell(cellremesas5);
                            tblremesasData.AddCell(cellremesas6);
                            tblremesasData.AddCell(cellremesas7);


                            foreach (Servicio servicio in auxservicios)
                            {
                                string tiposPagoOS = "";
                                foreach (var item in servicio.RegistroPagos)
                                {
                                    tiposPagoOS += $"{item.tipoPago.Type}, ";

                                    if (item.tipoPago.Type == "Cash")
                                    {
                                        ventastipopago[0] += item.valorPagado;
                                        canttipopago[0] += 1;
                                    }
                                    else if (item.tipoPago.Type == "Zelle")
                                    {
                                        ventastipopago[1] += item.valorPagado;
                                        canttipopago[1] += 1;
                                    }
                                    else if (item.tipoPago.Type == "Cheque")
                                    {
                                        ventastipopago[2] += item.valorPagado;
                                        canttipopago[2] += 1;
                                    }
                                    else if (item.tipoPago.Type == "Crédito o Débito")
                                    {
                                        ventastipopago[3] += item.valorPagado;
                                        canttipopago[3] += 1;
                                    }
                                    else if (item.tipoPago.Type == "Transferencia Bancaria")
                                    {
                                        ventastipopago[4] += item.valorPagado;
                                        canttipopago[4] += 1;
                                    }
                                    else if (item.tipoPago.Type == "Web")
                                    {
                                        ventastipopago[5] += item.valorPagado;
                                        canttipopago[5] += 1;
                                    }
                                    else if (item.tipoPago.Type == "Money Order")
                                    {
                                        ventastipopago[6] += item.valorPagado;
                                        canttipopago[6] += 1;
                                    }
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

                                var Client = servicio.cliente;
                                var phoneClient = _context.Phone.Where(x => x.ReferenceId == Client.ClientId && x.Type == "Móvil").FirstOrDefault();

                                cellremesas1.AddElement(new Phrase(servicio.numero, normalFont));
                                cellremesas2.AddElement(new Phrase(Client.Name + " " + Client.LastName, normalFont));
                                cellremesas2.AddElement(new Phrase(phoneClient?.Number, normalFont));
                                cellremesas3.AddElement(new Phrase(servicio.User != null ? servicio.User.Name + " " + servicio.User.LastName : "-", normalFont));
                                cellremesas4.AddElement(new Phrase(servicio.importeTotal.ToString(), normalFont));
                                cellremesas5.AddElement(new Phrase((servicio.costoMayorista + servicio.CostoXServicio).ToString(), normalFont));
                                decimal auxutilidad = servicio.importeTotal - servicio.costoMayorista - servicio.CostoXServicio;
                                cellremesas6.AddElement(new Phrase(auxutilidad.ToString(), normalFont));
                                cellremesas7.AddElement(new Phrase(tiposPagoOS, normalFont));

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
                            decimal auxTotal = auxservicios.Sum(x => x.importeTotal);
                            decimal auxCosto = auxservicios.Sum(x => x.costoMayorista + x.CostoXServicio);
                            decimal auxUtilidad = auxTotal - auxCosto;
                            cellremesas4.AddElement(new Phrase(auxTotal.ToString(), headFont));
                            cellremesas5.AddElement(new Phrase(auxCosto.ToString(), headFont));
                            cellremesas6.AddElement(new Phrase(auxUtilidad.ToString(), headFont));
                            cellremesas7.AddElement(new Phrase("", normalFont));
                            totalunitario += auxUtilidad;
                            totalcosto += auxCosto;
                            totalprecio += auxTotal;
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
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + totalremesas.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = paquetesTuristicos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PAQUETES TURISTICOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + totalPaquetesTuristicos.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosmaritimos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + enviosmaritimos.Sum(x => x.Amount - x.costoMayorista).ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    if (agency.Name == "Rapid Multiservice")
                    {
                        auxcant = envioscaribe.Count();
                        auxcanttoal += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + totalenviocaribe + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                    }

                    auxcant = envioscubiq.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + totalenviocubiq + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosaereos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);

                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxtotalutilidad.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscombos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);

                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxtotalutilidadCombo.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = passports.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + utilidadPassport + " usd", normalFont));
                        cellleft.AddElement(aux);
                        if (AgencyName.IsDistrictCuba(agency.AgencyId))
                        {
                            aux = new Phrase("COSTO CONSULAR: ", headFontRed);
                            aux.AddSpecial(new Phrase($"${costoConsular} usd", normalFontRed));
                            cellleft.AddElement(aux);
                        }

                    }

                    auxcant = boletos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + boletos.Sum(x => x.Total - x.Cost - x.Charges).ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    /*auxcant = boletosCarrier.Where(x => x.State != "Pendiente").Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + boletosCarrier.Where(x => x.State != "Pendiente").Sum(x => x.Total - x.Cost - x.Charges).ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }*/

                    auxcant = recargas.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + recargas.Sum(x => x.Import - x.costoMayorista).ToString() + " usd", normalFont));
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
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxservicio.Sum(x => x.importeTotal - x.costoMayorista - x.CostoXServicio).ToString() + " usd", normalFont));
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
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph total = new Paragraph("Total P. Venta: ", headFont2);
                    total.AddSpecial(new Phrase("$ " + totalprecio.ToString(), normalFont2));
                    total.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(total);
                    Paragraph porpagar = new Paragraph("Total Costo: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalcosto.ToString(), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);
                    Paragraph deuda = new Paragraph("Total Utilidad: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + totalunitario.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);

                    UtilityByOrder reportByOrder = new UtilityByOrder(_context);
                    var utilityCanceled = await reportByOrder.GetCanceledDay(agency.AgencyId, dateIni, dateFin);

                    Paragraph cancelaciones = new Paragraph("Cancelaciones: ", headFont2);
                    cancelaciones.AddSpecial(new Phrase(" $ " + utilityCanceled.Sum(x => x.Utility).ToString("0.00"), normalFont2));
                    cancelaciones.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(cancelaciones);
                    cellright.AddElement(Chunk.NEWLINE);

                    #endregion

                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    if (AgencyName.IsDistrictCuba(agency.AgencyId))
                    {
                        foreach (var service in reporteTiposPago)
                        {
                            doc.Add(new Phrase($"REPORTE TIPO PAGO - {service.Key.ToUpper()}", underLineFont));
                            float[] columnWidth = { 3, 2, 2, 2, 2, 2 };
                            PdfPTable tbl = new PdfPTable(columnWidth);
                            tbl.WidthPercentage = 100;

                            PdfPCell cell = new PdfPCell(new Phrase("Tipo de Pago", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Tramite", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Cantidad", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Venta", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Costo", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Utilidad", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);

                            bool verifyCash = service.Value.Any(x => x.TipoPago.Type == "Cash");
                            foreach (var tipoPago in service.Value.Where(x => x.TipoPago.Type != "Money Order").GroupBy(x => x.TipoPago))
                            {
                                foreach (var item in tipoPago)
                                {
                                    cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                }
                                List<AuxReportTipoPagoDCuba> moneyOrder = new List<AuxReportTipoPagoDCuba>();
                                if (tipoPago.Key.Type == "Cash")
                                {
                                    moneyOrder = service.Value.Where(x => x.TipoPago.Type == "Money Order").ToList();
                                    foreach (var item in moneyOrder)
                                    {
                                        cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                    }
                                }
                                cell = new PdfPCell(new Phrase(tipoPago.Key.Type, headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase("Totales", headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Cantidad) + moneyOrder.Sum(x => x.Cantidad)).ToString(), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Venta) + moneyOrder.Sum(x => x.Venta)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Costo) + moneyOrder.Sum(x => x.Costo)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Utilidad) + moneyOrder.Sum(x => x.Utilidad)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                            }
                            if (!verifyCash)
                            {
                                var moneyOrder = service.Value.Where(x => x.TipoPago.Type == "Money Order").ToList();
                                foreach (var item in moneyOrder)
                                {
                                    cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                }
                                cell = new PdfPCell(new Phrase("Money Order", headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase("Totales", headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Cantidad)).ToString(), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Venta)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Costo)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Utilidad)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                            }

                            cell = new PdfPCell(new Phrase("", normalFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Gran Total", headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase(service.Value.Sum(x => x.Cantidad).ToString(), headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase(service.Value.Sum(x => x.Venta).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase(service.Value.Sum(x => x.Costo).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase(service.Value.Sum(x => x.Utilidad).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);

                            doc.Add(tbl);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #region CANCELADAS

                    float[] columnWidthscanceladas = { (float)1.5, 2, 2, 2, 1 };
                    tblremesasData = new PdfPTable(columnWidthscanceladas);
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

                    doc.Add(new Phrase("CANCELACIONES", headFont));

                    cellremesas1.AddElement(new Phrase("No. Orden", headFont));
                    cellremesas2.AddElement(new Phrase("Fecha", headFont));
                    cellremesas3.AddElement(new Phrase("Tipo Trámite", headFont));
                    cellremesas4.AddElement(new Phrase("Empleado", headFont));
                    cellremesas5.AddElement(new Phrase("Utilidad", headFont));

                    tblremesasData.AddCell(cellremesas1);
                    tblremesasData.AddCell(cellremesas2);
                    tblremesasData.AddCell(cellremesas3);
                    tblremesasData.AddCell(cellremesas4);
                    tblremesasData.AddCell(cellremesas5);

                    foreach (var item in utilityCanceled.OrderBy(x => x.TipoServicio))
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

                        cellremesas1.AddElement(new Phrase(item.OrderNumber, normalFont));
                        cellremesas2.AddElement(new Phrase(item.Date.ToShortDateString(), normalFont));
                        cellremesas3.AddElement(new Phrase(item.Service.GetDescription(), normalFont));
                        cellremesas4.AddElement(new Phrase(item.Employee?.FullName, normalFont));
                        cellremesas5.AddElement(new Phrase(item.Utility.ToString("0.00"), normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                    }

                    doc.Add(tblremesasData);

                    #endregion

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

        public async static Task<object> GetReporteUtilidad(string strdate, User aUser, databaseContext _context, IWebHostEnvironment _env, IReportService _reportService, bool onlyClientsAgency = false)
        {
            var aAgency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId);

            var auxDate = strdate.Split('-');
            CultureInfo culture = new CultureInfo("es-US", true);
            var dateIni = DateTime.Parse(auxDate[0], culture).Date;
            var dateFin = DateTime.Parse(auxDate[1], culture).Date;


            using (MemoryStream MStream = new MemoryStream())
            {

                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, MStream);
                doc.SetPageSize(PageSize.A4);
                doc.Open();
                try
                {

                    iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font headFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font fonttransferida = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.ITALIC, BaseColor.RED);


                    Agency agency = aAgency.FirstOrDefault();
                    var agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    var tiposPago = _context.TipoPago.ToList();
                    tiposPago.Add(new TipoPago
                    {
                        TipoPagoId = Guid.Empty,
                        Type = "Pendiente"
                    });

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
                    Paragraph parPaq = new Paragraph("Utilidad por servicio del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);
                    /*
                     * 0: cash
                     * 1: zelle
                     * 2: cheque
                     * 3: credito o debito
                     * 4: Transferencia Bancaria
                     * 5: Web
                     * 6: Money Order
                     * */
                    decimal[] ventastipopago = new decimal[7];
                    int[] canttipopago = new int[7];
                    decimal totalcosto = 0;
                    decimal totalprecio = 0;
                    decimal totalunitario = 0;
                    decimal gastos = 0;
                    Dictionary<string, List<AuxReportTipoPagoDCuba>> reporteTiposPago = new Dictionary<string, List<AuxReportTipoPagoDCuba>>();

                    #region // REMESAS
                    var remesas = (await _reportService.UtilityByService(agency.AgencyId, STipo.Remesa, dateIni, dateFin)).Value;
                    float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    PdfPTable tblData = new PdfPTable(columnWidths);
                    tblData.WidthPercentage = 100;

                    PdfPCell cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    PdfPCell cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    PdfPCell cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    PdfPCell cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    PdfPCell cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    PdfPCell cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    PdfPCell cellData7 = new PdfPCell();
                    cellData7.Border = 1;
                    decimal totalremesas = 0;

                    if (remesas.Any())
                    {
                        doc.Add(new Phrase("Remesas", headFont));


                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("Utilidad", headFont));
                        cellData7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);

                        foreach (UtilityModel remesa in remesas)
                        {

                            string pagos = "";
                            foreach (var item in remesa.Pays)
                            {
                                pagos += item.TipoPago + ", ";
                            }

                            var index = remesas.IndexOf(remesa);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                            }

                            cellData1.AddElement(new Phrase(remesa.OrderNumber, normalFont));
                            cellData2.AddElement(new Phrase(remesa.Client.FullName, normalFont));
                            cellData2.AddElement(new Phrase(remesa.Client.PhoneNumber, normalFont));
                            cellData3.AddElement(new Phrase(remesa.Employee.FullName, normalFont));
                            cellData4.AddElement(new Phrase(remesa.SalePrice.ToString(), normalFont));
                            cellData5.AddElement(new Phrase(remesa.Cost.ToString(), normalFont));
                            cellData6.AddElement(new Phrase(remesa.Utility.ToString(), normalFont));
                            cellData7.AddElement(new Phrase(pagos, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                        }
                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;
                        decimal refTotalcostoR = remesas.Sum(x => x.Cost);
                        decimal refTotalunitarioR = remesas.Sum(x => x.Utility);
                        decimal refTotalprecioR = remesas.Sum(x => x.SalePrice);
                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase(refTotalprecioR.ToString(), headFont));
                        cellData5.AddElement(new Phrase(refTotalcostoR.ToString(), headFont));
                        cellData6.AddElement(new Phrase(refTotalunitarioR.ToString(), headFont));
                        cellData7.AddElement(new Phrase("", normalFont));
                        totalremesas = refTotalunitarioR;
                        totalunitario += refTotalunitarioR;
                        totalcosto += refTotalcostoR;
                        totalprecio += refTotalprecioR;
                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = remesas.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "REMESAS",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["REMESAS"] = tipoPagoDCuba;
                    }

                    #endregion

                    #region // PAQUETE TURISTICO
                    var paquetesTuristicos = (await _reportService.UtilityByService(agency.AgencyId, STipo.PTuristico, dateIni, dateFin)).Value;
                    tblData = new PdfPTable(columnWidths);
                    tblData.WidthPercentage = 100;

                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;
                    decimal totalPaquetesTuristicos = 0;

                    if (paquetesTuristicos.Any())
                    {
                        doc.Add(new Phrase("Paquete Turístico", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("Utilidad", headFont));
                        cellData7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);

                        foreach (UtilityModel paqueteTuristico in paquetesTuristicos)
                        {

                            string pagos = "";
                            foreach (var item in paqueteTuristico.Pays)
                            {
                                pagos += item.TipoPago + ", ";
                            }

                            var index = paquetesTuristicos.IndexOf(paqueteTuristico);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                            }

                            cellData1.AddElement(new Phrase(paqueteTuristico.OrderNumber, normalFont));
                            cellData2.AddElement(new Phrase(paqueteTuristico.Client.FullName, normalFont));
                            cellData2.AddElement(new Phrase(paqueteTuristico.Client.PhoneNumber, normalFont));
                            cellData3.AddElement(new Phrase(paqueteTuristico.Employee.FullName, normalFont));
                            cellData4.AddElement(new Phrase(paqueteTuristico.SalePrice.ToString(), normalFont));
                            cellData5.AddElement(new Phrase(paqueteTuristico.Cost.ToString(), normalFont));
                            cellData6.AddElement(new Phrase(paqueteTuristico.Utility.ToString(), normalFont));
                            cellData7.AddElement(new Phrase(pagos, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                        }
                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;
                        decimal refTotalcostoR = paquetesTuristicos.Sum(x => x.Cost);
                        decimal refTotalunitarioR = paquetesTuristicos.Sum(x => x.Utility);
                        decimal refTotalprecioR = paquetesTuristicos.Sum(x => x.SalePrice);
                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase(refTotalprecioR.ToString(), headFont));
                        cellData5.AddElement(new Phrase(refTotalcostoR.ToString(), headFont));
                        cellData6.AddElement(new Phrase(refTotalunitarioR.ToString(), headFont));
                        cellData7.AddElement(new Phrase("", normalFont));
                        totalPaquetesTuristicos = refTotalunitarioR;
                        totalunitario += refTotalunitarioR;
                        totalcosto += refTotalcostoR;
                        totalprecio += refTotalprecioR;
                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = paquetesTuristicos.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "PAQUETE TURISTICO",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["PAQUETE TURISTICO"] = tipoPagoDCuba;
                    }

                    #endregion

                    #region // ENVIOS MARITIMOS

                    float[] columnWidthsmaritimo = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsmaritimo);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;

                    var enviosmaritimos = (await _reportService.UtilityByService(agency.AgencyId, STipo.Maritimo, dateIni, dateFin)).Value;

                    if (enviosmaritimos.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Marítimos", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("Utilidad", headFont));
                        cellData7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);


                        foreach (var enviomaritimo in enviosmaritimos)
                        {
                            totalcosto += enviomaritimo.Cost;
                            totalunitario += enviomaritimo.Utility;
                            totalprecio += enviomaritimo.SalePrice;

                            string pagos = "";
                            foreach (var item in enviomaritimo.Pays)
                            {
                                pagos += item.TipoPago + ", ";
                            }


                            var index = enviosmaritimos.IndexOf(enviomaritimo);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                            }
                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == enviomaritimo.Client.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellData1.AddElement(new Phrase(enviomaritimo.OrderNumber, normalFont));
                            cellData2.AddElement(new Phrase(enviomaritimo.Client.FullName, normalFont));
                            cellData2.AddElement(new Phrase(enviomaritimo.Client.PhoneNumber, normalFont));
                            cellData3.AddElement(new Phrase(enviomaritimo.Employee.FullName, normalFont));
                            cellData4.AddElement(new Phrase(enviomaritimo.SalePrice.ToString(), normalFont));
                            cellData5.AddElement(new Phrase(enviomaritimo.Cost.ToString(), normalFont));
                            cellData6.AddElement(new Phrase(enviomaritimo.Utility.ToString(), normalFont));
                            cellData7.AddElement(new Phrase(pagos, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;

                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase(enviosmaritimos.Sum(x => x.SalePrice).ToString(), headFont));
                        cellData5.AddElement(new Phrase(enviosmaritimos.Sum(x => x.Cost).ToString(), headFont));
                        cellData6.AddElement(new Phrase(enviosmaritimos.Sum(x => x.Utility).ToString(), headFont));
                        cellData7.AddElement(new Phrase("", normalFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        if (agency.AgencyId != AgencyName.DCubaWashington)
                        {
                            List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                            foreach (var item in tiposPago)
                            {
                                var items = enviosmaritimos.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                                if (items.Any())
                                {
                                    tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                    {
                                        TipoPago = item,
                                        ServiceType = "ENVIO MARITIMO",
                                        Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                        Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                        Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                        Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                    });
                                }
                            }
                            reporteTiposPago["MARITIMO"] = tipoPagoDCuba;
                        }

                    }


                    #endregion

                    #region // ENVIOS PASAPORTE
                    PdfPCell cellremesas8 = new PdfPCell();
                    PdfPCell cellremesas9 = new PdfPCell();
                    float[] columnWidthspasaporte = { (float)1.5, 2, 2, 1, 1, 1, 1, 1 };
                    var passports = (await _reportService.UtilityByService(agency.AgencyId, STipo.Passport, dateIni, dateFin)).Value;
                    decimal utilidadPassport = 0;
                    decimal costoConsular = 0; //Para districtCuba
                    if (passports.Any())
                    {
                        if (AgencyName.IsDistrictCuba(agency.AgencyId) || agency.AgencyId == AgencyName.MiamiPlusService)
                        {
                            if (onlyClientsAgency)
                                passports = passports.Where(x => !x.ByTransferencia).ToList();

                            float[] columnWidthspasaporte2 = { (float)1.5, 2, 2, 1, 1, 1, 1, 1, 1 };
                            doc.Add(new Phrase("Pasaportes", headFont));

                            tblData = new PdfPTable(columnWidthspasaporte2);
                            tblData.WidthPercentage = 100;
                            cellData1 = new PdfPCell();
                            cellData1.Border = 1;
                            cellData2 = new PdfPCell();
                            cellData2.Border = 1;
                            cellData3 = new PdfPCell();
                            cellData3.Border = 1;
                            cellData4 = new PdfPCell();
                            cellData4.Border = 1;
                            cellData5 = new PdfPCell();
                            cellData5.Border = 1;
                            cellData6 = new PdfPCell();
                            cellData6.Border = 1;
                            cellData7 = new PdfPCell();
                            cellData7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 1;

                            cellData1.AddElement(new Phrase("No.", headFont));
                            cellData2.AddElement(new Phrase("Cliente", headFont));
                            cellData3.AddElement(new Phrase("Empleado", headFont));
                            cellData4.AddElement(new Phrase("S. Consular", headFont));
                            cellData5.AddElement(new Phrase("P. Venta", headFont));
                            cellData6.AddElement(new Phrase("Costo Consular", headFont));
                            cellData7.AddElement(new Phrase("C. Servicio", headFont));
                            cellremesas8.AddElement(new Phrase("Utilidad", headFont));
                            cellremesas9.AddElement(new Phrase("Tipos Pagos", headFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);
                            tblData.AddCell(cellremesas9);

                            foreach (var pasaporte in passports)
                            {

                                string pagos = "";
                                if (!pasaporte.ByTransferencia)
                                    foreach (var item in pasaporte.Pays)
                                    {
                                        pagos += item.TipoPago + ", ";
                                    }

                                var index = passports.IndexOf(pasaporte);
                                if (index == 0)
                                {
                                    cellData1 = new PdfPCell();
                                    cellData1.Border = 1;
                                    cellData2 = new PdfPCell();
                                    cellData2.Border = 1;
                                    cellData3 = new PdfPCell();
                                    cellData3.Border = 1;
                                    cellData4 = new PdfPCell();
                                    cellData4.Border = 1;
                                    cellData5 = new PdfPCell();
                                    cellData5.Border = 1;
                                    cellData6 = new PdfPCell();
                                    cellData6.Border = 1;
                                    cellData7 = new PdfPCell();
                                    cellData7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 1;
                                }
                                else
                                {
                                    cellData1 = new PdfPCell();
                                    cellData1.Border = 0;
                                    cellData2 = new PdfPCell();
                                    cellData2.Border = 0;
                                    cellData3 = new PdfPCell();
                                    cellData3.Border = 0;
                                    cellData4 = new PdfPCell();
                                    cellData4.Border = 0;
                                    cellData5 = new PdfPCell();
                                    cellData5.Border = 0;
                                    cellData6 = new PdfPCell();
                                    cellData6.Border = 0;
                                    cellData7 = new PdfPCell();
                                    cellData7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                    cellremesas9 = new PdfPCell();
                                    cellremesas9.Border = 0;
                                }
                                if (pasaporte.ByTransferencia)
                                {
                                    cellData1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                    cellData1.AddElement(new Phrase("T. " + pasaporte.TransferredAgencyName, fonttransferida));
                                }
                                else
                                {
                                    cellData1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                }
                                cellData2.AddElement(new Phrase(pasaporte.Client.FullName, normalFont));
                                cellData2.AddElement(new Phrase(pasaporte.Client.PhoneNumber, normalFont));
                                cellData3.AddElement(new Phrase(pasaporte.Employee.FullName, normalFont));
                                string servConsular = "";
                                switch (pasaporte.ServicioConsular)
                                {
                                    case ServicioConsular.None:
                                        break;
                                    case ServicioConsular.PrimerVez:
                                    case ServicioConsular.PrimerVez2:
                                        servConsular = "1 Vez";
                                        break;
                                    case ServicioConsular.Prorroga1:
                                        servConsular = "Prorro1";
                                        break;
                                    case ServicioConsular.Prorroga2:
                                        servConsular = "Prorro2";
                                        break;
                                    case ServicioConsular.Renovacion:
                                        servConsular = pasaporte.ServicioConsular.GetDescription();
                                        break;
                                    case ServicioConsular.HE11:
                                        servConsular = pasaporte.ServicioConsular.GetDescription();
                                        break;
                                    default:
                                        servConsular = pasaporte.ServicioConsular.GetDescription();
                                        break;
                                }

                                cellData4.AddElement(new Phrase(servConsular, normalFont));
                                cellData5.AddElement(new Phrase(pasaporte.SalePrice.ToString(), normalFont));
                                cellData6.AddElement(new Phrase(pasaporte.Cost.ToString(), normalFont));
                                cellData7.AddElement(new Phrase(pasaporte.CServicio.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pasaporte.Utility.ToString(), normalFont));
                                cellremesas9.AddElement(new Phrase(pasaporte.ByTransferencia ? "Pendiente" : pagos, normalFont));

                                tblData.AddCell(cellData1);
                                tblData.AddCell(cellData2);
                                tblData.AddCell(cellData3);
                                tblData.AddCell(cellData4);
                                tblData.AddCell(cellData5);
                                tblData.AddCell(cellData6);
                                tblData.AddCell(cellData7);
                                tblData.AddCell(cellremesas8);
                                tblData.AddCell(cellremesas9);
                            }

                            // Añado el total
                            cellData1 = new PdfPCell();
                            cellData1.Border = 0;
                            cellData2 = new PdfPCell();
                            cellData2.Border = 0;
                            cellData3 = new PdfPCell();
                            cellData3.Border = 0;
                            cellData4 = new PdfPCell();
                            cellData4.Border = 0;
                            cellData5 = new PdfPCell();
                            cellData5.Border = 0;
                            cellData6 = new PdfPCell();
                            cellData6.Border = 0;
                            cellData7 = new PdfPCell();
                            cellData7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;
                            cellremesas9 = new PdfPCell();
                            cellremesas9.Border = 0;

                            decimal reftotalprecios = passports.Sum(x => x.SalePrice);
                            decimal reftotalcosto = passports.Sum(x => x.Cost);
                            decimal reftotalcServicio = passports.Sum(x => x.CServicio);
                            decimal reftotalutilidad = passports.Sum(x => x.Utility);
                            cellData1.AddElement(new Phrase("Totales", headFont));
                            cellData2.AddElement(new Phrase("", normalFont));
                            cellData3.AddElement(new Phrase("", normalFont));
                            cellData4.AddElement(new Phrase("", headFont));
                            cellData5.AddElement(new Phrase(reftotalprecios.ToString("0.00"), headFont));
                            cellData6.AddElement(new Phrase(reftotalcosto.ToString("0.00"), headFont));
                            cellData7.AddElement(new Phrase(reftotalcServicio.ToString("0.00"), headFont));
                            cellremesas8.AddElement(new Phrase(reftotalutilidad.ToString("0.00"), headFont));
                            cellremesas9.AddElement(new Phrase("", normalFont));
                            utilidadPassport += reftotalutilidad;
                            totalunitario += reftotalutilidad;
                            totalcosto += reftotalcosto;
                            costoConsular += reftotalcosto;
                            totalprecio += reftotalprecios;
                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);
                            tblData.AddCell(cellremesas9);
                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                            var passportDCuba = passports.OrderBy(x => x.OrderNumber).GroupBy(x => x.ServicioConsular);
                            foreach (var PassportServConsular in passportDCuba)
                            {
                                foreach (var item in tiposPago)
                                {
                                    var items = PassportServConsular.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                                    if (items.Any())
                                    {
                                        tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                        {
                                            TipoPago = item,
                                            ServiceType = PassportServConsular.Key.GetDescription().ToUpper(),
                                            Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                            Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                            Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                            Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                        });
                                    }
                                }
                            }
                            reporteTiposPago["PASAPORTES"] = tipoPagoDCuba;
                        }
                        else
                        {
                            doc.Add(new Phrase("Pasaportes", headFont));
                            tblData = new PdfPTable(columnWidthspasaporte);
                            tblData.WidthPercentage = 100;
                            cellData1 = new PdfPCell();
                            cellData1.Border = 1;
                            cellData2 = new PdfPCell();
                            cellData2.Border = 1;
                            cellData3 = new PdfPCell();
                            cellData3.Border = 1;
                            cellData4 = new PdfPCell();
                            cellData4.Border = 1;
                            cellData5 = new PdfPCell();
                            cellData5.Border = 1;
                            cellData6 = new PdfPCell();
                            cellData6.Border = 1;
                            cellData7 = new PdfPCell();
                            cellData7.Border = 1;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 1;
                            cellData1.AddElement(new Phrase("No.", headFont));
                            cellData2.AddElement(new Phrase("Cliente", headFont));
                            cellData3.AddElement(new Phrase("Empleado", headFont));
                            cellData4.AddElement(new Phrase("P. Venta", headFont));
                            if (agency.AgencyId == agencyDCubaId)
                            {
                                cellData5.AddElement(new Phrase("Costo Consular", headFont));
                                cellData6.AddElement(new Phrase("S. Consular", headFont));
                            }
                            else
                            {
                                cellData5.AddElement(new Phrase("Costo", headFont));
                                cellData6.AddElement(new Phrase("C. Servicio", headFont));
                            }
                            cellData7.AddElement(new Phrase("Utilidad", headFont));
                            cellremesas8.AddElement(new Phrase("Tipos Pagos", headFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);

                            foreach (var pasaporte in passports)
                            {

                                string pagos = "";
                                if (!pasaporte.ByTransferencia)
                                    foreach (var item in pasaporte.Pays)
                                    {
                                        pagos += item.TipoPago + ", ";
                                    }

                                var index = passports.IndexOf(pasaporte);
                                if (index == 0)
                                {
                                    cellData1 = new PdfPCell();
                                    cellData1.Border = 1;
                                    cellData2 = new PdfPCell();
                                    cellData2.Border = 1;
                                    cellData3 = new PdfPCell();
                                    cellData3.Border = 1;
                                    cellData4 = new PdfPCell();
                                    cellData4.Border = 1;
                                    cellData5 = new PdfPCell();
                                    cellData5.Border = 1;
                                    cellData6 = new PdfPCell();
                                    cellData6.Border = 1;
                                    cellData7 = new PdfPCell();
                                    cellData7.Border = 1;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 1;
                                }
                                else
                                {
                                    cellData1 = new PdfPCell();
                                    cellData1.Border = 0;
                                    cellData2 = new PdfPCell();
                                    cellData2.Border = 0;
                                    cellData3 = new PdfPCell();
                                    cellData3.Border = 0;
                                    cellData4 = new PdfPCell();
                                    cellData4.Border = 0;
                                    cellData5 = new PdfPCell();
                                    cellData5.Border = 0;
                                    cellData6 = new PdfPCell();
                                    cellData6.Border = 0;
                                    cellData7 = new PdfPCell();
                                    cellData7.Border = 0;
                                    cellremesas8 = new PdfPCell();
                                    cellremesas8.Border = 0;
                                }
                                if (pasaporte.ByTransferencia)
                                {
                                    cellData1.AddElement(new Phrase("T. " + pasaporte.TransferredAgencyName, fonttransferida));
                                }
                                else
                                {
                                    cellData1.AddElement(new Phrase(pasaporte.OrderNumber, normalFont));
                                }
                                cellData2.AddElement(new Phrase(pasaporte.Client.FullName, normalFont));
                                cellData2.AddElement(new Phrase(pasaporte.Client.PhoneNumber, normalFont));
                                cellData3.AddElement(new Phrase(pasaporte.Employee.FullName, normalFont));
                                cellData4.AddElement(new Phrase(pasaporte.SalePrice.ToString(), normalFont));
                                if (agency.AgencyId == agencyDCubaId)
                                {
                                    cellData5.AddElement(new Phrase(pasaporte.Cost.ToString(), normalFont));
                                    string servConsular = "";
                                    switch (pasaporte.ServicioConsular)
                                    {
                                        case ServicioConsular.None:
                                            break;
                                        case ServicioConsular.PrimerVez:
                                        case ServicioConsular.PrimerVez2:
                                            servConsular = "1 Vez";
                                            break;
                                        case ServicioConsular.Prorroga1:
                                            servConsular = "Prorro1";
                                            break;
                                        case ServicioConsular.Prorroga2:
                                            servConsular = "Prorro2";
                                            break;
                                        case ServicioConsular.Renovacion:
                                            servConsular = pasaporte.ServicioConsular.GetDescription();
                                            break;
                                        case ServicioConsular.HE11:
                                            servConsular = pasaporte.ServicioConsular.GetDescription();
                                            break;
                                        default:
                                            servConsular = pasaporte.ServicioConsular.GetDescription();
                                            break;
                                    }
                                    cellData6.AddElement(new Phrase(servConsular, normalFont));
                                }
                                else
                                {
                                    cellData5.AddElement(new Phrase(pasaporte.Cost.ToString(), normalFont));
                                    cellData6.AddElement(new Phrase(pasaporte.CServicio.ToString(), normalFont));
                                }

                                cellData7.AddElement(new Phrase(pasaporte.Utility.ToString(), normalFont));
                                cellremesas8.AddElement(new Phrase(pasaporte.ByTransferencia ? "Pendiente" : pagos, normalFont));

                                tblData.AddCell(cellData1);
                                tblData.AddCell(cellData2);
                                tblData.AddCell(cellData3);
                                tblData.AddCell(cellData4);
                                tblData.AddCell(cellData5);
                                tblData.AddCell(cellData6);
                                tblData.AddCell(cellData7);
                                tblData.AddCell(cellremesas8);
                            }

                            // Añado el total
                            cellData1 = new PdfPCell();
                            cellData1.Border = 0;
                            cellData2 = new PdfPCell();
                            cellData2.Border = 0;
                            cellData3 = new PdfPCell();
                            cellData3.Border = 0;
                            cellData4 = new PdfPCell();
                            cellData4.Border = 0;
                            cellData5 = new PdfPCell();
                            cellData5.Border = 0;
                            cellData6 = new PdfPCell();
                            cellData6.Border = 0;
                            cellData7 = new PdfPCell();
                            cellData7.Border = 0;
                            cellremesas8 = new PdfPCell();
                            cellremesas8.Border = 0;

                            decimal reftotalprecios = passports.Sum(x => x.SalePrice);
                            decimal reftotalcosto = passports.Sum(x => x.Cost);
                            decimal reftotalutilidad = passports.Sum(x => x.Utility);
                            cellData1.AddElement(new Phrase("Totales", headFont));
                            cellData2.AddElement(new Phrase("", normalFont));
                            cellData3.AddElement(new Phrase("", normalFont));
                            cellData4.AddElement(new Phrase(reftotalprecios.ToString("0.00"), headFont));
                            cellData5.AddElement(new Phrase(reftotalcosto.ToString("0.00"), headFont));
                            cellData6.AddElement(new Phrase(passports.Sum(x => x.CServicio).ToString("0.00"), headFont));
                            cellData7.AddElement(new Phrase(reftotalutilidad.ToString("0.00"), headFont));
                            cellremesas8.AddElement(new Phrase("", normalFont));
                            utilidadPassport = reftotalutilidad;
                            totalunitario += reftotalutilidad;
                            costoConsular += reftotalcosto;
                            totalcosto += reftotalcosto;
                            totalprecio += reftotalprecios;
                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #endregion

                    cellremesas8 = new PdfPCell();
                    #region // ENVIOS CARIBE

                    float[] columnWidthscaribe = { 2, (float)1.7, (float)1.7, 1, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthscaribe);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    List<EnvioCaribe> envioscaribe = _context.EnvioCaribes
                        .Include(x => x.AgencyTransferida)
                        .Include(x => x.User)
                        .Include(x => x.Contact).ThenInclude(x => x.Address).Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago).Include(x => x.Client).Include(x => x.Contact).Where(x => x.User.Username != "Manuel14" && x.Status != "Cancelada" && (x.AgencyId == aAgency.FirstOrDefault().AgencyId || x.AgencyTransferidaId == agency.AgencyId) && x.Date.Date >= dateIni && x.Date.Date <= dateFin).ToList();
                    decimal totalenviocaribe = 0;
                    if (envioscaribe.Count != 0)
                    {
                        //Auxiliar para obtener el costo que aplica el mayorista Caribe a Rapid
                        var rapid = _context.Agency.FirstOrDefault(x => x.Name == "Rapid Multiservice");
                        var mayorista = _context.Wholesalers.Include(x => x.tipoServicioHabana).Include(x => x.tipoServicioRestoProv).FirstOrDefault(x => x.EsVisible && x.AgencyId == rapid.AgencyId && x.Category.category == "Maritimo-Aereo");

                        doc.Add(new Phrase("Envíos Caribe", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("C. Servicio", headFont));
                        cellData7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        decimal refTotalcostoEC = 0;
                        decimal refTotalunitarioEC = 0;
                        decimal refTotalprecioEC = 0;
                        foreach (EnvioCaribe enviocaribe in envioscaribe)
                        {
                            decimal refcostoEC = enviocaribe.costo + enviocaribe.OtrosCostos;
                            decimal refunitarioEC = enviocaribe.Amount - (enviocaribe.costo + enviocaribe.OtrosCostos);
                            decimal refprecioEC = enviocaribe.Amount;

                            bool isbytransferencia = false;
                            if (enviocaribe.AgencyTransferida != null)
                            {
                                if (enviocaribe.AgencyTransferida.AgencyId == agency.AgencyId)
                                {
                                    isbytransferencia = true;
                                    refcostoEC = 0;
                                    refunitarioEC = 0;
                                    refprecioEC = 0;

                                    if (mayorista != null)
                                    {
                                        refprecioEC = enviocaribe.costo;
                                        List<TipoServicioMayorista> serv = new List<TipoServicioMayorista>();
                                        if (enviocaribe.Contact.Address.City == "La Habana")
                                        {
                                            serv = mayorista.tipoServicioHabana;
                                        }
                                        else
                                        {
                                            serv = mayorista.tipoServicioRestoProv;
                                        }

                                        if (serv != null)
                                        {
                                            if (enviocaribe.servicio == "Correo-Aereo")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoAereo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                            else if (enviocaribe.servicio == "Correo-Maritimo")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoMaritimo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                            else if (enviocaribe.servicio == "Aerovaradero- Recogida")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Aereo_Varadero);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoAereo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                            else if (enviocaribe.servicio == "Maritimo-Palco Almacen")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoMaritimo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                            else if (enviocaribe.servicio == "Palco ENTREGA A DOMICILIO")
                                            {
                                                var servicio = serv.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                                decimal costo = (enviocaribe.paquetes.Sum(x => x.peso) * servicio.costoAereo) + enviocaribe.OtrosCostos;
                                                refunitarioEC = enviocaribe.costo - costo;
                                                refcostoEC = costo;
                                            }
                                        }
                                    }
                                }
                            }

                            refTotalcostoEC += refcostoEC;
                            refTotalprecioEC += refprecioEC;
                            refTotalunitarioEC += refunitarioEC;

                            totalcosto += refcostoEC;
                            totalprecio += refprecioEC;

                            string tipospagoec = "";

                            if (!isbytransferencia)
                            {
                                foreach (var item in enviocaribe.RegistroPagos)
                                {
                                    tipospagoec += item.tipoPago.Type + ", ";
                                }
                            }

                            if (tipospagoec == "")
                            {
                                tipospagoec = "-";
                            }


                            var index = envioscaribe.IndexOf(enviocaribe);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }


                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == enviocaribe.Client.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellData1.AddElement(new Phrase(enviocaribe.Number, normalFont));
                            cellData2.AddElement(new Phrase(enviocaribe.Client.Name + " " + enviocaribe.Client.LastName, normalFont));
                            cellData3.AddElement(new Phrase(enviocaribe.User != null ? enviocaribe.User.Name + " " + enviocaribe.User.LastName : "-", normalFont));
                            cellData4.AddElement(new Phrase(refprecioEC.ToString(), normalFont));
                            cellData5.AddElement(new Phrase((refcostoEC - enviocaribe.OtrosCostos).ToString(), normalFont));
                            cellData6.AddElement(new Phrase(enviocaribe.OtrosCostos.ToString(), normalFont));
                            cellData7.AddElement(new Phrase(refunitarioEC.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tipospagoec, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase((refTotalprecioEC).ToString(), headFont));
                        cellData5.AddElement(new Phrase((refTotalcostoEC - envioscaribe.Sum(x => x.OtrosCostos)).ToString(), headFont));
                        cellData6.AddElement(new Phrase(envioscaribe.Sum(x => x.OtrosCostos).ToString(), headFont));
                        cellData7.AddElement(new Phrase(refTotalunitarioEC.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalenviocaribe = refTotalunitarioEC;
                        totalunitario += refTotalunitarioEC;
                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region // ENVIOS CUBIQ

                    float[] columnWidthscubiq = { 2, (float)1.7, (float)1.7, 1, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthscubiq);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    var envioscubiq = (await _reportService.UtilityByService(agency.AgencyId, STipo.Cubiq, dateIni, dateFin)).Value;

                    decimal totalenviocubiq = 0;
                    if (envioscubiq.Count != 0)
                    {
                        doc.Add(new Phrase("Envíos Carga AM", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("C. Servicio", headFont));
                        cellData7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        decimal refTotalcostoCubiq = 0;
                        decimal refTotalunitarioCubiq = 0;
                        decimal refTotalprecioCubiq = 0;
                        foreach (var envioCubiq in envioscubiq)
                        {
                            decimal refcostoCubiq = envioCubiq.Cost;
                            decimal refunitarioCubiq = envioCubiq.Utility;
                            decimal refprecioCubiq = envioCubiq.SalePrice;

                            refTotalcostoCubiq += refcostoCubiq;
                            refTotalprecioCubiq += refprecioCubiq;
                            refTotalunitarioCubiq += refunitarioCubiq;

                            totalcosto += refcostoCubiq;
                            totalprecio += refprecioCubiq;

                            string tipospagoCubiq = "";

                            if (!envioCubiq.ByTransferencia)
                            {
                                foreach (var item in envioCubiq.Pays)
                                {
                                    tipospagoCubiq += item.TipoPago + ", ";
                                }
                            }

                            if (tipospagoCubiq == "")
                            {
                                tipospagoCubiq = "-";
                            }


                            var index = envioscubiq.IndexOf(envioCubiq);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }


                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == envioCubiq.Client.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellData1.AddElement(new Phrase(envioCubiq.OrderNumber, normalFont));
                            cellData2.AddElement(new Phrase(envioCubiq.Client.FullName, normalFont));
                            cellData3.AddElement(new Phrase(envioCubiq.Employee != null ? envioCubiq.Employee.FullName : "-", normalFont));
                            cellData4.AddElement(new Phrase(refprecioCubiq.ToString(), normalFont));
                            cellData5.AddElement(new Phrase(refcostoCubiq.ToString(), normalFont));
                            cellData6.AddElement(new Phrase(envioCubiq.CServicio.ToString(), normalFont));
                            cellData7.AddElement(new Phrase(refunitarioCubiq.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(tipospagoCubiq, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase(refTotalprecioCubiq.ToString(), headFont));
                        cellData5.AddElement(new Phrase(refTotalcostoCubiq.ToString(), headFont));
                        cellData6.AddElement(new Phrase(envioscubiq.Sum(x => x.CServicio).ToString(), headFont));
                        cellData7.AddElement(new Phrase(refTotalunitarioCubiq.ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalenviocubiq = refTotalunitarioCubiq;
                        totalunitario += refTotalunitarioCubiq;
                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = envioscubiq.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "CARGA AM",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["CUBIQ"] = tipoPagoDCuba;
                    }
                    #endregion

                    #region // ENVIOS AEREOS

                    float[] columnWidthsaereo = { (float)2, (float)1.7, (float)1.7, 1, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsaereo);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;


                    decimal auxtotalutilidad = 0;
                    var enviosaereos = (await _reportService.UtilityByService(agency.AgencyId, STipo.Paquete, dateIni, dateFin)).Value;


                    if (enviosaereos.Count != 0)
                    {
                        doc.Add(new Phrase("ENVÍOS", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("C. Service", headFont));
                        cellData7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        foreach (var envioaereo in enviosaereos)
                        {

                            bool usedcredito = false;
                            string tiposdepago = "";
                            foreach (var item in envioaereo.Pays)
                            {
                                tiposdepago += item.TipoPago + ", ";
                                if (item.TipoPago == "Crédito de Consumo")
                                {
                                    usedcredito = true;
                                }
                            }
                            if (tiposdepago == "")
                            {
                                tiposdepago = "-";
                            }
                            totalcosto += envioaereo.Cost;
                            totalprecio += envioaereo.SalePrice;

                            var index = enviosaereos.IndexOf(envioaereo);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;

                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;

                            }

                            cellData1.AddElement(new Phrase(envioaereo.OrderNumber, normalFont));
                            bool isByTransferencia = false;
                            if (envioaereo.ByTransferencia)
                            {
                                cellData1.AddElement(new Phrase("T. " + envioaereo.TransferredAgencyName, fonttransferida));
                            }
                            cellData2.AddElement(new Phrase(envioaereo.Client.FullName, normalFont));
                            cellData2.AddElement(new Phrase(isByTransferencia ? string.Empty : envioaereo.Client.PhoneNumber, normalFont));
                            cellData3.AddElement(new Phrase(envioaereo.Employee.FullName, normalFont));
                            cellData4.AddElement(new Phrase(envioaereo.SalePrice.ToString("0.00"), normalFont));
                            cellData5.AddElement(new Phrase(envioaereo.Cost.ToString("0.00"), normalFont));
                            cellData6.AddElement(new Phrase(envioaereo.CServicio.ToString("0.00"), normalFont)); //Fee Agencia
                            cellData7.AddElement(new Phrase(envioaereo.Utility.ToString("0.00"), normalFont));
                            cellremesas8.AddElement(new Phrase(tiposdepago, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        decimal totalPrecio = enviosaereos.Sum(x => x.SalePrice);
                        decimal totalCosto = enviosaereos.Sum(x => x.Cost);
                        decimal totalUtility = enviosaereos.Sum(x => x.Utility);
                        decimal totalCservicio = enviosaereos.Sum(x => x.CServicio);

                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase(totalPrecio.ToString("0.00"), headFont));
                        cellData5.AddElement(new Phrase(totalCosto.ToString("0.00"), headFont));
                        cellData6.AddElement(new Phrase(totalCservicio.ToString("0.00"), headFont));
                        cellData7.AddElement(new Phrase(totalUtility.ToString("0.00"), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalunitario += totalUtility;
                        auxtotalutilidad = totalUtility;

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = enviosaereos.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "ENVIOS",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["ENVIOS"] = tipoPagoDCuba;

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region // ENVIOS COMBO
                    tblData = new PdfPTable(columnWidthsaereo);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;


                    decimal auxtotalutilidadCombo = 0;
                    var envioscombos = (await _reportService.UtilityByService(agency.AgencyId, STipo.Combo, dateIni, dateFin)).Value;
                    if (envioscombos.Count != 0)
                    {
                        doc.Add(new Phrase("COMBOS", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("C. Service", headFont));
                        cellData7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);


                        foreach (UtilityModel enviocombo in envioscombos)
                        {

                            bool usedcredito = false;
                            string tiposdepago = "";
                            foreach (var item in enviocombo.Pays)
                            {
                                tiposdepago += item.TipoPago + ", ";
                                if (item.TipoPago == "Crédito de Consumo")
                                {
                                    usedcredito = true;
                                }
                            }
                            if (tiposdepago == "")
                            {
                                tiposdepago = "-";
                            }



                            var index = envioscombos.IndexOf(enviocombo);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;

                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;

                            }
                            cellData1.AddElement(new Phrase(enviocombo.OrderNumber, normalFont));
                            if (enviocombo.TransferredAgencyName != "")
                            {
                                cellData1.AddElement(new Phrase("T. " + enviocombo.TransferredAgencyName, fonttransferida));
                            }
                            cellData2.AddElement(new Phrase(enviocombo.Client.FullName, normalFont));
                            cellData2.AddElement(new Phrase(enviocombo.Client.PhoneNumber, normalFont));
                            cellData3.AddElement(new Phrase(enviocombo.Employee.FullName, normalFont));
                            cellData4.AddElement(new Phrase(enviocombo.SalePrice.ToString("0.00"), normalFont));
                            cellData5.AddElement(new Phrase(enviocombo.Cost.ToString("0.00"), normalFont));
                            cellData6.AddElement(new Phrase(enviocombo.CServicio.ToString("0.00"), normalFont)); //Fee Agencia
                            cellData7.AddElement(new Phrase(enviocombo.Utility.ToString("0.00"), normalFont));
                            cellremesas8.AddElement(new Phrase(tiposdepago, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;
                        decimal reftotalcosto = envioscombos.Sum(x => x.Cost);
                        decimal reftotalutilidad = envioscombos.Sum(x => x.Utility);
                        decimal reftotalprecio = envioscombos.Sum(x => x.SalePrice);
                        decimal refTotalOtrosCosto = envioscombos.Sum(x => x.CServicio);
                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase(reftotalprecio.ToString("0.00"), headFont));
                        cellData5.AddElement(new Phrase(reftotalcosto.ToString("0.00"), headFont));
                        cellData6.AddElement(new Phrase(refTotalOtrosCosto.ToString("0.00"), headFont));
                        cellData7.AddElement(new Phrase(reftotalutilidad.ToString("0.00"), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalunitario += reftotalutilidad;
                        totalcosto += reftotalcosto;
                        totalprecio += reftotalprecio;
                        auxtotalutilidadCombo = reftotalutilidad;

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = envioscombos.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "COMBOS",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["COMBOS"] = tipoPagoDCuba;
                    }

                    #endregion

                    #region // BOLETOS  

                    float[] columnWidthboletos = { (float)1.5, (float)1.5, (float)1.5, 1, (float)1, (float)1, (float)1, (float)1 };
                    tblData = new PdfPTable(columnWidthboletos);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    var boletosAll = (await _reportService.UtilityByService(agency.AgencyId, STipo.Reserva, dateIni, dateFin)).Value;
                    var boletos = boletosAll.Where(x => !x.IsCarrier).ToList();
                    if (boletos.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("Tipo", headFont));
                        cellData5.AddElement(new Phrase("P. Venta", headFont));
                        cellData6.AddElement(new Phrase("Costo", headFont));
                        cellData7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);


                        foreach (var boleto in boletos)
                        {
                            totalcosto += boleto.Cost;
                            totalprecio += boleto.SalePrice;

                            string pagos = "";

                            foreach (var item in boleto.Pays)
                            {
                                pagos += item.TipoPago + ", ";
                                if (item.TipoPago == "Cash")
                                {
                                    ventastipopago[0] += item.PaidValue;
                                    canttipopago[0] += 1;
                                }
                                else if (item.TipoPago == "Zelle")
                                {
                                    ventastipopago[1] += item.PaidValue;
                                    canttipopago[1] += 1;
                                }
                                else if (item.TipoPago == "Cheque")
                                {
                                    ventastipopago[2] += item.PaidValue;
                                    canttipopago[2] += 1;
                                }
                                else if (item.TipoPago == "Crédito o Débito")
                                {
                                    ventastipopago[3] += item.PaidValue;
                                    canttipopago[3] += 1;
                                }
                                else if (item.TipoPago == "Transferencia Bancaria")
                                {
                                    ventastipopago[4] += item.PaidValue;
                                    canttipopago[4] += 1;
                                }
                                else if (item.TipoPago == "Web")
                                {
                                    ventastipopago[5] += item.PaidValue;
                                    canttipopago[5] += 1;
                                }
                                else if (item.TipoPago == "Money Order")
                                {
                                    ventastipopago[6] += item.PaidValue;
                                    canttipopago[6] += 1;
                                }
                            }

                            var index = boletos.IndexOf(boleto);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }

                            cellData1.AddElement(new Phrase(boleto.OrderNumber, normalFont));
                            cellData2.AddElement(new Phrase(boleto.Client.FullName, normalFont));
                            cellData2.AddElement(new Phrase(boleto.Client.PhoneNumber, normalFont));
                            cellData3.AddElement(new Phrase(boleto.Employee.FullName, normalFont));
                            cellData4.AddElement(new Phrase(boleto.TipoServicio, normalFont));
                            cellData5.AddElement(new Phrase(boleto.SalePrice.ToString(), normalFont));
                            cellData6.AddElement(new Phrase(boleto.Cost.ToString(), normalFont));
                            cellData7.AddElement(new Phrase(boleto.Utility.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pagos, normalFont));
                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase("", normalFont));
                        cellData5.AddElement(new Phrase(boletos.Sum(x => x.SalePrice).ToString(), headFont));
                        cellData6.AddElement(new Phrase(boletos.Sum(x => x.Cost).ToString(), headFont));
                        cellData7.AddElement(new Phrase(boletos.Sum(x => x.Utility).ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        totalunitario += boletos.Sum(x => x.Utility);

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = boletos.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "COMBOS",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["BOLETOS"] = tipoPagoDCuba;
                    }

                    #endregion

                    #region // BOLETOS CARRIER 

                    tblData = new PdfPTable(columnWidthboletos);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;
                    cellremesas8 = new PdfPCell();
                    cellremesas8.Border = 1;

                    var boletosCarrier = boletosAll.Where(x => x.IsCarrier).ToList();

                    if (boletosCarrier.Count != 0)
                    {
                        doc.Add(new Phrase("Boletos Carrier", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("Tipo", headFont));
                        cellData5.AddElement(new Phrase("P. Venta", headFont));
                        cellData6.AddElement(new Phrase("Costo", headFont));
                        cellData7.AddElement(new Phrase("Utilidad", headFont));
                        cellremesas8.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);


                        foreach (var boleto in boletosCarrier)
                        {
                            string pagos = "";

                            foreach (var item in boleto.Pays)
                            {
                                pagos += item.TipoPago + ", ";
                                if (item.TipoPago == "Cash")
                                {
                                    ventastipopago[0] += item.PaidValue;
                                    canttipopago[0] += 1;
                                }
                                else if (item.TipoPago == "Zelle")
                                {
                                    ventastipopago[1] += item.PaidValue;
                                    canttipopago[1] += 1;
                                }
                                else if (item.TipoPago == "Cheque")
                                {
                                    ventastipopago[2] += item.PaidValue;
                                    canttipopago[2] += 1;
                                }
                                else if (item.TipoPago == "Crédito o Débito")
                                {
                                    ventastipopago[3] += item.PaidValue;
                                    canttipopago[3] += 1;
                                }
                                else if (item.TipoPago == "Transferencia Bancaria")
                                {
                                    ventastipopago[4] += item.PaidValue;
                                    canttipopago[4] += 1;
                                }
                                else if (item.TipoPago == "Web")
                                {
                                    ventastipopago[5] += item.PaidValue;
                                    canttipopago[5] += 1;
                                }
                                else if (item.TipoPago == "Money Order")
                                {
                                    ventastipopago[6] += item.PaidValue;
                                    canttipopago[6] += 1;
                                }
                            }

                            var index = boletosCarrier.IndexOf(boleto);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 1;
                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                                cellremesas8 = new PdfPCell();
                                cellremesas8.Border = 0;
                            }


                            cellData1.AddElement(new Phrase(boleto.OrderNumber, normalFont));
                            cellData2.AddElement(new Phrase(boleto.Client.FullName, normalFont));
                            cellData2.AddElement(new Phrase(boleto.Client.PhoneNumber, normalFont));
                            cellData3.AddElement(new Phrase(boleto.Employee.FullName, normalFont));
                            cellData4.AddElement(new Phrase(boleto.TipoServicio, normalFont));
                            cellData5.AddElement(new Phrase(boleto.SalePrice.ToString(), normalFont));
                            cellData6.AddElement(new Phrase(boleto.Cost.ToString(), normalFont));
                            cellData7.AddElement(new Phrase(boleto.Utility.ToString(), normalFont));
                            cellremesas8.AddElement(new Phrase(pagos, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                            tblData.AddCell(cellremesas8);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;
                        cellremesas8 = new PdfPCell();
                        cellremesas8.Border = 0;

                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase("", normalFont));
                        cellData5.AddElement(new Phrase(boletosCarrier.Sum(x => x.SalePrice).ToString(), headFont));
                        cellData6.AddElement(new Phrase(boletosCarrier.Sum(x => x.Cost).ToString(), headFont));
                        cellData7.AddElement(new Phrase(boletosCarrier.Sum(x => x.Utility).ToString(), headFont));
                        cellremesas8.AddElement(new Phrase("", normalFont));
                        //totalunitario += boletosCarrier.Sum(x => x.Utility);
                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);
                        tblData.AddCell(cellremesas8);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                        foreach (var item in tiposPago)
                        {
                            var items = boletosCarrier.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                            if (items.Any())
                            {
                                tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                {
                                    TipoPago = item,
                                    ServiceType = "COMBOS",
                                    Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                    Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                    Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                    Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                });
                            }
                        }
                        reporteTiposPago["BOLETOS CARRIER"] = tipoPagoDCuba;
                    }
                    #endregion

                    #region // RECARGAS

                    float[] columnWidthsrecarga = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsrecarga);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;

                    List<Rechargue> recargas = _context.Rechargue.Include(x => x.User).Include(x => x.tipoPago).Include(x => x.Client).Where(x => x.estado != "Cancelada" && x.AgencyId == aAgency.FirstOrDefault().AgencyId && x.date.Date >= dateIni && x.date.Date <= dateFin).ToList();
                    if (recargas.Count != 0)
                    {
                        doc.Add(new Phrase("RECARGAS", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("Utilidad", headFont));
                        cellData7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);


                        foreach (Rechargue recarga in recargas)
                        {
                            totalcosto += recarga.costoMayorista;
                            totalprecio += recarga.Import;

                            if (recarga.tipoPago.Type == "Cash")
                            {
                                ventastipopago[0] += recarga.Import - recarga.costoMayorista;
                                canttipopago[0] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Zelle")
                            {
                                ventastipopago[1] += recarga.Import - recarga.costoMayorista;
                                canttipopago[1] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Cheque")
                            {
                                ventastipopago[2] += recarga.Import - recarga.costoMayorista;
                                canttipopago[2] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Crédito o Débito")
                            {
                                ventastipopago[3] += recarga.Import - recarga.costoMayorista;
                                canttipopago[3] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Transferencia Bancaria")
                            {
                                ventastipopago[4] += recarga.Import - recarga.costoMayorista;
                                canttipopago[4] += 1;
                            }
                            else if (recarga.tipoPago.Type == "Web")
                            {
                                ventastipopago[5] += recarga.Import - recarga.costoMayorista;
                                canttipopago[5] += 1;
                            }

                            var index = recargas.IndexOf(recarga);
                            if (index == 0)
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 1;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 1;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 1;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 1;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 1;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 1;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 1;
                            }
                            else
                            {
                                cellData1 = new PdfPCell();
                                cellData1.Border = 0;
                                cellData2 = new PdfPCell();
                                cellData2.Border = 0;
                                cellData3 = new PdfPCell();
                                cellData3.Border = 0;
                                cellData4 = new PdfPCell();
                                cellData4.Border = 0;
                                cellData5 = new PdfPCell();
                                cellData5.Border = 0;
                                cellData6 = new PdfPCell();
                                cellData6.Border = 0;
                                cellData7 = new PdfPCell();
                                cellData7.Border = 0;
                            }


                            var Client = _context.Client.Find(recarga.ClientId);
                            var phoneClient = _context.Phone.Where(x => x.ReferenceId == recarga.ClientId && x.Type == "Móvil").FirstOrDefault();

                            cellData1.AddElement(new Phrase(recarga.Number, normalFont));
                            cellData2.AddElement(new Phrase(Client.Name + " " + Client.LastName, normalFont));
                            cellData2.AddElement(new Phrase(phoneClient?.Number, normalFont));
                            cellData3.AddElement(new Phrase(recarga.User != null ? recarga.User.Name + " " + recarga.User.LastName : "-", normalFont));
                            cellData4.AddElement(new Phrase(recarga.Import.ToString(), normalFont));
                            cellData5.AddElement(new Phrase(recarga.costoMayorista.ToString(), normalFont));
                            decimal auxutilidad = recarga.Import - recarga.costoMayorista;
                            cellData6.AddElement(new Phrase(auxutilidad.ToString(), normalFont));
                            cellData7.AddElement(new Phrase(recarga.tipoPago.Type, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;

                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase(recargas.Sum(x => x.Import).ToString(), headFont));
                        cellData5.AddElement(new Phrase(recargas.Sum(x => x.costoMayorista).ToString(), headFont));
                        cellData6.AddElement(new Phrase(recargas.Sum(x => x.Import - x.costoMayorista).ToString(), headFont));
                        cellData7.AddElement(new Phrase("", normalFont));
                        totalunitario += recargas.Sum(x => x.Import - x.costoMayorista);
                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }


                    #endregion

                    #region OTROS SERVICIOS
                    float[] columnWidthsservicios = { (float)1.5, 2, 2, 1, 1, 1, 1 };

                    var servicios = (await _reportService.UtilityByService(agency.AgencyId, STipo.Servicio, dateIni, dateFin)).Value;

                    //Hago una tabla para cada servicio
                    var tiposervicios = _context.TipoServicios.Where(x => x.agency.AgencyId == agency.AgencyId);
                    foreach (var tipo in tiposervicios)
                    {
                        var auxservicios = servicios.Where(x => x.TipoServicio == tipo.Nombre).ToList();

                        if(tipo.Nombre == "Gastos")
                        {
                            gastos += auxservicios.Sum(x => x.Utility);
                        }
                        else if (auxservicios.Count() != 0)
                        {
                            tblData = new PdfPTable(columnWidthsservicios);
                            tblData.WidthPercentage = 100;
                            cellData1 = new PdfPCell();
                            cellData1.Border = 1;
                            cellData2 = new PdfPCell();
                            cellData2.Border = 1;
                            cellData3 = new PdfPCell();
                            cellData3.Border = 1;
                            cellData4 = new PdfPCell();
                            cellData4.Border = 1;
                            cellData5 = new PdfPCell();
                            cellData5.Border = 1;
                            cellData6 = new PdfPCell();
                            cellData6.Border = 1;
                            cellData7 = new PdfPCell();
                            cellData7.Border = 1;
                            doc.Add(new Phrase(tipo.Nombre.ToUpper(), headFont));

                            cellData1.AddElement(new Phrase("No.", headFont));
                            cellData2.AddElement(new Phrase("Cliente", headFont));
                            cellData3.AddElement(new Phrase("Empleado", headFont));
                            cellData4.AddElement(new Phrase("P. Venta", headFont));
                            cellData5.AddElement(new Phrase("Costo", headFont));
                            cellData6.AddElement(new Phrase("Utilidad", headFont));
                            cellData7.AddElement(new Phrase("Tipo Pago", headFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);


                            foreach (var servicio in auxservicios)
                            {
                                string tiposPagoOS = "";
                                foreach (var item in servicio.Pays)
                                {
                                    tiposPagoOS += $"{item.TipoPago}, ";

                                    if (item.TipoPago == "Cash")
                                    {
                                        ventastipopago[0] += item.PaidValue;
                                        canttipopago[0] += 1;
                                    }
                                    else if (item.TipoPago == "Zelle")
                                    {
                                        ventastipopago[1] += item.PaidValue;
                                        canttipopago[1] += 1;
                                    }
                                    else if (item.TipoPago == "Cheque")
                                    {
                                        ventastipopago[2] += item.PaidValue;
                                        canttipopago[2] += 1;
                                    }
                                    else if (item.TipoPago == "Crédito o Débito")
                                    {
                                        ventastipopago[3] += item.PaidValue;
                                        canttipopago[3] += 1;
                                    }
                                    else if (item.TipoPago == "Transferencia Bancaria")
                                    {
                                        ventastipopago[4] += item.PaidValue;
                                        canttipopago[4] += 1;
                                    }
                                    else if (item.TipoPago == "Web")
                                    {
                                        ventastipopago[5] += item.PaidValue;
                                        canttipopago[5] += 1;
                                    }
                                    else if (item.TipoPago == "Money Order")
                                    {
                                        ventastipopago[6] += item.PaidValue;
                                        canttipopago[6] += 1;
                                    }
                                }

                                var index = auxservicios.IndexOf(servicio);
                                if (index == 0)
                                {
                                    cellData1 = new PdfPCell();
                                    cellData1.Border = 1;
                                    cellData2 = new PdfPCell();
                                    cellData2.Border = 1;
                                    cellData3 = new PdfPCell();
                                    cellData3.Border = 1;
                                    cellData4 = new PdfPCell();
                                    cellData4.Border = 1;
                                    cellData5 = new PdfPCell();
                                    cellData5.Border = 1;
                                    cellData6 = new PdfPCell();
                                    cellData6.Border = 1;
                                    cellData7 = new PdfPCell();
                                    cellData7.Border = 1;
                                }
                                else
                                {
                                    cellData1 = new PdfPCell();
                                    cellData1.Border = 0;
                                    cellData2 = new PdfPCell();
                                    cellData2.Border = 0;
                                    cellData3 = new PdfPCell();
                                    cellData3.Border = 0;
                                    cellData4 = new PdfPCell();
                                    cellData4.Border = 0;
                                    cellData5 = new PdfPCell();
                                    cellData5.Border = 0;
                                    cellData6 = new PdfPCell();
                                    cellData6.Border = 0;
                                    cellData7 = new PdfPCell();
                                    cellData7.Border = 0;
                                }

                                if (string.IsNullOrEmpty(servicio.Minorista))
                                    cellData1.AddElement(new Phrase(servicio.OrderNumber, normalFont));
                                else
                                    cellData1.AddElement(new Phrase(servicio.OrderNumber + " " + servicio.Minorista, normalFont));

                                cellData2.AddElement(new Phrase(servicio.Client.FullName, normalFont));
                                cellData2.AddElement(new Phrase(servicio.Client.PhoneNumber, normalFont));
                                cellData3.AddElement(new Phrase(servicio.Employee.FullName, normalFont));
                                cellData4.AddElement(new Phrase(servicio.SalePrice.ToString("0.00"), normalFont));
                                cellData5.AddElement(new Phrase(servicio.Cost.ToString("0.00"), normalFont));
                                cellData6.AddElement(new Phrase(servicio.Utility.ToString("0.00"), normalFont));
                                cellData7.AddElement(new Phrase(tiposPagoOS, normalFont));

                                tblData.AddCell(cellData1);
                                tblData.AddCell(cellData2);
                                tblData.AddCell(cellData3);
                                tblData.AddCell(cellData4);
                                tblData.AddCell(cellData5);
                                tblData.AddCell(cellData6);
                                tblData.AddCell(cellData7);
                            }

                            // Añado el total
                            cellData1 = new PdfPCell();
                            cellData1.Border = 0;
                            cellData2 = new PdfPCell();
                            cellData2.Border = 0;
                            cellData3 = new PdfPCell();
                            cellData3.Border = 0;
                            cellData4 = new PdfPCell();
                            cellData4.Border = 0;
                            cellData5 = new PdfPCell();
                            cellData5.Border = 0;
                            cellData6 = new PdfPCell();
                            cellData6.Border = 0;
                            cellData7 = new PdfPCell();
                            cellData7.Border = 0;

                            cellData1.AddElement(new Phrase("Totales", headFont));
                            cellData2.AddElement(new Phrase("", normalFont));
                            cellData3.AddElement(new Phrase("", normalFont));
                            decimal auxTotal = auxservicios.Sum(x => x.SalePrice);
                            decimal auxCosto = auxservicios.Sum(x => x.Cost);
                            decimal auxUtilidad = auxservicios.Sum(x => x.Utility);
                            cellData4.AddElement(new Phrase(auxTotal.ToString(), headFont));
                            cellData5.AddElement(new Phrase(auxCosto.ToString(), headFont));
                            cellData6.AddElement(new Phrase(auxUtilidad.ToString(), headFont));
                            cellData7.AddElement(new Phrase("", normalFont));
                            totalunitario += auxUtilidad;
                            totalcosto += auxCosto;
                            totalprecio += auxTotal;
                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);

                            List<AuxReportTipoPagoDCuba> tipoPagoDCuba = new List<AuxReportTipoPagoDCuba>();
                            foreach (var item in tiposPago)
                            {
                                var items = auxservicios.Where(x => x.Pays.Any(y => y.TipoPagoId == item.TipoPagoId));
                                if (items.Any())
                                {
                                    tipoPagoDCuba.Add(new AuxReportTipoPagoDCuba
                                    {
                                        TipoPago = item,
                                        ServiceType = tipo.Nombre.ToUpper(),
                                        Cantidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Count()),
                                        Venta = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.PaidValue)),
                                        Costo = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Costo)),
                                        Utilidad = items.Sum(x => x.Pays.Where(y => y.TipoPagoId == item.TipoPagoId).Sum(z => z.Utility))
                                    });
                                }
                            }
                            reporteTiposPago[tipo.Nombre.ToUpper()] = tipoPagoDCuba;

                            // Añado la tabla al documento
                            doc.Add(tblData);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }
                    #endregion

                    #region MERCADO
                    float[] columnWidthsmercado = { (float)1.5, 2, 2, 1, 1, 1, 1 };
                    tblData = new PdfPTable(columnWidthsmercado);
                    tblData.WidthPercentage = 100;
                    cellData1 = new PdfPCell();
                    cellData1.Border = 1;
                    cellData2 = new PdfPCell();
                    cellData2.Border = 1;
                    cellData3 = new PdfPCell();
                    cellData3.Border = 1;
                    cellData4 = new PdfPCell();
                    cellData4.Border = 1;
                    cellData5 = new PdfPCell();
                    cellData5.Border = 1;
                    cellData6 = new PdfPCell();
                    cellData6.Border = 1;
                    cellData7 = new PdfPCell();
                    cellData7.Border = 1;

                    var mercados = (await _reportService.UtilityByService(agency.AgencyId, STipo.Mercado, dateIni, dateFin)).Value;

                    if (mercados.Count != 0)
                    {
                        doc.Add(new Phrase("MERCADO", headFont));

                        cellData1.AddElement(new Phrase("No.", headFont));
                        cellData2.AddElement(new Phrase("Cliente", headFont));
                        cellData3.AddElement(new Phrase("Empleado", headFont));
                        cellData4.AddElement(new Phrase("P. Venta", headFont));
                        cellData5.AddElement(new Phrase("Costo", headFont));
                        cellData6.AddElement(new Phrase("Utilidad", headFont));
                        cellData7.AddElement(new Phrase("Tipo Pago", headFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);

                        foreach (var mercado in mercados)
                        {
                            totalcosto += mercado.Cost;
                            totalprecio += mercado.SalePrice;
                            totalunitario += mercado.Utility;

                            string tiposPagoMercado = "";
                            foreach (var item in mercado.Pays)
                            {
                                tiposPagoMercado += $"{item.TipoPago}, ";
                            }

                            int borderValue = (mercados.IndexOf(mercado) == 0) ? 1 : 0;

                            cellData1 = new PdfPCell { Border = borderValue };
                            cellData2 = new PdfPCell { Border = borderValue };
                            cellData3 = new PdfPCell { Border = borderValue };
                            cellData4 = new PdfPCell { Border = borderValue };
                            cellData5 = new PdfPCell { Border = borderValue };
                            cellData6 = new PdfPCell { Border = borderValue };
                            cellData7 = new PdfPCell { Border = borderValue };

                            cellData1.AddElement(new Phrase(mercado.OrderNumber, normalFont));
                            cellData2.AddElement(new Phrase(mercado.Client.FullName, normalFont));
                            cellData2.AddElement(new Phrase(mercado.Client.PhoneNumber, normalFont));
                            cellData3.AddElement(new Phrase(mercado.Employee.FullName, normalFont));
                            cellData4.AddElement(new Phrase(mercado.SalePrice.ToString("0.00"), normalFont));
                            cellData5.AddElement(new Phrase(mercado.Cost.ToString("0.00"), normalFont));
                            cellData6.AddElement(new Phrase(mercado.Utility.ToString("0.00"), normalFont));
                            cellData7.AddElement(new Phrase(tiposPagoMercado, normalFont));

                            tblData.AddCell(cellData1);
                            tblData.AddCell(cellData2);
                            tblData.AddCell(cellData3);
                            tblData.AddCell(cellData4);
                            tblData.AddCell(cellData5);
                            tblData.AddCell(cellData6);
                            tblData.AddCell(cellData7);
                        }

                        // Añado el total
                        cellData1 = new PdfPCell();
                        cellData1.Border = 0;
                        cellData2 = new PdfPCell();
                        cellData2.Border = 0;
                        cellData3 = new PdfPCell();
                        cellData3.Border = 0;
                        cellData4 = new PdfPCell();
                        cellData4.Border = 0;
                        cellData5 = new PdfPCell();
                        cellData5.Border = 0;
                        cellData6 = new PdfPCell();
                        cellData6.Border = 0;
                        cellData7 = new PdfPCell();
                        cellData7.Border = 0;

                        cellData1.AddElement(new Phrase("Totales", headFont));
                        cellData2.AddElement(new Phrase("", normalFont));
                        cellData3.AddElement(new Phrase("", normalFont));
                        cellData4.AddElement(new Phrase(mercados.Sum(x => x.SalePrice).ToString("0.00"), headFont));
                        cellData5.AddElement(new Phrase(mercados.Sum(x => x.Cost).ToString("0.00"), headFont));
                        cellData6.AddElement(new Phrase(mercados.Sum(x => x.Utility).ToString("0.00"), headFont));
                        cellData7.AddElement(new Phrase("", normalFont));

                        tblData.AddCell(cellData1);
                        tblData.AddCell(cellData2);
                        tblData.AddCell(cellData3);
                        tblData.AddCell(cellData4);
                        tblData.AddCell(cellData5);
                        tblData.AddCell(cellData6);
                        tblData.AddCell(cellData7);

                        // Añado la tabla al documento
                        doc.Add(tblData);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                    }
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
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + totalremesas.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    auxcant = paquetesTuristicos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PAQUETES TURISTICOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + totalPaquetesTuristicos.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosmaritimos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS MARÍTIMOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + enviosmaritimos.Sum(x => x.SalePrice - x.Cost).ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }
                    if (agency.Name == "Rapid Multiservice")
                    {
                        auxcant = envioscaribe.Count();
                        auxcanttoal += auxcant;

                        if (auxcant != 0)
                        {
                            aux = new Phrase("ENVÍOS CARIBE: ", headFont);
                            aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + totalenviocaribe + " usd", normalFont));
                            cellleft.AddElement(aux);
                        }
                    }

                    auxcant = envioscubiq.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS CARGA AM: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + totalenviocubiq + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = enviosaereos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS AÉREOS: ", headFont);

                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxtotalutilidad.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = envioscombos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("ENVÍOS COMBOS: ", headFont);

                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxtotalutilidadCombo.ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    auxcant = passports.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("PASAPORTES: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Pasaportes -- $" + utilidadPassport + " usd", normalFont));
                        cellleft.AddElement(aux);
                        if (AgencyName.IsDistrictCuba(agency.AgencyId))
                        {
                            aux = new Phrase("COSTO CONSULAR: ", headFontRed);
                            aux.AddSpecial(new Phrase($"${costoConsular} usd", normalFontRed));
                            cellleft.AddElement(aux);
                        }

                    }

                    auxcant = boletos.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + boletos.Sum(x => x.Utility).ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    /*auxcant = boletosCarrier.Where(x => x.State != "Pendiente").Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("BOLETOS CARRIER: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + boletosCarrier.Where(x => x.State != "Pendiente").Sum(x => x.Total - x.Cost - x.Charges).ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }*/

                    auxcant = recargas.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("RECARGAS: ", headFont);
                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + recargas.Sum(x => x.Import - x.costoMayorista).ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    foreach (var item in tiposervicios)
                    {
                        var auxservicioBySub = servicios.Where(x => x.TipoServicio == item.Nombre).GroupBy(x => x.SubServicio).ToList();
                        foreach (var auxservicio in auxservicioBySub)
                        {
                            auxcant = auxservicio.Count();
                            auxcanttoal += auxcant;

                            if (auxcant != 0)
                            {
                                string serviceName = string.IsNullOrEmpty(auxservicio.Key) ? $"{item.Nombre.ToUpper()}: " : $"{item.Nombre.ToUpper()} - {auxservicio.Key.ToUpper()}: ";
                                aux = new Phrase(serviceName, headFont);
                                aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + auxservicio.Sum(x => x.Utility).ToString() + " usd", normalFont));
                                cellleft.AddElement(aux);
                            }
                        }
                    }

                    auxcant = mercados.Count();
                    auxcanttoal += auxcant;

                    if (auxcant != 0)
                    {
                        aux = new Phrase("MERCADO: ", headFont);

                        aux.AddSpecial(new Phrase(auxcant + " Órdenes -- $" + mercados.Sum(x => x.SalePrice - x.Cost).ToString() + " usd", normalFont));
                        cellleft.AddElement(aux);
                    }

                    cellleft.AddElement(Chunk.NEWLINE);
                    aux = new Phrase("Total de Órdenes: ", headFont);
                    aux.AddSpecial(new Phrase(auxcanttoal + " Órdenes", normalFont));
                    cellleft.AddElement(aux);
                    cellleft.AddElement(Chunk.NEWLINE);
                    #endregion

                    #region //GRAN TOTAL
                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph total = new Paragraph("Total P. Venta: ", headFont2);
                    total.AddSpecial(new Phrase("$ " + totalprecio.ToString(), normalFont2));
                    total.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(total);
                    Paragraph porpagar = new Paragraph("Total Costo: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalcosto.ToString(), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);
                    if(gastos > 0)
                    {
                        Paragraph pGastos = new Paragraph("Gastos: ", headFont2);
                        pGastos.AddSpecial(new Phrase("$ " + gastos.ToString(), normalFont2));
                        pGastos.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(pGastos);
                        totalunitario -= gastos;
                    }
                    Paragraph deuda = new Paragraph("Total Utilidad: ", headFont2);
                    deuda.AddSpecial(new Phrase("$ " + totalunitario.ToString(), normalFont2));
                    deuda.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deuda);
                    

                    /*UtilityByOrder reportByOrder = new UtilityByOrder(_context);
                    var utilityCanceled = await reportByOrder.GetCanceledDay(agency.AgencyId, dateIni, dateFin);

                    Paragraph cancelaciones = new Paragraph("Cancelaciones: ", headFont2);
                    cancelaciones.AddSpecial(new Phrase(" $ " + utilityCanceled.Sum(x => x.Utility).ToString("0.00"), normalFont2));
                    cancelaciones.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(cancelaciones);
                    cellright.AddElement(Chunk.NEWLINE);*/

                    #endregion

                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    if (AgencyName.IsDistrictCuba(agency.AgencyId) || agency.AgencyId == AgencyName.MiamiPlusService)
                    {
                        foreach (var service in reporteTiposPago)
                        {
                            doc.Add(new Phrase($"REPORTE TIPO PAGO - {service.Key.ToUpper()}", underLineFont));
                            float[] columnWidth = { 3, 2, 2, 2, 2, 2 };
                            PdfPTable tbl = new PdfPTable(columnWidth);
                            tbl.WidthPercentage = 100;

                            PdfPCell cell = new PdfPCell(new Phrase("Tipo de Pago", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Tramite", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Cantidad", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Venta", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Costo", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Utilidad", headFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            tbl.AddCell(cell);

                            bool verifyCash = service.Value.Any(x => x.TipoPago.Type == "Cash");
                            foreach (var tipoPago in service.Value.Where(x => x.TipoPago.Type != "Money Order").GroupBy(x => x.TipoPago))
                            {
                                foreach (var item in tipoPago)
                                {
                                    cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                }
                                List<AuxReportTipoPagoDCuba> moneyOrder = new List<AuxReportTipoPagoDCuba>();
                                if (tipoPago.Key.Type == "Cash")
                                {
                                    moneyOrder = service.Value.Where(x => x.TipoPago.Type == "Money Order").ToList();
                                    foreach (var item in moneyOrder)
                                    {
                                        cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                        cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                        cell.Border = PdfPCell.NO_BORDER;
                                        tbl.AddCell(cell);
                                    }
                                }
                                cell = new PdfPCell(new Phrase(tipoPago.Key.Type, headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase("Totales", headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Cantidad) + moneyOrder.Sum(x => x.Cantidad)).ToString(), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Venta) + moneyOrder.Sum(x => x.Venta)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Costo) + moneyOrder.Sum(x => x.Costo)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((tipoPago.Sum(x => x.Utilidad) + moneyOrder.Sum(x => x.Utilidad)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                            }
                            if (!verifyCash)
                            {
                                var moneyOrder = service.Value.Where(x => x.TipoPago.Type == "Money Order").ToList();
                                foreach (var item in moneyOrder)
                                {
                                    cell = new PdfPCell(new Phrase(item.TipoPago.Type.ToUpper(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.ServiceType, normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Venta.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Costo.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                    cell = new PdfPCell(new Phrase(item.Utilidad.ToString("0.00"), normalFont));
                                    cell.Border = PdfPCell.NO_BORDER;
                                    tbl.AddCell(cell);
                                }
                                cell = new PdfPCell(new Phrase("Money Order", headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase("Totales", headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Cantidad)).ToString(), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Venta)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Costo)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                                cell = new PdfPCell(new Phrase((moneyOrder.Sum(x => x.Utilidad)).ToString("0.00"), headFont));
                                cell.Border = PdfPCell.NO_BORDER;
                                tbl.AddCell(cell);
                            }

                            cell = new PdfPCell(new Phrase("", normalFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Gran Total", headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase(service.Value.Sum(x => x.Cantidad).ToString(), headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase(service.Value.Sum(x => x.Venta).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase(service.Value.Sum(x => x.Costo).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);
                            cell = new PdfPCell(new Phrase(service.Value.Sum(x => x.Utilidad).ToString("0.00"), headFont));
                            cell.Border = PdfPCell.TOP_BORDER;
                            tbl.AddCell(cell);

                            doc.Add(tbl);
                            doc.Add(Chunk.NEWLINE);
                        }
                    }

                    #region CANCELADAS
                    /*
                    float[] columnWidthscanceladas = { (float)1.5, 2, 2, 2, 1 };
                    tblremesasData = new PdfPTable(columnWidthscanceladas);
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

                    doc.Add(new Phrase("CANCELACIONES", headFont));

                    cellremesas1.AddElement(new Phrase("No. Orden", headFont));
                    cellremesas2.AddElement(new Phrase("Fecha", headFont));
                    cellremesas3.AddElement(new Phrase("Tipo Trámite", headFont));
                    cellremesas4.AddElement(new Phrase("Empleado", headFont));
                    cellremesas5.AddElement(new Phrase("Utilidad", headFont));

                    tblremesasData.AddCell(cellremesas1);
                    tblremesasData.AddCell(cellremesas2);
                    tblremesasData.AddCell(cellremesas3);
                    tblremesasData.AddCell(cellremesas4);
                    tblremesasData.AddCell(cellremesas5);

                    foreach (var item in utilityCanceled.OrderBy(x => x.TipoServicio))
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

                        cellremesas1.AddElement(new Phrase(item.OrderNumber, normalFont));
                        cellremesas2.AddElement(new Phrase(item.Date.ToShortDateString(), normalFont));
                        cellremesas3.AddElement(new Phrase(item.Service.GetDescription(), normalFont));
                        cellremesas4.AddElement(new Phrase(item.Employee?.FullName, normalFont));
                        cellremesas5.AddElement(new Phrase(item.Utility.ToString("0.00"), normalFont));

                        tblremesasData.AddCell(cellremesas1);
                        tblremesasData.AddCell(cellremesas2);
                        tblremesasData.AddCell(cellremesas3);
                        tblremesasData.AddCell(cellremesas4);
                        tblremesasData.AddCell(cellremesas5);
                    }

                    doc.Add(tblremesasData);*/

                    #endregion
                }
                catch (Exception e)
                {
                    Serilog.Log.Fatal(e, "Server Error");
                    throw;
                }
                finally
                {
                    doc.Close();
                    pdf.Close();
                }

                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        private class AuxReportTipoPagoDCuba
        {
            public TipoPago TipoPago { get; set; }
            public string ServiceType { get; set; }
            public int Cantidad { get; set; }
            public decimal Venta { get; set; }
            public decimal Costo { get; set; }
            public decimal Utilidad { get; set; }
        }
    }
}
