using System;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agenciapp.Domain.Configuration
{
    public class MinoristaConfiguration : IEntityTypeConfiguration<Minorista>
    {
        public void Configure(EntityTypeBuilder<Minorista> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Type).IsRequired(true).HasConversion(t => t.ToString(), t => (STipo)Enum.Parse(typeof(STipo), t)); 
        }
    }
}