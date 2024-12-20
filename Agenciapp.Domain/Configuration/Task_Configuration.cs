using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class Task_Configuration : IEntityTypeConfiguration<Task_>
    {
        public void Configure(EntityTypeBuilder<Task_> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(p => p.Status)
                .HasConversion(t => t.ToString(), t => (Agenciapp.Domain.Enums.TaskStatus)Enum.Parse(typeof(Agenciapp.Domain.Enums.TaskStatus), t));
            builder.Property(p => p.Priority)
                .HasConversion(t => t.ToString(), t => (TaskPriority)Enum.Parse(typeof(TaskPriority), t));
            builder.HasOne(x => x.Client).WithMany(x => x.Tasks).HasForeignKey("ClientId").OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Employee).WithMany(x => x.Tasks).HasForeignKey("EmployeeId").OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Logs).WithOne(x => x.Task).HasForeignKey("TaskId").OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Subject);
        }
    }

    public class TaskLogConfiguration : IEntityTypeConfiguration<TaskLog>
    {
        public void Configure(EntityTypeBuilder<TaskLog> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Agency);
        }
    }
}
