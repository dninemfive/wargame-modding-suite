using System.IO;

namespace moddingSuite.ViewModel.Edata;

public class FileViewModel : FileSystemItemViewModel
{
    public FileViewModel(FileInfo info)
    {
        Info = info;
    }

    public FileInfo Info { get; set; }

    public override string Name => Info.Name;
}
