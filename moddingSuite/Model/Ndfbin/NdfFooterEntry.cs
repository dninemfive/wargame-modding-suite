using moddingSuite.ViewModel.Base;

namespace moddingSuite.Model.Ndfbin;

public class NdfFooterEntry : ViewModelBase
{
    private string _name;
    private long _offset;
    private long _size;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(() => Name);
        }
    }

    public long Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            OnPropertyChanged(() => Offset);
        }
    }

    public long Size
    {
        get => _size;
        set
        {
            _size = value;
            OnPropertyChanged(() => Size);
        }
    }
}