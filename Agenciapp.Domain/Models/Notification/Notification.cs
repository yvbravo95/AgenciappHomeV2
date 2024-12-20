using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.Notification
{
    public class Notification
    {
        protected Notification()
        {

        }
        public Notification(string title, string description, NotificationStatus status, NotificationType type, User employee)
        {
            CreatedAt = DateTime.Now;
            Title = title;
            Description = description;
            Status = status;
            Type = type;
            Employee = employee;
        }

        public void ChangeStatus(NotificationStatus status)
        {
            Status = status;
        }

        [Key] public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public NotificationStatus Status { get; private set; }
        public NotificationType Type { get; private set; }
        public User Employee { get; private set; }

        public string getTimeRemaining { get { return (DateTime.Now - CreatedAt).ToString(@"dd\d\ hh\h\ mm\m\ "); } }
    }

    public enum NotificationStatus
    {
        [Description("unread")]UnRead,
        [Description("read")]Read
    }

    public enum NotificationType
    {
        Information,
        Alert,
        Warning,
        Danger
    }
}
