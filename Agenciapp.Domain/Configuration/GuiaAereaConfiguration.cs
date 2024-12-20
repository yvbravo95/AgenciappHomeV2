using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;

namespace Agenciapp.Domain.Configuration
{
    public class GuiaAereaConfiguration : IEntityTypeConfiguration<GuiaAerea>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<GuiaAerea> builder)
        {
             builder.HasMany(x => x.AccessGuiaAereaAgencies)
                .WithOne(x => x.GuiaAerea)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}