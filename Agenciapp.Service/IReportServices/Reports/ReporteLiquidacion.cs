
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

namespace Agenciapp.Service.IReportServices.Reports
{
    public static partial class Reporte
    {
        public async static Task<string> GetReporteLiquidacion(string strdate, User aUser, databaseContext _context, IHostingEnvironment _env)
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
                    iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

                    Agency agency = aAgency.FirstOrDefault();
                    var agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
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

                    var auxDate = strdate.Split('-');
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
                    Paragraph parPaq = new Paragraph("Liquidación por empleado del " + texto, line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);
                    /*
                     * 0: cash
                     * 1: zelle
                     * 2: cheque
                     * 3: credito o debito
                     * 4: transferencia bancaria
                     * 5: web
                     * 6: Money Order
                     * 7: Crédito de Consumo
                     * */
                    decimal[] ventastipopago = new decimal[9];
                    int[] canttipopago = new int[9];
                    decimal grantotal = 0;
                    decimal totalpagado = 0;
                    decimal total_Tpagado = 0;
                    decimal totalcantidad = 0;

                    var ventasEmpleado = await _context.ReporteLiquidacion
                    .Where(x => x.AgencyId == agency.AgencyId && x.Date.ToLocalTime().Date >= dateIni.Date && x.Date.ToLocalTime().Date <= dateFin.Date && x.CuentaBancariaId == null && x.MercadoId == null && x.BillId == null && x.FacturaId == null)
                    .Where(x => x.Order_MinoristaId == null)
                    .ToListAsync();

                    foreach (var item in ventasEmpleado)
                    {
                        DateTime? canceledDate = null;
                        if (item.EnvioCaribeId != null)
                            canceledDate = _context.EnvioCaribes.FirstOrDefault(x => x.EnvioCaribeId == item.EnvioCaribeId && x.Status == EnvioCaribe.STATUS_CANCELADA)?.CanceledDate;
                        if (item.EnvioMaritimoId != null)
                            canceledDate = _context.EnvioMaritimo.FirstOrDefault(x => x.Id == item.EnvioMaritimoId && x.Status == EnvioMaritimo.STATUS_CANCELADA)?.CanceledDate;
                        if (item.OrderCubiqId != null)
                            canceledDate = _context.OrderCubiqs.FirstOrDefault(x => x.OrderCubiqId == item.OrderCubiqId && x.Status == OrderCubiq.STATUS_CANCELADA)?.CanceledDate;
                        if (item.OrderId != null)
                            canceledDate = _context.Order.FirstOrDefault(x => x.OrderId == item.OrderId && x.Status == Order.STATUS_CANCELADA)?.CanceledDate;
                        if (item.PassportId != null)
                            canceledDate = _context.Passport.FirstOrDefault(x => x.PassportId == item.PassportId && x.Status == Passport.STATUS_CANCELADA)?.CanceledDate;
                        if (item.RechargueId != null)
                            canceledDate = _context.Rechargue.FirstOrDefault(x => x.RechargueId == item.RechargueId && x.estado == Rechargue.STATUS_CANCELADA)?.CanceledDate;
                        if (item.RemittanceId != null)
                            canceledDate = _context.Remittance.FirstOrDefault(x => x.RemittanceId == item.RemittanceId && x.Status == Remittance.STATUS_CANCELADA)?.CanceledDate;
                        if (item.ServicioId != null)
                            canceledDate = _context.Servicios.FirstOrDefault(x => x.ServicioId == item.ServicioId && x.estado == Servicio.EstadoCancelado)?.CanceledDate;
                        if (item.TicketId != null)
                            canceledDate = _context.Ticket.FirstOrDefault(x => x.TicketId == item.TicketId && x.State == Ticket.STATUS_CANCELADA)?.CanceledDate;

                        if (canceledDate != null && ((DateTime)canceledDate).Date <= dateFin)
                        {
                            ventasEmpleado.Remove(item);
                        }
                    }

