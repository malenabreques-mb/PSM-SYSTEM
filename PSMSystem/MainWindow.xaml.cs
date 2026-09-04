using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PSMSystem.Views;
using PSMSystem.ViewsModels;

namespace PSMSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MostrarClientes();
        }

        private void MostrarClientes_Click(object sender, RoutedEventArgs e) => MostrarClientes();
        private void MostrarVehiculos_Click(object sender, RoutedEventArgs e) => MostrarVehiculos();
        private void MostrarTurnos_Click(object sender, RoutedEventArgs e) => MostrarTurnos();
        private void MostrarOrdenes_Click(object sender, RoutedEventArgs e) => MostrarOrdenes();
        private void MostrarHistorial_Click(object sender, RoutedEventArgs e) => MostrarHistorial();
        private void MostrarRepuestos_Click(object sender, RoutedEventArgs e) => MostrarRepuestos();

        private void MostrarClientes()
        {
            ContenidoPrincipal.Content = new ClientesView
            {
                DataContext = App.Services.GetRequiredService<ClientesViewModel>()
            };
        }

        private void MostrarVehiculos()
        {
            ContenidoPrincipal.Content = new VehiculosView
            {
                DataContext = App.Services.GetRequiredService<VehiculosViewModel>()
            };
        }

        private void MostrarTurnos()
        {
            ContenidoPrincipal.Content = new TurnosView
            {
                DataContext = App.Services.GetRequiredService<TurnosViewModel>()
            };
        }

        private void MostrarOrdenes()
        {
            ContenidoPrincipal.Content = new OrdenesTrabajoView
            {
                DataContext = App.Services.GetRequiredService<OrdenesTrabajoViewModel>()
            };
        }

        private void MostrarHistorial()
        {
            ContenidoPrincipal.Content = new HistorialView
            {
                DataContext = App.Services.GetRequiredService<HistorialViewModel>()
            };
        }

        private void MostrarRepuestos()
        {
            ContenidoPrincipal.Content = new RepuestosView
            {
                DataContext = App.Services.GetRequiredService<RepuestosViewModel>()
            };
        }
    }
}