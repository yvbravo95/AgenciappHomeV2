namespace Agenciapp.Service.IReportServices.Models
{
    public class DataBoxDCubaModel
    {
        public decimal UtilityProrrogas { get; set; }
        public decimal UtilityRenovar {get; set;}
        public decimal UtilityPrimerVez { get; set; }
        public int ProrrogasThisMonth { get; set; }
        public int ProrrogasDiffLastMonth { get; set; }
        public int RenovarThisMonth { get; set; }
        public int RenovarDiffLastMonth { get; set; }
        public int PrimerVezThisMonth { get; set; }
        public int PrimerVezDiffLastMonth { get; set; }
        public int ClientsThisMonth { get; set; }
        public int ClientsTotal { get; set; }
    }
}