using System.Collections.ObjectModel;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class OrdenTrabajoEditViewModel : ViewModelBase
{
    private readonly OrdenTrabajoService _ordenService;
    private readonly VehiculoService _vehiculoService;
    private readonly TurnoService _turnoService;
    private readonly int? _idOrdenExistente;

    private DateTime? _fechaIngreso = DateTime.Today;
    private DateTime? _fechaEntregaEstimada;
    private string _motivoIngreso = string.Empty;
    private string _diagnosticoInicial = string.Empty;
    private string _observaciones = string.Empty;
    private int _idClienteSeleccionado;
    private int _idVehiculoSeleccionado;
    private int _idTurnoSeleccionado;
    private int _idEstadoSeleccionado = 1;
    private string? _mensajeError;

    public OrdenTrabajoEditViewModel(
        OrdenTrabajoService ordenService, ClienteService clienteService, VehiculoService vehiculoService,
        TurnoService turnoService, OrdenTrabajo? ordenAEditar)
    {
        _ordenService = ordenService;
        _vehiculoService = vehiculoService;
        _turnoService = turnoService;

        ClientesDisponibles = new ObservableCollection<Cliente>();
        VehiculosDisponibles = new ObservableCollection<Vehiculo>();
        TurnosDisponibles = new ObservableCollection<Turno>();
        EstadosDisponibles = new ObservableCollection<EstadoOrdenTrabajo>();

        EsEdicion = ordenAEditar is not null;

        if (ordenAEditar is not null)
        {
            _idOrdenExistente = ordenAEditar.IdOrdenTrabajo;
            _idClienteSeleccionado = ordenAEditar.IdCliente;
            _idVehiculoSeleccionado = ordenAEditar.IdVehiculo;
            _idTurnoSeleccionado = ordenAEditar.IdTurno ?? 0;
            _idEstadoSeleccionado = ordenAEditar.IdEstadoOrden;
            _motivoIngreso = ordenAEditar.MotivoIngreso;
            _diagnosticoInicial = ordenAEditar.DiagnosticoInicial;
            _observaciones = ordenAEditar.Observaciones ?? string.Empty;
            _fechaIngreso = ordenAEditar.FechaIngreso.ToDateTime(TimeOnly.MinValue);
            _fechaEntregaEstimada = ordenAEditar.FechaEntregaEstimada?.ToDateTime(TimeOnly.MinValue);
        }

        GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
        CancelarCommand = new RelayCommand(_ => CerrarVentana(false));

        _ = InicializarAsync(clienteService);
    }

    public bool EsEdicion { get; }
    public string Titulo => EsEdicion ? "Editar orden de trabajo" : "Crear orden de trabajo";
    public bool PuedeCambiarClienteYVehiculo => !EsEdicion;

    public ObservableCollection<Cliente> ClientesDisponibles { get; }
    public ObservableCollection<Vehiculo> VehiculosDisponibles { get; }
    public ObservableCollection<Turno> TurnosDisponibles { get; }
    public ObservableCollection<EstadoOrdenTrabajo> EstadosDisponibles { get; }

    public DateTime? FechaIngreso { get => _fechaIngreso; set => SetProperty(ref _fechaIngreso, value); }
    public DateTime? FechaEntregaEstimada { get => _fechaEntregaEstimada; set => SetProperty(ref _fechaEntregaEstimada, value); }
    public string MotivoIngreso { get => _motivoIngreso; set => SetProperty(ref _motivoIngreso, value); }
    public string DiagnosticoInicial { get => _diagnosticoInicial; set => SetProperty(ref _diagnosticoInicial, value); }
    public string Observaciones { get => _observaciones; set => SetProperty(ref _observaciones, value); }

    public int IdClienteSeleccionado
    {
        get => _idClienteSeleccionado;
        set
        {
            if (SetProperty(ref _idClienteSeleccionado, value))
            {
                _ = CargarVehiculosDelClienteAsync();
                _ = CargarTurnosDelClienteAsync();
            }
        }
    }

    public int IdVehiculoSeleccionado { get => _idVehiculoSeleccionado; set => SetProperty(ref _idVehiculoSeleccionado, value); }
    public int IdTurnoSeleccionado { get => _idTurnoSeleccionado; set => SetProperty(ref _idTurnoSeleccionado, value); }
    public int IdEstadoSeleccionado { get => _idEstadoSeleccionado; set => SetProperty(ref _idEstadoSeleccionado, value); }

    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public ICommand GuardarCommand { get; }
    public ICommand CancelarCommand { get; }

    public event EventHandler<bool>? SolicitudCierre;

    private async Task InicializarAsync(ClienteService clienteService)
    {
        var clientes = await clienteService.ObtenerClientesActivosAsync();
        ClientesDisponibles.Clear();
        foreach (var cliente in clientes)
            ClientesDisponibles.Add(cliente);

        var estados = await _ordenService.ObtenerEstadosAsync();
        EstadosDisponibles.Clear();
        foreach (var estado in estados)
            EstadosDisponibles.Add(estado);

        if (_idClienteSeleccionado > 0)
        {
            await CargarVehiculosDelClienteAsync();
            await CargarTurnosDelClienteAsync();
        }
    }

    private async Task CargarVehiculosDelClienteAsync()
    {
        VehiculosDisponibles.Clear();
        if (IdClienteSeleccionado <= 0) return;

        var vehiculos = await _vehiculoService.ObtenerVehiculosActivosPorClienteAsync(IdClienteSeleccionado);
        foreach (var vehiculo in vehiculos)
            VehiculosDisponibles.Add(vehiculo);

        if (!EsEdicion && VehiculosDisponibles.Count == 1)
            IdVehiculoSeleccionado = VehiculosDisponibles[0].IdVehiculo;
    }

    private async Task CargarTurnosDelClienteAsync()
    {
        TurnosDisponibles.Clear();
        TurnosDisponibles.Add(new Turno { IdTurno = 0, Motivo = "(Sin turno asociado)" });

        if (IdClienteSeleccionado <= 0) return;

        var turnos = await _turnoService.ObtenerTurnosPorClienteAsync(IdClienteSeleccionado);
        foreach (var turno in turnos)
            TurnosDisponibles.Add(turno);

        if (!EsEdicion && turnos.Count == 1)
            IdTurnoSeleccionado = turnos[0].IdTurno;
    }

    private async Task GuardarAsync()
    {
        MensajeError = null;

        if (!FechaIngreso.HasValue)
        {
            MensajeError = "Seleccioná la fecha de ingreso.";
            return;
        }

        if (!FechaEntregaEstimada.HasValue)
        {
            MensajeError = "Seleccioná la fecha estimada de entrega.";
            return;
        }

        var orden = new OrdenTrabajo
        {
            IdOrdenTrabajo = _idOrdenExistente ?? 0,
            IdCliente = IdClienteSeleccionado,
            IdVehiculo = IdVehiculoSeleccionado,
            IdTurno = IdTurnoSeleccionado == 0 ? null : IdTurnoSeleccionado,
            IdEstadoOrden = IdEstadoSeleccionado,
            MotivoIngreso = MotivoIngreso.Trim(),
            DiagnosticoInicial = DiagnosticoInicial.Trim(),
            Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones.Trim(),
            FechaIngreso = DateOnly.FromDateTime(FechaIngreso.Value),
            FechaEntregaEstimada = DateOnly.FromDateTime(FechaEntregaEstimada.Value)
        };

        try
        {
            if (EsEdicion)
                await _ordenService.ActualizarAsync(orden);
            else
                await _ordenService.CrearAsync(orden);

            CerrarVentana(true);
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo guardar. Verifique que SQL Server esté iniciado e intente de nuevo.";
        }
    }

    private void CerrarVentana(bool guardadoConExito) => SolicitudCierre?.Invoke(this, guardadoConExito);
}