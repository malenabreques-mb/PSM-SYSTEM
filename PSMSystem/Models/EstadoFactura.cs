using System;
namespace PSMSystem.Models;

  public class EstadoFactura
{
    public int IdEstadoFactura { get; set; }
    public required string Nombre { get; set; }

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}