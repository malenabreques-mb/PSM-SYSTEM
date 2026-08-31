using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class TurnoService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public TurnoService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<EstadoTurno>> ObtenerEstadosAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.EstadosTurno.AsNoTracking().OrderBy(e => e.IdEstadoTurno).ToListAsync(ct);
    }

    public async Task<ResultadoPagina<Turno>> BuscarTurnosAsync(
        string? texto, int? idEstadoTurno, DateOnly? fecha, int pagina, int tamanioPagina, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var query = context.Turnos.AsNoTracking()
            .Include(t => t.Cliente)
            .Include(t => t.Vehiculo)
            .Include(t => t.EstadoTurno)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = texto.Trim();
            query = query.Where(t =>
                EF.Functions.Like(t.Cliente!.Nombre, $"%{filtro}%") ||
                EF.Functions.Like(t.Cliente!.Apellido, $"%{filtro}%") ||
                EF.Functions.Like(t.Vehiculo!.Patente, $"%{filtro}%") ||
                EF.Functions.Like(t.Vehiculo!.Marca, $"%{filtro}%") ||
                EF.Functions.Like(t.Vehiculo!.Modelo, $"%{filtro}%"));
        }

        if (idEstadoTurno.HasValue && idEstadoTurno.Value > 0)
            query = query.Where(t => t.IdEstadoTurno == idEstadoTurno.Value);

        if (fecha.HasValue)
            query = query.Where(t => t.Fecha == fecha.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => t.Fecha)
            .ThenBy(t => t.Hora)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync(ct);

        return new ResultadoPagina<Turno>(items, total);
    }

    public async Task<Turno> CrearAsync(Turno turno, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(turno);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        await ValidarClienteYVehiculoAsync(context, turno, ct);
        await ValidarDisponibilidadAsync(context, turno, ct);

        turno.FechaCreacion = DateOnly.FromDateTime(DateTime.Now);

        context.Turnos.Add(turno);
        await context.SaveChangesAsync(ct);

        return turno;
    }

    public async Task ActualizarAsync(Turno turno, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(turno);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var existente = await context.Turnos.FirstOrDefaultAsync(t => t.IdTurno == turno.IdTurno, ct)
            ?? throw new ReglaNegocioException("El turno que intentás editar ya no existe.");

        await ValidarClienteYVehiculoAsync(context, turno, ct);
        await ValidarDisponibilidadAsync(context, turno, ct);

        existente.IdCliente = turno.IdCliente;
        existente.IdVehiculo = turno.IdVehiculo;
        existente.IdEstadoTurno = turno.IdEstadoTurno;
        existente.Fecha = turno.Fecha;
        existente.Hora = turno.Hora;
        existente.Motivo = turno.Motivo;

        await context.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(int idTurno, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var turno = await context.Turnos.FirstOrDefaultAsync(t => t.IdTurno == idTurno, ct)
            ?? throw new ReglaNegocioException("El turno ya no existe (puede que ya se haya eliminado).");

        var generoOrden = await context.OrdenesTrabajo.AnyAsync(o => o.IdTurno == idTurno, ct);
        if (generoOrden)
            throw new ReglaNegocioException("No se puede eliminar: este turno ya generó una orden de trabajo.");

        context.Turnos.Remove(turno);
        await context.SaveChangesAsync(ct);
    }

    private static async Task ValidarClienteYVehiculoAsync(PsmDbContext context, Turno turno, CancellationToken ct)
    {
        var clienteExiste = await context.Clientes.AnyAsync(c => c.IdCliente == turno.IdCliente, ct);
        if (!clienteExiste)
            throw new ReglaNegocioException("Seleccioná un cliente válido.");

        var vehiculoExiste = await context.Vehiculos.AnyAsync(v => v.IdVehiculo == turno.IdVehiculo, ct);
        if (!vehiculoExiste)
            throw new ReglaNegocioException("Seleccioná un vehículo válido.");
    }

    private static async Task ValidarDisponibilidadAsync(PsmDbContext context, Turno turno, CancellationToken ct)
    {
        var ocupado = await context.Turnos.AnyAsync(t =>
            t.Fecha == turno.Fecha && t.Hora == turno.Hora && t.IdTurno != turno.IdTurno, ct);

        if (ocupado)
            throw new ReglaNegocioException(
                $"Ya existe un turno registrado el {turno.Fecha:dd/MM/yyyy} a las {turno.Hora:HH:mm}.");
    }

    private static void ValidarDatosObligatorios(Turno turno)
    {
        if (turno.IdCliente <= 0)
            throw new ReglaNegocioException("Tenés que seleccionar un cliente.");

        if (turno.IdVehiculo <= 0)
            throw new ReglaNegocioException("Tenés que seleccionar un vehículo.");

        if (string.IsNullOrWhiteSpace(turno.Motivo))
            throw new ReglaNegocioException("El motivo es obligatorio.");
    }

    public async Task<List<Turno>> ObtenerTurnosPorClienteAsync(int idCliente, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Turnos
            .AsNoTracking()
            .Where(t => t.IdCliente == idCliente)
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Hora)
            .ToListAsync(ct);
    }
}