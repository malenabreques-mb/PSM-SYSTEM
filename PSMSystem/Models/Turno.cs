using System;

namespace PSMSystem.Models;

public class Turno
{
    public int IdTurno { get; set; }
    public int IdCliente { get; set; }
    public int IdVehiculo { get; set; }
    public int IdEstadoTurno { get; set; }
    public DateOnly Fecha { get; set; }
    public required string Motivo { get; set; }
    public DateOnly? FechaCreacion { get; set; }

    public Cliente? Cliente { get; set; }
    public Vehiculo? Vehiculo { get; set; }
    public EstadoTurno? EstadoTurno { get; set; }
    public ICollection<OrdenTrabajo> OrdenesTrabajo { get; set; } = new List<OrdenTrabajo>();
}