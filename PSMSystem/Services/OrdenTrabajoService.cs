using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class OrdenTrabajoService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public OrdenTrabajoService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<EstadoOrdenTrabajo>> ObtenerEstadosAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.EstadosOrdenTrabajo.AsNoTracking().OrderBy(e => e.IdEstadoOrden).ToListAsync(ct);
    }

    public async Task<ResultadoPagina<OrdenTrabajo>> BuscarOrdenesAsync(
        string? texto, int? idEstadoOrden, DateOnly? fecha, int pagina, int tamanioPagina, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var query = context.OrdenesTrabajo.AsNoTracking()
            .Include(o => o.Cliente)
            .Include(o => o.Vehiculo)
            .Include(o => o.EstadoOrden)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = texto.Trim();
            query = query.Where(o =>
                EF.Functions.Like(o.Cliente!.Nombre, $"%{filtro}%") ||
                EF.Functions.Like(o.Cliente!.Apellido, $"%{filtro}%") ||
                EF.Functions.Like(o.Vehiculo!.Patente, $"%{filtro}%") ||
                EF.Functions.Like(o.Vehiculo!.Marca, $"%{filtro}%") ||
                EF.Functions.Like(o.Vehiculo!.Modelo, $"%{filtro}%") ||
                EF.Functions.Like(o.MotivoIngreso, $"%{filtro}%"));
        }

        if (idEstadoOrden.HasValue && idEstadoOrden.Value > 0)
            query = query.Where(o => o.IdEstadoOrden == idEstadoOrden.Value);

        if (fecha.HasValue)
            query = query.Where(o => o.FechaIngreso == fecha.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.FechaIngreso)
            .ThenByDescending(o => o.IdOrdenTrabajo)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync(ct);

        return new ResultadoPagina<OrdenTrabajo>(items, total);
    }

    public async Task<OrdenTrabajo> CrearAsync(OrdenTrabajo orden, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(orden);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        await ValidarClienteYVehiculoAsync(context, orden, ct);

        orden.FechaCreacion = DateOnly.FromDateTime(DateTime.Now);
        orden.FechaModificacion = orden.FechaCreacion;

        context.OrdenesTrabajo.Add(orden);
        await context.SaveChangesAsync(ct);

        return orden;
    }

    public async Task ActualizarAsync(OrdenTrabajo orden, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(orden);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var existente = await context.OrdenesTrabajo.FirstOrDefaultAsync(o => o.IdOrdenTrabajo == orden.IdOrdenTrabajo, ct)
            ?? throw new ReglaNegocioException("La orden que intentás editar ya no existe.");

        // CU15: se actualizan estado, diagnóstico y observaciones. Cliente y vehículo no se reasignan.
        existente.IdEstadoOrden = orden.IdEstadoOrden;
        existente.MotivoIngreso = orden.MotivoIngreso;
        existente.DiagnosticoInicial = orden.DiagnosticoInicial;
        existente.Observaciones = orden.Observaciones;
        existente.FechaEntregaEstimada = orden.FechaEntregaEstimada;
        existente.FechaModificacion = DateOnly.FromDateTime(DateTime.Now);

        await context.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(int idOrdenTrabajo, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var orden = await context.OrdenesTrabajo
            .Include(o => o.EstadoOrden)
            .FirstOrDefaultAsync(o => o.IdOrdenTrabajo == idOrdenTrabajo, ct)
            ?? throw new ReglaNegocioException("La orden ya no existe (puede que ya se haya eliminado).");

        if (orden.EstadoOrden?.Nombre == "En proceso")
            throw new ReglaNegocioException("No se puede eliminar: la orden está en proceso.");

        var tieneAvances = await context.AvancesTrabajo.AnyAsync(a => a.IdOrdenTrabajo == idOrdenTrabajo, ct);
        if (tieneAvances)
            throw new ReglaNegocioException("No se puede eliminar: la orden ya tiene avances registrados.");

        var tienePresupuesto = await context.Presupuestos.AnyAsync(p => p.IdOrdenTrabajo == idOrdenTrabajo, ct);
        if (tienePresupuesto)
            throw new ReglaNegocioException("No se puede eliminar: la orden ya tiene un presupuesto asociado.");

        context.OrdenesTrabajo.Remove(orden);
        await context.SaveChangesAsync(ct);
    }

    private static async Task ValidarClienteYVehiculoAsync(PsmDbContext context, OrdenTrabajo orden, CancellationToken ct)
    {
        var clienteExiste = await context.Clientes.AnyAsync(c => c.IdCliente == orden.IdCliente, ct);
        if (!clienteExiste)
            throw new ReglaNegocioException("Seleccioná un cliente válido.");

        var vehiculoExiste = await context.Vehiculos.AnyAsync(v => v.IdVehiculo == orden.IdVehiculo, ct);
        if (!vehiculoExiste)
            throw new ReglaNegocioException("Seleccioná un vehículo válido.");
    }

    private static void ValidarDatosObligatorios(OrdenTrabajo orden)
    {
        if (orden.IdCliente <= 0)
            throw new ReglaNegocioException("Tenés que seleccionar un cliente.");

        if (orden.IdVehiculo <= 0)
            throw new ReglaNegocioException("Tenés que seleccionar un vehículo.");

        if (string.IsNullOrWhiteSpace(orden.MotivoIngreso))
            throw new ReglaNegocioException("El motivo de ingreso es obligatorio.");

        if (string.IsNullOrWhiteSpace(orden.DiagnosticoInicial))
            throw new ReglaNegocioException("El diagnóstico inicial es obligatorio.");

        if (orden.FechaEntregaEstimada is null)
            throw new ReglaNegocioException("La fecha estimada de entrega es obligatoria.");
    }
}