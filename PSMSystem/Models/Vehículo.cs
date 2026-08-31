using System;
namespace PSMSystem.Models;

public class Vehiculo
{
    public int IdVehiculo { get; set; }
    public int IdCliente { get; set; }
    public required string Marca { get; set; }
    public required string Modelo { get; set; }
    public int? Anio { get; set; }
    public required string Patente { get; set; }
    public bool Activo { get; set; } = true;
    public DateOnly? FechaCreacion { get; set; }
    public DateOnly? FechaModificacion { get; set; }

    public string ClienteDescripcion => Cliente is null ? "-" : Cliente.NombreCompleto;
    public string DescripcionCombo => $"{Patente} - {Marca} {Modelo}";

    public Cliente? Cliente { get; set; }
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
    public ICollection<OrdenTrabajo> OrdenesTrabajo { get; set; } = new List<OrdenTrabajo>();
}