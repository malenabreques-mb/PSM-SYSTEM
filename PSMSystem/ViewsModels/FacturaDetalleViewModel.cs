using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Helpers;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class FacturaDetalleViewModel : ViewModelBase
{
    private readonly FacturaService _facturaService;
    private readonly RepuestoService _repuestoService;
    private readonly int _idFactura;

    private Factura? _factura;
    private string _tipoItemSeleccionado;
    private int _idRepuestoSeleccionado;
    private string _descripcionItem = string.Empty;
    private string _cantidad = string.Empty;
    private string _precioUnitario = string.Empty;
    private string? _mensajeError;
    private bool _huboCambios;

    public FacturaDetalleViewModel(FacturaService facturaService, RepuestoService repuestoService, int idFactura)
    {
        _facturaService = facturaService;
        _repuestoService = repuestoService;
        _idFactura = idFactura;

        Detalles = new ObservableCollection<DetalleFactura>();
        RepuestosDisponibles = new ObservableCollection<Repuesto>();
        TiposItem = TiposItemPresupuesto.Todas;
        _tipoItemSeleccionado = TiposItem[0];

        AgregarItemCommand = new AsyncRelayCommand(async _ => await AgregarItemAsync());
        EliminarItemCommand = new AsyncRelayCommand(async parametro => await EliminarItemAsync(parametro as DetalleFactura));
        EmitirCommand = new AsyncRelayCommand(async _ => await EmitirAsync());
        CerrarCommand = new RelayCommand(_ => SolicitudCierre?.Invoke(this, _huboCambios));

        _ = InicializarAsync();
    }

    public ObservableCollection<DetalleFactura> Detalles { get; }
    public ObservableCollection<Repuesto> RepuestosDisponibles { get; }
    public IReadOnlyList<string> TiposItem { get; }

    public string NumeroFactura => _factura?.NumeroFactura ?? "";
    public string EstadoActual => _factura?.EstadoFactura?.Nombre ?? "";
    public bool PendienteDeEmision => EstadoActual == "Pendiente de emisión";

    public string ClienteYVehiculo => _factura?.Presupuesto?.OrdenTrabajo is { } orden
        ? $"{orden.Cliente?.NombreCompleto} — {orden.Vehiculo?.DescripcionCombo}"
        : "";

    public decimal Subtotal => _factura?.Subtotal ?? 0;
    public decimal Iva => _factura?.Iva ?? 0;
    public decimal Total => _factura?.Total ?? 0;

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

    public ICommand AgregarItemCommand { get; }
    public ICommand EliminarItemCommand { get; }
    public ICommand EmitirCommand { get; }
    public ICommand CerrarCommand { get; }

    public event EventHandler<bool>? SolicitudCierre;

    private async Task InicializarAsync()
    {
        var repuestos = await _repuestoService.ObtenerActivosAsync();
        RepuestosDisponibles.Clear();
        foreach (var repuesto in repuestos)
            RepuestosDisponibles.Add(repuesto);

        await CargarFacturaAsync();
    }

    private async Task CargarFacturaAsync()
    {
        _factura = await _facturaService.ObtenerPorIdAsync(_idFactura);

        Detalles.Clear();
        if (_factura is not null)
        {
            foreach (var detalle in _factura.Detalles)
                Detalles.Add(detalle);
        }

        OnPropertyChanged(nameof(NumeroFactura));
        OnPropertyChanged(nameof(EstadoActual));
        OnPropertyChanged(nameof(PendienteDeEmision));
        OnPropertyChanged(nameof(ClienteYVehiculo));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Iva));
        OnPropertyChanged(nameof(Total));
    }

    private async Task AgregarItemAsync()
    {
        MensajeError = null;

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

        var detalle = new DetalleFactura
        {
            IdFactura = _idFactura,
            TipoItem = TipoItemSeleccionado,
            IdRepuesto = EsRepuesto ? IdRepuestoSeleccionado : null,
            Descripcion = EsRepuesto ? null : (string.IsNullOrWhiteSpace(DescripcionItem) ? null : DescripcionItem.Trim()),
            Cantidad = cantidadValor,
            PrecioUnitario = precioValor
        };

        try
        {
            await _facturaService.AgregarDetalleAsync(detalle);
            _huboCambios = true;
            DescripcionItem = string.Empty;
            Cantidad = string.Empty;
            PrecioUnitario = string.Empty;
            await CargarFacturaAsync();
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

    private async Task EliminarItemAsync(DetalleFactura? detalle)
    {
        if (detalle is null) return;

        try
        {
            await _facturaService.EliminarDetalleAsync(detalle.IdDetalleFactura);
            _huboCambios = true;
            await CargarFacturaAsync();
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

    private async Task EmitirAsync()
    {
        var confirmar = MessageBox.Show(
            $"¿Confirmás la emisión de la factura {NumeroFactura}? Una vez emitida no se puede volver a editar.",
            "Confirmar emisión",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes) return;

        try
        {
            await _facturaService.EmitirAsync(_idFactura);
            _huboCambios = true;
            await CargarFacturaAsync();
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo emitir la factura. Verifique que SQL Server esté iniciado e intente de nuevo.";
        }
    }
}