                    //Cash 
                    ventastipopago[0] += ventasEmpleado.Where(x => x.Type == "Cash").Sum(x => x.valorPagado);
                    canttipopago[0] += ventasEmpleado.Where(x => x.Type == "Cash").Count();
                    //Zelle
                    ventastipopago[1] += ventasEmpleado.Where(x => x.Type == "Zelle").Sum(x => x.valorPagado);
                    canttipopago[1] += ventasEmpleado.Where(x => x.Type == "Zelle").Count();
                    //Cheque
                    ventastipopago[2] += ventasEmpleado.Where(x => x.Type == "Cheque").Sum(x => x.valorPagado);
                    canttipopago[2] += ventasEmpleado.Where(x => x.Type == "Cheque").Count();
                    //Crédito o Débito
                    ventastipopago[3] += ventasEmpleado.Where(x => x.Type == "Crédito o Débito").Sum(x => x.valorPagado);
                    canttipopago[3] += ventasEmpleado.Where(x => x.Type == "Crédito o Débito").Count();
                    //Transferencia Bancaria
                    ventastipopago[4] += ventasEmpleado.Where(x => x.Type == "Transferencia Bancaria").Sum(x => x.valorPagado);
                    canttipopago[4] += ventasEmpleado.Where(x => x.Type == "Transferencia Bancaria").Count();
                    //Web
                    ventastipopago[5] += ventasEmpleado.Where(x => x.Type == "Web").Sum(x => x.valorPagado);
                    canttipopago[5] += ventasEmpleado.Where(x => x.Type == "Web").Count();
                    //Money Order
                    ventastipopago[6] += ventasEmpleado.Where(x => x.Type == "Money Order").Sum(x => x.valorPagado);
                    canttipopago[6] += ventasEmpleado.Where(x => x.Type == "Money Order").Count();
                    //Crédito de Consumo
                    ventastipopago[7] += ventasEmpleado.Where(x => x.Type == "Crédito de Consumo").Sum(x => x.valorPagado);
                    canttipopago[7] += ventasEmpleado.Where(x => x.Type == "Crédito de Consumo").Count();
                    //Cash App
                    ventastipopago[8] += ventasEmpleado.Where(x => x.Type == "Cash App").Sum(x => x.valorPagado);
                    canttipopago[8] += ventasEmpleado.Where(x => x.Type == "Cash App").Count();


