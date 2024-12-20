using Agenciapp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class InvoiceNoteConfiguration : IEntityTypeConfiguration<InvoiceNote>
    {
        public void Configure(EntityTypeBuilder<InvoiceNote> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.NoteNumber).IsUnique();
            builder.HasOne(x => x.Agency);
            builder.HasOne(x => x.Client);
            builder.HasOne(x => x.Wholesaler);
            builder.HasOne(x => x.Order);
            builder.Property(x => x.Agency).IsRequired();
        }
    }
}
