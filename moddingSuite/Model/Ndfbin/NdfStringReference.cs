using moddingSuite.ViewModel.Base;
using System.Globalization;

namespace moddingSuite.Model.Ndfbin;

public class NdfStringReference : ViewModelBase
{
    private int _id;
    private string _value;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged(() => Id);
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(() => Value);
        }
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}