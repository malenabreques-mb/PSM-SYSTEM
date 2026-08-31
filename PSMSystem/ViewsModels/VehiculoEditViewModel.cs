using System;

using System.Collections.ObjectModel;
using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class VehiculoEditViewModel : ViewModelBase
{
    private readonly VehiculoService _vehiculoService;
    private readonly int? _idVehiculoExistente;

    private string _marca = string.Empty;
    private string _modelo = string.Empty;
    private string _patente = string.Empty;
    private int? _anio;
    private int _idClienteSeleccionado;
    private string? _mensajeError;

    public VehiculoEditViewModel(VehiculoService vehiculoService, ClienteService clienteService, Vehiculo? vehiculoAEditar)
    {
        _vehiculoService = vehiculoService;

        ClientesDisponibles = new ObservableCollection<Cliente>();

        if (vehiculoAEditar is not null)
        {
            _idVehiculoExistente = vehiculoAEditar.IdVehiculo;
            _marca = vehiculoAEditar.Marca;
            _modelo = vehiculoAEditar.Modelo;
            _patente = vehiculoAEditar.Patente;
            _anio = vehiculoAEditar.Anio;
            _idClienteSeleccionado = vehiculoAEditar.IdCliente;
        }

        GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
        CancelarCommand = new RelayCommand(_ => CerrarVentana(false));

        _ = CargarClientesAsync(clienteService);
    }

    public bool EsEdicion => _idVehiculoExistente.HasValue;
    public string Titulo => EsEdicion ? "Editar vehículo" : "Agregar vehículo";

    public ObservableCollection<Cliente> ClientesDisponibles { get; }

    public string Marca { get => _marca; set => SetProperty(ref _marca, value); }
    public string Modelo { get => _modelo; set => SetProperty(ref _modelo, value); }
    public string Patente { get => _patente; set => SetProperty(ref _patente, value); }
    public int? Anio { get => _anio; set => SetProperty(ref _anio, value); }

    public int IdClienteSeleccionado
    {
        get => _idClienteSeleccionado;
        set => SetProperty(ref _idClienteSeleccionado, value);
    }

    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public ICommand GuardarCommand { get; }
    public ICommand CancelarCommand { get; }

    public event EventHandler<bool>? SolicitudCierre;

    private async Task CargarClientesAsync(ClienteService clienteService)
    {
        var clientes = await clienteService.ObtenerClientesActivosAsync();

        ClientesDisponibles.Clear();
        foreach (var cliente in clientes)
            ClientesDisponibles.Add(cliente);
    }

    private async Task GuardarAsync()
    {
        MensajeError = null;

        var vehiculo = new Vehiculo
        {
            IdVehiculo = _idVehiculoExistente ?? 0,
            Marca = Marca.Trim(),
            Modelo = Modelo.Trim(),
            Patente = Patente.Trim(),
            Anio = Anio,
            IdCliente = IdClienteSeleccionado
        };

        try
        {
            if (EsEdicion)
                await _vehiculoService.ActualizarAsync(vehiculo);
            else
                await _vehiculoService.CrearAsync(vehiculo);

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
