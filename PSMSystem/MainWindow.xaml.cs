using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PSMSystem.ViewsModels;

namespace PSMSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ClientesViewControl.DataContext = App.Services.GetRequiredService<ClientesViewModel>();
        }
    }
}