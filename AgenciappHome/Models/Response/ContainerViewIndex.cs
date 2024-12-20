using System.Collections.Generic;
using Agenciapp.Domain.Models;

namespace AgenciappHome.Models.Response
{
    public class ContainerViewIndex
    {
        public decimal TotalOrders { get; set; }
        public List<Pallet> Containers { get; set; }
    }
}