using System;

namespace PSMSystem.Models;

public class OrdenTrabajo
{
    public int IdOrdenTrabajo { get; set; }
    public int IdCliente { get; set; }
    public int IdVehiculo { get; set; }
    public int? IdTurno { get; set; }
    public int IdEstadoOrden { get; set; }
    public required string MotivoIngreso { get; set; }
    public required string DiagnosticoInicial { get; set; }
    public string? Observaciones { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public DateOnly? FechaEntregaEstimada { get; set; }
    public DateOnly? FechaCreacion { get; set; }
    public DateOnly? FechaModificacion { get; set; }

    public Cliente? Cliente { get; set; }
    public Vehiculo? Vehiculo { get; set; }
    public Turno? Turno { get; set; }
    public EstadoOrdenTrabajo? EstadoOrden { get; set; }
    public ICollection<AvanceTrabajo> AvancesTrabajo { get; set; } = new List<AvanceTrabajo>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
}
