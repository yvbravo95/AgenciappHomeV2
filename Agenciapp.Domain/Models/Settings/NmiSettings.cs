namespace RapidMultiservice.Models.Settings
{
    public interface INmiSettings
    {
         string NmiUsername { get; set; }
         string NmiPassword { get; set; }
         string NmiSecurityKey { get; set; }
         string NmiUrl { get; set; }
    }
    
    public class NmiSettings : INmiSettings
    {
        public string NmiUsername { get; set; }
        public string NmiPassword { get; set; }
        public string NmiSecurityKey { get; set; }
        public string NmiUrl { get; set; }
    }
}