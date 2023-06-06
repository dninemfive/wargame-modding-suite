using moddingSuite.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Globalization;

namespace moddingSuite.Model.Ndfbin;

public class NdfClass : ViewModelBase
{
    private uint _id;

    private string _name;

    public uint Id
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

    public ObservableCollection<NdfProperty> Properties { get; } = new();

    public ObservableCollection<NdfObject> Instances { get; } = new();

    public NdfBinary Manager { get; protected set; }

    public NdfClass(NdfBinary mgr, uint id)
    {
        Manager = mgr;
        Id = id;
    }

    public override string ToString() => Name.ToString(CultureInfo.InvariantCulture);
}