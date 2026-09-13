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

        var presupuestoExiste = await context.Presupuestos.AnyAsync(p => p.IdPresupuesto == detalle.IdPresupuesto, ct);
        if (!presupuestoExiste)
            throw new ReglaNegocioException("El presupuesto ya no existe.");

        if (detalle.TipoItem == TiposItemPresupuesto.Repuesto)
        {
            var repuestoExiste = await context.Repuestos.AnyAsync(r => r.IdRepuesto == detalle.IdRepuesto, ct);
            if (!repuestoExiste)
                throw new ReglaNegocioException("Verifique los datos ingresados: el repuesto seleccionado no existe en el inventario.");
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
