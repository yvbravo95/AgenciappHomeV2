using Agenciapp.Domain.Models.BuildEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration.BuildEmail
{
    public class EmailBodyConfiguration : IEntityTypeConfiguration<EmailBody>
    {
        public void Configure(EntityTypeBuilder<EmailBody> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.EmailAttacheds)
                .WithOne(x => x.EmailBody).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.EmailTemplate)
                .WithMany(x => x.EmailBodies).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
