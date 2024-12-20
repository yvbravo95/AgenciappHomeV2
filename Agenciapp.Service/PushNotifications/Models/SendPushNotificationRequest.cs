using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.PushNotifications.Models
{
    public class SendPushNotificationRequest
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Tokens { get; set; }
    }
}
