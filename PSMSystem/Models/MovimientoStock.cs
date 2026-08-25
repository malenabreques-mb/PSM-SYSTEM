using System;

namespace PSMSystem.Models;

public class MovimientoStock
{
    public int IdMovimientoStock { get; set; }
    public int IdRepuesto { get; set; }
    public int IdTipoMovimiento { get; set; }
    public int Cantidad { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Observacion { get; set; }

    public Repuesto? Repuesto { get; set; }
    public TipoMovimientoStock? TipoMovimiento { get; set; }
}