using Agenciapp.Domain.Enums;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class PaqueteConfiguration : IEntityTypeConfiguration<Paquete>
    {
        public void Configure(EntityTypeBuilder<Paquete> builder)
        {
            builder.Property(p => p.Type).IsRequired(true).HasConversion(t => t.ToString(), t => (CubiqPackageType)Enum.Parse(typeof(CubiqPackageType), t));
        }
    }
}
