using System;

namespace PSMSystem.Models;

public class Repuesto
{
    public int IdRepuesto { get; set; }
    public required string Nombre { get; set; }
    public string? Categoria { get; set; }
    public string? Proveedor { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public int StockActual { get; set; }
    public int? StockMinimo { get; set; }
    public bool? Activo { get; set; }
    public DateOnly? FechaCreacion { get; set; }

    public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
    public ICollection<DetallePresupuesto> DetallesPresupuesto { get; set; } = new List<DetallePresupuesto>();
    public ICollection<DetalleFactura> DetallesFactura { get; set; } = new List<DetalleFactura>();
}
