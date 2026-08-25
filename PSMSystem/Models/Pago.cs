using System;

namespace PSMSystem.Models;

public class Pago
{
    public int IdPago { get; set; }
    public int IdFactura { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal Monto { get; set; }
    public required string MedioPago { get; set; }

    public Factura? Factura { get; set; }
}
