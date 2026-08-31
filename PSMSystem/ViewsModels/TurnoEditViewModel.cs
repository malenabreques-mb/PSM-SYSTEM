using System.Collections.ObjectModel;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class TurnoEditViewModel : ViewModelBase
{
    private readonly TurnoService _turnoService;
    private readonly VehiculoService _vehiculoService;
    private readonly int? _idTurnoExistente;

    private DateTime? _fechaSeleccionada = DateTime.Today;
    private string _horaTexto = string.Empty;
    private string _motivo = string.Empty;
    private int _idClienteSeleccionado;
    private int _idVehiculoSeleccionado;
    private int _idEstadoSeleccionado = 1;
    private string? _mensajeError;

    public TurnoEditViewModel(
        TurnoService turnoService, ClienteService clienteService, VehiculoService vehiculoService, Turno? turnoAEditar)
    {
        _turnoService = turnoService;
        _vehiculoService = vehiculoService;

        ClientesDisponibles = new ObservableCollection<Cliente>();
        VehiculosDisponibles = new ObservableCollection<Vehiculo>();
        EstadosDisponibles = new ObservableCollection<EstadoTurno>();

        if (turnoAEditar is not null)
        {
            _idTurnoExistente = turnoAEditar.IdTurno;
            _fechaSeleccionada = turnoAEditar.Fecha.ToDateTime(TimeOnly.MinValue);
            _horaTexto = turnoAEditar.Hora.ToString("HH:mm");
            _motivo = turnoAEditar.Motivo;
            _idClienteSeleccionado = turnoAEditar.IdCliente;
            _idVehiculoSeleccionado = turnoAEditar.IdVehiculo;
            _idEstadoSeleccionado = turnoAEditar.IdEstadoTurno;
        }

        GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
        CancelarCommand = new RelayCommand(_ => CerrarVentana(false));

        _ = InicializarAsync(clienteService);
    }

    public bool EsEdicion => _idTurnoExistente.HasValue;
    public string Titulo => EsEdicion ? "Editar turno" : "Agendar turno";

    public ObservableCollection<Cliente> ClientesDisponibles { get; }
    public ObservableCollection<Vehiculo> VehiculosDisponibles { get; }
    public ObservableCollection<EstadoTurno> EstadosDisponibles { get; }

    public DateTime? FechaSeleccionada { get => _fechaSeleccionada; set => SetProperty(ref _fechaSeleccionada, value); }
    public string HoraTexto { get => _horaTexto; set => SetProperty(ref _horaTexto, value); }
    public string Motivo { get => _motivo; set => SetProperty(ref _motivo, value); }

    public int IdClienteSeleccionado
    {
        get => _idClienteSeleccionado;
        set
        {
            if (SetProperty(ref _idClienteSeleccionado, value))
                _ = CargarVehiculosDelClienteAsync();
        }
    }

    public int IdVehiculoSeleccionado { get => _idVehiculoSeleccionado; set => SetProperty(ref _idVehiculoSeleccionado, value); }
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

        var estados = await _turnoService.ObtenerEstadosAsync();
        EstadosDisponibles.Clear();
        foreach (var estado in estados)
            EstadosDisponibles.Add(estado);

        if (_idClienteSeleccionado > 0)
            await CargarVehiculosDelClienteAsync();
    }

    private async Task CargarVehiculosDelClienteAsync()
    {
        VehiculosDisponibles.Clear();

        if (IdClienteSeleccionado <= 0) return;

        var vehiculos = await _vehiculoService.ObtenerVehiculosActivosPorClienteAsync(IdClienteSeleccionado);
        foreach (var vehiculo in vehiculos)
            VehiculosDisponibles.Add(vehiculo);
    }

    private async Task GuardarAsync()
    {
        MensajeError = null;

        if (!FechaSeleccionada.HasValue)
        {
            MensajeError = "Seleccioná una fecha.";
            return;
        }

        if (!TimeOnly.TryParse(HoraTexto, out var hora))
        {
            MensajeError = "Ingresá una hora válida, en formato HH:mm (ej: 14:30).";
            return;
        }

        var turno = new Turno
        {
            IdTurno = _idTurnoExistente ?? 0,
            IdCliente = IdClienteSeleccionado,
            IdVehiculo = IdVehiculoSeleccionado,
            IdEstadoTurno = IdEstadoSeleccionado,
            Fecha = DateOnly.FromDateTime(FechaSeleccionada.Value),
            Hora = hora,
            Motivo = Motivo.Trim()
        };

        try
        {
            if (EsEdicion)
                await _turnoService.ActualizarAsync(turno);
            else
                await _turnoService.CrearAsync(turno);

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