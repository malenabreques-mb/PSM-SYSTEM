using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class FacturaDetalleView : Window
{
    public FacturaDetalleView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is FacturaDetalleViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is FacturaDetalleViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, bool huboCambios)
    {
        DialogResult = huboCambios;
    }
}