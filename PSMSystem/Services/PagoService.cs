using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Helpers;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class PagoService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public PagoService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Pago>> ObtenerPorFacturaAsync(int idFactura, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Pagos
            .AsNoTracking()
            .Where(p => p.IdFactura == idFactura)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync(ct);
    }

    public async Task<Pago> RegistrarAsync(Pago pago, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(pago);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var factura = await context.Facturas.Include(f => f.EstadoFactura)
            .FirstOrDefaultAsync(f => f.IdFactura == pago.IdFactura, ct)
            ?? throw new ReglaNegocioException("La factura ya no existe.");

        if (factura.EstadoFactura?.Nombre != "Pendiente de pago")
            throw new ReglaNegocioException("Solo se puede registrar un pago sobre una factura pendiente de pago.");

        var totalPagado = await context.Pagos
            .Where(p => p.IdFactura == pago.IdFactura)
            .SumAsync(p => p.Monto, ct);

        var saldoPendiente = (factura.Total ?? 0) - totalPagado;

        if (pago.Monto > saldoPendiente)
            throw new ReglaNegocioException($"El monto supera el saldo pendiente ({saldoPendiente:C2}).");

        context.Pagos.Add(pago);

        var saldoRestante = saldoPendiente - pago.Monto;
        if (saldoRestante <= 0)
        {
            var estadoPagada = await context.EstadosFactura.FirstOrDefaultAsync(e => e.Nombre == "Pagada", ct)
                ?? throw new ReglaNegocioException("No se encontró el estado 'Pagada'.");
            factura.IdEstadoFactura = estadoPagada.IdEstadoFactura;
        }

        await context.SaveChangesAsync(ct);

        return pago;
    }

    private static void ValidarDatosObligatorios(Pago pago)
    {
        if (pago.Monto <= 0)
            throw new ReglaNegocioException("El monto tiene que ser mayor a cero.");

        if (!MediosDePago.Todos.Contains(pago.MedioPago))
            throw new ReglaNegocioException("Medio de pago inválido.");
    }
}