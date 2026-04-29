using Microsoft.EntityFrameworkCore;
using NorthLog.Domain.Entities;

namespace NorthLog.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Operator> Operators { get; }
    DbSet<Field> Fields { get; }
    DbSet<Well> Wells { get; }
    DbSet<Wellbore> Wellbores { get; }
    DbSet<DailyReport> DailyReports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}