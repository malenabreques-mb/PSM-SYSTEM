namespace PSMSystem.Helpers;

public static class TiposItemPresupuesto
{
    public const string Repuesto = "Repuesto";
    public const string ManoDeObra = "Mano de obra";

    public static readonly IReadOnlyList<string> Todas = new[] { Repuesto, ManoDeObra };
}