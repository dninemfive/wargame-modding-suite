using moddingSuite.ViewModel.Base;

namespace moddingSuite.ViewModel.Edata;

public abstract class FileSystemItemViewModel : ViewModelBase
{
    public abstract string Name { get; }

    public void Invalidate()
    {
        OnPropertyChanged(nameof(Name));
    }
}
