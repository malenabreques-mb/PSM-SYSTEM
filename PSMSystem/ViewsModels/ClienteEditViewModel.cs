using System;

using System.Windows.Input;
using PSMSystem.Commands;
using PSMSystem.Models;
using PSMSystem.Services;

namespace PSMSystem.ViewsModels;

public class ClienteEditViewModel : ViewModelBase
{
    private readonly ClienteService _clienteService;
    private readonly int? _idClienteExistente;

    private string _nombre = string.Empty;
    private string _apellido = string.Empty;
    private string _telefono = string.Empty;
    private string _direccion = string.Empty;
    private string? _mensajeError;

    public ClienteEditViewModel(ClienteService clienteService, Cliente? clienteAEditar)
    {
        _clienteService = clienteService;

        if (clienteAEditar is not null)
        {
            _idClienteExistente = clienteAEditar.IdCliente;
            _nombre = clienteAEditar.Nombre;
            _apellido = clienteAEditar.Apellido;
            _telefono = clienteAEditar.Telefono ?? string.Empty;
            _direccion = clienteAEditar.Direccion ?? string.Empty;
        }

        GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
        CancelarCommand = new RelayCommand(_ => CerrarVentana(false));
    }

    public bool EsEdicion => _idClienteExistente.HasValue;
    public string Titulo => EsEdicion ? "Editar cliente" : "Agregar cliente";

    public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
    public string Apellido { get => _apellido; set => SetProperty(ref _apellido, value); }
    public string Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }
    public string Direccion { get => _direccion; set => SetProperty(ref _direccion, value); }

    public string? MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

    public ICommand GuardarCommand { get; }
    public ICommand CancelarCommand { get; }


    public event EventHandler<bool>? SolicitudCierre;

    private async Task GuardarAsync()
    {
        MensajeError = null;

        var cliente = new Cliente
        {
            IdCliente = _idClienteExistente ?? 0,
            Nombre = Nombre.Trim(),
            Apellido = Apellido.Trim(),
            Telefono = Telefono.Trim(),
            Direccion = Direccion.Trim()
        };

        try
        {
            if (EsEdicion)
                await _clienteService.ActualizarAsync(cliente);
            else
                await _clienteService.CrearAsync(cliente);

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
