using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models.ValueObject;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class OrderCubiqConfiguration : IEntityTypeConfiguration<OrderCubiq>
    {
        public void Configure(EntityTypeBuilder<OrderCubiq> builder)
        {
            builder.OwnsOne(x => x.HandlingAndTransportation);
        }
    }
}
