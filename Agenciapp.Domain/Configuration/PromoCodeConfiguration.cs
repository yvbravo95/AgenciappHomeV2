using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
    {
        public void Configure(EntityTypeBuilder<PromoCode> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasOne(p => p.CreatedBy);
            builder.HasOne(p => p.Agency);
            builder.HasKey(p => p.Code);
            builder.Property(p => p.Code).IsRequired(true);
            builder.Property(p => p.Value).IsRequired(true);
            builder.Property(p => p.DateInit).IsRequired(true);
            builder.Property(p => p.DateEnd).IsRequired(true);
            builder.Property(p => p.OrderType).IsRequired(true).HasConversion(t => t.ToString(), t => (OrderType)Enum.Parse(typeof(OrderType), t)); 
            builder.Property(p => p.PromoType).IsRequired(true).HasConversion(t => t.ToString(), t => (RateType)Enum.Parse(typeof(RateType), t));
            builder.Property(p => p.Agency).IsRequired(true);
            builder.Property(p => p.CreatedBy).IsRequired(true);
            builder.Property(p => p.isActive).HasDefaultValue(true);
        }
    }
}
