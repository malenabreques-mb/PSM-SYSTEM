using System;
namespace PSMSystem.Models;

public class TipoMovimientoStock
{
    public int IdTipoMovimiento { get; set; }
    public required string Nombre { get; set; }

    public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
}
