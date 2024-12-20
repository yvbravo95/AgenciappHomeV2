using Agenciapp.Domain.Models.Notification;
using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Common.Services.INotificationServices.Models
{
    public class CreateNotificationModel
    {
        [Required] public string Title { get; set; }
        [Required] public string Description { get; set; }
        [Required] public NotificationStatus Status { get; set; }
        [Required] public NotificationType Type { get; set; }
        [Required] public User Employee { get; set; }
    }
}
