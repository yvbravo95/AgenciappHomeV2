using Agenciapp.Domain.Enums;
using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Agenciapp.Domain.Models
{
    public class Task_
    {
        protected Task_()
        {

        }
        public Task_(Client client, User employee, string subject, string nota, DateTime dueDate, TaskPriority priority)
        {
            Status = Agenciapp.Domain.Enums.TaskStatus.EnCurso;
            Number = $"T{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            CreatedAt = DateTime.UtcNow;
            Client = client;
            Employee = employee;
            Nota = nota;
            DueDate = dueDate.Date.AddHours(9);
            Priority = priority;
            Subject = subject;        
        }

        public void Update(Client client, User employee, string subject,  string nota, DateTime dueDate, TaskPriority priority, Agenciapp.Domain.Enums.TaskStatus status)
        {
            Client = client;
            Employee = employee;
            Nota = nota;
            DueDate = dueDate.Date;
            Subject = subject;
            Priority = priority;
            Status = status;
            AddTaskLog(new TaskLog("La tarea fue actualizada", this));
        }

        public TimeSpan GetTimeRemaining()
        {
            return DueDate - DateTime.Now;
        }

        public void AddTaskLog(TaskLog log)
        {
            _taskLogs.Add(log);
        }

        public void RemoveTaskLog(TaskLog log)
        {
            _taskLogs.Remove(log);
        }

        public void ChangeStatus(Agenciapp.Domain.Enums.TaskStatus status)
        {
            Status = status;
            var log = new TaskLog($"Se modificó el estado a {status}", this);
            AddTaskLog(log);
        }

        [Key] public Guid Id { get; private set; }
        public string Nota { get; private set; }
        public Agenciapp.Domain.Enums.TaskStatus Status { get; private set; }
        public TaskPriority Priority { get; private set; }
        public string Number { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime DueDate { get; private set; }

        public Client Client { get; private set; }
        public User Employee { get; private set; }
        public string Subject { get; set; }
        private readonly List<TaskLog> _taskLogs = new List<TaskLog>();
        public virtual IReadOnlyList<TaskLog> Logs => _taskLogs.ToList();
    }

    public class TaskLog
    {
        protected TaskLog()
        {

        }
        public TaskLog(string info, Task_ task)
        {
            CreatedAt = DateTime.UtcNow;
            Info = info;
            Task = task;
        }
        [Key] public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string Info { get; private set; }
        public Task_ Task { get; private set; }
    }

    public class Subject
    {
        [Key]public Guid Id { get; set; }
        public string subject { get; set; }
        public Agency Agency { get; set; }
    }

}
