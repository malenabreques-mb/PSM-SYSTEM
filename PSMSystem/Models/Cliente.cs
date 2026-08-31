using System;

namespace PSMSystem.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
    public DateOnly? FechaCreacion { get; set; }
    public DateOnly? FechaModificacion { get; set; }


    public string NombreCompleto => $"{Apellido}, {Nombre}";

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
    public ICollection<OrdenTrabajo> OrdenesTrabajo { get; set; } = new List<OrdenTrabajo>();
}