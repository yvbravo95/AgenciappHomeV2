using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.PushNotifications.Models
{
    public class SendPushNotificationResult
    {
        public int Total { get; set; }
        public int Failed { get; set; }
        public int Succeed { get; set; }
    }
}
