using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class TopClient
    {
        private static databaseContext _context = new databaseContext();

        public Guid id;
        public int cantidad;
        public decimal precio = 0;

        public TopClient(Guid id, int cantidad, decimal precioTotal)
        {
            this.id = id;
            this.cantidad = cantidad;
            this.precio = precioTotal;
        }

        public Client getClient()
        {
            if (id != null)
            {
                Client client = _context.Client.Find(this.id);
                return client;
            }
            return null;
        }
    }
}
