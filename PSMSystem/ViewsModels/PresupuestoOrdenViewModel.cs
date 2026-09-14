using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Helpers;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class PresupuestoOrdenViewModel : ViewModelBase
{
    private readonly PresupuestoService _presupuestoService;
    private readonly RepuestoService _repuestoService;
    private readonly FacturaService _facturaService;
    private readonly int _idOrdenTrabajo;

    private Presupuesto? _presupuesto;
    private string _tipoItemSeleccionado;
    private int _idRepuestoSeleccionado;
    private string _descripcionItem = string.Empty;
    private string _cantidad = string.Empty;
    private string _precioUnitario = string.Empty;
    private string? _mensajeError;
    private bool _huboCambios;

    public PresupuestoOrdenViewModel(
        PresupuestoService presupuestoService, RepuestoService repuestoService,
        FacturaService facturaService, OrdenTrabajo orden)
    {
        _presupuestoService = presupuestoService;
        _repuestoService = repuestoService;
        _facturaService = facturaService;
        _idOrdenTrabajo = orden.IdOrdenTrabajo;

        Orden = orden;
        Detalles = new ObservableCollection<DetallePresupuesto>();
        Estados = new ObservableCollection<EstadoPresupuesto>();
        RepuestosDisponibles = new ObservableCollection<Repuesto>();
        TiposItem = TiposItemPresupuesto.Todas;
        _tipoItemSeleccionado = TiposItem[0];

        CrearPresupuestoCommand = new AsyncRelayCommand(async _ => await CrearPresupuestoAsync());
        AgregarItemCommand = new AsyncRelayCommand(async _ => await AgregarItemAsync());
        EliminarItemCommand = new AsyncRelayCommand(async parametro => await EliminarItemAsync(parametro as DetallePresupuesto));
        AprobarCommand = new AsyncRelayCommand(async _ => await CambiarEstadoAsync("Aprobado"));
        RechazarCommand = new AsyncRelayCommand(async _ => await CambiarEstadoAsync("Rechazado"));
        VolverAElaboracionCommand = new AsyncRelayCommand(async _ => await CambiarEstadoAsync("En elaboración"));
        ConvertirEnFacturaCommand = new AsyncRelayCommand(async _ => await ConvertirEnFacturaAsync());
        CerrarCommand = new RelayCommand(_ => SolicitudCierre?.Invoke(this, _huboCambios));

        _ = InicializarAsync();
    }

    public OrdenTrabajo Orden { get; }
    public ObservableCollection<DetallePresupuesto> Detalles { get; }
    public ObservableCollection<EstadoPresupuesto> Estados { get; }
    public ObservableCollection<Repuesto> RepuestosDisponibles { get; }
    public IReadOnlyList<string> TiposItem { get; }

    public bool ExistePresupuesto => _presupuesto is not null;
    public decimal Total => _presupuesto?.Total ?? 0;
    public string EstadoActual => _presupuesto?.EstadoPresupuesto?.Nombre ?? "";
    public bool EstaEnElaboracion => EstadoActual == "En elaboración";
    public bool EstaAprobado => EstadoActual == "Aprobado";
    public bool PuedeVolverAElaboracion => EstaAprobado || EstadoActual == "Rechazado";
    public bool PuedeEditarItems => EstaEnElaboracion;

    public string TipoItemSeleccionado
    {
        get => _tipoItemSeleccionado;
        set
        {
            if (SetProperty(ref _tipoItemSeleccionado, value))
            {
                OnPropertyChanged(nameof(EsRepuesto));
                if (!EsRepuesto && string.IsNullOrWhiteSpace(Cantidad))
                    Cantidad = "1";
            }
        }
    }

    public bool EsRepuesto => TipoItemSeleccionado == TiposItemPresupuesto.Repuesto;

    public int IdRepuestoSeleccionado
    {
        get => _idRepuestoSeleccionado;
        set
        {
            if (SetProperty(ref _idRepuestoSeleccionado, value))
            {
                var repuesto = RepuestosDisponibles.FirstOrDefault(r => r.IdRepuesto == value);
                if (repuesto?.PrecioUnitario is decimal precio)
                    PrecioUnitario = precio.ToString();
            }
        }
    }

    public string DescripcionItem { get => _descripcionItem; set => SetProperty(ref _descripcionItem, value); }
    public string Cantidad { get => _cantidad; set => SetProperty(ref _cantidad, value); }
    public string PrecioUnitario { get => _precioUnitario; set => SetProperty(ref _precioUnitario, value); }

    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public ICommand CrearPresupuestoCommand { get; }
    public ICommand AgregarItemCommand { get; }
    public ICommand EliminarItemCommand { get; }
    public ICommand AprobarCommand { get; }
    public ICommand RechazarCommand { get; }
    public ICommand VolverAElaboracionCommand { get; }
    public ICommand ConvertirEnFacturaCommand { get; }
    public ICommand CerrarCommand { get; }

    public event EventHandler<bool>? SolicitudCierre;

    private async Task InicializarAsync()
    {
        var estados = await _presupuestoService.ObtenerEstadosAsync();
        Estados.Clear();
        foreach (var estado in estados)
            Estados.Add(estado);

        var repuestos = await _repuestoService.ObtenerActivosAsync();
        RepuestosDisponibles.Clear();
        foreach (var repuesto in repuestos)
            RepuestosDisponibles.Add(repuesto);

        await CargarPresupuestoAsync();
    }

    private async Task CargarPresupuestoAsync()
    {
        _presupuesto = await _presupuestoService.ObtenerPorOrdenAsync(_idOrdenTrabajo);

        Detalles.Clear();
        if (_presupuesto is not null)
        {
            foreach (var detalle in _presupuesto.Detalles)
                Detalles.Add(detalle);
        }

        OnPropertyChanged(nameof(ExistePresupuesto));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(EstadoActual));
        OnPropertyChanged(nameof(EstaEnElaboracion));
        OnPropertyChanged(nameof(EstaAprobado));
        OnPropertyChanged(nameof(PuedeVolverAElaboracion));
        OnPropertyChanged(nameof(PuedeEditarItems));
    }

    private async Task CrearPresupuestoAsync()
    {
        MensajeError = null;
        try
        {
            await _presupuestoService.CrearAsync(_idOrdenTrabajo);
            _huboCambios = true;
            await CargarPresupuestoAsync();
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo crear el presupuesto. Verifique que SQL Server esté iniciado e intente de nuevo.";
        }
    }

    private async Task AgregarItemAsync()
    {
        MensajeError = null;

        if (_presupuesto is null) return;

        if (!int.TryParse(Cantidad, out var cantidadValor))
        {
            MensajeError = "Verifique los datos ingresados: la cantidad no es un número válido.";
            return;
        }

        if (!decimal.TryParse(PrecioUnitario, out var precioValor))
        {
            MensajeError = "Verifique los datos ingresados: el precio no es un número válido.";
            return;
        }

        var detalle = new DetallePresupuesto
        {
            IdPresupuesto = _presupuesto.IdPresupuesto,
            TipoItem = TipoItemSeleccionado,
            IdRepuesto = EsRepuesto ? IdRepuestoSeleccionado : null,
            Descripcion = EsRepuesto ? null : (string.IsNullOrWhiteSpace(DescripcionItem) ? null : DescripcionItem.Trim()),
            Cantidad = cantidadValor,
            PrecioUnitario = precioValor
        };

        try
        {
            await _presupuestoService.AgregarDetalleAsync(detalle);
            _huboCambios = true;
            DescripcionItem = string.Empty;
            Cantidad = string.Empty;
            PrecioUnitario = string.Empty;
            await CargarPresupuestoAsync();
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

    private async Task EliminarItemAsync(DetallePresupuesto? detalle)
    {
        if (detalle is null) return;

        try
        {
            await _presupuestoService.EliminarDetalleAsync(detalle.IdDetallePresupuesto);
            _huboCambios = true;
            await CargarPresupuestoAsync();
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo eliminar el ítem. Verifique que SQL Server esté iniciado e intente de nuevo.";
        }
    }

    private async Task CambiarEstadoAsync(string nombreEstado)
    {
        if (_presupuesto is null) return;

        var estado = Estados.FirstOrDefault(e => e.Nombre == nombreEstado);
        if (estado is null) return;

        try
        {
            await _presupuestoService.ActualizarEstadoAsync(_presupuesto.IdPresupuesto, estado.IdEstadoPresupuesto);
            _huboCambios = true;
            await CargarPresupuestoAsync();
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo cambiar el estado. Verifique que SQL Server esté iniciado e intente de nuevo.";
        }
    }

    private async Task ConvertirEnFacturaAsync()
    {
        if (_presupuesto is null) return;

        try
        {
            await _facturaService.ConvertirPresupuestoAsync(_presupuesto.IdPresupuesto);
            _huboCambios = true;
            await CargarPresupuestoAsync();
            MessageBox.Show(
                "Factura generada correctamente. Podés verla y emitirla desde el módulo Facturación.",
                "PSM System", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo convertir en factura. Verifique que SQL Server esté iniciado e intente de nuevo.";
        }
    }
}