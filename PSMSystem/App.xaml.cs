using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSMSystem.Data;

namespace PSMSystem;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("PsmSystem")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'PsmSystem' en appsettings.json.");

        var services = new ServiceCollection();

        services.AddDbContextFactory<PsmDbContext>(options =>
            options.UseSqlServer(connectionString));



        Services = services.BuildServiceProvider();

        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}