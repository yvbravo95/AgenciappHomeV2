using Agenciapp.Domain.Models;
using Agenciapp.Service.IOrderContainer.Models;
using AgenciappHome.Models;
using iTextSharp.text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.IOrderContainer
{
    public interface IOrderContainerService
    {
        Task<OrderContainer> Create(OrderContainer order);
        Task<OrderContainer> Update(OrderContainer order);
        Task<PaginateOrderContainer> Paginate(User user, int page, int pageSize, string search);
        Task<OrderContainer> GetById(Guid id);
        Task ImportExcel(User user, Guid afiliadoId, IFormFile file);
        Task DistributeOrder(User user, List<Guid> orderIds, Guid distributorId);
        Task<OrderContainer> ChangeStatus(User user, Guid id, string status);
    }

    public class OrderContainerService : IOrderContainerService
    {
        private readonly databaseContext _context;
        public OrderContainerService(databaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create Order Container
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<OrderContainer> Create(OrderContainer order)
        {
            _context.OrderContainers.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }
        
        /// <summary>
        /// Update Order Container
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<OrderContainer> Update(OrderContainer order)
        {
            _context.OrderContainers.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        /// <summary>
        /// Paginate Order Container
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<PaginateOrderContainer> Paginate(User user, int page, int pageSize, string search)
        {
            var query = _context.OrderContainers
                .Where(x => x.AgencyId == user.AgencyId);

            if(user.Type == "PrincipalDistributor")
            {

            }
            else if(user.Type == "DistributorCuba")
            {
                query = query.Where(x => x.DistributorId == user.UserId);
            }
            else if(user.Type == "EmpleadoCuba")
            {
                query = query.Where(x => x.RepartidorId == user.UserId);
            }
            else
            {
                throw new Exception("Tipo de usuario no permitido");
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(x => 
                x.BillNumber.ToLower().Contains(search) || 
                x.ContainerNumber.ToLower().Contains(search) ||
                x.AgencyRef.ToLower().Contains(search) ||
                x.Hbl.ToLower().Contains(search) ||
                x.ContactName.ToLower().Contains(search) ||
                x.ContactAddress.ToLower().Contains(search) ||
                x.ContactPhone.ToLower().Contains(search) ||
                x.ContactProvince.ToLower().Contains(search) ||
                x.ContactMunicipality.ToLower().Contains(search));
            }

            var data = await query
                .Include(x => x.Afiliado)
                .Include(x => x.Distributor)
                .Include(x => x.Repartidor)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var total = await query.CountAsync();

            return new PaginateOrderContainer
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Search = search,
                Data = data
            };
        }
    

        public async Task<OrderContainer> GetById(Guid id)
        {
            return await _context.OrderContainers.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Import excel file
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task ImportExcel(User user, Guid afiliadoId, IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                    var rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        string billNumber = worksheet.Cells[row, 1].Value.ToString().Trim();
                        string containerNumber = worksheet.Cells[row, 2].Value.ToString().Trim();
                        string agencyRef = worksheet.Cells[row, 3].Value.ToString().Trim();
                        int totalHbl = int.Parse(worksheet.Cells[row, 4].Value.ToString().Trim());
                        string hbl = worksheet.Cells[row, 5].Value.ToString().Trim();
                        string contactName = worksheet.Cells[row, 6].Value.ToString().Trim();
                        string contactAddress = worksheet.Cells[row, 7].Value.ToString().Trim();
                        string contactPhone = worksheet.Cells[row, 8].Value.ToString().Trim();
                        string contactProvince = worksheet.Cells[row, 9].Value.ToString().Trim();
                        string contactMunicipality = worksheet.Cells[row, 10].Value.ToString().Trim();
                        decimal weight = decimal.Parse(worksheet.Cells[row, 11].Value.ToString().Trim());

                        var order = new OrderContainer(user, afiliadoId, billNumber, containerNumber, agencyRef, totalHbl, hbl, contactName, contactAddress, contactPhone, contactProvince, contactMunicipality, weight);

                        _context.OrderContainers.Add(order);
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }
    
        public async Task DistributeOrder(User user, List<Guid> orderIds, Guid distributorId)
        {
            var userDistributor = await _context.User.FirstOrDefaultAsync(x => x.UserId == distributorId);
            if(user.AgencyId != userDistributor.AgencyId) throw new Exception("El usuario no pertenece a la misma agencia");
            if (user.Type == "PrincipalDistributor" && userDistributor.Type != "DistributorCuba") throw new Exception("Solo puede distribuir tramites a un usuario Distribuidor");
            if (user.Type == "DistributorCuba" && userDistributor.Type != "EmpleadoCuba") throw new Exception("Solo puede distribuir tramites a un usuario Repartidor");

            var orders = await _context.OrderContainers.Where(x => orderIds.Contains(x.Id)).ToListAsync();

            foreach (var order in orders)
            {
                if(userDistributor.Type == "DistributorCuba")
                {
                    order.SetDistributor(user, distributorId);
                }
                else if(userDistributor.Type == "EmpleadoCuba")
                {
                    order.SetRepartidor(user, distributorId);
                }
                else throw new Exception("Tipo de usuario no permitido");
                _context.Attach(order);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<OrderContainer> ChangeStatus(User user, Guid id, string status)
        {
            var order = await _context.OrderContainers.FirstOrDefaultAsync(x => x.Id == id && x.AgencyId == user.AgencyId);
            if (order == null) throw new Exception("No se encontro el tramite");

            order.ChangeStatus(user, status);
            _context.Attach(order);
            await _context.SaveChangesAsync();

            return order;
        }
    }
}
