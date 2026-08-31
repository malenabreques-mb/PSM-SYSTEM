using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class OrdenTrabajoEditView : Window
{
    public OrdenTrabajoEditView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is OrdenTrabajoEditViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is OrdenTrabajoEditViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, bool guardadoConExito)
    {
        DialogResult = guardadoConExito;
    }
}