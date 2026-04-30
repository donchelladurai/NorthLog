using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NorthLog.Application;
using NorthLog.Infrastructure;
using NorthLog.Infrastructure.Seed;
using NorthLog.Wpf.ViewModels;
using NorthLog.Wpf.Views;
using System.Data;
using System.Windows;
using Wolverine;
using Wolverine.FluentValidation;

namespace NorthLog.Wpf;

public partial class App : System.Windows.Application
{
    public IHost? Host { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder();

        builder.UseWolverine(opts =>
        {
            // Wolverine scans the entry assembly by default. Tell it to also
            // pick up the Application assembly where our handlers and
            // validators live.
            opts.Discovery.IncludeAssembly(typeof(NorthLog.Application.DependencyInjection).Assembly);

            // Built-in middleware that runs FluentValidation before each
            // handler. Failed validations throw FluentValidation.ValidationException.
            opts.UseFluentValidation();
        });

        builder.ConfigureServices(services =>
        {
            services.AddApplication();      // FluentValidation validators
            services.AddInfrastructure();   // EF Core + seeder

            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();
            services.AddTransient<LegacyReportsWindow>();
        });

        Host = builder.Build();
        await Host.StartAsync();

        // Seed the in-memory store once at startup.
        using (var scope = Host.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
        }

        var window = Host.Services.GetRequiredService<MainWindow>();
        window.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (Host is not null)
        {
            await Host.StopAsync();
            Host.Dispose();
        }
        base.OnExit(e);
    }
}