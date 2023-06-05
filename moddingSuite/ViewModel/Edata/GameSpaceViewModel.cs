using moddingSuite.Model.Settings;
using System.IO;

namespace moddingSuite.ViewModel.Edata;

public class GameSpaceViewModel : FileSystemOverviewViewModelBase
{
    public GameSpaceViewModel(Settings settings)
    {
        RootPath = settings.WargamePath;

        if (Directory.Exists(RootPath))
            Root.Add(ParseRoot());
    }
}
