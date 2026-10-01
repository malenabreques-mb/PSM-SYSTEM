using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;
using PSMSystem.Views;

namespace PSMSystem.ViewsModels;

public class SaldosPendientesViewModel : ViewModelBase
{
    private readonly FacturaService _facturaService;
    private readonly RepuestoService _repuestoService;
    private readonly PagoService _pagoService;

    private bool _cargando;
    private int _total;

    public SaldosPendientesViewModel(FacturaService facturaService, RepuestoService repuestoService, PagoService pagoService)
    {
        _facturaService = facturaService;
        _repuestoService = repuestoService;
        _pagoService = pagoService;

        Facturas = new ObservableCollection<Factura>();
        VerDetalleCommand = new RelayCommand(parametro => AbrirDetalle(parametro as Factura));

        _ = CargarAsync();
    }

    public ObservableCollection<Factura> Facturas { get; }
    public bool Cargando { get => _cargando; private set => SetProperty(ref _cargando, value); }
    public bool SinSaldosPendientes => !Cargando && _total == 0;

    public ICommand VerDetalleCommand { get; }

    private async Task CargarAsync()
    {
        Cargando = true;
        try
        {
            var facturas = await _facturaService.ObtenerPendientesDePagoAsync();

            Facturas.Clear();
            foreach (var factura in facturas)
                Facturas.Add(factura);

            _total = Facturas.Count;
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
            OnPropertyChanged(nameof(SinSaldosPendientes));
        }
    }

    private void AbrirDetalle(Factura? factura)
    {
        if (factura is null) return;

        var dialogo = new FacturaDetalleView
        {
            DataContext = new FacturaDetalleViewModel(_facturaService, _repuestoService, _pagoService, factura.IdFactura),
            Owner = Application.Current.MainWindow
        };

        if (dialogo.ShowDialog() == true)
            _ = CargarAsync();
    }
}