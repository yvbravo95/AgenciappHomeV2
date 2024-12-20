using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Services.INotificationServices.Models
{
    public class TwilioSetting
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string PhoneFrom { get; set; }
    }
}
