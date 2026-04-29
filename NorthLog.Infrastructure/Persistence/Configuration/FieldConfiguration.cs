using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthLog.Domain.Entities;

namespace NorthLog.Infrastructure.Persistence.Configuration
{
    public class FieldConfiguration : IEntityTypeConfiguration<Field>
    {
        public void Configure(EntityTypeBuilder<Field> b)
        {
            b.HasKey(f => f.Id);
            b.Property(f => f.Name).IsRequired().HasMaxLength(120);
            b.Property(f => f.Block).IsRequired().HasMaxLength(20);
            b.HasOne(f => f.Operator).WithMany().HasForeignKey(f => f.OperatorId);
        }
    }
}
