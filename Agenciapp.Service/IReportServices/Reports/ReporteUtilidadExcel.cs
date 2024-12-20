using Agenciapp.Service.IReportServices;
using Agenciapp.Service.IReportServices.Models;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.IReportServices.Reports
{
    public static partial class Reporte
    {
        public async static Task<byte[]> GetReporteUtilidadExcel(string strdate, User aUser, databaseContext _context, IReportService _reportService, bool onlyClientsAgency = false)
        {
            var aAgency = await _context.Agency.FirstOrDefaultAsync(x => x.AgencyId == aUser.AgencyId);

            var auxDate = strdate.Split('-');
            CultureInfo culture = new CultureInfo("es-US", true);
            var dateIni = DateTime.Parse(auxDate[0], culture);
            var dateFin = DateTime.Parse(auxDate[1], culture);

            var data = (await _reportService.GetAllUtility(aAgency.AgencyId, dateIni, dateFin)).Value;
            if (data.Count() == 0)
                return null;

            var byService = data.GroupBy(x => x.Service);

            byte[] fileContents = null;
            using (var package = new ExcelPackage())
            {
                foreach (var service in byService)
                {
                    string serviceName = "";
                    switch (service.Key)
                    {
                        case STipo.Remesa:
                            serviceName = "Remesas";
                            break;
                        case STipo.PTuristico:
                            serviceName = "Paquete Turistico";
                            break;
                        case STipo.Recarga:
                            serviceName = "Recargas";
                            break;
                        case STipo.Servicio:
                            serviceName = "Otros Servicios";
                            break;
                        case STipo.Passport:
                            serviceName = "Pasaporte";
                            break;
                        case STipo.EnvioCaribe:
                            serviceName = "Envíos Caribe";
                            break;
                        case STipo.Paquete:
                            serviceName = "Evíos Aéreos";
                            break;
                        case STipo.Maritimo:
                            serviceName = "Envíos Marítimos";
                            break;
                        case STipo.Reserva:
                            serviceName = "Reservas";
                            break;
                        case STipo.Combo:
                            serviceName = "Combos";
                            break;
                        default:
                            break;
                    }

                    if (service.Key == STipo.Servicio)
                    {
                        var otherServices = service.GroupBy(x => x.TipoServicio);
                        foreach (var otherService in otherServices)
                        {
                            AuxUtilityExcel(otherService.ToList(), otherService.Key, package);
                        }
                    }
                    else if (service.Key == STipo.Reserva)
                    {
                        var carriers = service.Where(x => x.IsCarrier).ToList();
                        if (carriers.Any())
                        {
                            AuxUtilityExcel(carriers.ToList(), "Reserva - Carrier", package);
                        }
                        var pasaje = service.Where(x => !x.IsCarrier && x.TipoServicio == "pasaje").ToList();
                        var auto = service.Where(x => !x.IsCarrier && x.TipoServicio == "auto").ToList();
                        var hotel = service.Where(x => !x.IsCarrier && x.TipoServicio == "hotel").ToList();
                        if (pasaje.Any())
                        {
                            AuxUtilityExcel(pasaje.ToList(), "Reserva - Pasaje", package);
                        }
                        if (auto.Any())
                        {
                            AuxUtilityExcel(auto.ToList(), "Reserva - Auto", package);
                        }
                        if (hotel.Any())
                        {
                            AuxUtilityExcel(hotel.ToList(), "Reserva - Hotel", package);
                        }
                    }
                    else if (service.Key != STipo.Servicio)
                    {
                        if (service.Key == STipo.Passport && onlyClientsAgency)
                            AuxUtilityExcel(service.Where(x => !x.ByTransferencia).ToList(), serviceName, package);
                        else
                            AuxUtilityExcel(service.ToList(), serviceName, package);
                    }
                }
                fileContents = package.GetAsByteArray();
            }
            return fileContents;
        }

        private static void AuxUtilityExcel(List<UtilityModel> items, string serviceName, ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add(serviceName);

            //Añado los encabezados
            worksheet.Cells[1, 1].Value = "No. Orden";
            worksheet.Cells[1, 1].Style.Font.Size = 12;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 2].Value = "Fecha";
            worksheet.Cells[1, 2].Style.Font.Size = 12;
            worksheet.Cells[1, 2].Style.Font.Bold = true;
            worksheet.Cells[1, 2].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 3].Value = "Cliente";
            worksheet.Cells[1, 3].Style.Font.Size = 12;
            worksheet.Cells[1, 3].Style.Font.Bold = true;
            worksheet.Cells[1, 3].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 4].Value = "Empleado";
            worksheet.Cells[1, 4].Style.Font.Size = 12;
            worksheet.Cells[1, 4].Style.Font.Bold = true;
            worksheet.Cells[1, 4].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 5].Value = "Precio Venta";
            worksheet.Cells[1, 5].Style.Font.Size = 12;
            worksheet.Cells[1, 5].Style.Font.Bold = true;
            worksheet.Cells[1, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 6].Value = "Costo";
            worksheet.Cells[1, 6].Style.Font.Size = 12;
            worksheet.Cells[1, 6].Style.Font.Bold = true;
            worksheet.Cells[1, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 7].Value = "Utilidad";
            worksheet.Cells[1, 7].Style.Font.Size = 12;
            worksheet.Cells[1, 7].Style.Font.Bold = true;
            worksheet.Cells[1, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 8].Value = "Transferida De";
            worksheet.Cells[1, 8].Style.Font.Size = 12;
            worksheet.Cells[1, 8].Style.Font.Bold = true;
            worksheet.Cells[1, 8].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            worksheet.Cells[1, 9].Value = "Tipo Pagos";
            worksheet.Cells[1, 9].Style.Font.Size = 12;
            worksheet.Cells[1, 9].Style.Font.Bold = true;
            worksheet.Cells[1, 9].Style.Border.Top.Style = ExcelBorderStyle.Hair;

            var col = 2;
            foreach (var item in items)
            {
                worksheet.Cells[col, 1].Value = item.OrderNumber;
                worksheet.Cells[col, 2].Value = item.Date.ToShortDateString();
                worksheet.Cells[col, 3].Value = item.Client.FullName;
                worksheet.Cells[col, 4].Value = item.Employee.FullName;
                worksheet.Cells[col, 5].Value = item.SalePrice;
                worksheet.Cells[col, 6].Value = item.Cost;
                worksheet.Cells[col, 7].Value = item.Utility;
                worksheet.Cells[col, 8].Value = item.TransferredAgencyName;
                string pays = "";
                foreach (var pay in item.Pays)
                {
                    pays += $"{pay.TipoPago}, ";
                }
                worksheet.Cells[col, 9].Value = pays;

                col++;
            }
            worksheet.Cells[col, 1].Value = "Total";
            worksheet.Cells[col, 2].Value = "";
            worksheet.Cells[col, 3].Value = "";
            worksheet.Cells[col, 4].Value = "";
            worksheet.Cells[col, 5].Value = items.Sum(x => x.SalePrice);
            worksheet.Cells[col, 6].Value = items.Sum(x => x.Cost);
            worksheet.Cells[col, 7].Value = items.Sum(x => x.Utility);
            worksheet.Cells[col, 8].Value = "";

            worksheet.Cells[col, 1].Style.Font.Size = 12;
            worksheet.Cells[col, 1].Style.Font.Bold = true;
            worksheet.Cells[col, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;
            worksheet.Cells[col, 5].Style.Font.Size = 12;
            worksheet.Cells[col, 5].Style.Font.Bold = true;
            worksheet.Cells[col, 5].Style.Border.Top.Style = ExcelBorderStyle.Hair;
            worksheet.Cells[col, 6].Style.Font.Size = 12;
            worksheet.Cells[col, 6].Style.Font.Bold = true;
            worksheet.Cells[col, 6].Style.Border.Top.Style = ExcelBorderStyle.Hair;
            worksheet.Cells[col, 7].Style.Font.Size = 12;
            worksheet.Cells[col, 7].Style.Font.Bold = true;
            worksheet.Cells[col, 7].Style.Border.Top.Style = ExcelBorderStyle.Hair;
        }

    }
}
