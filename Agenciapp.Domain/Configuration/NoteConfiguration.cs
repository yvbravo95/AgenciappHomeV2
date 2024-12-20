using Agenciapp.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Agenciapp.Domain.Configuration
{
    public class NoteConfiguration : IEntityTypeConfiguration<Note>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Note> builder)
        {
            builder.HasKey(x => x.id);
            builder.HasOne(x => x.CreatedByUser);
            builder.HasOne(x => x.Client).WithMany(x => x.Notes).HasForeignKey();
        }
    }
}