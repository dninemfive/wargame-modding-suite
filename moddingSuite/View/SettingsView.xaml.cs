using moddingSuite.Model.Settings;
using System.Windows;
using System.Windows.Forms;

namespace moddingSuite.View;

/// <summary>
/// Interaktionslogik für SettingsView.xaml
/// </summary>
public partial class SettingsView : Window
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CanceButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void WorkSpaceBrowserButtonClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not Settings settings)
            return;

        FolderBrowserDialog folderDlg = new()
        {
            SelectedPath = settings.SavePath,
            //RootFolder = Environment.SpecialFolder.MyComputer,
            ShowNewFolderButton = true,
        };

        if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            settings.SavePath = folderDlg.SelectedPath;
    }

    private void GameSpaceButtonClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not Settings settings)
            return;

        FolderBrowserDialog folderDlg = new()
        {
            SelectedPath = settings.WargamePath,
            //RootFolder = Environment.SpecialFolder.MyComputer,
            ShowNewFolderButton = true,
        };

        if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            settings.WargamePath = folderDlg.SelectedPath;
    }
}
