using System;

namespace PSMSystem.Models;

public class DetallePresupuesto
{
    public int IdDetallePresupuesto { get; set; }
    public int IdPresupuesto { get; set; }
    public int IdRepuesto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Presupuesto? Presupuesto { get; set; }
    public Repuesto? Repuesto { get; set; }
}
