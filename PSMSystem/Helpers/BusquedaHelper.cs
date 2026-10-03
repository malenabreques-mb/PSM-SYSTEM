using System.Globalization;
using System.Text;

namespace PSMSystem.Helpers;

public static class BusquedaHelper
{
    public static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        var descompuesto = valor.Normalize(NormalizationForm.FormD);

        var sinTildes = new StringBuilder();
        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                sinTildes.Append(caracter);
        }

        return sinTildes.ToString().Normalize(NormalizationForm.FormC).Trim().ToLowerInvariant();
    }

    public static bool Coincide(string? textoCompleto, string? filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro))
            return true;

        return Normalizar(textoCompleto).Contains(Normalizar(filtro));
    }
}