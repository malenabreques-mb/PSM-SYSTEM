using Microsoft.EntityFrameworkCore;
using PSMSystem.Data;
using PSMSystem.Helpers;
using PSMSystem.Models;

namespace PSMSystem.Services;

public class AvanceTrabajoService
{
    private readonly IDbContextFactory<PsmDbContext> _contextFactory;

    public AvanceTrabajoService(IDbContextFactory<PsmDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<AvanceTrabajo>> ObtenerPorOrdenAsync(int idOrdenTrabajo, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.AvancesTrabajo
            .AsNoTracking()
            .Where(a => a.IdOrdenTrabajo == idOrdenTrabajo)
            .OrderBy(a => a.Fecha)
            .ThenBy(a => a.IdAvance)
            .ToListAsync(ct);
    }

    public async Task<AvanceTrabajo> RegistrarAsync(AvanceTrabajo avance, CancellationToken ct = default)
    {
        ValidarDatosObligatorios(avance);

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var ordenExiste = await context.OrdenesTrabajo.AnyAsync(o => o.IdOrdenTrabajo == avance.IdOrdenTrabajo, ct);
        if (!ordenExiste)
            throw new ReglaNegocioException("La orden de trabajo asociada ya no existe.");

        context.AvancesTrabajo.Add(avance);
        await context.SaveChangesAsync(ct);

        return avance;
    }

    private static void ValidarDatosObligatorios(AvanceTrabajo avance)
    {
        if (avance.IdOrdenTrabajo <= 0)
            throw new ReglaNegocioException("La orden de trabajo es obligatoria.");

        if (!EtapasAvanceTrabajo.Todas.Contains(avance.Etapa))
            throw new ReglaNegocioException("Seleccioná una etapa válida.");

        if (string.IsNullOrWhiteSpace(avance.Descripcion))
            throw new ReglaNegocioException("La descripción del avance es obligatoria.");
    }
}