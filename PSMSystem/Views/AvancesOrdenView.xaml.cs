using System.Windows;
using PSMSystem.ViewsModels;

namespace PSMSystem.Views;

public partial class AvancesOrdenView : Window
{
    public AvancesOrdenView()
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is AvancesOrdenViewModel anterior)
                anterior.SolicitudCierre -= OnSolicitudCierre;

            if (e.NewValue is AvancesOrdenViewModel nuevo)
                nuevo.SolicitudCierre += OnSolicitudCierre;
        };
    }

    private void OnSolicitudCierre(object? sender, EventArgs e) => Close();
}