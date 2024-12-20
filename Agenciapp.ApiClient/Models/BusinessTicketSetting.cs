using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.ApiClient.Models
{
    public class BusinessTicketSetting
    {
        public string BaseUrl { get; set; }
        public string GetAutoByFilter { get; set; }
        public string GetHotelByFilter { get; set; }
        public string GetMisOrdenesByFilter { get; set; }
        public string GetPasajeByFilter { get; set; }
    }
}
