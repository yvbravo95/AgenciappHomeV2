using Agenciapp.Domain.Models.BuildEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration.BuildEmail
{
    public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
    {
        public void Configure(EntityTypeBuilder<EmailTemplate> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.EmailBodies)
                .WithOne(x => x.EmailTemplate).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Agency).
                WithMany()
                .HasForeignKey(x => x.AgencyId);
        }
    }
}
