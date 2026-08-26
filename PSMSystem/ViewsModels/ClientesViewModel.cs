using System;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;
using PSMSystem.Views;

namespace PSMSystem.ViewsModels;

public class ClientesViewModel : ViewModelBase
{
    private const int TamanioPagina = 10;

    private readonly ClienteService _clienteService;

    private string? _textoBusqueda;
    private int _paginaActual = 1;
    private int _totalClientes;
    private bool _cargando;

    public ClientesViewModel(ClienteService clienteService)
    {
        _clienteService = clienteService;

        Clientes = new ObservableCollection<Cliente>();

        BuscarCommand = new AsyncRelayCommand(async _ => await CambiarPaginaAsync(1));
        AgregarClienteCommand = new RelayCommand(_ => AbrirDialogoCliente(null));
        EditarClienteCommand = new RelayCommand(parametro => AbrirDialogoCliente(parametro as Cliente));
        DesactivarReactivarClienteCommand =
            new AsyncRelayCommand(async parametro => await DesactivarReactivarAsync(parametro as Cliente));
        PaginaAnteriorCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual - 1),
            _ => _paginaActual > 1);
        PaginaSiguienteCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual + 1),
            _ => _paginaActual < TotalPaginas);

        _ = CambiarPaginaAsync(1);
    }

    public ObservableCollection<Cliente> Clientes { get; }

    public string? TextoBusqueda
    {
        get => _textoBusqueda;
        set => SetProperty(ref _textoBusqueda, value);
    }

    public int PaginaActual
    {
        get => _paginaActual;
        private set => SetProperty(ref _paginaActual, value);
    }

    public int TotalPaginas => _totalClientes == 0 ? 1 : (int)Math.Ceiling(_totalClientes / (double)TamanioPagina);

    public bool Cargando
    {
        get => _cargando;
        private set => SetProperty(ref _cargando, value);
    }

    public ICommand BuscarCommand { get; }
    public ICommand AgregarClienteCommand { get; }
    public ICommand EditarClienteCommand { get; }
    public ICommand DesactivarReactivarClienteCommand { get; }
    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSiguienteCommand { get; }

    private async Task CambiarPaginaAsync(int pagina)
    {
        Cargando = true;
        try
        {
            var resultado = await _clienteService.BuscarClientesAsync(TextoBusqueda, pagina, TamanioPagina);

            Clientes.Clear();
            foreach (var cliente in resultado.Items)
                Clientes.Add(cliente);

            _totalClientes = resultado.Total;
            PaginaActual = pagina;
            OnPropertyChanged(nameof(TotalPaginas));
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
        finally
        {
            Cargando = false;
        }
    }

    private void AbrirDialogoCliente(Cliente? clienteAEditar)
    {
        var dialogo = new ClienteEditView
        {
            DataContext = new ClienteEditViewModel(_clienteService, clienteAEditar),
            Owner = Application.Current.MainWindow
        };

        if (dialogo.ShowDialog() == true)
            _ = CambiarPaginaAsync(PaginaActual);
    }

    private async Task DesactivarReactivarAsync(Cliente? cliente)
    {
        if (cliente is null) return;

        try
        {
            if (cliente.Activo)
            {
                var confirmar = MessageBox.Show(
                    $"¿Seguro que querés desactivar a {cliente.Nombre} {cliente.Apellido}? " +
                    "Dejará de estar disponible para nuevas órdenes de trabajo.",
                    "Confirmar desactivación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmar != MessageBoxResult.Yes) return;

                await _clienteService.DesactivarAsync(cliente.IdCliente);
            }
            else
            {
                await _clienteService.ReactivarAsync(cliente.IdCliente);
            }

            await CambiarPaginaAsync(PaginaActual);
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private static void MostrarError(Exception ex)
    {
        var mensaje = ex is ReglaNegocioException
            ? ex.Message
            : "No se pudo completar la operación. Verifique que SQL Server esté iniciado e intente de nuevo.";

        MessageBox.Show(mensaje, "PSM System", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
