using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Settings
    {
        public Settings()
        {
            MayoristasCarga = new List<MayoristaCargaConfig>();
        }
        public string UrlBookingTourAdvisor { get; set; }
        public string SecretKeyTourAdvisor { get; set; }
        public List<MayoristaCargaConfig> MayoristasCarga { get; set; }
        public List<InfoDocument> HAWB_AirportDeparture { get; set; }
        public List<InfoDocument> HAWB_ByFirstCarrier { get; set; }
        public List<InfoDocument> HAWB_AirportOfDestination { get; set; }
        public List<InfoDocument> HAWB_Place { get; set; }

        public bool IsMayoristaCarga(Guid id)
        {
            return MayoristasCarga.Any(x => x.Id == id);
        }
    }

    public class MayoristaCargaConfig
    {
        public Guid Id { get; set; }
        public Guid? CubanacanId { get; set; }
        public Guid? GlobestarId { get; set; }
        public Guid? BluelineId { get; set; }
        public Guid? CargoJetId { get; set; }
    }

    public class InfoDocument
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
