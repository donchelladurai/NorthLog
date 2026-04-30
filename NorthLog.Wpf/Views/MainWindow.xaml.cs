using System.Windows;
using NorthLog.Wpf.ViewModels;

namespace NorthLog.Wpf.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += async (_, _) => await vm.LoadWellboresCommand.ExecuteAsync();
    }

    private void OpenLegacy_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)System.Windows.Application.Current;
        var window = (LegacyReportsWindow)app.Host!.Services
            .GetService(typeof(LegacyReportsWindow))!;
        window.Owner = this;
        window.ShowDialog();
    }
}