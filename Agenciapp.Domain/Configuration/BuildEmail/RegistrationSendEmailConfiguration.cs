using Agenciapp.Domain.Models.BuildEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration.BuildEmail
{
    public class RegistrationSendEmailConfiguration : IEntityTypeConfiguration<RegistrationSendEmail>
    {
        public void Configure(EntityTypeBuilder<RegistrationSendEmail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.EmailBody);
            builder.HasOne(x => x.Client)
                .WithMany(x => x.RegistrationSendEmail).HasForeignKey();
        }
    }
}
