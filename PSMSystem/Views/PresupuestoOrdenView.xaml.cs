using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class PresupuestoOrdenView : Window
{
    public PresupuestoOrdenView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is PresupuestoOrdenViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is PresupuestoOrdenViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, bool huboCambios)
    {
        DialogResult = huboCambios;
    }
}