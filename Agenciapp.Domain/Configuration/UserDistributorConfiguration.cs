using Agenciapp.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Configuration
{
    public class UserDistributorConfiguration : IEntityTypeConfiguration<UserDistributor>
    {
        public void Configure(EntityTypeBuilder<UserDistributor> builder)
        {
            
        }
    }
}
