using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class TurnoEditView : Window
{
    public TurnoEditView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is TurnoEditViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is TurnoEditViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, bool guardadoConExito)
    {
        DialogResult = guardadoConExito;
    }
}