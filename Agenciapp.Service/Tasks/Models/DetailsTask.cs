using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.Tasks.Models
{
    public class DetailsTask
    {
        public Guid Id { get; set; }
        public string Nota { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Number { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public string DateRange { get; set; }
        public ClientTask Client { get; set; }
        public EmployeeTask Employee { get; set; }
        public string Subject { get; set; }
        public List<TaskLogItem> TaskLogs = new List<TaskLogItem>();

        public class ClientTask
        {
            public Guid Id { get; set; }
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class EmployeeTask
        {
            public Guid Id { get; set; }
            public string FullName { get; set; }
        }

        public class TaskLogItem
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Info { get; set; }
        }
    }

    
}
