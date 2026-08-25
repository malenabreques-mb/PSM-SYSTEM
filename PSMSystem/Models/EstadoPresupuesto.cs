using System;

namespace PSMSystem.Models;

public class EstadoPresupuesto
{
    public int IdEstadoPresupuesto { get; set; }
    public required string Nombre { get; set; }

    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
}