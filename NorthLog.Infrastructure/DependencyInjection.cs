using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NorthLog.Application.Common.Interfaces;
using NorthLog.Infrastructure.Persistence;
using NorthLog.Infrastructure.Seed;

namespace NorthLog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    { 
        var root = new InMemoryDatabaseRoot();
        
        services.AddSingleton(root);

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseInMemoryDatabase("NorthLog", root));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IDataSeeder, DataSeeder>();

        return services;
    }
}