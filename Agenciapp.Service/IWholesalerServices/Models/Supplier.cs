using Agenciapp.Service.ReyEnviosStore.Product.Models;
using System;
using System.Collections.Generic;

namespace Agenciapp.Service.IWholesalerServices.Models
{
    public class Supplier
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<CostByProvince> CostByProvinces { get; set; }


        public class CostByProvince
        {
            public Guid Id { get; set; }
            public decimal Cost { get; set; }
            public Province Province { get; set; }
            public TypeCost Type { get; set; }
        }

        public enum TypeCost
        {
            Paquete,
            Medicina,
            Combo
        }
    }
}
