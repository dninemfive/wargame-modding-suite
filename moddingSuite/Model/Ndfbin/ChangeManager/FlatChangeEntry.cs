using moddingSuite.Model.Ndfbin.Types.AllTypes;

namespace moddingSuite.Model.Ndfbin.ChangeManager;

public class FlatChangeEntry : ChangeEntryBase
{
    private NdfValueWrapper _newValue;

    public FlatChangeEntry(NdfPropertyValue affectedPropertyValue, NdfValueWrapper newValue)
        : base(affectedPropertyValue)
    {
        NewValue = newValue;
    }

    public NdfValueWrapper NewValue
    {
        get => _newValue;
        set
        {
            _newValue = value;
            OnPropertyChanged(() => NewValue);
        }
    }
}