using moddingSuite.ViewModel.Base;
using System.IO;

namespace moddingSuite.ViewModel.VersionManager;

public class VersionFileViewModel : ViewModelBase
{
    private string _path;
    private FileInfo _fileInfo;

    public string Path
    {
        get { return _path; }
        set { _path = value; OnPropertyChanged(() => Path); }
    }

    public FileInfo FileInfo
    {
        get { return _fileInfo; }
        set { _fileInfo = value; OnPropertyChanged(() => FileInfo); }
    }

    public VersionFileViewModel(FileInfo i)
    {
        FileInfo = i;
        Path = i.FullName;
    }
}
