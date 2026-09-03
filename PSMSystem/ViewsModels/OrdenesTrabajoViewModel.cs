using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;
using PSMSystem.Views;

namespace PSMSystem.ViewsModels;

public class OrdenesTrabajoViewModel : ViewModelBase
{
    private const int TamanioPagina = 10;

    private readonly OrdenTrabajoService _ordenService;
    private readonly ClienteService _clienteService;
    private readonly VehiculoService _vehiculoService;
    private readonly TurnoService _turnoService;
    private readonly AvanceTrabajoService _avanceService;

    private string? _textoBusqueda;
    private EstadoOrdenTrabajo? _estadoFiltro;
    private DateTime? _fechaFiltro;
    private int _paginaActual = 1;
    private int _totalOrdenes;
    private bool _cargando;

    public OrdenesTrabajoViewModel(
        OrdenTrabajoService ordenService, ClienteService clienteService,
        VehiculoService vehiculoService, TurnoService turnoService, AvanceTrabajoService avanceService)
    {
        _ordenService = ordenService;
        _clienteService = clienteService;
        _vehiculoService = vehiculoService;
        _turnoService = turnoService;
        _avanceService = avanceService;

        Ordenes = new ObservableCollection<OrdenTrabajo>();
        EstadosFiltro = new ObservableCollection<EstadoOrdenTrabajo>
        {
            new EstadoOrdenTrabajo { IdEstadoOrden = 0, Nombre = "Todos" }
        };
        _estadoFiltro = EstadosFiltro[0];

        BuscarCommand = new AsyncRelayCommand(async _ => await CambiarPaginaAsync(1));
        AgregarOrdenCommand = new RelayCommand(_ => AbrirDialogoOrden(null));
        EditarOrdenCommand = new RelayCommand(parametro => AbrirDialogoOrden(parametro as OrdenTrabajo));
        EliminarOrdenCommand = new AsyncRelayCommand(async parametro => await EliminarAsync(parametro as OrdenTrabajo));
        VerAvancesCommand = new RelayCommand(parametro => AbrirAvances(parametro as OrdenTrabajo));
        PaginaAnteriorCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual - 1), _ => _paginaActual > 1);
        PaginaSiguienteCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual + 1), _ => _paginaActual < TotalPaginas);

        _ = CargarEstadosAsync();
        _ = CambiarPaginaAsync(1);
    }

    public ObservableCollection<OrdenTrabajo> Ordenes { get; }
    public ObservableCollection<EstadoOrdenTrabajo> EstadosFiltro { get; }

    public string? TextoBusqueda { get => _textoBusqueda; set => SetProperty(ref _textoBusqueda, value); }

    public EstadoOrdenTrabajo? EstadoFiltro
    {
        get => _estadoFiltro;
        set
        {
            if (SetProperty(ref _estadoFiltro, value))
                _ = CambiarPaginaAsync(1);
        }
    }

    public DateTime? FechaFiltro
    {
        get => _fechaFiltro;
        set
        {
            if (SetProperty(ref _fechaFiltro, value))
                _ = CambiarPaginaAsync(1);
        }
    }

    public int PaginaActual { get => _paginaActual; private set => SetProperty(ref _paginaActual, value); }
    public int TotalPaginas => _totalOrdenes == 0 ? 1 : (int)Math.Ceiling(_totalOrdenes / (double)TamanioPagina);
    public bool Cargando { get => _cargando; private set => SetProperty(ref _cargando, value); }
    public bool SinResultados => !Cargando && _totalOrdenes == 0;

    public ICommand BuscarCommand { get; }
    public ICommand AgregarOrdenCommand { get; }
    public ICommand EditarOrdenCommand { get; }
    public ICommand EliminarOrdenCommand { get; }
    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSiguienteCommand { get; }
    public ICommand VerAvancesCommand { get; }

    private async Task CargarEstadosAsync()
    {
        var estados = await _ordenService.ObtenerEstadosAsync();
        foreach (var estado in estados)
            EstadosFiltro.Add(estado);
    }

    private async Task CambiarPaginaAsync(int pagina)
    {
        Cargando = true;
        try
        {
            var idEstado = EstadoFiltro is { IdEstadoOrden: > 0 } ? EstadoFiltro.IdEstadoOrden : (int?)null;
            var fecha = FechaFiltro.HasValue ? DateOnly.FromDateTime(FechaFiltro.Value) : (DateOnly?)null;

            var resultado = await _ordenService.BuscarOrdenesAsync(TextoBusqueda, idEstado, fecha, pagina, TamanioPagina);

            Ordenes.Clear();
            foreach (var orden in resultado.Items)
                Ordenes.Add(orden);

            _totalOrdenes = resultado.Total;
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
            OnPropertyChanged(nameof(SinResultados));
        }
    }

    private void AbrirDialogoOrden(OrdenTrabajo? ordenAEditar)
    {
        var dialogo = new OrdenTrabajoEditView
        {
            DataContext = new OrdenTrabajoEditViewModel(
                _ordenService, _clienteService, _vehiculoService, _turnoService, ordenAEditar),
            Owner = Application.Current.MainWindow
        };

        if (dialogo.ShowDialog() == true)
            _ = CambiarPaginaAsync(PaginaActual);
    }

    private void AbrirAvances(OrdenTrabajo? orden)
    {
        if (orden is null) return;

        var dialogo = new AvancesOrdenView
        {
            DataContext = new AvancesOrdenViewModel(_avanceService, orden),
            Owner = Application.Current.MainWindow
        };

        dialogo.ShowDialog();
    }

    private async Task EliminarAsync(OrdenTrabajo? orden)
    {
        if (orden is null) return;

        var confirmar = MessageBox.Show(
            $"¿Seguro que querés eliminar la orden #{orden.IdOrdenTrabajo}? Esta acción no se puede deshacer.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmar != MessageBoxResult.Yes) return;

        try
        {
            await _ordenService.EliminarAsync(orden.IdOrdenTrabajo);
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