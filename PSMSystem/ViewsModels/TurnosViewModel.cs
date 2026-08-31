using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;
using PSMSystem.Views;

namespace PSMSystem.ViewsModels;

public class TurnosViewModel : ViewModelBase
{
    private const int TamanioPagina = 10;

    private readonly TurnoService _turnoService;
    private readonly ClienteService _clienteService;
    private readonly VehiculoService _vehiculoService;

    private string? _textoBusqueda;
    private EstadoTurno? _estadoFiltro;
    private DateTime? _fechaFiltro;
    private int _paginaActual = 1;
    private int _totalTurnos;
    private bool _cargando;

    public TurnosViewModel(TurnoService turnoService, ClienteService clienteService, VehiculoService vehiculoService)
    {
        _turnoService = turnoService;
        _clienteService = clienteService;
        _vehiculoService = vehiculoService;

        Turnos = new ObservableCollection<Turno>();
        EstadosFiltro = new ObservableCollection<EstadoTurno>
        {
            new EstadoTurno { IdEstadoTurno = 0, Nombre = "Todos" }
        };
        _estadoFiltro = EstadosFiltro[0];

        BuscarCommand = new AsyncRelayCommand(async _ => await CambiarPaginaAsync(1));
        AgregarTurnoCommand = new RelayCommand(_ => AbrirDialogoTurno(null));
        EditarTurnoCommand = new RelayCommand(parametro => AbrirDialogoTurno(parametro as Turno));
        EliminarTurnoCommand = new AsyncRelayCommand(async parametro => await EliminarAsync(parametro as Turno));
        PaginaAnteriorCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual - 1), _ => _paginaActual > 1);
        PaginaSiguienteCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual + 1), _ => _paginaActual < TotalPaginas);

        _ = CargarEstadosAsync();
        _ = CambiarPaginaAsync(1);
    }

    public ObservableCollection<Turno> Turnos { get; }
    public ObservableCollection<EstadoTurno> EstadosFiltro { get; }

    public string? TextoBusqueda { get => _textoBusqueda; set => SetProperty(ref _textoBusqueda, value); }
    public EstadoTurno? EstadoFiltro
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
    public int TotalPaginas => _totalTurnos == 0 ? 1 : (int)Math.Ceiling(_totalTurnos / (double)TamanioPagina);
    public bool Cargando { get => _cargando; private set => SetProperty(ref _cargando, value); }

    public ICommand BuscarCommand { get; }
    public ICommand AgregarTurnoCommand { get; }
    public ICommand EditarTurnoCommand { get; }
    public ICommand EliminarTurnoCommand { get; }
    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSiguienteCommand { get; }

    private async Task CargarEstadosAsync()
    {
        var estados = await _turnoService.ObtenerEstadosAsync();
        foreach (var estado in estados)
            EstadosFiltro.Add(estado);
    }

    private async Task CambiarPaginaAsync(int pagina)
    {
        Cargando = true;
        try
        {
            var idEstado = EstadoFiltro is { IdEstadoTurno: > 0 } ? EstadoFiltro.IdEstadoTurno : (int?)null;
            var fecha = FechaFiltro.HasValue ? DateOnly.FromDateTime(FechaFiltro.Value) : (DateOnly?)null;

            var resultado = await _turnoService.BuscarTurnosAsync(TextoBusqueda, idEstado, fecha, pagina, TamanioPagina);

            Turnos.Clear();
            foreach (var turno in resultado.Items)
                Turnos.Add(turno);

            _totalTurnos = resultado.Total;
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

    private void AbrirDialogoTurno(Turno? turnoAEditar)
    {
        var dialogo = new TurnoEditView
        {
            DataContext = new TurnoEditViewModel(_turnoService, _clienteService, _vehiculoService, turnoAEditar),
            Owner = Application.Current.MainWindow
        };

        if (dialogo.ShowDialog() == true)
            _ = CambiarPaginaAsync(PaginaActual);
    }

    private async Task EliminarAsync(Turno? turno)
    {
        if (turno is null) return;

        var confirmar = MessageBox.Show(
            $"¿Seguro que querés eliminar el turno del {turno.Fecha:dd/MM/yyyy} a las {turno.Hora:HH:mm}? " +
            "Esta acción no se puede deshacer.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmar != MessageBoxResult.Yes) return;

        try
        {
            await _turnoService.EliminarAsync(turno.IdTurno);
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