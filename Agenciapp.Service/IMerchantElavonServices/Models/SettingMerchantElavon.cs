using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IMerchantElavonServices.Models
{
    public interface ISettingMerchantElavon
    {
         string username { get; set; }
         string password { get; set; }
    }
    public class SettingMerchantElavon: ISettingMerchantElavon
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}
