using moddingSuite.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace moddingSuite.ViewModel.Edata;

public class DirectoryViewModel : FileSystemItemViewModel
{
    private ObservableCollection<FileSystemItemViewModel> _items = new();

    public DirectoryViewModel(DirectoryInfo info)
    {
        Info = info;
        OpenInFileExplorerCommand = new ActionCommand(OpenInFileExplorerExecute);
    }

    private void OpenInFileExplorerExecute(object obj) => Process.Start(Info.FullName);

    public ObservableCollection<FileSystemItemViewModel> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    public DirectoryInfo Info { get; set; }

    public ICommand OpenInFileExplorerCommand
    {
        get; set;
    }

    public override string Name => Info.Name;
}
