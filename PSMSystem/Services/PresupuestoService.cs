using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Helpers;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class PresupuestoService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public PresupuestoService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<EstadoPresupuesto>> ObtenerEstadosAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.EstadosPresupuesto
            .AsNoTracking()
            .Where(e => e.Nombre != "Facturado")
            .OrderBy(e => e.IdEstadoPresupuesto)
            .ToListAsync(ct);
    }

    public async Task<Presupuesto?> ObtenerPorOrdenAsync(int idOrdenTrabajo, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Presupuestos
            .AsNoTracking()
            .Include(p => p.EstadoPresupuesto)
            .Include(p => p.Detalles).ThenInclude(d => d.Repuesto)
            .FirstOrDefaultAsync(p => p.IdOrdenTrabajo == idOrdenTrabajo, ct);
    }

    public async Task<Presupuesto> CrearAsync(int idOrdenTrabajo, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var ordenExiste = await context.OrdenesTrabajo.AnyAsync(o => o.IdOrdenTrabajo == idOrdenTrabajo, ct);
        if (!ordenExiste)
            throw new ReglaNegocioException("La orden de trabajo ya no existe.");

        var yaExiste = await context.Presupuestos.AnyAsync(p => p.IdOrdenTrabajo == idOrdenTrabajo, ct);
        if (yaExiste)
            throw new ReglaNegocioException("Esta orden ya tiene un presupuesto generado.");

        var estadoInicial = await context.EstadosPresupuesto.FirstOrDefaultAsync(e => e.Nombre == "En elaboración", ct)
            ?? throw new ReglaNegocioException("No se encontró el estado inicial de presupuesto.");

        var presupuesto = new Presupuesto
        {
            IdOrdenTrabajo = idOrdenTrabajo,
            IdEstadoPresupuesto = estadoInicial.IdEstadoPresupuesto,
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Subtotal = 0,
            Total = 0
        };

        context.Presupuestos.Add(presupuesto);
        await context.SaveChangesAsync(ct);

        return presupuesto;
    }

    public async Task<DetallePresupuesto> AgregarDetalleAsync(DetallePresupuesto detalle, CancellationToken ct = default)
    {
        ValidarDetalle(detalle);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var presupuesto = await context.Presupuestos
            .Include(p => p.EstadoPresupuesto)
            .FirstOrDefaultAsync(p => p.IdPresupuesto == detalle.IdPresupuesto, ct)
            ?? throw new ReglaNegocioException("El presupuesto ya no existe.");

        if (presupuesto.EstadoPresupuesto?.Nombre != "En elaboración")
            throw new ReglaNegocioException("Solo se pueden agregar ítems a un presupuesto en elaboración.");

        if (detalle.TipoItem == TiposItemPresupuesto.Repuesto)
        {
            var repuesto = await context.Repuestos
                .FirstOrDefaultAsync(r => r.IdRepuesto == detalle.IdRepuesto, ct)
                ?? throw new ReglaNegocioException(
                    "Verifique los datos ingresados: el repuesto seleccionado no existe en el inventario.");

            if (repuesto.StockActual < detalle.Cantidad)
                throw new ReglaNegocioException(
                    $"Stock insuficiente de {repuesto.Nombre}: hay {repuesto.StockActual} unidades y se necesitan {detalle.Cantidad}.");

            var tipoEgreso = await context.TiposMovimientoStock
                .FirstOrDefaultAsync(t => t.Nombre == "Egreso", ct)
                ?? throw new ReglaNegocioException("No se encontró el tipo de movimiento 'Egreso'.");

            repuesto.StockActual -= detalle.Cantidad;

            context.MovimientosStock.Add(new MovimientoStock
            {
                IdRepuesto = repuesto.IdRepuesto,
                IdTipoMovimiento = tipoEgreso.IdTipoMovimiento,
                Cantidad = detalle.Cantidad,
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Observacion = $"Uso en orden de trabajo #{presupuesto.IdOrdenTrabajo} (presupuesto #{presupuesto.IdPresupuesto})"
            });
        }

        detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
        context.DetallesPresupuesto.Add(detalle);

        
        await context.SaveChangesAsync(ct);

        await RecalcularTotalesAsync(context, detalle.IdPresupuesto, ct);

        return detalle;
    }

    public async Task EliminarDetalleAsync(int idDetalle, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var detalle = await context.DetallesPresupuesto.FirstOrDefaultAsync(d => d.IdDetallePresupuesto == idDetalle, ct)
            ?? throw new ReglaNegocioException("El ítem ya no existe.");

        var idPresupuesto = detalle.IdPresupuesto;

        var presupuesto = await context.Presupuestos
            .Include(p => p.EstadoPresupuesto)
            .FirstOrDefaultAsync(p => p.IdPresupuesto == idPresupuesto, ct)
            ?? throw new ReglaNegocioException("El presupuesto ya no existe.");

        if (presupuesto.EstadoPresupuesto?.Nombre != "En elaboración")
            throw new ReglaNegocioException("Solo se pueden quitar ítems de un presupuesto en elaboración.");

        if (detalle.TipoItem == TiposItemPresupuesto.Repuesto && detalle.IdRepuesto.HasValue)
        {
            var repuesto = await context.Repuestos
                .FirstOrDefaultAsync(r => r.IdRepuesto == detalle.IdRepuesto.Value, ct);

            var tipoIngreso = await context.TiposMovimientoStock
                .FirstOrDefaultAsync(t => t.Nombre == "Ingreso", ct);

            if (repuesto is not null && tipoIngreso is not null)
            {
                repuesto.StockActual += detalle.Cantidad;

                context.MovimientosStock.Add(new MovimientoStock
                {
                    IdRepuesto = repuesto.IdRepuesto,
                    IdTipoMovimiento = tipoIngreso.IdTipoMovimiento,
                    Cantidad = detalle.Cantidad,
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Observacion = $"Reintegro por ítem quitado del presupuesto #{idPresupuesto}"
                });
            }
        }

        context.DetallesPresupuesto.Remove(detalle);
        await context.SaveChangesAsync(ct);

        await RecalcularTotalesAsync(context, idPresupuesto, ct);
    }

    public async Task ActualizarEstadoAsync(int idPresupuesto, int idEstado, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var presupuesto = await context.Presupuestos.FirstOrDefaultAsync(p => p.IdPresupuesto == idPresupuesto, ct)
            ?? throw new ReglaNegocioException("El presupuesto ya no existe.");

        presupuesto.IdEstadoPresupuesto = idEstado;
        await context.SaveChangesAsync(ct);
    }

    private static async Task RecalcularTotalesAsync(PsmDbContext context, int idPresupuesto, CancellationToken ct)
    {
        var presupuesto = await context.Presupuestos.FirstOrDefaultAsync(p => p.IdPresupuesto == idPresupuesto, ct);
        if (presupuesto is null) return;

        var total = await context.DetallesPresupuesto
            .Where(d => d.IdPresupuesto == idPresupuesto)
            .SumAsync(d => d.Subtotal, ct);

        presupuesto.Subtotal = total;
        presupuesto.Total = total;

        await context.SaveChangesAsync(ct);
    }

    private static void ValidarDetalle(DetallePresupuesto detalle)
    {
        if (detalle.Cantidad <= 0)
            throw new ReglaNegocioException("Verifique los datos ingresados: la cantidad tiene que ser mayor a cero.");

        if (detalle.PrecioUnitario <= 0)
            throw new ReglaNegocioException("Verifique los datos ingresados: el precio tiene que ser mayor a cero.");

        if (detalle.TipoItem == TiposItemPresupuesto.Repuesto)
        {
            if (!detalle.IdRepuesto.HasValue || detalle.IdRepuesto.Value <= 0)
                throw new ReglaNegocioException("Seleccioná un repuesto.");
        }
        else if (detalle.TipoItem == TiposItemPresupuesto.ManoDeObra)
        {
            if (string.IsNullOrWhiteSpace(detalle.Descripcion))
                throw new ReglaNegocioException("Describí el trabajo de mano de obra.");
        }
        else
        {
            throw new ReglaNegocioException("Tipo de ítem inválido.");
        }
    }
}