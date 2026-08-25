using System;

namespace PSMSystem.Helpers;

public static class EtapasAvanceTrabajo
{
    public const string Diagnostico = "Diagnóstico";
    public const string Reparacion = "Reparación";
    public const string EsperaRepuestos = "Espera de repuestos";
    public const string Finalizacion = "Finalización";

    public static readonly IReadOnlyList<string> Todas = new[]
    {
        Diagnostico,
        Reparacion,
        EsperaRepuestos,
        Finalizacion
    };
}
