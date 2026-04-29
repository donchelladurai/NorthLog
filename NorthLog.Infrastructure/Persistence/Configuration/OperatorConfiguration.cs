using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthLog.Domain.Entities;

namespace NorthLog.Infrastructure.Persistence.Configuration;

public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> b)
    {
        b.HasKey(o => o.Id);
        b.Property(o => o.Name).IsRequired().HasMaxLength(120);
    }
}