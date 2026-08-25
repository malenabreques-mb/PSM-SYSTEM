using System;

namespace PSMSystem.Models;

public class DetalleFactura
{
    public int IdDetalleFactura { get; set; }
    public int IdFactura { get; set; }
    public int? IdRepuesto { get; set; }
    public string? Descripcion { get; set; }
    public int? Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? Subtotal { get; set; }

    public Factura? Factura { get; set; }
    public Repuesto? Repuesto { get; set; }
}