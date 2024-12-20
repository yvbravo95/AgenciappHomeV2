using AgenciappHome.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.IReportServices.Reports
{
    public class ReportPendings
    {
        private readonly databaseContext _context;
        private readonly IHostingEnvironment _env;
        public ReportPendings(databaseContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<string> GetResportPendings(DateTime init, DateTime end, Guid agencyId)
        {
            init = init.Date;
            end = end.Date;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                Document doc = new Document();
                PdfWriter pdf = PdfWriter.GetInstance(doc, memoryStream);
                doc.Open();
                try
                {//https://localhost:44375/api/v1/report/pendings/ea2aa866-eb26-450c-800e-ef1e590f6f88?rangeDate=09/14/2022-09/14/2022
                    #region Build Document
                    var agency = await _context.Agency
                        .FindAsync(agencyId);
                    var agencyPhone = _context.Phone.FirstOrDefault(x => x.ReferenceId == agency.AgencyId);
                    var agencyAddress = _context.Address.FirstOrDefault(x => x.ReferenceId == agency.AgencyId);

                    #region Encabezado
                    float[] columnWidths = { 5, 1 };
                    PdfPTable table = new PdfPTable(columnWidths);
                    table.WidthPercentage = 100;

                    PdfPCell cell1 = new PdfPCell();
                    cell1.BorderWidth = 0;

                    PdfPCell cell2 = new PdfPCell();
                    cell2.BorderWidth = 0;

                   /* string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        cell2.AddElement(imagelogo);
                    }*/

                    cell1.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                    cell1.AddElement(new Phrase(agencyAddress?.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                   
                    Phrase telefono = new Phrase("Teléfono: ", headFont);
                    telefono.AddSpecial(new Phrase(agencyPhone?.Number, normalFont));
                    cell1.AddElement(telefono);

                    Phrase fecha = new Phrase("Fecha: ", headFont);
                    fecha.AddSpecial(new Phrase(DateTime.Now.ToShortDateString(), normalFont));
                    cell1.AddElement(fecha);

                    table.AddCell(cell1);
                    table.AddCell(cell2);
                    doc.Add(table);
                    doc.Add(Chunk.NEWLINE);
                    #endregion

                    #region tramites
                    var data = await GetData(init, end, agencyId);
                    foreach (var type in data.GroupBy(x => x.Type))
                    {
                        doc.Add(Chunk.NEWLINE);
                        AddTable(doc, type);
                        doc.Add(Chunk.NEWLINE);
                    }
                    #endregion

                    #region totales
                    doc.Add(Chunk.NEWLINE);
                    doc.Add(Chunk.NEWLINE);

                    Paragraph linetotal = new Paragraph("______________________", normalFont);
                    linetotal.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(linetotal);

                    Paragraph total = new Paragraph("Total: ", headFont2);
                    total.AddSpecial(new Phrase($"$ {data.Sum(x => x.Total)}", normalFont2));
                    total.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(total);

                    Paragraph payed = new Paragraph("Pagado: ", headFont2);
                    payed.AddSpecial(new Phrase($"$ {data.Sum(x => x.Payed)}", normalFont2));
                    payed.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(payed);

                    Paragraph balance = new Paragraph("Balance: ", headFont2);
                    balance.AddSpecial(new Phrase($"$ {data.Sum(x => x.Debit)}", normalFont2));
                    balance.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(balance);
                    #endregion

                    #endregion
                    doc.Close();
                }
                catch (Exception e)
                {
                    doc.Close();
                    pdf.Close();
                    throw;
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        private void AddTable(Document doc, IGrouping<string, OrderData> data)
        {
            doc.Add(new Phrase($"TRAMITE {data.Key.ToUpper()}" , headFont));

            float[] columnWidths = { (float)1.5, 2, 2, 1, 1, 1, 1 };
            PdfPTable tbl = new PdfPTable(columnWidths);
            tbl.WidthPercentage = 100;

            tbl.AddCell(new PdfPCell(new Phrase("Fecha", headFont)));
            tbl.AddCell(new PdfPCell(new Phrase("Numero", headFont)));
            tbl.AddCell(new PdfPCell(new Phrase("Cliente", headFont)));
            tbl.AddCell(new PdfPCell(new Phrase("Empleado", headFont)));
            tbl.AddCell(new PdfPCell(new Phrase("Total", headFont)));
            tbl.AddCell(new PdfPCell(new Phrase("Pagado", headFont)));
            tbl.AddCell(new PdfPCell(new Phrase("Balance", headFont)));

            foreach (var item in data)
            {
                tbl.AddCell(new PdfPCell(new Phrase(item.CreatedAt.ToShortDateString(), normalFont)));
                tbl.AddCell(new PdfPCell(new Phrase(item.Number, normalFont)));
                tbl.AddCell(new PdfPCell(new Phrase(item.Client, normalFont)));
                tbl.AddCell(new PdfPCell(new Phrase(item.Employee, normalFont)));
                tbl.AddCell(new PdfPCell(new Phrase(item.Total.ToString("0.00"), normalFont)));
                tbl.AddCell(new PdfPCell(new Phrase(item.Payed.ToString("0.00"), normalFont)));
                tbl.AddCell(new PdfPCell(new Phrase(item.Debit.ToString("0.00"), normalFont)));
            }

            tbl.AddCell(new PdfPCell(new Phrase("Totales", headFont)));
            tbl.AddCell(new PdfPCell(new Phrase(string.Empty, headFont)));
            tbl.AddCell(new PdfPCell(new Phrase(string.Empty, headFont)));
            tbl.AddCell(new PdfPCell(new Phrase(string.Empty, headFont)));
            tbl.AddCell(new PdfPCell(new Phrase(data.Sum(x => x.Total).ToString("0.00"), headFont)));
            tbl.AddCell(new PdfPCell(new Phrase(data.Sum(x => x.Payed).ToString("0.00"), headFont)));
            tbl.AddCell(new PdfPCell(new Phrase(data.Sum(x => x.Debit).ToString("0.00"), headFont)));

            doc.Add(tbl);
        }

        private async Task<List<OrderData>> GetData(DateTime init, DateTime end, Guid agencyId)
        {
            List<OrderData> response = new List<OrderData>();
            end = end.AddDays(1);

            var remittance = await _context.Remittance
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId)
                .Where(x => x.Status != Remittance.STATUS_CANCELADA)
                .Where(x => x.Date >= init && x.Date < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Remesa",
                    Id = x.RemittanceId,
                    CreatedAt = x.Date,
                    Number = x.Number,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Amount,
                    Payed = x.TotalPagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(remittance);

            var turisticPackage = await _context.PaquetesTuristicos
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId)
                .Where(x => x.Status != PaqueteTuristico.STATUS_CANCELADA)
                .Where(x => x.Date >= init && x.Date < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Paquete Turistico",
                    Id = x.PaqueteId,
                    CreatedAt = x.Date,
                    Number = x.Number,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Amount,
                    Payed = x.ValorPagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(turisticPackage);

            var maritime = await _context.EnvioMaritimo
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId)
                .Where(x => x.Status != EnvioMaritimo.STATUS_CANCELADA)
                .Where(x => x.Date >= init && x.Date < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Envio Maritimo",
                    Id = x.Id,
                    CreatedAt = x.Date,
                    Number = x.Number,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Amount,
                    Payed = x.ValorPagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(maritime);

            var caribbenShipping = await _context.EnvioCaribes
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId)
                .Where(x => x.Status != EnvioCaribe.STATUS_CANCELADA)
                .Where(x => x.Date >= init && x.Date < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Envio Caribe",
                    Id = x.EnvioCaribeId,
                    CreatedAt = x.Date,
                    Number = x.Number,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Amount,
                    Payed = x.ValorPagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(caribbenShipping);

            var cubiq = await _context.OrderCubiqs
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId)
                .Where(x => x.Status != OrderCubiq.STATUS_CANCELADA)
                .Where(x => x.Date >= init && x.Date < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Carga AM",
                    Id = x.OrderCubiqId,
                    CreatedAt = x.Date,
                    Number = x.Number,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Amount,
                    Payed = x.ValorPagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(cubiq);

            var passport = await _context.Passport
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId)
                .Where(x => x.Status != Passport.STATUS_CANCELADA)
                .Where(x => x.FechaSolicitud >= init && x.FechaSolicitud < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Pasaporte",
                    Id = x.PassportId,
                    CreatedAt = x.FechaSolicitud,
                    Number = x.OrderNumber,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Total,
                    Payed = x.Pagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(passport);

            var airShipping = await _context.Order
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId && x.Type != "Remesas" && x.Type != "Combo")
                .Where(x => x.Status != Order.STATUS_CANCELADA)
                .Where(x => x.Date >= init && x.Date < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Envio Paquete",
                    Id = x.OrderId,
                    CreatedAt = x.Date,
                    Number = x.Number,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Amount,
                    Payed = x.ValorPagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(airShipping);

            var combos = await _context.Order
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId && x.Type == "Combo")
                .Where(x => x.Status != Order.STATUS_CANCELADA)
                .Where(x => x.Date >= init && x.Date < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Combo",
                    Id = x.OrderId,
                    CreatedAt = x.Date,
                    Number = x.Number,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Amount,
                    Payed = x.ValorPagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(combos);

            var tickets = await _context.Ticket
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId && x.PaqueteTuristicoId == null)
                .Where(x => x.State != Ticket.STATUS_CANCELADA)
                .Where(x => x.RegisterDate >= init && x.RegisterDate < end && x.Debit > 0)
                .Select(x => new OrderData
                {
                    Type = $"Reserva",
                    Id = x.TicketId,
                    CreatedAt = x.RegisterDate,
                    Number = x.ReservationNumber,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Total,
                    Payed = x.Payment,
                    Debit = x.Debit
                }).ToListAsync();

            response.AddRange(tickets);

            var rechargue = await _context.Rechargue
                .Include(x => x.Client)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId)
                .Where(x => x.estado != Rechargue.STATUS_CANCELADA)
                .Where(x => x.date >= init && x.date < end && x.Balance > 0)
                .Select(x => new OrderData
                {
                    Type = "Recarga",
                    Id = x.RechargueId,
                    CreatedAt = x.date,
                    Number = x.Number,
                    Client = x.Client.FullData,
                    Employee = x.User.FullName,
                    Total = x.Import,
                    Payed = x.valorPagado,
                    Debit = x.Balance
                }).ToListAsync();

            response.AddRange(rechargue);

            end = end.ToUniversalTime();
            var services = await _context.Servicios
                .Include(x => x.cliente)
                .Include(x => x.User)
                .Where(x => x.AgencyId == agencyId && x.PaqueteTuristicoId == null)
                .Where(x => x.estado != Rechargue.STATUS_CANCELADA)
                .Where(x => x.fecha >= init && x.fecha < end && x.debe > 0)
                .Select(x => new OrderData
                {
                    Type = x.tipoServicio.Nombre,
                    Id = x.ServicioId,
                    CreatedAt = x.fecha,
                    Number = x.numero,
                    Client = x.cliente.FullData,
                    Employee = x.User.FullName,
                    Total = x.importeTotal,
                    Payed = x.importePagado,
                    Debit = x.debe
                }).ToListAsync();

            response.AddRange(services);

            return response;
        }

        private class OrderData
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Type { get; set; }
            public string Number { get; set; }
            public string Client { get; set; }
            public string Employee { get; set; }
            public decimal Total { get; set; }
            public decimal Payed { get; set; }
            public decimal Debit { get; set; }
        }

        iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
        iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        iTextSharp.text.Font headRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
        iTextSharp.text.Font headFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        iTextSharp.text.Font normalRedFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
        iTextSharp.text.Font fonttransferida = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 7, iTextSharp.text.Font.ITALIC, BaseColor.RED);
        iTextSharp.text.Font normalFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
    }
}
