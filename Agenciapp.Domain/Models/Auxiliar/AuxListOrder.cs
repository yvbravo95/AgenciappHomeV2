using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxListOrder
    {
        private  databaseContext _context;
        public List<ProductoCantidad> productos;
        public Order order { get; set; }
        public List<Wholesaler> wholesalers;
        public List<Wholesaler> wnodespachados;

        public AuxListOrder(Order order, databaseContext context)
        {
            _context = context;
            productos = new List<ProductoCantidad>();
            wholesalers = new List<Wholesaler>();
            wnodespachados = new List<Wholesaler>();
            this.order = order;
            //order.Client.PhoneNumber = this._context.Phone.Where(x => x.ReferenceId == order.ClientId).FirstOrDefault().Number;
            //this.order.Contact.PhoneNumber2 = this._context.Phone.Where(p => p.ReferenceId == order.ContactId && p.Type == "Móvil").First().Number;
            //Añado los productos
            foreach (var bag in order.Bag)
            {
                foreach (BagItem vaux in bag.BagItems)
                {
                    ProductoCantidad pc = new ProductoCantidad();
                    pc.cantidad = vaux.Qty;
                    pc.producto = vaux.Product;
                    productos.Add(pc);
                    if (vaux.Product.Wholesaler != null)
                    {
                        if (!wholesalers.Where(x => x.IdWholesaler == vaux.Product.Wholesaler.IdWholesaler).Any())
                        {
                            wholesalers.Add(vaux.Product.Wholesaler);
                        }
                        if (!wnodespachados.Where(x => x.IdWholesaler == vaux.Product.Wholesaler.IdWholesaler).Any() && vaux.Product.esDespachado == false)
                        {
                            wnodespachados.Add(vaux.Product.Wholesaler);
                        }
                    }
                }
            }
        }

        public bool containMayorista(Guid idmayorista)
        {
            if (wholesalers.Where(x => x.IdWholesaler == idmayorista).Any())
            {
                return true;
            }
            return false;
        }
    }

    public class ProductoCantidad
    {
        public Product producto { get; set; }
        public int cantidad { get; set; }
    }

    public class DespachoCombos
    {
        public ProductoCantidad productocantidad { get; set; }
        public Wholesaler mayorista { get; set; }
        public Order order { get; set; }
    }
}