                    List<string> noOrders = new List<string>();
                    //var empleados = _context.User.Where(x => x.AgencyId == aAgency.FirstOrDefault().AgencyId && (x.Type == "Agencia" || x.Type == "Empleado")).Select(x => new { x.UserId, x.Name, x.LastName });
                    foreach (var ventas in ventasEmpleado.GroupBy(x => new { x.UserId, x.User_Name, x.User_LastName }))
                    {

                        decimal totaldeudatbl = 0;
                        decimal totalimportetbl = 0;
                        decimal totalpagadotbl = 0;
                        decimal total_Tpagadotbl = 0;
                        decimal totalcantidadtbl = 0;

                        float[] width = { (float)3, 2, 2, 2, 2, 2 };
                        PdfPTable aux = new PdfPTable(width);
                        aux.WidthPercentage = 100;

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
                        PdfPCell cell6 = new PdfPCell();
                        cell6.Border = 1;

                        doc.Add(new Phrase(ventas.Key.User_Name + " " + ventas.Key.User_LastName, headFont));

                        cell1.AddElement(new Phrase("Tipo", headFont));
                        cell2.AddElement(new Phrase("Cantidad", headFont));
                        cell3.AddElement(new Phrase("Pagado", headFont));
                        cell4.AddElement(new Phrase("T. Pagado", headFont));
                        cell5.AddElement(new Phrase("Total", headFont));
                        cell6.AddElement(new Phrase("Debe", headFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Remesas
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
                        var remesas = ventas.Where(x => x.RemittanceId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.RemittanceId, x.Remesa_Amount, x.Remesa_Date, x.Remesa_Number, x.Remesa_Status, x.Remesa_Pagado });

                        decimal pagado = remesas.Sum(x => x.Sum(y => y.valorPagado));
                        decimal totalPagado = remesas.Sum(x => (decimal)x.Key.Remesa_Pagado);
                        decimal total = remesas.Sum(x => (decimal)x.Key.Remesa_Amount);
                        decimal deuda = total - totalPagado;
                        decimal cantidad = remesas.Count();
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += remesas.Where(x => !noOrders.Contains(x.Key.Remesa_Number)).Sum(x => (decimal)x.Key.Remesa_Amount);
                        totalpagado += pagado;
                        total_Tpagado += remesas.Where(x => !noOrders.Contains(x.Key.Remesa_Number)).Sum(x => (decimal)x.Key.Remesa_Pagado);
                        totalcantidad += remesas.Where(x => !noOrders.Contains(x.Key.Remesa_Number)).Count();

                        noOrders = noOrders.Union(remesas.Select(x => x.Key.Remesa_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Remesa", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envios
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

                        var envios = ventas.Where(x => x.OrderId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.OrderId, x.Order_Amount, x.Order_Date, x.Order_MinoristaId, x.Order_Number, x.Order_Pagado, x.Order_Status, x.Order_Type }).Where(x => x.Key.Order_Type != "Remesas" && x.Key.Order_Type != "Combo");
                        pagado = envios.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = envios.Sum(x => (decimal)x.Key.Order_Pagado);
                        total = envios.Sum(x => (decimal)x.Key.Order_Amount);
                        deuda = total - totalPagado;
                        cantidad = envios.Count();

                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += envios.Where(x => !noOrders.Contains(x.Key.Order_Number)).Sum(x => (decimal)x.Key.Order_Amount);
                        totalpagado += pagado;
                        total_Tpagado += envios.Where(x => !noOrders.Contains(x.Key.Order_Number)).Sum(x => (decimal)x.Key.Order_Pagado);
                        totalcantidad += envios.Where(x => !noOrders.Contains(x.Key.Order_Number)).Count();

                        noOrders = noOrders.Union(envios.Select(x => x.Key.Order_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envíos", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envios Cubiqs
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

                        var enviosCubiq = ventas.Where(x => x.OrderCubiqId != null && x.Type != "Crédito de Consumo" && x.Type != "Cubiq").GroupBy(x => new { x.OrderCubiqId, x.Cubiq_Amount, x.Cubiq_Date, x.Cubiq_Number, x.Cubiq_Pagado, x.Cubiq_Status });
                        pagado = enviosCubiq.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviosCubiq.Sum(x => (decimal)x.Key.Cubiq_Pagado);
                        total = enviosCubiq.Sum(x => (decimal)x.Key.Cubiq_Amount);
                        deuda = total - totalPagado;
                        cantidad = enviosCubiq.Count();

                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Cubiq_Number)).Sum(x => (decimal)x.Key.Cubiq_Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Cubiq_Number)).Sum(x => (decimal)x.Key.Cubiq_Pagado);
                        totalcantidad += enviosCubiq.Where(x => !noOrders.Contains(x.Key.Cubiq_Number)).Count();

                        noOrders = noOrders.Union(enviosCubiq.Select(x => x.Key.Cubiq_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envíos Carga AM", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Combos
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

                        var combos = ventas.Where(x => x.OrderId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.OrderId, x.Order_Amount, x.Order_Date, x.Order_MinoristaId, x.Order_Number, x.Order_Pagado, x.Order_Status, x.Order_Type }).Where(x => x.Key.Order_Type == "Combo");
                        pagado = combos.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = combos.Sum(x => (decimal)x.Key.Order_Pagado);
                        total = combos.Sum(x => (decimal)x.Key.Order_Amount);
                        deuda = total - totalPagado;
                        cantidad = 0;
                        int cantidad2 = 0;

                        foreach (var item in combos.Select(x => x.Key))
                        {
                            var bags = _context.Bag.Include(x => x.BagItems).Where(x => x.OrderId == item.OrderId);
                            foreach (var x in bags)
                            {
                                if (!noOrders.Contains(item.Order_Number))
                                    cantidad2 += x.BagItems.Count();

                                cantidad += x.BagItems.Count();
                            }
                        }
                        totalcantidadtbl += cantidad;
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;

                        grantotal += combos.Where(x => !noOrders.Contains(x.Key.Order_Number)).Sum(x => (decimal)x.Key.Order_Amount);
                        totalpagado += pagado;
                        total_Tpagado += combos.Where(x => !noOrders.Contains(x.Key.Order_Number)).Sum(x => (decimal)x.Key.Order_Pagado);
                        totalcantidad += cantidad2;

                        noOrders = noOrders.Union(combos.Select(x => x.Key.Order_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Combos", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Recarga
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

                        var recarga = ventas.Where(x => x.RechargueId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.RechargueId, x.Recarga_Date, x.Recarga_Import, x.Recarga_Number, x.Recarga_Pagado, x.Recarga_Status });
                        pagado = recarga.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = recarga.Sum(x => (decimal)x.Key.Recarga_Pagado);
                        total = recarga.Sum(x => (decimal)x.Key.Recarga_Import);
                        deuda = total - totalPagado;
                        cantidad = recarga.Count();
                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += recarga.Where(x => !noOrders.Contains(x.Key.Recarga_Number)).Sum(x => (decimal)x.Key.Recarga_Import);
                        totalpagado += pagado;
                        total_Tpagado += recarga.Where(x => !noOrders.Contains(x.Key.Recarga_Number)).Sum(x => (decimal)x.Key.Recarga_Pagado);
                        totalcantidad += recarga.Where(x => !noOrders.Contains(x.Key.Recarga_Number)).Count();

                        noOrders = noOrders.Union(recarga.Select(x => x.Key.Recarga_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Recargas", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Envio Maritimo
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

                        var enviomaritimo = ventas.Where(x => x.EnvioMaritimoId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.EnvioMaritimoId, x.Maritimo_Amount, x.Maritimo_Date, x.Maritimo_Number, x.Maritimo_Pagado, x.Maritimo_Status });
                        pagado = enviomaritimo.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviomaritimo.Sum(x => (decimal)x.Key.Maritimo_Pagado);
                        total = enviomaritimo.Sum(x => (decimal)x.Key.Maritimo_Amount);
                        deuda = total - totalPagado;
                        cantidad = enviomaritimo.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Maritimo_Number)).Sum(x => (decimal)x.Key.Maritimo_Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Maritimo_Number)).Sum(x => (decimal)x.Key.Maritimo_Pagado);
                        totalcantidad += enviomaritimo.Where(x => !noOrders.Contains(x.Key.Maritimo_Number)).Count();

                        noOrders = noOrders.Union(enviomaritimo.Select(x => x.Key.Maritimo_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envío Marítimo", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);
                        //Envio Caribe
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

                        var enviocaribe = ventas.Where(x => x.EnvioCaribeId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.EnvioCaribeId, x.Caribe_Amount, x.Caribe_Date, x.Caribe_Number, x.Caribe_Pagado, x.Caribe_Status });
                        pagado = enviocaribe.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = enviocaribe.Sum(x => (decimal)x.Key.Caribe_Pagado);
                        total = enviocaribe.Sum(x => (decimal)x.Key.Caribe_Amount);
                        deuda = total - totalPagado;
                        cantidad = enviocaribe.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += enviocaribe.Where(x => !noOrders.Contains(x.Key.Caribe_Number)).Sum(x => (decimal)x.Key.Caribe_Amount);
                        totalpagado += pagado;
                        total_Tpagado += enviocaribe.Where(x => !noOrders.Contains(x.Key.Caribe_Number)).Sum(x => (decimal)x.Key.Caribe_Pagado);
                        totalcantidad += enviocaribe.Where(x => !noOrders.Contains(x.Key.Caribe_Number)).Count();

                        noOrders = noOrders.Union(enviocaribe.Select(x => x.Key.Caribe_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Envío Caribe", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);
                        //Pasaporte
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

                        var pasaporte = ventas.Where(x => x.PassportId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.PassportId, x.Passport_Amount, x.Passport_Date, x.Passport_Number, x.Passport_Pagado, x.Passport_Status });
                        pagado = pasaporte.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = pasaporte.Sum(x => (decimal)x.Key.Passport_Pagado);
                        total = pasaporte.Sum(x => (decimal)x.Key.Passport_Amount);
                        deuda = total - totalPagado;
                        cantidad = pasaporte.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += pasaporte.Where(x => !noOrders.Contains(x.Key.Passport_Number)).Sum(x => (decimal)x.Key.Passport_Amount);
                        totalpagado += pagado;
                        total_Tpagado += pasaporte.Where(x => !noOrders.Contains(x.Key.Passport_Number)).Sum(x => (decimal)x.Key.Passport_Pagado);
                        totalcantidad += pasaporte.Where(x => !noOrders.Contains(x.Key.Passport_Number)).Count();

                        noOrders = noOrders.Union(pasaporte.Select(x => x.Key.Passport_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Pasaporte", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));
                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //Otros Servicios
                        var servicios = ventas.Where(x => x.ServicioId != null && x.Type != "Crédito de Consumo").GroupBy(x => new { x.ServicioId, x.TipoServicioId, x.TipoServicio_Nombre, x.Servicio_Amount, x.Servicio_Date, x.Servicio_Number, x.Servicio_Pagado, x.Servicio_Status });
                        foreach (var item in servicios.GroupBy(x => new { x.Key.TipoServicioId, x.Key.TipoServicio_Nombre }))
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

                            pagado = item.Sum(x => x.Sum(y => y.valorPagado));
                            totalPagado = item.Sum(x => (decimal)x.Key.Servicio_Pagado);
                            total = item.Sum(x => (decimal)x.Key.Servicio_Amount);
                            deuda = total - totalPagado;
                            cantidad = item.Count();

                            totaldeudatbl += deuda;
                            totalimportetbl += total;
                            totalpagadotbl += pagado;
                            total_Tpagadotbl += totalPagado;
                            totalcantidadtbl += cantidad;

                            grantotal += item.Where(x => !noOrders.Contains(x.Key.Servicio_Number)).Sum(x => (decimal)x.Key.Servicio_Amount);
                            totalpagado += pagado;
                            total_Tpagado += item.Where(x => !noOrders.Contains(x.Key.Servicio_Number)).Sum(x => (decimal)x.Key.Servicio_Pagado);
                            totalcantidad += item.Where(x => !noOrders.Contains(x.Key.Servicio_Number)).Count();

                            noOrders = noOrders.Union(item.Select(x => x.Key.Servicio_Number).ToList()).ToList();

                            cell1.AddElement(new Phrase(item.Key.TipoServicio_Nombre, normalFont));
                            cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                            cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                            cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                            cell5.AddElement(new Phrase(total.ToString(), normalFont));
                            cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                            aux.AddCell(cell1);
                            aux.AddCell(cell2);
                            aux.AddCell(cell3);
                            aux.AddCell(cell4);
                            aux.AddCell(cell5);
                            aux.AddCell(cell6);
                        }

                        //reserva
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

                        var reserva = ventas.Where(x => x.TicketId != null && !((bool)x.ClientIsCarrier) && x.Type != "Crédito de Consumo").GroupBy(x => new { x.TicketId, x.Ticket_Amount, x.Ticket_Date, x.Ticket_Number, x.Ticket_Pagado, x.Ticket_Status });
                        pagado = reserva.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = reserva.Sum(x => (decimal)x.Key.Ticket_Pagado);
                        total = reserva.Sum(x => (decimal)x.Key.Ticket_Amount);
                        deuda = total - totalPagado;
                        cantidad = reserva.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += reserva.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Sum(x => (decimal)x.Key.Ticket_Amount);
                        totalpagado += pagado;
                        total_Tpagado += reserva.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Sum(x => (decimal)x.Key.Ticket_Pagado);
                        totalcantidad += reserva.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Count();

                        noOrders = noOrders.Union(reserva.Select(x => x.Key.Ticket_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Reserva", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        //reserva Carrier
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

                        var reservaCarrier = ventas.Where(x => x.TicketId != null && (bool)x.ClientIsCarrier && x.Type != "Crédito de Consumo").GroupBy(x => new { x.TicketId, x.Ticket_Amount, x.Ticket_Date, x.Ticket_Number, x.Ticket_Pagado, x.Ticket_Status });
                        pagado = reservaCarrier.Sum(x => x.Sum(y => y.valorPagado));
                        totalPagado = reservaCarrier.Sum(x => (decimal)x.Key.Ticket_Pagado);
                        total = reservaCarrier.Sum(x => (decimal)x.Key.Ticket_Amount);
                        deuda = total - totalPagado;
                        cantidad = reservaCarrier.Count();

                        totaldeudatbl += deuda;
                        totalimportetbl += total;
                        totalpagadotbl += pagado;
                        total_Tpagadotbl += totalPagado;
                        totalcantidadtbl += cantidad;

                        grantotal += reservaCarrier.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Sum(x => (decimal)x.Key.Ticket_Amount);
                        totalpagado += pagado;
                        total_Tpagado += reservaCarrier.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Sum(x => (decimal)x.Key.Ticket_Pagado);
                        totalcantidad += reservaCarrier.Where(x => !noOrders.Contains(x.Key.Ticket_Number)).Count();

                        noOrders = noOrders.Union(reservaCarrier.Select(x => x.Key.Ticket_Number).ToList()).ToList();

                        cell1.AddElement(new Phrase("Reserva Carrier", normalFont));
                        cell2.AddElement(new Phrase(cantidad.ToString(), normalFont));
                        cell3.AddElement(new Phrase(pagado.ToString(), normalFont));
                        cell4.AddElement(new Phrase(totalPagado.ToString(), normalFont));
                        cell5.AddElement(new Phrase(total.ToString(), normalFont));
                        cell6.AddElement(new Phrase(deuda.ToString(), normalFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);


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


                        cell1.AddElement(new Phrase("Totales", headFont));
                        cell2.AddElement(new Phrase(totalcantidadtbl.ToString(), headFont));
                        cell3.AddElement(new Phrase(totalpagadotbl.ToString(), headFont));
                        cell4.AddElement(new Phrase(total_Tpagadotbl.ToString(), headFont));
                        cell5.AddElement(new Phrase(totalimportetbl.ToString(), headFont));
                        cell6.AddElement(new Phrase(totaldeudatbl.ToString(), headFont));

                        aux.AddCell(cell1);
                        aux.AddCell(cell2);
                        aux.AddCell(cell3);
                        aux.AddCell(cell4);
                        aux.AddCell(cell5);
                        aux.AddCell(cell6);

                        // Añado la tabla al documento
                        doc.Add(aux);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        #region // VENTAS POR TIPO DE PAGO CLIENTE

                        doc.Add(new Phrase($"VENTAS POR TIPO DE PAGO - {ventas.Key.User_Name} {ventas.Key.User_LastName}", headFont));
                        float[] columns = { 3, 2, 2 };
                        PdfPTable tablePagoCliente = new PdfPTable(columns);
                        tablePagoCliente.WidthPercentage = 100;
                        PdfPCell cellpago1 = new PdfPCell();
                        cellpago1.Border = 1;
                        PdfPCell cellpago2 = new PdfPCell();
                        cellpago2.Border = 1;
                        PdfPCell cellpago3 = new PdfPCell();
                        cellpago3.Border = 1;

                        cellpago1.AddElement(new Phrase("Tipo de Pago", headFont));
                        cellpago2.AddElement(new Phrase("Cantidad", headFont));
                        cellpago3.AddElement(new Phrase("Importe", headFont));
                        tablePagoCliente.AddCell(cellpago1);
                        tablePagoCliente.AddCell(cellpago2);
                        tablePagoCliente.AddCell(cellpago3);
                        cellpago1 = new PdfPCell();
                        cellpago1.Border = 1;
                        cellpago2 = new PdfPCell();
                        cellpago2.Border = 1;
                        cellpago3 = new PdfPCell();
                        cellpago3.Border = 1;
                        foreach (var item in ventas.GroupBy(x => new { x.tipoPagoId, x.Type }))
                        {
                            cellpago1.AddElement(new Phrase(item.Key.Type, normalFont));
                            cellpago2.AddElement(new Phrase(item.Count().ToString(), normalFont));
                            cellpago3.AddElement(new Phrase(item.Sum(x => x.valorPagado).ToString(), normalFont));
                            tablePagoCliente.AddCell(cellpago1);
                            tablePagoCliente.AddCell(cellpago2);
                            tablePagoCliente.AddCell(cellpago3);
                            cellpago1 = new PdfPCell();
                            cellpago1.Border = 0;
                            cellpago2 = new PdfPCell();
                            cellpago2.Border = 0;
                            cellpago3 = new PdfPCell();
                            cellpago3.Border = 0;
                        }
                        doc.Add(tablePagoCliente);
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);
                        #endregion
                    }

                    #region // VENTAS POR TIPO DE PAGO

                    doc.Add(new Phrase("VENTAS POR TIPO DE PAGO", headFont));
                    float[] columnWidthstipopago = { 3, 2, 2 };
                    PdfPTable tabletipopago = new PdfPTable(columnWidthstipopago);
                    tabletipopago.WidthPercentage = 100;
                    PdfPCell celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 1;
                    PdfPCell celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 1;
                    PdfPCell celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 1;

                    celltipopago1.AddElement(new Phrase("Tipo de Pago", headFont));
                    celltipopago2.AddElement(new Phrase("Cantidad", headFont));
                    celltipopago3.AddElement(new Phrase("Importe", headFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 1;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 1;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 1;

                    celltipopago1.AddElement(new Phrase("Cash", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[0].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[0].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Zelle", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[1].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[1].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Cheque", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[2].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[2].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);
                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Crédito o Débito", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[3].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[3].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Transferencia Bancaria", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[4].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[4].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Web", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[5].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[5].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Money Order", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[6].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[6].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Crédito de Consumo", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[7].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[7].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    celltipopago1 = new PdfPCell();
                    celltipopago1.Border = 0;
                    celltipopago2 = new PdfPCell();
                    celltipopago2.Border = 0;
                    celltipopago3 = new PdfPCell();
                    celltipopago3.Border = 0;

                    celltipopago1.AddElement(new Phrase("Cash App", normalFont));
                    celltipopago2.AddElement(new Phrase(canttipopago[8].ToString(), normalFont));
                    celltipopago3.AddElement(new Phrase(ventastipopago[8].ToString(), normalFont));
                    tabletipopago.AddCell(celltipopago1);
                    tabletipopago.AddCell(celltipopago2);
                    tabletipopago.AddCell(celltipopago3);

                    doc.Add(tabletipopago);
                    #endregion

                    #region //GRAN TOTAL
                    float[] columnwhidts2 = { 5, 5 };
                    PdfPTable tableEnd = new PdfPTable(columnwhidts2);
                    tableEnd.WidthPercentage = 100;
                    PdfPCell cellleft = new PdfPCell();
                    cellleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;
                    doc.Add(Chunk.NEWLINE);

                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(linetotal);
                    Paragraph totalcantidadaux = new Paragraph("Cantidad: ", headFont2);
                    totalcantidadaux.AddSpecial(new Phrase(totalcantidad.ToString(), normalFont2));
                    totalcantidadaux.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(totalcantidadaux);
                    Paragraph porpagar = new Paragraph("Pagado: ", headFont2);
                    porpagar.AddSpecial(new Phrase("$ " + totalpagado.ToString(), normalFont2));
                    porpagar.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(porpagar);
                    Paragraph deudaaux = new Paragraph("Pendiente a pago: ", headFont2);
                    deudaaux.AddSpecial(new Phrase("$ " + (grantotal - total_Tpagado).ToString(), normalFont2));
                    deudaaux.Alignment = Element.ALIGN_RIGHT;
                    cellright.AddElement(deudaaux);

                    #region Facturas Cobradas
                    var facturasCobradas = _context.Facturas
                    .Include(x => x.RegistroPagos).ThenInclude(x => x.tipoPago)
                    .Include(x => x.RegistroPagos).ThenInclude(x => x.User)
                    .Where(x => x.agencyId == agency.AgencyId && x.RegistroPagos.Any(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date));
                    int noFacturas = await facturasCobradas.CountAsync();
                    decimal montoFacturas = await facturasCobradas.SumAsync(x => x.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date).Sum(y => y.valorPagado));
                    if (noFacturas > 0)
                    {
                        Paragraph cobradoFacturas = new Paragraph("Cobrado por contabilidad: ", headFont2);
                        cobradoFacturas.AddSpecial(new Phrase($"{noFacturas} facturas -- ${montoFacturas}", normalFont2));
                        cobradoFacturas.Alignment = Element.ALIGN_RIGHT;
                        cellright.AddElement(cobradoFacturas);
                    }

                    #endregion

                    cellright.AddElement(Chunk.NEWLINE);
                    #endregion
                    tableEnd.AddCell(cellleft);
                    tableEnd.AddCell(cellright);
                    doc.Add(tableEnd);

                    doc.Add(Chunk.NEWLINE);
                    doc.Add(new Phrase("COBROS POR CONTABILIDAD", headFont));
                    float[] widthTable = { 3, 2, 1, 2 };
                    PdfPTable table = new PdfPTable(widthTable);
                    table.WidthPercentage = 100;

                    PdfPCell cell = new PdfPCell(new Phrase("Empleado", headFont));
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase("Factura", headFont));
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase("Monto", headFont));
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase("Tipo Pago", headFont));
                    cell.Border = PdfPCell.BOTTOM_BORDER;
                    table.AddCell(cell);

                    foreach (var factura in facturasCobradas)
                    {
                        foreach (var pago in factura.RegistroPagos.Where(y => y.date.ToLocalTime().Date >= dateIni.Date && y.date.ToLocalTime().Date <= dateFin.Date))
                        {
                            cell = new PdfPCell(new Phrase(pago.User.FullName, normalFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            table.AddCell(cell);
                            cell = new PdfPCell(new Phrase(factura.NoFactura, normalFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            table.AddCell(cell);
                            cell = new PdfPCell(new Phrase(pago.valorPagado.ToString("0.00"), normalFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            table.AddCell(cell);
                            cell = new PdfPCell(new Phrase(pago.tipoPago.Type, normalFont));
                            cell.Border = PdfPCell.BOTTOM_BORDER;
                            table.AddCell(cell);
                        }
                    }
                    doc.Add(table);

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
    }
}
