namespace AgenciappHome.Models.Templates.OrderContainer
{
    public class CreateOrderContainer
    {
        public string BillNumber { get; set; }
        public string ContainerNumber { get; private set; }
        public string AgencyRef { get; private set; }
        public int TotalHbl { get; private set; }
        public string Hbl { get; private set; }
        public string ContactName { get; private set; }
        public string ContactAddress { get; private set; }
        public string ContactPhone { get; private set; }
        public string ContactProvince { get; private set; }
        public string ContactMunicipality { get; private set; }
        public decimal Weight { get; private set; }
    }
}
