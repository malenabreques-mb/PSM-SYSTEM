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
    }
}