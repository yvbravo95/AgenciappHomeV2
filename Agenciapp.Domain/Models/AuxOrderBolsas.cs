using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgenciappHome.Models
{
    public class AuxOrderBolsas
    {
        private databaseContext _context;
        public string code { get; set; }
        public string qr { get; set; }
        public bool IsComplete { get; set; }
        public string CheckedNote { get; set; }
        public int PhysicalBag { get; set; }

        public List<Item> items;

        public AuxOrderBolsas(Guid id, string code, databaseContext _context)
        {
            this._context = _context;

            this.code = code;
            qr = "/UserFiles/QR/" + code + "jpg";
            items = new List<Item>();
            var bag = _context.Bag
                .Include(x => x.BagItems)
                .First(x => x.BagId.Equals(id));
            PhysicalBag = bag.PhysicalBags;

            IsComplete = bag.IsComplete;
            CheckedNote = bag.CheckedNote;

            var bagItems = bag.BagItems;
            foreach (var v in bagItems.OrderBy(x => x.Order))
            {
                Item item = new Item(v.ProductId, v.Qty, v.Order, this._context);
                items.Add(item);
            }
        }

        public AuxOrderBolsas(Guid id, string code, databaseContext _context, Agency agency)
        {
            this._context = _context;

            this.code = code;
            qr = "/UserFiles/QR/" + code + "jpg";
            items = new List<Item>();
            var bag = _context.Bag
                .Include(x => x.BagItems)
                .First(x => x.BagId.Equals(id));
            PhysicalBag = bag.PhysicalBags;

            IsComplete = bag.IsComplete;
            CheckedNote = bag.CheckedNote;

            var bagItems = bag.BagItems;
            foreach (var v in bagItems.OrderBy(x => x.Order))
            {
                Item item = new Item(v.ProductId, v.Qty, v.Order, this._context, agency);
                items.Add(item);
            }
        }


    }

    public class Item
    {
        public int cantidad;
        public int order;
        public string code;
        public string description;
        public string tipo;
        public string RealName { get; set; }
        public string color;
        public string talla;
        public Guid id;
        public decimal? PriceProductoBodega { get; set; }
        public Item(Guid id, int cantidad, int order, databaseContext _context)
        {
            Product item = _context.Product.Include(x => x.ProductoBodega).FirstOrDefault(x => x.ProductId == id);
            this.id = id;
            this.cantidad = cantidad;
            this.code = item.Code;
            this.description = item.Description;
            this.tipo = item.Tipo;
            this.color = item.Color;
            this.talla = item.TallaMarca;
            this.RealName = item.ProductoBodega?.Nombre;
            this.PriceProductoBodega = item.ProductoBodega?.PrecioVentaReferencial;
            this.order = order;

        }

        public Item(Guid id, int cantidad, int order, databaseContext _context, Agency agency)
        {
            Product item = _context.Product
                .Include(x => x.ProductoBodega)
                .Include(x => x.ProductoBodega.SettingMinoristas).ThenInclude(x => x.Agency)
                .Include(x => x.ProductoBodega.Precio1Minorista)
                .Include(x => x.ProductoBodega.Precio2Minorista)
                .Include(x => x.ProductoBodega.Precio3Minorista)
                .FirstOrDefault(x => x.ProductId == id);

            this.id = id;
            this.cantidad = cantidad;
            this.code = item.Code;
            this.description = item.Description;
            this.tipo = item.Tipo;
            this.color = item.Color;
            this.talla = item.TallaMarca;
            this.RealName = item.ProductoBodega?.Nombre;
            this.PriceProductoBodega = item.ProductoBodega != null ? item.ProductoBodega.GetPrice(agency) : decimal.Zero;
            this.order = order;

        }
    }
}
