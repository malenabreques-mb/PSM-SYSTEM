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
    private readonly int _idOrdenTrabajo;

    private string _etapaSeleccionada;
    private string _descripcion = string.Empty;
    private DateTime? _fecha = DateTime.Today;
    private string? _mensajeError;

    public AvancesOrdenViewModel(AvanceTrabajoService avanceService, OrdenTrabajo orden, bool soloLectura = false)
    {
        _avanceService = avanceService;
        _idOrdenTrabajo = orden.IdOrdenTrabajo;

        Orden = orden;
        SoloLectura = soloLectura;
        Avances = new ObservableCollection<AvanceTrabajo>();
        Etapas = EtapasAvanceTrabajo.Todas;
        _etapaSeleccionada = Etapas[0];

        AgregarAvanceCommand = new AsyncRelayCommand(async _ => await AgregarAvanceAsync());
        CerrarCommand = new RelayCommand(_ => SolicitudCierre?.Invoke(this, EventArgs.Empty));

        _ = CargarAvancesAsync();
    }

    public OrdenTrabajo Orden { get; }
    public bool SoloLectura { get; }
    public bool MuestraFormularioAlta => !SoloLectura;
    public string Titulo => SoloLectura ? "Detalle de la reparación" : "Avances de la orden";

    public ObservableCollection<AvanceTrabajo> Avances { get; }
    public IReadOnlyList<string> Etapas { get; }

    public string EtapaSeleccionada { get => _etapaSeleccionada; set => SetProperty(ref _etapaSeleccionada, value); }
    public string Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }
    public DateTime? Fecha { get => _fecha; set => SetProperty(ref _fecha, value); }
    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public ICommand AgregarAvanceCommand { get; }
    public ICommand CerrarCommand { get; }

    public event EventHandler? SolicitudCierre;

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
}