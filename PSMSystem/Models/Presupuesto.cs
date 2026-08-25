using System;
namespace PSMSystem.Models;

public class Presupuesto
{
    public int IdPresupuesto { get; set; }
    public int IdOrdenTrabajo { get; set; }
    public int IdEstadoPresupuesto { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? Total { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Observaciones { get; set; }

    public OrdenTrabajo? OrdenTrabajo { get; set; }
    public EstadoPresupuesto? EstadoPresupuesto { get; set; }
    public ICollection<DetallePresupuesto> Detalles { get; set; } = new List<DetallePresupuesto>();
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
