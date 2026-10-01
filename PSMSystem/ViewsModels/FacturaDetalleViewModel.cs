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
    private readonly PagoService _pagoService;
    private readonly int _idFactura;

    private Factura? _factura;
    private string _tipoItemSeleccionado;
    private int _idRepuestoSeleccionado;
    private string _descripcionItem = string.Empty;
    private string _cantidad = string.Empty;
    private string _precioUnitario = string.Empty;
    private string _medioPagoSeleccionado;
    private string _montoPago = string.Empty;
    private string? _mensajeError;
    private bool _huboCambios;
    private DetalleFactura? _detalleEnEdicion;
    private string _nuevaCantidad = string.Empty;

    public FacturaDetalleViewModel(
        FacturaService facturaService, RepuestoService repuestoService, PagoService pagoService, int idFactura)
    {
        _facturaService = facturaService;
        _repuestoService = repuestoService;
        _pagoService = pagoService;
        _idFactura = idFactura;

        Detalles = new ObservableCollection<DetalleFactura>();
        Pagos = new ObservableCollection<Pago>();
        RepuestosDisponibles = new ObservableCollection<Repuesto>();
        TiposItem = TiposItemPresupuesto.Todas;
        _tipoItemSeleccionado = TiposItem[0];
        MediosDePago = Helpers.MediosDePago.Todos;
        _medioPagoSeleccionado = MediosDePago[0];

        AgregarItemCommand = new AsyncRelayCommand(async _ => await AgregarItemAsync());
        EliminarItemCommand = new AsyncRelayCommand(async parametro => await EliminarItemAsync(parametro as DetalleFactura));
        EmitirCommand = new AsyncRelayCommand(async _ => await EmitirAsync());
        RegistrarPagoCommand = new AsyncRelayCommand(async _ => await RegistrarPagoAsync());
        IniciarEdicionCantidadCommand = new RelayCommand(parametro => IniciarEdicionCantidad(parametro as DetalleFactura));
        GuardarCantidadCommand = new AsyncRelayCommand(async _ => await GuardarCantidadAsync());
        CancelarEdicionCantidadCommand = new RelayCommand(_ => CancelarEdicionCantidad());
        CerrarCommand = new RelayCommand(_ => SolicitudCierre?.Invoke(this, _huboCambios));

        _ = InicializarAsync();
    }

    public ObservableCollection<DetalleFactura> Detalles { get; }
    public ObservableCollection<Pago> Pagos { get; }
    public ObservableCollection<Repuesto> RepuestosDisponibles { get; }
    public IReadOnlyList<string> TiposItem { get; }
    public IReadOnlyList<string> MediosDePago { get; }

    public string NumeroFactura => _factura?.NumeroFactura ?? "";
    public string EstadoActual => _factura?.EstadoFactura?.Nombre ?? "";
    public bool PendienteDeEmision => EstadoActual == "Pendiente de emisión";
    public bool PendienteDePago => EstadoActual == "Pendiente de pago";

    public string ClienteYVehiculo => _factura?.Presupuesto?.OrdenTrabajo is { } orden
        ? $"{orden.Cliente?.NombreCompleto} — {orden.Vehiculo?.DescripcionCombo}"
        : "";

    public decimal Subtotal => _factura?.Subtotal ?? 0;
    public decimal Iva => _factura?.Iva ?? 0;
    public decimal Total => _factura?.Total ?? 0;
    public decimal TotalPagado => Pagos.Sum(p => p.Monto);
    public decimal SaldoPendiente => Total - TotalPagado;

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
    public string MedioPagoSeleccionado { get => _medioPagoSeleccionado; set => SetProperty(ref _medioPagoSeleccionado, value); }
    public string MontoPago { get => _montoPago; set => SetProperty(ref _montoPago, value); }

    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public DetalleFactura? DetalleEnEdicion
    {
        get => _detalleEnEdicion;
        set
        {
            if (SetProperty(ref _detalleEnEdicion, value))
                OnPropertyChanged(nameof(EstaEditandoCantidad));
        }
    }

    public bool EstaEditandoCantidad => DetalleEnEdicion is not null;

    public string NuevaCantidad { get => _nuevaCantidad; set => SetProperty(ref _nuevaCantidad, value); }

    public ICommand AgregarItemCommand { get; }
    public ICommand EliminarItemCommand { get; }
    public ICommand EmitirCommand { get; }
    public ICommand RegistrarPagoCommand { get; }
    public ICommand IniciarEdicionCantidadCommand { get; }
    public ICommand GuardarCantidadCommand { get; }
    public ICommand CancelarEdicionCantidadCommand { get; }
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

        var pagos = await _pagoService.ObtenerPorFacturaAsync(_idFactura);
        Pagos.Clear();
        foreach (var pago in pagos)
            Pagos.Add(pago);

        MontoPago = SaldoPendiente.ToString();

        OnPropertyChanged(nameof(NumeroFactura));
        OnPropertyChanged(nameof(EstadoActual));
        OnPropertyChanged(nameof(PendienteDeEmision));
        OnPropertyChanged(nameof(PendienteDePago));
        OnPropertyChanged(nameof(ClienteYVehiculo));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Iva));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(TotalPagado));
        OnPropertyChanged(nameof(SaldoPendiente));
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

    private void IniciarEdicionCantidad(DetalleFactura? detalle)
    {
        if (detalle is null) return;

        MensajeError = null;
        DetalleEnEdicion = detalle;
        NuevaCantidad = detalle.Cantidad?.ToString() ?? "";
    }

    private void CancelarEdicionCantidad()
    {
        DetalleEnEdicion = null;
        NuevaCantidad = string.Empty;
    }

    private async Task GuardarCantidadAsync()
    {
        if (DetalleEnEdicion is null) return;

        MensajeError = null;

        if (!int.TryParse(NuevaCantidad, out var cantidadValor))
        {
            MensajeError = "Verifique los datos ingresados: la cantidad no es un número válido.";
            return;
        }

        try
        {
            await _facturaService.ActualizarCantidadDetalleAsync(DetalleEnEdicion.IdDetalleFactura, cantidadValor);
            _huboCambios = true;
            DetalleEnEdicion = null;
            NuevaCantidad = string.Empty;
            await CargarFacturaAsync();
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo actualizar la cantidad. Verifique que SQL Server esté iniciado e intente de nuevo.";
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

    private async Task RegistrarPagoAsync()
    {
        MensajeError = null;

        if (!decimal.TryParse(MontoPago, out var montoValor))
        {
            MensajeError = "Verifique los datos ingresados: el monto no es un número válido.";
            return;
        }

        var pago = new Pago
        {
            IdFactura = _idFactura,
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Monto = montoValor,
            MedioPago = MedioPagoSeleccionado
        };

        try
        {
            await _pagoService.RegistrarAsync(pago);
            _huboCambios = true;
            await CargarFacturaAsync();

            var mensaje = PendienteDePago
                ? $"Pago registrado. Saldo pendiente: {SaldoPendiente:C2}."
                : "Pago registrado. La factura queda marcada como Pagada.";
            MessageBox.Show(mensaje, "PSM System", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception)
        {
            MensajeError = "No se pudo registrar el pago. Verifique que SQL Server esté iniciado e intente de nuevo.";
        }
    }
}