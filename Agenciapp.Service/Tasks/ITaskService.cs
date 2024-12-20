using Agenciapp.Common.Models;
using Agenciapp.Common.Services;
using Agenciapp.Common.Services.INotificationServices;
using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using Agenciapp.Service.Tasks.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log = Serilog.Log;
using TaskStatus = Agenciapp.Domain.Enums.TaskStatus;

namespace Agenciapp.Service.Tasks
{
    public interface ITaskService
    {
        Task<List<TaskListItem>> GetList(bool showAll, string status, string priority, string subject, Guid? employee);
        Task<PaginatedList<Task_>> GetListByClient(Guid ClientId, string filters, string sort, int page = 1, int pageSize = 20);
        Task<Result<DetailsTask>> GetDetails(Guid id);
        Task<Result<Task_>> Create(CreateTaskModel model);
        Task<Result<Task_>> Update(UpdateTaskModel model);
        Task<Result<Guid>> Remove(Guid id);
        Task<Result<Guid>> ChangeStatus(Guid id, TaskStatus status);
        Task AlertFinishDueDate();
    }

    public class TaskService : ITaskService
    {
        private readonly databaseContext _context;
        private readonly IUserResolverService _user;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly INotificationService _notification;

        public TaskService(databaseContext context, IUserResolverService user, ISieveProcessor sieveProcessor, INotificationService notification)
        {
            _context = context;
            _user = user;
            _sieveProcessor = sieveProcessor;
            _notification = notification;
        }

        public async Task AlertFinishDueDate()
        {
            try
            {
                var tasks = await _context.Tasks.Include(x => x.Employee)
                    .Where(x => x.Status == TaskStatus.EnCurso).ToListAsync();
                foreach (var task in tasks)
                {
                    var phoneEmploye = await _context.Phone.FirstOrDefaultAsync(x => x.ReferenceId == task.Employee.UserId);
                    Agency agency = await _context.Agency.Include(x => x.EmailAddress).FirstOrDefaultAsync(x => x.AgencyId == task.Employee.AgencyId);
                    if(phoneEmploye != null)
                    {
                        if (task.GetTimeRemaining() < TimeSpan.Zero)
                        {
                            await _notification.Create(new Common.Services.INotificationServices.Models.CreateNotificationModel
                            {
                                Title = $"Notificación de Tarea #{task.Number}",
                                Description = "La fecha de caducidad de la tarea a finalizado. Debe tomar una acción al respecto",
                                Employee = task.Employee,
                                Status = Domain.Models.Notification.NotificationStatus.UnRead,
                                Type = Domain.Models.Notification.NotificationType.Information
                            });

                            var result = await _notification.sendEmail
                                 (
                                     new SendGrid.Helpers.Mail.EmailAddress { Email = agency.EmailAddress?.Email, Name = agency.EmailAddress?.Name },
                                     new SendGrid.Helpers.Mail.EmailAddress { Email = task.Employee.Email, Name = task.Employee.FullName },
                                     $"Notificación de Tarea #{task.Number}",
                                     "La fecha de caducidad de la tarea a finalizado. Debe tomar una acción al respecto",
                                     null,
                                     false
                                 );
                            if (result.IsSuccess)
                            {
                                task.AddTaskLog(new TaskLog($"Notificación. Se ha enviado una notificación al empleado {task.Employee.FullName}. La tarea ha caducado", task));
                            }
                        }
                        _context.Tasks.Attach(task);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                Log.Fatal(e, "Server Error");
            }
        }

        public async Task<Result<Guid>> ChangeStatus(Guid id, TaskStatus status)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    return Result.Failure<Guid>("La tarea no existe");
                }
                task.ChangeStatus(status);
                _context.Attach(task);
                await _context.SaveChangesAsync();

                return Result.Success(task.Id);
            }
            catch(Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<Guid>("No se ha podido cambiar el estado de la tarea");
            }
        }

