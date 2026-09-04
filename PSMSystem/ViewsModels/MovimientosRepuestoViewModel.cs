using System.Collections.ObjectModel;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class MovimientosRepuestoViewModel : ViewModelBase
{
    private readonly MovimientoStockService _movimientoService;
    private readonly RepuestoService _repuestoService;
    private readonly int _idRepuesto;

    private int _idTipoSeleccionado;
    private string _cantidad = string.Empty;
    private string _observacion = string.Empty;
    private DateTime? _fecha = DateTime.Today;
    private int _stockActual;
    private string? _mensajeError;
    private bool _huboCambios;

    public MovimientosRepuestoViewModel(
        MovimientoStockService movimientoService, RepuestoService repuestoService, Repuesto repuesto)
    {
        _movimientoService = movimientoService;
        _repuestoService = repuestoService;
        _idRepuesto = repuesto.IdRepuesto;

        Repuesto = repuesto;
        _stockActual = repuesto.StockActual;

        Movimientos = new ObservableCollection<MovimientoStock>();
        Tipos = new ObservableCollection<TipoMovimientoStock>();

        RegistrarCommand = new AsyncRelayCommand(async _ => await RegistrarAsync());
        CerrarCommand = new RelayCommand(_ => SolicitudCierre?.Invoke(this, _huboCambios));

        _ = InicializarAsync();
    }

    public Repuesto Repuesto { get; }
    public ObservableCollection<MovimientoStock> Movimientos { get; }
    public ObservableCollection<TipoMovimientoStock> Tipos { get; }

    public int StockActual { get => _stockActual; private set => SetProperty(ref _stockActual, value); }

    public int IdTipoSeleccionado { get => _idTipoSeleccionado; set => SetProperty(ref _idTipoSeleccionado, value); }
    public string Cantidad { get => _cantidad; set => SetProperty(ref _cantidad, value); }
    public string Observacion { get => _observacion; set => SetProperty(ref _observacion, value); }
    public DateTime? Fecha { get => _fecha; set => SetProperty(ref _fecha, value); }

    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public ICommand RegistrarCommand { get; }
    public ICommand CerrarCommand { get; }

    public event EventHandler<bool>? SolicitudCierre;

    private async Task InicializarAsync()
    {
        var tipos = await _movimientoService.ObtenerTiposAsync();
        Tipos.Clear();
        foreach (var tipo in tipos)
            Tipos.Add(tipo);

        if (Tipos.Count > 0)
            IdTipoSeleccionado = Tipos[0].IdTipoMovimiento;

        await CargarMovimientosAsync();
    }

    private async Task CargarMovimientosAsync()
    {
        var movimientos = await _movimientoService.ObtenerPorRepuestoAsync(_idRepuesto);
        Movimientos.Clear();
        foreach (var movimiento in movimientos)
            Movimientos.Add(movimiento);
    }

    private async Task RegistrarAsync()
    {
        MensajeError = null;

        if (!Fecha.HasValue)
        {
            MensajeError = "Seleccioná una fecha.";
            return;
        }

        if (!int.TryParse(Cantidad, out var cantidadValor) || cantidadValor <= 0)
        {
            MensajeError = "Ingresá una cantidad válida, mayor a cero.";
            return;
        }

        var movimiento = new MovimientoStock
        {
            IdRepuesto = _idRepuesto,
            IdTipoMovimiento = IdTipoSeleccionado,
            Cantidad = cantidadValor,
            Fecha = DateOnly.FromDateTime(Fecha.Value),
            Observacion = string.IsNullOrWhiteSpace(Observacion) ? null : Observacion.Trim()
        };

        try
        {
            await _movimientoService.RegistrarAsync(movimiento);
            _huboCambios = true;
            Cantidad = string.Empty;
            Observacion = string.Empty;

            await CargarMovimientosAsync();

            var actualizado = await _repuestoService.ObtenerPorIdAsync(_idRepuesto);
            if (actualizado is not null)
                StockActual = actualizado.StockActual;
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