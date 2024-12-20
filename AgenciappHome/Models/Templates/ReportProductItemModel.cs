namespace AgenciappHome.Models.Templates
{
    public class ReportProductItemModel
    {
        public int Quantity { get; set; }
        public ProductoBodega Product { get; set; }
        public Wholesaler Provider { get; set; }
    }
}
