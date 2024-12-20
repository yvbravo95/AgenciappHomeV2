using Agenciapp.Domain.Models;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class ChequeConfiguration : IEntityTypeConfiguration<Cheque>
    {
        public void Configure(EntityTypeBuilder<Cheque> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.ManifiestoPasaporte)
                .WithMany(x => x.Cheques).HasForeignKey().OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ChequePrimeraVezConfiguration : IEntityTypeConfiguration<ChequePrimeraVez>
    {
        public void Configure(EntityTypeBuilder<ChequePrimeraVez> builder)
        {
            builder.HasOne(x => x.Passport);
        }
    }
    
    public class ChequeOtorgamientoConfiguration : IEntityTypeConfiguration<ChequeOtorgamiento>
    {
        public void Configure(EntityTypeBuilder<ChequeOtorgamiento> builder)
        {
            builder.HasOne(x => x.Passport)
                .WithOne(x => x.ChequeOtorgamiento)
                .HasForeignKey<Passport>(b => b.ChequeOtorgamientoId);
        }
    }
}
