using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class RepuestoService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public RepuestoService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Repuesto?> ObtenerPorIdAsync(int idRepuesto, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Repuestos.AsNoTracking().FirstOrDefaultAsync(r => r.IdRepuesto == idRepuesto, ct);
    }

    public async Task<ResultadoPagina<Repuesto>> BuscarRepuestosAsync(
        string? texto, bool soloStockBajo, int pagina, int tamanioPagina, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var query = context.Repuestos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = texto.Trim();
            query = query.Where(r =>
                EF.Functions.Like(r.Nombre, $"%{filtro}%") ||
                (r.Categoria != null && EF.Functions.Like(r.Categoria, $"%{filtro}%")) ||
                (r.Proveedor != null && EF.Functions.Like(r.Proveedor, $"%{filtro}%")));
        }

        if (soloStockBajo)
            query = query.Where(r => r.StockMinimo != null && r.StockActual < r.StockMinimo);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(r => r.Nombre)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync(ct);

        return new ResultadoPagina<Repuesto>(items, total);
    }

    public async Task<Repuesto> CrearAsync(Repuesto repuesto, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(repuesto);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        repuesto.Activo = true;
        repuesto.FechaCreacion = DateOnly.FromDateTime(DateTime.Now);

        context.Repuestos.Add(repuesto);
        await context.SaveChangesAsync(ct);

        return repuesto;
    }

    public async Task ActualizarAsync(Repuesto repuesto, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(repuesto);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var existente = await context.Repuestos.FirstOrDefaultAsync(r => r.IdRepuesto == repuesto.IdRepuesto, ct)
            ?? throw new ReglaNegocioException("El repuesto que intentás editar ya no existe.");

        existente.Nombre = repuesto.Nombre;
        existente.Categoria = repuesto.Categoria;
        existente.Proveedor = repuesto.Proveedor;
        existente.PrecioUnitario = repuesto.PrecioUnitario;
        existente.StockMinimo = repuesto.StockMinimo;

        await context.SaveChangesAsync(ct);
    }

    public async Task DesactivarAsync(int idRepuesto, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var repuesto = await context.Repuestos.FirstOrDefaultAsync(r => r.IdRepuesto == idRepuesto, ct)
            ?? throw new ReglaNegocioException("El repuesto que intentás desactivar ya no existe.");

        repuesto.Activo = false;
        await context.SaveChangesAsync(ct);
    }

    public async Task ReactivarAsync(int idRepuesto, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var repuesto = await context.Repuestos.FirstOrDefaultAsync(r => r.IdRepuesto == idRepuesto, ct)
            ?? throw new ReglaNegocioException("El repuesto que intentás reactivar ya no existe.");

        repuesto.Activo = true;
        await context.SaveChangesAsync(ct);
    }

    private static void ValidarDatosObligatorios(Repuesto repuesto)
    {
        if (string.IsNullOrWhiteSpace(repuesto.Nombre))
            throw new ReglaNegocioException("El nombre del repuesto es obligatorio.");

        if (repuesto.StockActual < 0)
            throw new ReglaNegocioException("El stock no puede ser negativo.");

        if (repuesto.StockMinimo.HasValue && repuesto.StockMinimo.Value < 0)
            throw new ReglaNegocioException("El stock mínimo no puede ser negativo.");

        if (repuesto.PrecioUnitario.HasValue && repuesto.PrecioUnitario.Value < 0)
            throw new ReglaNegocioException("El precio no puede ser negativo.");
    }
}