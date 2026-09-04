using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class RepuestoEditView : Window
{
    public RepuestoEditView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is RepuestoEditViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is RepuestoEditViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, bool guardadoConExito)
    {
        DialogResult = guardadoConExito;
    }
}