using System;

using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Models;

namespace PSMSystem.Services;           

public class VehiculoService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public VehiculoService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ResultadoPagina<Vehiculo>> BuscarVehiculosAsync(
        string? texto, int pagina, int tamanioPagina, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var query = context.Vehiculos.AsNoTracking().Include(v => v.Cliente).AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var filtro = texto.Trim();
            query = query.Where(v =>
                EF.Functions.Like(v.Patente, $"%{filtro}%") ||
                EF.Functions.Like(v.Marca, $"%{filtro}%") ||
                EF.Functions.Like(v.Modelo, $"%{filtro}%") ||
                EF.Functions.Like(v.Cliente!.Nombre, $"%{filtro}%") ||
                EF.Functions.Like(v.Cliente!.Apellido, $"%{filtro}%"));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(v => v.Marca)
            .ThenBy(v => v.Modelo)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync(ct);

        return new ResultadoPagina<Vehiculo>(items, total);
    }

    public async Task<Vehiculo> CrearAsync(Vehiculo vehiculo, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(vehiculo);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var clienteExiste = await context.Clientes.AnyAsync(c => c.IdCliente == vehiculo.IdCliente, ct);
        if (!clienteExiste)
            throw new ReglaNegocioException("Seleccioná un cliente válido para el vehículo.");

        var patenteNormalizada = vehiculo.Patente.Trim().ToUpperInvariant();

        var patenteDuplicada = await context.Vehiculos.AnyAsync(v => v.Patente == patenteNormalizada, ct);
        if (patenteDuplicada)
            throw new ReglaNegocioException($"Ya existe un vehículo registrado con la patente {patenteNormalizada}.");

        vehiculo.Patente = patenteNormalizada;
        vehiculo.Activo = true;
        vehiculo.FechaCreacion = DateOnly.FromDateTime(DateTime.Now);
        vehiculo.FechaModificacion = vehiculo.FechaCreacion;

        context.Vehiculos.Add(vehiculo);
        await context.SaveChangesAsync(ct);

        return vehiculo;
    }

    public async Task ActualizarAsync(Vehiculo vehiculo, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(vehiculo);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var existente = await context.Vehiculos.FirstOrDefaultAsync(v => v.IdVehiculo == vehiculo.IdVehiculo, ct)
            ?? throw new ReglaNegocioException("El vehículo que intentás editar ya no existe.");

        var patenteNormalizada = vehiculo.Patente.Trim().ToUpperInvariant();

        var patenteDuplicada = await context.Vehiculos.AnyAsync(v =>
            v.Patente == patenteNormalizada && v.IdVehiculo != vehiculo.IdVehiculo, ct);
        if (patenteDuplicada)
            throw new ReglaNegocioException($"Ya existe un vehículo registrado con la patente {patenteNormalizada}.");

        var clienteExiste = await context.Clientes.AnyAsync(c => c.IdCliente == vehiculo.IdCliente, ct);
        if (!clienteExiste)
            throw new ReglaNegocioException("Seleccioná un cliente válido para el vehículo.");

        existente.Marca = vehiculo.Marca;
        existente.Modelo = vehiculo.Modelo;
        existente.Anio = vehiculo.Anio;
        existente.Patente = patenteNormalizada;
        existente.IdCliente = vehiculo.IdCliente;
        existente.FechaModificacion = DateOnly.FromDateTime(DateTime.Now);

        await context.SaveChangesAsync(ct);
    }

    public async Task DesactivarAsync(int idVehiculo, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var vehiculo = await context.Vehiculos.FirstOrDefaultAsync(v => v.IdVehiculo == idVehiculo, ct)
            ?? throw new ReglaNegocioException("El vehículo que intentás desactivar ya no existe.");

        var tieneOrdenesActivas = await context.OrdenesTrabajo
            .Include(o => o.EstadoOrden)
            .AnyAsync(o => o.IdVehiculo == idVehiculo && o.EstadoOrden!.Nombre != "Finalizada", ct);

        if (tieneOrdenesActivas)
            throw new ReglaNegocioException(
                "No es posible desactivar: el vehículo tiene órdenes de trabajo activas.");

        vehiculo.Activo = false;
        vehiculo.FechaModificacion = DateOnly.FromDateTime(DateTime.Now);
        await context.SaveChangesAsync(ct);
    }

    public async Task ReactivarAsync(int idVehiculo, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var vehiculo = await context.Vehiculos.FirstOrDefaultAsync(v => v.IdVehiculo == idVehiculo, ct)
            ?? throw new ReglaNegocioException("El vehículo que intentás reactivar ya no existe.");

        vehiculo.Activo = true;
        vehiculo.FechaModificacion = DateOnly.FromDateTime(DateTime.Now);
        await context.SaveChangesAsync(ct);
    }

    private static void ValidarDatosObligatorios(Vehiculo vehiculo)
    {
        if (string.IsNullOrWhiteSpace(vehiculo.Marca))
            throw new ReglaNegocioException("La marca es obligatoria.");

        if (string.IsNullOrWhiteSpace(vehiculo.Modelo))
            throw new ReglaNegocioException("El modelo es obligatorio.");

        if (string.IsNullOrWhiteSpace(vehiculo.Patente))
            throw new ReglaNegocioException("La patente es obligatoria.");

        if (vehiculo.IdCliente <= 0)
            throw new ReglaNegocioException("Tenés que seleccionar un cliente.");
    }


    public async Task<List<Vehiculo>> ObtenerVehiculosActivosPorClienteAsync(int idCliente, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Vehiculos
            .AsNoTracking()
            .Where(v => v.IdCliente == idCliente && v.Activo)
            .OrderBy(v => v.Marca)
            .ThenBy(v => v.Modelo)
            .ToListAsync(ct);
    }
}
