using System;

namespace PSMSystem.Models;

public class Factura
{
    public int IdFactura { get; set; }
    public int IdPresupuesto { get; set; }
    public int IdEstadoFactura { get; set; }
    public required string NumeroFactura { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? Iva { get; set; }
    public decimal? Total { get; set; }
    public DateOnly? FechaEmision { get; set; }

    public Presupuesto? Presupuesto { get; set; }
    public EstadoFactura? EstadoFactura { get; set; }
    public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
