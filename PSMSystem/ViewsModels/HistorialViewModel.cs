using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;
using PSMSystem.Views;

namespace PSMSystem.ViewsModels;

public class HistorialViewModel : ViewModelBase
{
    private const int TamanioPagina = 10;

    private readonly OrdenTrabajoService _ordenService;
    private readonly AvanceTrabajoService _avanceService;

    private string? _textoBusqueda;
    private DateTime? _fechaFiltro;
    private int _paginaActual = 1;
    private int _totalOrdenes;
    private bool _cargando;

    public HistorialViewModel(OrdenTrabajoService ordenService, AvanceTrabajoService avanceService)
    {
        _ordenService = ordenService;
        _avanceService = avanceService;

        Ordenes = new ObservableCollection<OrdenTrabajo>();

        BuscarCommand = new AsyncRelayCommand(async _ => await CambiarPaginaAsync(1));
        VerDetalleCommand = new RelayCommand(parametro => VerDetalle(parametro as OrdenTrabajo));
        PaginaAnteriorCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual - 1), _ => _paginaActual > 1);
        PaginaSiguienteCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual + 1), _ => _paginaActual < TotalPaginas);

        _ = CambiarPaginaAsync(1);
    }

    public ObservableCollection<OrdenTrabajo> Ordenes { get; }

    public string? TextoBusqueda { get => _textoBusqueda; set => SetProperty(ref _textoBusqueda, value); }

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
    public int TotalPaginas => _totalOrdenes == 0 ? 1 : (int)Math.Ceiling(_totalOrdenes / (double)TamanioPagina);
    public bool Cargando { get => _cargando; private set => SetProperty(ref _cargando, value); }
    public bool SinResultados => !Cargando && _totalOrdenes == 0;

    public ICommand BuscarCommand { get; }
    public ICommand VerDetalleCommand { get; }
    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSiguienteCommand { get; }

    private async Task CambiarPaginaAsync(int pagina)
    {
        Cargando = true;
        try
        {
            var fecha = FechaFiltro.HasValue ? DateOnly.FromDateTime(FechaFiltro.Value) : (DateOnly?)null;

            var resultado = await _ordenService.BuscarHistorialAsync(TextoBusqueda, fecha, pagina, TamanioPagina);

            Ordenes.Clear();
            foreach (var orden in resultado.Items)
                Ordenes.Add(orden);

            _totalOrdenes = resultado.Total;
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

    private void VerDetalle(OrdenTrabajo? orden)
    {
        if (orden is null) return;

        var dialogo = new AvancesOrdenView
        {
            DataContext = new AvancesOrdenViewModel(_avanceService, orden, soloLectura: true),
            Owner = Application.Current.MainWindow
        };

        dialogo.ShowDialog();
    }
}