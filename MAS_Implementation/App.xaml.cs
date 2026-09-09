using System.Windows;
using MAS_Implementation.Data;
using MAS_Implementation.Services;
using Microsoft.EntityFrameworkCore;

namespace MAS_Implementation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        using var service = new AmsService();
        service.SeedDatabase();
    }
}