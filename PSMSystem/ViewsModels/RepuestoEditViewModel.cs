using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class RepuestoEditViewModel : ViewModelBase
{
    private readonly RepuestoService _repuestoService;
    private readonly int? _idRepuestoExistente;

    private string _nombre = string.Empty;
    private string _categoria = string.Empty;
    private string _proveedor = string.Empty;
    private string _precioUnitario = string.Empty;
    private string _stockMinimo = string.Empty;
    private string _stockInicial = "0";
    private string? _mensajeError;

    public RepuestoEditViewModel(RepuestoService repuestoService, Repuesto? repuestoAEditar)
    {
        _repuestoService = repuestoService;

        EsEdicion = repuestoAEditar is not null;

        if (repuestoAEditar is not null)
        {
            _idRepuestoExistente = repuestoAEditar.IdRepuesto;
            _nombre = repuestoAEditar.Nombre;
            _categoria = repuestoAEditar.Categoria ?? string.Empty;
            _proveedor = repuestoAEditar.Proveedor ?? string.Empty;
            _precioUnitario = repuestoAEditar.PrecioUnitario?.ToString() ?? string.Empty;
            _stockMinimo = repuestoAEditar.StockMinimo?.ToString() ?? string.Empty;
            _stockInicial = repuestoAEditar.StockActual.ToString();
        }

        GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
        CancelarCommand = new RelayCommand(_ => CerrarVentana(false));
    }

    public bool EsEdicion { get; }
    public string Titulo => EsEdicion ? "Editar repuesto" : "Agregar repuesto";
    public bool PuedeEditarStockInicial => !EsEdicion;

    public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
    public string Categoria { get => _categoria; set => SetProperty(ref _categoria, value); }
    public string Proveedor { get => _proveedor; set => SetProperty(ref _proveedor, value); }
    public string PrecioUnitario { get => _precioUnitario; set => SetProperty(ref _precioUnitario, value); }
    public string StockMinimo { get => _stockMinimo; set => SetProperty(ref _stockMinimo, value); }
    public string StockInicial { get => _stockInicial; set => SetProperty(ref _stockInicial, value); }

    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public ICommand GuardarCommand { get; }
    public ICommand CancelarCommand { get; }

    public event EventHandler<bool>? SolicitudCierre;

    private async Task GuardarAsync()
    {
        MensajeError = null;

        decimal? precio = null;
        if (!string.IsNullOrWhiteSpace(PrecioUnitario))
        {
            if (!decimal.TryParse(PrecioUnitario, out var precioValor))
            {
                MensajeError = "El precio unitario no es un número válido.";
                return;
            }
            precio = precioValor;
        }

        int? stockMinimo = null;
        if (!string.IsNullOrWhiteSpace(StockMinimo))
        {
            if (!int.TryParse(StockMinimo, out var stockMinimoValor))
            {
                MensajeError = "El stock mínimo no es un número válido.";
                return;
            }
            stockMinimo = stockMinimoValor;
        }

        if (!int.TryParse(StockInicial, out var stockInicialValor))
        {
            MensajeError = "El stock inicial no es un número válido.";
            return;
        }

        var repuesto = new Repuesto
        {
            IdRepuesto = _idRepuestoExistente ?? 0,
            Nombre = Nombre.Trim(),
            Categoria = string.IsNullOrWhiteSpace(Categoria) ? null : Categoria.Trim(),
            Proveedor = string.IsNullOrWhiteSpace(Proveedor) ? null : Proveedor.Trim(),
            PrecioUnitario = precio,
            StockMinimo = stockMinimo,
            StockActual = stockInicialValor
        };

        try
        {
            if (EsEdicion)
                await _repuestoService.ActualizarAsync(repuesto);
            else
                await _repuestoService.CrearAsync(repuesto);

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