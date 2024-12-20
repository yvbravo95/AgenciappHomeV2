namespace Agenciapp.Common.Models.AirShippingModule
{
    public class ComboListQuery: ListQueryBase
    {
        public DateTime? CreatedAt { get; set; }
        public DateTime? DispatchDate { get; set; }
        public string Number { get; set; }
        public string ClientPhone { get; set; }
        public string ContactProvince { get; set; }
        public string Products { get; set; }
        public string ClientFullData { get; set; }
        public string ContactFullData { get; set; }
        public string Status { get; set; }
        public string DeliverDate { get; set; }
        public string Amount { get; set; }
        public bool SearchPending { get; set; }

    }
}
