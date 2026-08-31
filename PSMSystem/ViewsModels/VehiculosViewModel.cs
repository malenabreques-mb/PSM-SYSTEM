using System;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;
using PSMSystem.Views;

namespace PSMSystem.ViewsModels;

public class VehiculosViewModel : ViewModelBase
{
    private const int TamanioPagina = 10;

    private readonly VehiculoService _vehiculoService;
    private readonly ClienteService _clienteService;

    private string? _textoBusqueda;
    private int _paginaActual = 1;
    private int _totalVehiculos;
    private bool _cargando;

    public VehiculosViewModel(VehiculoService vehiculoService, ClienteService clienteService)
    {
        _vehiculoService = vehiculoService;
        _clienteService = clienteService;

        Vehiculos = new ObservableCollection<Vehiculo>();

        BuscarCommand = new AsyncRelayCommand(async _ => await CambiarPaginaAsync(1));
        AgregarVehiculoCommand = new RelayCommand(_ => AbrirDialogoVehiculo(null));
        EditarVehiculoCommand = new RelayCommand(parametro => AbrirDialogoVehiculo(parametro as Vehiculo));
        DesactivarReactivarVehiculoCommand =
            new AsyncRelayCommand(async parametro => await DesactivarReactivarAsync(parametro as Vehiculo));
        PaginaAnteriorCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual - 1),
            _ => _paginaActual > 1);
        PaginaSiguienteCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual + 1),
            _ => _paginaActual < TotalPaginas);

        _ = CambiarPaginaAsync(1);
    }

    public ObservableCollection<Vehiculo> Vehiculos { get; }

    public string? TextoBusqueda
    {
        get => _textoBusqueda;
        set => SetProperty(ref _textoBusqueda, value);
    }

    public int PaginaActual { get => _paginaActual; private set => SetProperty(ref _paginaActual, value); }

    public int TotalPaginas => _totalVehiculos == 0 ? 1 : (int)Math.Ceiling(_totalVehiculos / (double)TamanioPagina);

    public bool Cargando { get => _cargando; private set => SetProperty(ref _cargando, value); }

    public ICommand BuscarCommand { get; }
    public ICommand AgregarVehiculoCommand { get; }
    public ICommand EditarVehiculoCommand { get; }
    public ICommand DesactivarReactivarVehiculoCommand { get; }
    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSiguienteCommand { get; }

    private async Task CambiarPaginaAsync(int pagina)
    {
        Cargando = true;
        try
        {
            var resultado = await _vehiculoService.BuscarVehiculosAsync(TextoBusqueda, pagina, TamanioPagina);

            Vehiculos.Clear();
            foreach (var vehiculo in resultado.Items)
                Vehiculos.Add(vehiculo);

            _totalVehiculos = resultado.Total;
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

    private void AbrirDialogoVehiculo(Vehiculo? vehiculoAEditar)
    {
        var dialogo = new VehiculoEditView
        {
            DataContext = new VehiculoEditViewModel(_vehiculoService, _clienteService, vehiculoAEditar),
            Owner = Application.Current.MainWindow
        };

        if (dialogo.ShowDialog() == true)
            _ = CambiarPaginaAsync(PaginaActual);
    }

    private async Task DesactivarReactivarAsync(Vehiculo? vehiculo)
    {
        if (vehiculo is null) return;

        try
        {
            if (vehiculo.Activo)
            {
                var confirmar = MessageBox.Show(
                    $"¿Seguro que querés desactivar el vehículo {vehiculo.Patente}? " +
                    "Dejará de estar disponible para nuevas órdenes de trabajo.",
                    "Confirmar desactivación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmar != MessageBoxResult.Yes) return;

                await _vehiculoService.DesactivarAsync(vehiculo.IdVehiculo);
            }
            else
            {
                await _vehiculoService.ReactivarAsync(vehiculo.IdVehiculo);
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
