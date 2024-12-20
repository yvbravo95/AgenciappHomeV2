using Agenciapp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Tasks.Models
{
    public class TaskListItem
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Client_FullName { get; set; }
        public string Client_PhoneNumber { get; set; }
        public Guid Client_Id { get; set; }
        public string Employee_FullName { get; set; }
        public string Employe_ImageProfile { get; set; }
        public DateTime DueDate { get; set; }
        public string Nota { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Subject { get; set; }
        public Agenciapp.Domain.Enums.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public string dateRange { get; set; }
        public bool isFinished { get; set; }
    }
}
