using Agenciapp.Domain.Models.AirTimeModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class AirportConfiguration : IEntityTypeConfiguration<Airport>
    {
        public void Configure(EntityTypeBuilder<Airport> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.iata_airport_code).HasMaxLength(3);
            builder.Property(p => p.iata_country_code).HasMaxLength(3);
        }
    }
}
