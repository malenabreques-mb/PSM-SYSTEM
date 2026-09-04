using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class MovimientoStockService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    private static readonly string[] TiposQueSuman = { "Ingreso", "Compra" };
    private static readonly string[] TiposQueRestan = { "Egreso", "Devolución" };

    public MovimientoStockService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<TipoMovimientoStock>> ObtenerTiposAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.TiposMovimientoStock.AsNoTracking().OrderBy(t => t.IdTipoMovimiento).ToListAsync(ct);
    }

    public async Task<List<MovimientoStock>> ObtenerPorRepuestoAsync(int idRepuesto, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.MovimientosStock
            .AsNoTracking()
            .Include(m => m.TipoMovimiento)
            .Where(m => m.IdRepuesto == idRepuesto)
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.IdMovimientoStock)
            .ToListAsync(ct);
    }

    public async Task<MovimientoStock> RegistrarAsync(MovimientoStock movimiento, CancellationToken ct = default)
    {
        if (movimiento.IdRepuesto <= 0)
            throw new ReglaNegocioException("Seleccioná un repuesto válido.");

        if (movimiento.Cantidad <= 0)
            throw new ReglaNegocioException("La cantidad tiene que ser mayor a cero.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var repuesto = await context.Repuestos.FirstOrDefaultAsync(r => r.IdRepuesto == movimiento.IdRepuesto, ct)
            ?? throw new ReglaNegocioException("El repuesto ya no existe.");

        var tipo = await context.TiposMovimientoStock
            .FirstOrDefaultAsync(t => t.IdTipoMovimiento == movimiento.IdTipoMovimiento, ct)
            ?? throw new ReglaNegocioException("Seleccioná un tipo de movimiento válido.");

        if (TiposQueSuman.Contains(tipo.Nombre))
        {
            repuesto.StockActual += movimiento.Cantidad;
        }
        else if (TiposQueRestan.Contains(tipo.Nombre))
        {
            if (repuesto.StockActual < movimiento.Cantidad)
                throw new ReglaNegocioException(
                    $"Stock insuficiente: hay {repuesto.StockActual} unidades y se quieren egresar {movimiento.Cantidad}.");

            repuesto.StockActual -= movimiento.Cantidad;
        }

        context.MovimientosStock.Add(movimiento);

        
        await context.SaveChangesAsync(ct);

        return movimiento;
    }
}