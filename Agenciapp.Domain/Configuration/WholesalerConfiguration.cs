using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class WholesalerConfiguration : IEntityTypeConfiguration<Wholesaler>
    {
        public void Configure(EntityTypeBuilder<Wholesaler> builder)
        {
            builder.Property(e => e.PriceTypePackage).HasColumnType("decimal(18,10)");
            builder.Property(e => e.CostTypePackage).HasColumnType("decimal(18,10)");
        }
    }
}
