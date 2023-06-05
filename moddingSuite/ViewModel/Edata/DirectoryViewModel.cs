using moddingSuite.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace moddingSuite.ViewModel.Edata;

public class DirectoryViewModel : FileSystemItemViewModel
{
    private ObservableCollection<FileSystemItemViewModel> _items = new();
    private  DirectoryInfo _directoryInfo;

    public DirectoryViewModel(DirectoryInfo info)
    {
        _directoryInfo = info;
        OpenInFileExplorerCommand = new ActionCommand(OpenInFileExplorerExecute);
    }

    private void OpenInFileExplorerExecute(object obj)
    {
        Process.Start(this.Info.FullName);
    }

    public ObservableCollection<FileSystemItemViewModel> Items
    {
        get
        {
            return _items;
        }
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    public DirectoryInfo Info
    {
        get
        {
            return _directoryInfo;
        }
        set
        {
            _directoryInfo = value;
        }
    }

    public ICommand OpenInFileExplorerCommand
    {
        get; set;
    }

    public override string Name
    {
        get
        {
            return Info.Name;
        }
    }
}
