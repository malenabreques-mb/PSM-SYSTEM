namespace PSMSystem.Models;

public class DetallePresupuesto
{
    public int IdDetallePresupuesto { get; set; }
    public int IdPresupuesto { get; set; }
    public int? IdRepuesto { get; set; }
    public required string TipoItem { get; set; }
    public string? Descripcion { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public string ItemDescripcion => IdRepuesto.HasValue ? (Repuesto?.Nombre ?? "Repuesto") : (Descripcion ?? "-");

    public Presupuesto? Presupuesto { get; set; }
    public Repuesto? Repuesto { get; set; }
}

