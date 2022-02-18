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
