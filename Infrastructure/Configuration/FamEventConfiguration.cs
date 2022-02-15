using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class FamEventConfiguration : IEntityTypeConfiguration<FamEvent>
    {
        public void Configure(EntityTypeBuilder<FamEvent> builder)
        {
            builder.HasNoKey().ToView(null);
        }
    }
}
