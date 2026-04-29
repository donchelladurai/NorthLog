using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthLog.Domain.Entities;

namespace NorthLog.Infrastructure.Persistence.Configuration
{
    public class WellboreConfiguration : IEntityTypeConfiguration<Wellbore>
    {
        public void Configure(EntityTypeBuilder<Wellbore> b)
        {
            b.HasKey(w => w.Id);
            b.Property(w => w.Name).IsRequired().HasMaxLength(120);
            b.Property(w => w.KickoffDepthMeters).HasPrecision(10, 2);
            b.Property(w => w.Status).HasConversion<int>();

            b.Metadata.FindNavigation(nameof(Wellbore.Reports))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
