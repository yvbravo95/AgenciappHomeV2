using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class.ServicioPorCobrar
{
    public class ServicioPorCobrarClass
    {
        private readonly databaseContext _context;
        public ServicioPorCobrarClass(databaseContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportExcel(Agency agency)
        {
            var query = await _context.servicioxCobrar
                .Include(x => x.remitente)
                .Include(x => x.destinatario).ThenInclude(x => x.Address)
                .Include(x => x.minorista)
                .Include(x => x.cliente)
                .Where(x => x.factura == null && x.mayorista == agency && x.cobrado == 0).OrderByDescending(x => x.date).ToListAsync();

            byte[] fileContents = null;

            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Reservas Pasaje");
                AddHeadersExcel(new string[] { "Fecha", "Servicio", "No. Servicio", "Minorista", "Cliente", "Importe", "Remitente", "Destinatario", "Direccion" }, worksheet);
                
                int row = 2;
                foreach (var item in query)
                {
                    int column = 1;
                    worksheet.Cells[row, column++].Value = item.date.ToShortDateString();
                    worksheet.Cells[row, column++].Value = item.tramite;
                    worksheet.Cells[row, column++].Value = item.NoServicio;
                    worksheet.Cells[row, column++].Value = item.minorista?.Name;
                    worksheet.Cells[row, column++].Value = item.cliente?.FullData;
                    worksheet.Cells[row, column++].Value = item.importeACobrar.ToString("0.00");
                    worksheet.Cells[row, column++].Value = item.remitente?.FullData;
                    worksheet.Cells[row, column++].Value = item.destinatario?.FullData;
                    worksheet.Cells[row, column++].Value = item.destinatario?.Address?.getAddressContact;

                    row++;
                }

                fileContents = package.GetAsByteArray();
            }

            return fileContents;
        }

        private void AddHeadersExcel(string[] titles, ExcelWorksheet worksheet)
        {
            for (int i = 0; i < titles.Length; i++)
            {
                worksheet.Cells[1, i+1].Value = titles[i];
                worksheet.Cells[1, i+1].Style.Font.Size = 12;
                worksheet.Cells[1, i+1].Style.Font.Bold = true;
                worksheet.Cells[1, 1+1].Style.Border.Top.Style = ExcelBorderStyle.Hair;
            }
        }
    }
}
