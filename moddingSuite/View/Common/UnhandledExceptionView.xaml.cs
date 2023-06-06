using System.Windows;

namespace moddingSuite.View.Common;

/// <summary>
/// Interaction logic for UnhandledExceptionView.xaml
/// </summary>
public partial class UnhandledExceptionView : Window
{
    public UnhandledExceptionView()
    {
        InitializeComponent();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e) => Close();
}
