using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class ClienteEditView : Window
{
    public ClienteEditView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is ClienteEditViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is ClienteEditViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, bool guardadoConExito)
    {
        DialogResult = guardadoConExito;
    }
}