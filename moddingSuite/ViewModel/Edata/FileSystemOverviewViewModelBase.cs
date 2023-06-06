using moddingSuite.ViewModel.Base;
using System.Collections.ObjectModel;
using System.IO;

namespace moddingSuite.ViewModel.Edata;

public abstract class FileSystemOverviewViewModelBase : ViewModelBase
{
    private string _rootPath;

    public string RootPath
    {
        get => _rootPath;
        set
        {
            _rootPath = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DirectoryViewModel> Root { get; } = new();

    protected DirectoryViewModel ParseRoot() => ParseDirectory(new DirectoryInfo(RootPath));

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
