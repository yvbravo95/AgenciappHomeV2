using Agenciapp.Domain.Enums;
using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IComboServices.Models
{
    public class CreateComboModel
    {
        public Guid ClientId { get; set; }
        public Guid ContactId { get; set; }
        public Guid UserId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? OfficeId { get; set; }
        public decimal Amount { get; set; }
        public decimal ProductsPrice { get; set; }
        public decimal AddCosto { get; set; }
        public decimal AddPrecio { get; set; }
        public decimal CostService { get; set; }
        public decimal Credito { get; set; }
        public decimal Shipping { get; set; }
        public bool Express { get; set; }
        public StoreType StoreType { get; set; } = StoreType.Agenciapp;
        public bool Editable { get; set; } = true;
        public string Nota { get; set; }
        public string OrderNumber { get; set; }
        public string OrderNumber2 { get; set; } //Para los tramites que se crear externo a la app tener constancia del numero de orden
        public AuthorizationCard AuthorizationCard { get; set; }
        public List<WarehouseProduct> Products { get; set; }
        public List<Pay> Pays { get; set; }
    }

    public class Pay
    {
        public Guid TipoPago { get; set; }
        public decimal ValorPagado { get; set; }
    }

    public class WarehouseProduct
    {
        public Guid Id { get; set; }
        public int Qty { get; set; }
    }
}
