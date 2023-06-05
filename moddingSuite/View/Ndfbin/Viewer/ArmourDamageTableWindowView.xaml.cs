using moddingSuite.ViewModel.Ndf;
using System.Windows;
using System.Windows.Controls;

namespace moddingSuite.View.Ndfbin.Viewer;

/// <summary>
/// Interaction logic for ArmourDamageTableWindow.xaml
/// </summary>
public partial class ArmourDamageTableWindowView : Window
{
    public ArmourDamageTableWindowView()
    {
        InitializeComponent();
    }
    void DataGrid_LoadingRow(object w, DataGridRowEventArgs e)
    {
        System.Collections.ObjectModel.ObservableCollection<string> headers = ((ArmourDamageViewModel)DataContext).RowHeaders;
        if (e.Row.GetIndex() < headers.Count)
        {
            e.Row.Header = headers[e.Row.GetIndex()];
        }
    }
}
