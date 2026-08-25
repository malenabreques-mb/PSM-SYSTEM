using System;

namespace PSMSystem.Models;

public class EstadoOrdenTrabajo
{
    public int IdEstadoOrden { get; set; }
    public required string Nombre { get; set; }

    public ICollection<OrdenTrabajo> OrdenesTrabajo { get; set; } = new List<OrdenTrabajo>();
}
