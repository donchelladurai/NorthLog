using Microsoft.EntityFrameworkCore;
using NorthLog.Application.Common.Interfaces;
using NorthLog.Domain.Entities;

namespace NorthLog.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<Well> Wells => Set<Well>();
    public DbSet<Wellbore> Wellbores => Set<Wellbore>();
    public DbSet<DailyReport> DailyReports => Set<DailyReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}