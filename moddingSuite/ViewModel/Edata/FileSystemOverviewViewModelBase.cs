using moddingSuite.ViewModel.Base;
using System.Collections.ObjectModel;
using System.IO;

namespace moddingSuite.ViewModel.Edata;

public abstract class FileSystemOverviewViewModelBase : ViewModelBase
{
    private string _rootPath;
    private readonly ObservableCollection<DirectoryViewModel> _root = new();

    public string RootPath
    {
        get { return _rootPath; }
        set
        {
            _rootPath = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DirectoryViewModel> Root
    {
        get { return _root; }
    }

    protected DirectoryViewModel ParseRoot()
    {
        return ParseDirectory(new DirectoryInfo(RootPath));
    }

    protected DirectoryViewModel ParseDirectory(DirectoryInfo info)
    {
        DirectoryViewModel dirVm = new(info);

        foreach (DirectoryInfo directoryInfo in dirVm.Info.EnumerateDirectories())
        {
            dirVm.Items.Add(ParseDirectory(directoryInfo));
        }

        foreach (FileInfo fileInfo in dirVm.Info.EnumerateFiles())
        {
            dirVm.Items.Add(new FileViewModel(fileInfo));
        }

        return dirVm;
    }
}
