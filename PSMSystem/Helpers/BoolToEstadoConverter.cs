using System;

using System.Globalization;
using System.Windows.Data;

namespace PSMSystem.Helpers;

public class BoolToEstadoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "Activo" : "Inactivo";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