        public async Task<Result<Task_>> Create(CreateTaskModel model)
        {
            try
            {
                var client = await _context.Client.Include(x => x.Phone).FirstOrDefaultAsync(x => x.ClientId == model.ClientId);
                if (client == null)
                    return Result.Failure<Task_>("El cliente no existe.");
                var employee = await _context.User.FirstOrDefaultAsync(x => x.UserId == model.EmployeeId);
                if (employee == null)
                    return Result.Failure<Task_>("El empleado no existe.");
                var verifyTime = model.DueDate - DateTime.Now;
                if(verifyTime < TimeSpan.Zero)
                {
                    return Result.Failure<Task_>("Debe especificar un tiempo de vencimiento mayor.");
                }

                TaskPriority priority = (TaskPriority)Enum.Parse(typeof(TaskPriority), model.Priority);
                var task = new Task_(client, employee, model.SubjectId, model.Nota, model.DueDate, priority);
                _context.Tasks.Attach(task);
                await _context.SaveChangesAsync();

                //Enviar notificacion al empleado

                await _notification.Create(new Common.Services.INotificationServices.Models.CreateNotificationModel
                {
                    Description = $"La tarea #{task.Number} se le ha asignado",
                    Title = $"Notificación de Tarea #{task.Number}",
                    Employee = employee,
                    Status = Domain.Models.Notification.NotificationStatus.UnRead,
                    Type = Domain.Models.Notification.NotificationType.Information
                });

                var agency = await _context.Agency.Include(x => x.EmailAddress).FirstOrDefaultAsync(x => x.AgencyId == employee.AgencyId);
                await _notification.sendEmail(
                    new SendGrid.Helpers.Mail.EmailAddress { Email = agency.EmailAddress?.Email, Name = agency.EmailAddress?.Name },
                    new SendGrid.Helpers.Mail.EmailAddress { Email = employee.Email, Name = employee.FullName},
                    $"Notificación de Tarea #{task.Number}",
                    $"La tarea #{task.Number} se le ha asignado",
                    null,
                    false
                    );

                return Result.Success(task);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<Task_>("La tarea no ha podido ser creada.");
            }
        }

        public async Task<Result<DetailsTask>> GetDetails(Guid id)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(x => x.Logs)
                    .Include(x => x.Client).ThenInclude(x => x.Phone)
                    .Include(x => x.Employee)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (task == null)
                    return Result.Failure<DetailsTask>("La tarea no existe");

