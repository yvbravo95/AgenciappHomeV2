using System;
using System.Collections.Generic;
using Agenciapp.Service.IComboServices.Models;
using AgenciappHome.Models;

namespace Agenciapp.Service.IStoreServices.Models
{
    public class CreateOrderTiendaModel
    {
        public Guid ClientId { get; set; }
        public Guid ContactId { get; set; }
        public Guid UserId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid WholesalerId { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal CostService { get; set; }
        public decimal Credito { get; set; }
        public decimal CustomTaxes  {get; set;}
        public decimal Shipping { get; set; }
        public bool Express { get; set; }
        public string Nota { get; set; }
        public string OrderNumber { get; set; }
        public string OrderNumber2 { get; set; } //Para los tramites que se crear externo a la app tener constancia del numero de orden
        public AuthorizationCard AuthorizationCard { get; set; }
        public List<WarehouseProduct> Products { get; set; }
        public List<Pay> Pays { get; set; }
    }
}