using moddingSuite.ViewModel.Base;
using System.Globalization;

namespace moddingSuite.Model.Ndfbin;

public class NdfProperty : ViewModelBase
{
    private int _id;
    private string _name;
    private NdfClass _class;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged(() => Id);
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(() => Name);
        }
    }

    public NdfClass Class
    {
        get => _class;
        set
        {
            _class = value;
            OnPropertyChanged(() => Class);
        }
    }

    public NdfProperty(int id)
    {
        Id = id;
    }

    public override string ToString() => Name.ToString(CultureInfo.InvariantCulture);
}