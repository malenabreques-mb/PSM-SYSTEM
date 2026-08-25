using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;

namespace PSMSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            var factory = App.Services.GetRequiredService<IDbContextFactory<PsmDbContext>>();
            using var context = factory.CreateDbContext();
            var cantidadClientes = context.Clientes.Count();
            MessageBox.Show($"Conexión OK. Clientes en la base: {cantidadClientes}");
        }
    }
}