using Agenciapp.Domain.Models.BuildEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration.BuildEmail
{
    public class EmailAttachedConfiguration : IEntityTypeConfiguration<EmailAttached>
    {
        public void Configure(EntityTypeBuilder<EmailAttached> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
