using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class MovimientosRepuestoView : Window
{
    public MovimientosRepuestoView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is MovimientosRepuestoViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is MovimientosRepuestoViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, bool huboCambios)
    {
        DialogResult = huboCambios;
    }
}