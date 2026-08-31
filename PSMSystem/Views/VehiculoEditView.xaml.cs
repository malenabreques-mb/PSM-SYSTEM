using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class VehiculoEditView : Window
{
    public VehiculoEditView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is VehiculoEditViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is VehiculoEditViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, bool guardadoConExito)
    {
        DialogResult = guardadoConExito;
    }
}