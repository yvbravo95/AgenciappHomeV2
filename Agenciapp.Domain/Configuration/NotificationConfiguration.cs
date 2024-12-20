using Agenciapp.Domain.Models.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(p => p.Type).IsRequired(true).HasConversion(t => t.ToString(), t => (NotificationType)Enum.Parse(typeof(NotificationType), t));
            builder.Property(p => p.Status).IsRequired(true).HasConversion(t => t.ToString(), t => (NotificationStatus)Enum.Parse(typeof(NotificationStatus), t));
            builder.Property(p => p.Type).IsRequired();
            builder.Property(p => p.Status).IsRequired();
            builder.Property(p => p.Title).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Description).IsRequired().HasMaxLength(150);
            builder.Property(p => p.Employee).IsRequired();
        }
    }
}
