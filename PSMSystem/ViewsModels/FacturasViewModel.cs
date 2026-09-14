using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;
using PSMSystem.Views;

namespace PSMSystem.ViewsModels;

public class FacturasViewModel : ViewModelBase
{
    private const int TamanioPagina = 10;

    private readonly FacturaService _facturaService;
    private readonly RepuestoService _repuestoService;

    private string? _textoBusqueda;
    private EstadoFactura? _estadoFiltro;
    private DateTime? _fechaFiltro;
    private int _paginaActual = 1;
    private int _totalFacturas;
    private bool _cargando;

    public FacturasViewModel(FacturaService facturaService, RepuestoService repuestoService)
    {
        _facturaService = facturaService;
        _repuestoService = repuestoService;

        Facturas = new ObservableCollection<Factura>();
        EstadosFiltro = new ObservableCollection<EstadoFactura>
        {
            new EstadoFactura { IdEstadoFactura = 0, Nombre = "Todos" }
        };
        _estadoFiltro = EstadosFiltro[0];

        BuscarCommand = new AsyncRelayCommand(async _ => await CambiarPaginaAsync(1));
        VerDetalleCommand = new RelayCommand(parametro => AbrirDetalle(parametro as Factura));
        PaginaAnteriorCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual - 1), _ => _paginaActual > 1);
        PaginaSiguienteCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual + 1), _ => _paginaActual < TotalPaginas);

        _ = CargarEstadosAsync();
        _ = CambiarPaginaAsync(1);
    }

    public ObservableCollection<Factura> Facturas { get; }
    public ObservableCollection<EstadoFactura> EstadosFiltro { get; }

    public string? TextoBusqueda { get => _textoBusqueda; set => SetProperty(ref _textoBusqueda, value); }

    public EstadoFactura? EstadoFiltro
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
    public int TotalPaginas => _totalFacturas == 0 ? 1 : (int)Math.Ceiling(_totalFacturas / (double)TamanioPagina);
    public bool Cargando { get => _cargando; private set => SetProperty(ref _cargando, value); }
    public bool SinResultados => !Cargando && _totalFacturas == 0;

    public ICommand BuscarCommand { get; }
    public ICommand VerDetalleCommand { get; }
    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSiguienteCommand { get; }

    private async Task CargarEstadosAsync()
    {
        var estados = await _facturaService.ObtenerEstadosAsync();
        foreach (var estado in estados)
            EstadosFiltro.Add(estado);
    }

    private async Task CambiarPaginaAsync(int pagina)
    {
        Cargando = true;
        try
        {
            var idEstado = EstadoFiltro is { IdEstadoFactura: > 0 } ? EstadoFiltro.IdEstadoFactura : (int?)null;
            var fecha = FechaFiltro.HasValue ? DateOnly.FromDateTime(FechaFiltro.Value) : (DateOnly?)null;

            var resultado = await _facturaService.BuscarFacturasAsync(TextoBusqueda, idEstado, fecha, pagina, TamanioPagina);

            Facturas.Clear();
            foreach (var factura in resultado.Items)
                Facturas.Add(factura);

            _totalFacturas = resultado.Total;
            PaginaActual = pagina;
            OnPropertyChanged(nameof(TotalPaginas));
        }
        catch (Exception)
        {
            MessageBox.Show(
                "No se pudo completar la operación. Verifique que SQL Server esté iniciado e intente de nuevo.",
                "PSM System", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Cargando = false;
            OnPropertyChanged(nameof(SinResultados));
        }
    }

    private void AbrirDetalle(Factura? factura)
    {
        if (factura is null) return;

        var dialogo = new FacturaDetalleView
        {
            DataContext = new FacturaDetalleViewModel(_facturaService, _repuestoService, factura.IdFactura),
            Owner = Application.Current.MainWindow
        };

        if (dialogo.ShowDialog() == true)
            _ = CambiarPaginaAsync(PaginaActual);
    }
}