namespace Agenciapp.Service.IAirShippingServices.Models
{
    public class OrderWholesalerResponse
    {
        public string Message { get; set; }
        public string Status { get; set; }
        public ItemsResult Items { get; set; }
    }
}
