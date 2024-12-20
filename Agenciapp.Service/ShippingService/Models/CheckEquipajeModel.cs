using System;

namespace Agenciapp.Service.ShippingService.Models
{
    public class CheckEquipajeModel
    {
        public Guid Id { get; set; }
        public bool IsChecked { get; set; }
        public string PackingNumber { get; set; }
        public string BagNumber { get; set; }
        public string Description { get; set; }
    }
}
