using moddingSuite.ViewModel.Base;
using System.Globalization;

namespace moddingSuite.Model.Edata;

public abstract class EdataEntity : ViewModelBase
{
    private int _fileEntrySize;
    private string _name;

    public EdataEntity(string name)
    {
        Name = name;
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged("Name");
        }
    }

    public int FileEntrySize
    {
        get => _fileEntrySize;
        set
        {
            _fileEntrySize = value;
            OnPropertyChanged("FileEntrySize");
        }
    }

    public override string ToString() => Name.ToString(CultureInfo.CurrentCulture);
}