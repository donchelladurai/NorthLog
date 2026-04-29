using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthLog.Domain.Entities;

namespace NorthLog.Infrastructure.Persistence.Configuration
{
    public class WellConfiguration : IEntityTypeConfiguration<Well>
    {
        public void Configure(EntityTypeBuilder<Well> b)
        {
            b.HasKey(w => w.Id);
            b.Property(w => w.Name).IsRequired().HasMaxLength(120);
            b.HasOne(w => w.Field).WithMany().HasForeignKey(w => w.FieldId);

            b.Metadata.FindNavigation(nameof(Well.Wellbores))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
