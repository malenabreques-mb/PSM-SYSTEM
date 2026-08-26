using System;

using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class ClienteService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public ClienteService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ResultadoPagina<Cliente>> BuscarClientesAsync(
        string? texto, int pagina, int tamanioPagina, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var query = context.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = texto.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.Nombre, $"%{filtro}%") ||
                EF.Functions.Like(c.Apellido, $"%{filtro}%") ||
                (c.Telefono != null && EF.Functions.Like(c.Telefono, $"%{filtro}%")));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Apellido)
            .ThenBy(c => c.Nombre)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync(ct);

        return new ResultadoPagina<Cliente>(items, total);
    }

    public async Task<Cliente> CrearAsync(Cliente cliente, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(cliente);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var yaExiste = await context.Clientes.AnyAsync(c =>
            c.Nombre == cliente.Nombre &&
            c.Apellido == cliente.Apellido &&
            c.Telefono == cliente.Telefono, ct);

        if (yaExiste)
            throw new ReglaNegocioException(
                "Ya existe un cliente registrado con ese nombre, apellido y teléfono.");

        cliente.Activo = true;
        cliente.FechaCreacion = DateOnly.FromDateTime(DateTime.Now);
        cliente.FechaModificacion = cliente.FechaCreacion;

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(ct);

        return cliente;
    }

    public async Task ActualizarAsync(Cliente cliente, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(cliente);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var existente = await context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == cliente.IdCliente, ct)
            ?? throw new ReglaNegocioException("El cliente que intentás editar ya no existe.");

        existente.Nombre = cliente.Nombre;
        existente.Apellido = cliente.Apellido;
        existente.Telefono = cliente.Telefono;
        existente.Direccion = cliente.Direccion;
        existente.FechaModificacion = DateOnly.FromDateTime(DateTime.Now);

        await context.SaveChangesAsync(ct);
    }

    public async Task DesactivarAsync(int idCliente, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == idCliente, ct)
            ?? throw new ReglaNegocioException("El cliente que intentás desactivar ya no existe.");

        var tieneVehiculosActivos = await context.Vehiculos
            .AnyAsync(v => v.IdCliente == idCliente && v.Activo, ct);

        var tieneTurnosActivos = await context.Turnos
            .Include(t => t.EstadoTurno)
            .AnyAsync(t => t.IdCliente == idCliente && t.EstadoTurno!.Nombre != "Finalizado", ct);

        var tieneOrdenesActivas = await context.OrdenesTrabajo
            .Include(o => o.EstadoOrden)
            .AnyAsync(o => o.IdCliente == idCliente && o.EstadoOrden!.Nombre != "Finalizada", ct);

        if (tieneVehiculosActivos || tieneTurnosActivos || tieneOrdenesActivas)
            throw new ReglaNegocioException(
                "No se puede desactivar: el cliente tiene vehículos, turnos u órdenes de trabajo activos.");

        cliente.Activo = false;
        cliente.FechaModificacion = DateOnly.FromDateTime(DateTime.Now);
        await context.SaveChangesAsync(ct);
    }

    public async Task ReactivarAsync(int idCliente, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == idCliente, ct)
            ?? throw new ReglaNegocioException("El cliente que intentás reactivar ya no existe.");

        cliente.Activo = true;
        cliente.FechaModificacion = DateOnly.FromDateTime(DateTime.Now);
        await context.SaveChangesAsync(ct);
    }

    private static void ValidarDatosObligatorios(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            throw new ReglaNegocioException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(cliente.Apellido))
            throw new ReglaNegocioException("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(cliente.Telefono))
            throw new ReglaNegocioException("El teléfono es obligatorio.");

        if (string.IsNullOrWhiteSpace(cliente.Direccion))
            throw new ReglaNegocioException("La dirección es obligatoria.");
    }
}