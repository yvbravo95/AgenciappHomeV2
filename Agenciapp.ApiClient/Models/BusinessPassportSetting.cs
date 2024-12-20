using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.ApiClient.Models
{
    public class BusinessPassportSetting
    {
        public string BaseUrl { get; set; }
        public string GetPassportByFilter { get; set; }
        public string GetImportadosByFilter { get; set; }
    }
}
