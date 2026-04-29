using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthLog.Domain.Entities;

namespace NorthLog.Infrastructure.Persistence.Configuration
{
    public class DailyReportConfiguration : IEntityTypeConfiguration<DailyReport>
    {
        public void Configure(EntityTypeBuilder<DailyReport> b)
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.DepthOut).HasPrecision(10, 2);
            b.Property(r => r.DepthOut).HasPrecision(10, 2);
            b.Property(r => r.TotalOilInBarrels).HasPrecision(8, 2);
            b.Property(r => r.LithologySummary).IsRequired().HasMaxLength(2000);
            b.Property(r => r.Notes).HasMaxLength(4000);
        }
    }
}
