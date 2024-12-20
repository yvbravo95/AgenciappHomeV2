namespace Agenciapp.Service.IAirShippingServices.Models
{
    public class ParamsWholesaler
    {
        public string UserPhone { get; set; }
        public string Note { get; set; }
        public Extras Extras { get; set; }
        public int ShippingType { get; set; }
        public UserShipping UserShipping { get; set; }
        public List<ProductWholesaler> Products { get; set; }
    }
}
