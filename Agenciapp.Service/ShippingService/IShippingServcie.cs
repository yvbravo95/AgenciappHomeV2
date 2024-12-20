using Agenciapp.ApiClient.BusinessMain;
using Agenciapp.Common.Headers;
using Agenciapp.Common.Models;
using Agenciapp.Common.Models.Dto;
using Agenciapp.Common.Models.ShippingModule;
using Agenciapp.Service.ShippingService.Models;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shipping = AgenciappHome.Models.Shipping;

namespace Agenciapp.Service.ShippingService
{
    public interface IShippingService
    {
        ListFilterResponse<ShippingDto> GetShippingByFilter(Header header, ShippingListQuery query);
        ShippingDto GetById(Header header, Guid id);
        Task CheckEquipaje(List<CheckEquipajeModel> model, Guid userId);
    }

    public class ShippingService : IShippingService
    {
        private readonly databaseContext _context;
        private readonly ShippingApi _shippingApi;

        public ShippingService(databaseContext context, ShippingApi shippingApi)
        {
            _context = context;
            _shippingApi = shippingApi;
        }

        public ListFilterResponse<ShippingDto> GetShippingByFilter(Header header, ShippingListQuery query)
        {
            var data = _shippingApi.GetShippingByfilter(header, query);
            return data;
        }

        public ShippingDto GetById(Header header, Guid id)
        {
            var shipping = _shippingApi.GetShippingById(header, id);
            return shipping;
        }

        public async Task CheckEquipaje(List<CheckEquipajeModel> model, Guid userId)
        {

            var user = _context.User.Find(userId);

            List<Guid> ordersId = new List<Guid>();
            foreach (var item in model.GroupBy(x => x.Id))
            {
                var shipping = _context.Shipping
                    .Include(x => x.ShippingItem).
                    ThenInclude(x => x.Product).
                    ThenInclude(x => x.BagItem).
                    ThenInclude(x => x.Bag)
                    .FirstOrDefault(x => x.PackingId == item.Key);

                if (shipping == null)
                    throw new Exception("El equipaje no existe.");

                if (!shipping.status.Equals(Shipping.STATUS_ENVIADO) && !shipping.status.Equals(Shipping.STATUS_RECIBIDO))
                    throw new Exception($"El equipaje No.{shipping.Number} no puede ser revisado");

                shipping.status = Shipping.STATUS_RECIBIDO;

                foreach (var itemCheck in item)
                {
                    var shippingItem = shipping.ShippingItem
                        .Where(x => x.Product.BagItem.Bag.Code.Equals(itemCheck.BagNumber));

                    var bag = shippingItem.First().Product.BagItem.Bag;
                    bag.IsComplete = itemCheck.IsChecked;
                    bag.CheckedNote = itemCheck.Description;
                    ordersId.Add((Guid)bag.OrderId);
                }

                Parallel.ForEach(shipping.ShippingItem, shi => shi.Product.BagItem.QtyReceived += (int)shi.Qty);
            }

            await _context.SaveChangesAsync();

            foreach (var item in ordersId.GroupBy(x => x))
            {
                var order = _context.Order
                    .Include(x => x.Bag).ThenInclude(x => x.BagItems)
                    .Include(x => x.RegistroEstados)
                    .First(x => x.OrderId == item.Key);

                order.UpdateStatus(Order.STATUS_RECIBIDA, user);

                _context.Update(order);
            }

            await _context.SaveChangesAsync();
        }
    }
}
