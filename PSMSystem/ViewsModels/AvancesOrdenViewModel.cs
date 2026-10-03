using System.Collections.ObjectModel;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Helpers;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class AvancesOrdenViewModel : ViewModelBase
{
    private readonly AvanceTrabajoService _avanceService;
    private readonly OrdenTrabajoService _ordenTrabajoService;
    private readonly int _idOrdenTrabajo;

    private string _etapaSeleccionada;
    private string _descripcion = string.Empty;
    private DateTime? _fecha = DateTime.Today;
    private string? _mensajeError;
    private int _idEstadoSeleccionado;

    public AvancesOrdenViewModel(
        AvanceTrabajoService avanceService, OrdenTrabajoService ordenTrabajoService, OrdenTrabajo orden, bool soloLectura = false)
    {
        _avanceService = avanceService;
        _ordenTrabajoService = ordenTrabajoService;
        _idOrdenTrabajo = orden.IdOrdenTrabajo;

        Orden = orden;
        SoloLectura = soloLectura;
        Avances = new ObservableCollection<AvanceTrabajo>();
        EstadosOrden = new ObservableCollection<EstadoOrdenTrabajo>();
        Etapas = EtapasAvanceTrabajo.Todas;
        _etapaSeleccionada = Etapas[0];
        _idEstadoSeleccionado = orden.IdEstadoOrden;

        AgregarAvanceCommand = new AsyncRelayCommand(async _ => await AgregarAvanceAsync());
        CerrarCommand = new RelayCommand(_ => SolicitudCierre?.Invoke(this, EventArgs.Empty));

        _ = InicializarAsync();
    }

    public OrdenTrabajo Orden { get; }
    public bool SoloLectura { get; }
    public bool MuestraFormularioAlta => !SoloLectura;
    public bool MuestraCambioEstado => !SoloLectura;
    public string Titulo => SoloLectura ? "Detalle de la reparación" : "Avances de la orden";

    public ObservableCollection<AvanceTrabajo> Avances { get; }
    public ObservableCollection<EstadoOrdenTrabajo> EstadosOrden { get; }
    public IReadOnlyList<string> Etapas { get; }

    public string EtapaSeleccionada { get => _etapaSeleccionada; set => SetProperty(ref _etapaSeleccionada, value); }
    public string Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }
    public DateTime? Fecha { get => _fecha; set => SetProperty(ref _fecha, value); }
    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public int IdEstadoSeleccionado
    {
        get => _idEstadoSeleccionado;
        set
        {
            if (SetProperty(ref _idEstadoSeleccionado, value))
                _ = CambiarEstadoAsync(value);
        }
    }

    public ICommand AgregarAvanceCommand { get; }
    public ICommand CerrarCommand { get; }

    public event EventHandler? SolicitudCierre;

    private async Task InicializarAsync()
    {
        var estados = await _ordenTrabajoService.ObtenerEstadosAsync();
        EstadosOrden.Clear();
        foreach (var estado in estados)
            EstadosOrden.Add(estado);

        await CargarAvancesAsync();
    }

    private async Task CargarAvancesAsync()
    {
        var avances = await _avanceService.ObtenerPorOrdenAsync(_idOrdenTrabajo);
        Avances.Clear();
        foreach (var avance in avances)
            Avances.Add(avance);
    }

    private async Task AgregarAvanceAsync()
    {
        MensajeError = null;

        if (!Fecha.HasValue)
        {
            MensajeError = "Seleccioná una fecha.";
            return;
        }

        var avance = new AvanceTrabajo
        {
            IdOrdenTrabajo = _idOrdenTrabajo,
            Etapa = EtapaSeleccionada,
            Descripcion = Descripcion.Trim(),
            Fecha = DateOnly.FromDateTime(Fecha.Value)
        };

        try
        {
            await _avanceService.RegistrarAsync(avance);
            Descripcion = string.Empty;
            await CargarAvancesAsync();
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

    private async Task CambiarEstadoAsync(int idEstadoNuevo)
    {
        MensajeError = null;

        var estadoAnterior = Orden.IdEstadoOrden;
        Orden.IdEstadoOrden = idEstadoNuevo;

        try
        {
            await _ordenTrabajoService.ActualizarAsync(Orden);
        }
        catch (ReglaNegocioException ex)
        {
            Orden.IdEstadoOrden = estadoAnterior;
            _idEstadoSeleccionado = estadoAnterior;
            OnPropertyChanged(nameof(IdEstadoSeleccionado));
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            Orden.IdEstadoOrden = estadoAnterior;
            _idEstadoSeleccionado = estadoAnterior;
            OnPropertyChanged(nameof(IdEstadoSeleccionado));
            MensajeError = "No se pudo actualizar el estado. Verifique que SQL Server esté iniciado e intente de nuevo.";
        }
    }
}