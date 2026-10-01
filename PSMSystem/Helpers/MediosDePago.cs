namespace PSMSystem.Helpers;

public static class MediosDePago
{
    public const string Efectivo = "Efectivo";
    public const string Transferencia = "Transferencia";

    public static readonly IReadOnlyList<string> Todos = new[] { Efectivo, Transferencia };
}