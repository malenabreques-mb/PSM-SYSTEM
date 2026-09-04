using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;
using PSMSystem.Views;

namespace PSMSystem.ViewsModels;

public class RepuestosViewModel : ViewModelBase
{
    private const int TamanioPagina = 10;

    private readonly RepuestoService _repuestoService;
    private readonly MovimientoStockService _movimientoService;

    private string? _textoBusqueda;
    private bool _soloStockBajo;
    private int _paginaActual = 1;
    private int _totalRepuestos;
    private bool _cargando;

    public RepuestosViewModel(RepuestoService repuestoService, MovimientoStockService movimientoService)
    {
        _repuestoService = repuestoService;
        _movimientoService = movimientoService;

        Repuestos = new ObservableCollection<Repuesto>();

        BuscarCommand = new AsyncRelayCommand(async _ => await CambiarPaginaAsync(1));
        AgregarRepuestoCommand = new RelayCommand(_ => AbrirDialogoRepuesto(null));
        EditarRepuestoCommand = new RelayCommand(parametro => AbrirDialogoRepuesto(parametro as Repuesto));
        VerMovimientosCommand = new RelayCommand(parametro => AbrirMovimientos(parametro as Repuesto));
        DesactivarReactivarRepuestoCommand =
            new AsyncRelayCommand(async parametro => await DesactivarReactivarAsync(parametro as Repuesto));
        PaginaAnteriorCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual - 1), _ => _paginaActual > 1);
        PaginaSiguienteCommand = new AsyncRelayCommand(
            async _ => await CambiarPaginaAsync(_paginaActual + 1), _ => _paginaActual < TotalPaginas);

        _ = CambiarPaginaAsync(1);
    }

    public ObservableCollection<Repuesto> Repuestos { get; }

    public string? TextoBusqueda { get => _textoBusqueda; set => SetProperty(ref _textoBusqueda, value); }

    public bool SoloStockBajo
    {
        get => _soloStockBajo;
        set
        {
            if (SetProperty(ref _soloStockBajo, value))
                _ = CambiarPaginaAsync(1);
        }
    }

    public int PaginaActual { get => _paginaActual; private set => SetProperty(ref _paginaActual, value); }
    public int TotalPaginas => _totalRepuestos == 0 ? 1 : (int)Math.Ceiling(_totalRepuestos / (double)TamanioPagina);
    public bool Cargando { get => _cargando; private set => SetProperty(ref _cargando, value); }

    private bool HayFiltroActivo => !string.IsNullOrWhiteSpace(TextoBusqueda) || SoloStockBajo;
    public bool SinRepuestosEnAbsoluto => !Cargando && _totalRepuestos == 0 && !HayFiltroActivo;
    public bool SinResultadosFiltrados => !Cargando && _totalRepuestos == 0 && HayFiltroActivo;

    public ICommand BuscarCommand { get; }
    public ICommand AgregarRepuestoCommand { get; }
    public ICommand EditarRepuestoCommand { get; }
    public ICommand VerMovimientosCommand { get; }
    public ICommand DesactivarReactivarRepuestoCommand { get; }
    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSiguienteCommand { get; }

    private async Task CambiarPaginaAsync(int pagina)
    {
        Cargando = true;
        try
        {
            var resultado = await _repuestoService.BuscarRepuestosAsync(TextoBusqueda, SoloStockBajo, pagina, TamanioPagina);

            Repuestos.Clear();
            foreach (var repuesto in resultado.Items)
                Repuestos.Add(repuesto);

            _totalRepuestos = resultado.Total;
            PaginaActual = pagina;
            OnPropertyChanged(nameof(TotalPaginas));
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
        finally
        {
            Cargando = false;
            OnPropertyChanged(nameof(SinRepuestosEnAbsoluto));
            OnPropertyChanged(nameof(SinResultadosFiltrados));
        }
    }

    private void AbrirDialogoRepuesto(Repuesto? repuestoAEditar)
    {
        var dialogo = new RepuestoEditView
        {
            DataContext = new RepuestoEditViewModel(_repuestoService, repuestoAEditar),
            Owner = Application.Current.MainWindow
        };

        if (dialogo.ShowDialog() == true)
            _ = CambiarPaginaAsync(PaginaActual);
    }

    private void AbrirMovimientos(Repuesto? repuesto)
    {
        if (repuesto is null) return;

        var dialogo = new MovimientosRepuestoView
        {
            DataContext = new MovimientosRepuestoViewModel(_movimientoService, _repuestoService, repuesto),
            Owner = Application.Current.MainWindow
        };

        if (dialogo.ShowDialog() == true)
            _ = CambiarPaginaAsync(PaginaActual);
    }

    private async Task DesactivarReactivarAsync(Repuesto? repuesto)
    {
        if (repuesto is null) return;

        try
        {
            if (repuesto.Activo == true)
            {
                var confirmar = MessageBox.Show(
                    $"¿Seguro que querés desactivar {repuesto.Nombre}?",
                    "Confirmar desactivación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmar != MessageBoxResult.Yes) return;

                await _repuestoService.DesactivarAsync(repuesto.IdRepuesto);
            }
            else
            {
                await _repuestoService.ReactivarAsync(repuesto.IdRepuesto);
            }

            await CambiarPaginaAsync(PaginaActual);
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private static void MostrarError(Exception ex)
    {
        var mensaje = ex is ReglaNegocioException
            ? ex.Message
            : "No se pudo completar la operación. Verifique que SQL Server esté iniciado e intente de nuevo.";

        MessageBox.Show(mensaje, "PSM System", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}