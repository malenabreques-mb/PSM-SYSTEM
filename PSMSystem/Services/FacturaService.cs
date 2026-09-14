using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Helpers;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class FacturaService
{
    private const decimal PorcentajeIva = 0.21m;

    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public FacturaService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<EstadoFactura>> ObtenerEstadosAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.EstadosFactura.AsNoTracking().OrderBy(e => e.IdEstadoFactura).ToListAsync(ct);
    }

    public async Task<ResultadoPagina<Factura>> BuscarFacturasAsync(
        string? texto, int? idEstado, DateOnly? fecha, int pagina, int tamanioPagina, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var query = context.Facturas.AsNoTracking()
            .Include(f => f.EstadoFactura)
            .Include(f => f.Presupuesto!.OrdenTrabajo!.Cliente)
            .Include(f => f.Presupuesto!.OrdenTrabajo!.Vehiculo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = texto.Trim();
            query = query.Where(f =>
                EF.Functions.Like(f.NumeroFactura, $"%{filtro}%") ||
                EF.Functions.Like(f.Presupuesto!.OrdenTrabajo!.Cliente!.Nombre, $"%{filtro}%") ||
                EF.Functions.Like(f.Presupuesto!.OrdenTrabajo!.Cliente!.Apellido, $"%{filtro}%") ||
                EF.Functions.Like(f.Presupuesto!.OrdenTrabajo!.Vehiculo!.Patente, $"%{filtro}%"));
        }

        if (idEstado.HasValue && idEstado.Value > 0)
            query = query.Where(f => f.IdEstadoFactura == idEstado.Value);

        if (fecha.HasValue)
            query = query.Where(f => f.FechaEmision == fecha.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(f => f.IdFactura)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync(ct);

        return new ResultadoPagina<Factura>(items, total);
    }

    public async Task<Factura?> ObtenerPorIdAsync(int idFactura, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Facturas.AsNoTracking()
            .Include(f => f.EstadoFactura)
            .Include(f => f.Presupuesto!.OrdenTrabajo!.Cliente)
            .Include(f => f.Presupuesto!.OrdenTrabajo!.Vehiculo)
            .Include(f => f.Detalles).ThenInclude(d => d.Repuesto)
            .FirstOrDefaultAsync(f => f.IdFactura == idFactura, ct);
    }

    public async Task<Factura> ConvertirPresupuestoAsync(int idPresupuesto, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var presupuesto = await context.Presupuestos
            .Include(p => p.EstadoPresupuesto)
            .Include(p => p.OrdenTrabajo!.EstadoOrden)
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPresupuesto == idPresupuesto, ct)
            ?? throw new ReglaNegocioException("El presupuesto ya no existe.");

        if (presupuesto.EstadoPresupuesto?.Nombre != "Aprobado")
            throw new ReglaNegocioException("El presupuesto tiene que estar aprobado para convertirlo en factura.");

        var yaFacturado = await context.Facturas.AnyAsync(f => f.IdPresupuesto == idPresupuesto, ct);
        if (yaFacturado)
            throw new ReglaNegocioException("El presupuesto ya fue facturado.");

        if (presupuesto.OrdenTrabajo?.EstadoOrden?.Nombre != "Finalizada")
            throw new ReglaNegocioException("La orden de trabajo tiene que estar finalizada para poder facturar.");

        var estadoInicial = await context.EstadosFactura.FirstOrDefaultAsync(e => e.Nombre == "Pendiente de emisión", ct)
            ?? throw new ReglaNegocioException("No se encontró el estado inicial de factura.");

        var factura = new Factura
        {
            IdPresupuesto = idPresupuesto,
            IdEstadoFactura = estadoInicial.IdEstadoFactura,
            NumeroFactura = "TEMP",
            Subtotal = 0,
            Iva = 0,
            Total = 0,
            FechaEmision = null
        };

        foreach (var detallePresupuesto in presupuesto.Detalles)
        {
            factura.Detalles.Add(new DetalleFactura
            {
                TipoItem = detallePresupuesto.TipoItem,
                IdRepuesto = detallePresupuesto.IdRepuesto,
                Descripcion = detallePresupuesto.Descripcion,
                Cantidad = detallePresupuesto.Cantidad,
                PrecioUnitario = detallePresupuesto.PrecioUnitario,
                Subtotal = detallePresupuesto.Subtotal
            });
        }

        context.Facturas.Add(factura);
        await context.SaveChangesAsync(ct);

       
        factura.NumeroFactura = $"F-{DateTime.Now:yyyy}-{factura.IdFactura:D6}";

        var subtotal = factura.Detalles.Sum(d => d.Subtotal ?? 0);
        factura.Subtotal = subtotal;
        factura.Iva = Math.Round(subtotal * PorcentajeIva, 2);
        factura.Total = factura.Subtotal + factura.Iva;

        var estadoFacturado = await context.EstadosPresupuesto.FirstOrDefaultAsync(e => e.Nombre == "Facturado", ct);
        if (estadoFacturado is not null)
            presupuesto.IdEstadoPresupuesto = estadoFacturado.IdEstadoPresupuesto;

        await context.SaveChangesAsync(ct);

        return factura;
    }

    public async Task AgregarDetalleAsync(DetalleFactura detalle, CancellationToken ct = default)
    {
        ValidarDetalle(detalle);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var factura = await context.Facturas.Include(f => f.EstadoFactura)
            .FirstOrDefaultAsync(f => f.IdFactura == detalle.IdFactura, ct)
            ?? throw new ReglaNegocioException("La factura ya no existe.");

        if (factura.EstadoFactura?.Nombre != "Pendiente de emisión")
            throw new ReglaNegocioException("Solo se puede editar una factura pendiente de emisión.");

        if (detalle.TipoItem == TiposItemPresupuesto.Repuesto)
        {
            var repuestoExiste = await context.Repuestos.AnyAsync(r => r.IdRepuesto == detalle.IdRepuesto, ct);
            if (!repuestoExiste)
                throw new ReglaNegocioException("Verifique los datos ingresados: el repuesto seleccionado no existe en el inventario.");
        }

        detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
        context.DetallesFactura.Add(detalle);
        await context.SaveChangesAsync(ct);

        await RecalcularTotalesAsync(context, detalle.IdFactura, ct);
    }

    public async Task EliminarDetalleAsync(int idDetalle, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var detalle = await context.DetallesFactura.FirstOrDefaultAsync(d => d.IdDetalleFactura == idDetalle, ct)
            ?? throw new ReglaNegocioException("El ítem ya no existe.");

        var factura = await context.Facturas.Include(f => f.EstadoFactura)
            .FirstOrDefaultAsync(f => f.IdFactura == detalle.IdFactura, ct)
            ?? throw new ReglaNegocioException("La factura ya no existe.");

        if (factura.EstadoFactura?.Nombre != "Pendiente de emisión")
            throw new ReglaNegocioException("Solo se puede editar una factura pendiente de emisión.");

        var cantidadItems = await context.DetallesFactura.CountAsync(d => d.IdFactura == detalle.IdFactura, ct);
        if (cantidadItems <= 1)
            throw new ReglaNegocioException("No se puede eliminar: la factura tiene que tener al menos un ítem.");

        context.DetallesFactura.Remove(detalle);
        await context.SaveChangesAsync(ct);

        await RecalcularTotalesAsync(context, detalle.IdFactura, ct);
    }

    public async Task EmitirAsync(int idFactura, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var factura = await context.Facturas.Include(f => f.EstadoFactura)
            .FirstOrDefaultAsync(f => f.IdFactura == idFactura, ct)
            ?? throw new ReglaNegocioException("La factura ya no existe.");

        if (factura.EstadoFactura?.Nombre != "Pendiente de emisión")
            throw new ReglaNegocioException("Solo se puede emitir una factura pendiente de emisión.");

        var estadoEmitida = await context.EstadosFactura.FirstOrDefaultAsync(e => e.Nombre == "Pendiente de pago", ct)
            ?? throw new ReglaNegocioException("No se encontró el estado 'Pendiente de pago'.");

        factura.FechaEmision = DateOnly.FromDateTime(DateTime.Now);
        factura.IdEstadoFactura = estadoEmitida.IdEstadoFactura;

        await context.SaveChangesAsync(ct);
    }

    private static async Task RecalcularTotalesAsync(PsmDbContext context, int idFactura, CancellationToken ct)
    {
        var factura = await context.Facturas.FirstOrDefaultAsync(f => f.IdFactura == idFactura, ct);
        if (factura is null) return;

        var subtotal = await context.DetallesFactura
            .Where(d => d.IdFactura == idFactura)
            .SumAsync(d => d.Subtotal ?? 0, ct);

        factura.Subtotal = subtotal;
        factura.Iva = Math.Round(subtotal * PorcentajeIva, 2);
        factura.Total = factura.Subtotal + factura.Iva;

        await context.SaveChangesAsync(ct);
    }

    private static void ValidarDetalle(DetalleFactura detalle)
    {
        if (!detalle.Cantidad.HasValue || detalle.Cantidad.Value <= 0)
            throw new ReglaNegocioException("Verifique los datos ingresados: la cantidad tiene que ser mayor a cero.");

        if (!detalle.PrecioUnitario.HasValue || detalle.PrecioUnitario.Value <= 0)
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