                return Result.Success(new DetailsTask { 
                Client = new DetailsTask.ClientTask { FullName = task.Client.FullData, Id = task.Client.ClientId, PhoneNumber = task.Client.Phone.Number },
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                Employee = new DetailsTask.EmployeeTask { Id = task.Employee.UserId, FullName = task.Employee.FullName},
                Id = task.Id,
                DateRange = $"{task.CreatedAt.ToLocalTime().ToShortDateString()} - {task.DueDate.ToShortDateString()}",
                Nota = task.Nota,
                Number = task.Number,
                Priority = task.Priority.ToString(),
                Status = task.Status.ToString(),
                Subject = task.Subject,
                TaskLogs = task.Logs.Select(y => new DetailsTask.TaskLogItem { Id = y.Id, CreatedAt = y.CreatedAt,Info = y.Info}).ToList(),
                });
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<DetailsTask>("No se ha podido obtener la tarea");
            }
        }

        public async Task<List<TaskListItem>> GetList(bool showAll, string status, string priority, string subjectId, Guid? employee)
        {
            try
            {
                var user = _user.GetUser();
                if (user == null)
                    return PaginatedList<TaskListItem>.Empty();
                var tasks = _context.Tasks
                    .Include(x => x.Employee)
                    .Include(x => x.Client).ThenInclude(x => x.Phone)
                    .Include(x => x.Subject)
                    .Where(x => x.Employee.AgencyId == user.AgencyId && (employee != null? x.Employee.UserId == employee: true))
                    .OrderBy(x => x.GetTimeRemaining())
                    .Select(x => new TaskListItem
                    {
                        Client_FullName = x.Client.FullData,
                        CreatedAt = x.CreatedAt,
                        DueDate = x.DueDate,
                        Employee_FullName = x.Employee.FullName,
                        Id = x.Id,
                        Nota = x.Nota,
                        Priority = x.Priority,
                        Status = x.Status,
                        Subject = x.Subject,
                        dateRange = $"{x.CreatedAt.ToLocalTime().ToShortDateString()} - {x.DueDate.ToShortDateString()}",
                        Number = x.Number,
                        Client_Id = x.Client.ClientId,
                        Client_PhoneNumber = x.Client.Phone.Number,
                        isFinished = x.GetTimeRemaining() < TimeSpan.Zero? true: false,
                        Employe_ImageProfile = x.Employee.ImagenProfile
                    });

                if (status != null)
                {
                    TaskStatus statusEnum = (TaskStatus)Enum.Parse(typeof(TaskStatus), status);
                    tasks = (IOrderedQueryable<TaskListItem>)tasks.Where(x => x.Status == statusEnum);
                }
                else if(!showAll)
                {
                    tasks = (IOrderedQueryable<TaskListItem>)tasks.Where(x => x.Status != TaskStatus.Cancelado && x.Status != TaskStatus.Completado);
                }
                if (priority != null)
                {
                    TaskPriority priorityEnum = (TaskPriority)Enum.Parse(typeof(TaskPriority), priority);
                    tasks = (IOrderedQueryable<TaskListItem>)tasks.Where(x => x.Priority == priorityEnum);
                }
                if (subjectId != null)
                {
                    tasks = (IOrderedQueryable<TaskListItem>)tasks.Where(x => x.Subject == subjectId);
                }
                if (!showAll)
                {
                    tasks.Take(50);
                }
                return await tasks.ToListAsync();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return new List<TaskListItem>();
            }
        }

        public async Task<PaginatedList<Task_>> GetListByClient(Guid ClientId, string filters, string sort, int page = 1, int pageSize = 20)
        {
            try
            {
                var user = _user.GetUser();
                if (user == null)
                    return PaginatedList<Task_>.Empty();
                var tasks = _context.Tasks.Where(x => x.Employee.AgencyId == user.AgencyId && x.Client.ClientId == ClientId);
                var total = await tasks.CountAsync();
                var taskPaged = await _sieveProcessor.Apply(new SieveModel
                {
                    Page = page,
                    PageSize = pageSize,
                    Filters = filters,
                    Sorts = sort
                }, tasks).ToListAsync();

                return new PaginatedList<Task_>(page, pageSize, total, taskPaged);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return PaginatedList<Task_>.Empty();
            }
        }

        public async Task<Result<Guid>> Remove(Guid id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                    return Result.Failure<Guid>("La tarea no existe");

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return Result.Success(id);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<Guid>("La tarea no ha podido ser eliminada");
            }

        }

        public async Task<Result<Task_>> Update(UpdateTaskModel model)
        {
            try
            {
                var task = await _context.Tasks.Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == model.Id);
                var employeeOld = task.Employee;
                if (task == null)
                    return Result.Failure<Task_>("La tarea no existe");
                var client = await _context.Client.Include(x => x.Phone).FirstOrDefaultAsync(x => x.ClientId == model.ClientId);
                if (client == null)
                    return Result.Failure<Task_>("El cliente no existe");
                var employee = await _context.User.FindAsync(model.EmployeeId);
                if(employee == null)
                {
                    return Result.Failure<Task_>("El empleado no existe");
                }
                var verifyTime = model.DueDate - DateTime.Now;
                if (verifyTime < TimeSpan.Zero)
                {
                    return Result.Failure<Task_>("Debe especificar un tiempo de vencimiento mayor.");
                }
                TaskStatus status = (TaskStatus)Enum.Parse(typeof(Domain.Enums.TaskStatus), model.Status);
                TaskPriority priority = (TaskPriority)Enum.Parse(typeof(TaskPriority), model.Priority);
                task.Update(client, employee, model.SubjectId, model.Nota, model.DueDate, priority, status);
                _context.Tasks.Attach(task);
                await _context.SaveChangesAsync();

                if(employeeOld != employee)
                {
                    await _notification.Create(new Common.Services.INotificationServices.Models.CreateNotificationModel
                    {
                        Title = $"Notificación de Tarea #{task.Number}",
                        Description = $"La tarea #{task.Number} se le ha asignado",
                        Employee = employee,
                        Status = Domain.Models.Notification.NotificationStatus.UnRead,
                        Type = Domain.Models.Notification.NotificationType.Information
                    });

                    var agency = await _context.Agency.Include(x => x.EmailAddress).FirstOrDefaultAsync(x => x.AgencyId == employee.AgencyId);
                    await _notification.sendEmail(
                    new SendGrid.Helpers.Mail.EmailAddress { Email = agency.EmailAddress?.Email, Name = agency.EmailAddress?.Name },
                    new SendGrid.Helpers.Mail.EmailAddress { Email = employee.Email, Name = employee.FullName },
                    $"Notificación de Tarea #{task.Number}",
                    $"La tarea #{task.Number} se le ha asignado",
                    null,
                    false
                    );
                }
                else
                {
                    await _notification.Create(new Common.Services.INotificationServices.Models.CreateNotificationModel
                    {
                        Title = $"Notificación de Tarea #{task.Number}",
                        Description = $"La tarea #{task.Number} ha sido actualizada",
                        Employee = employee,
                        Status = Domain.Models.Notification.NotificationStatus.UnRead,
                        Type = Domain.Models.Notification.NotificationType.Information
                    });
                }
                

                return Result.Success(task);
            }
            catch(Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<Task_>("La tarea no ha podido ser actualizada");
            }
        }
    }
}
