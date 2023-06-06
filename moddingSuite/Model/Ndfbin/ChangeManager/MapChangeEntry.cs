namespace moddingSuite.Model.Ndfbin.ChangeManager;

public class MapChangeEntry : ChangeEntryBase
{
    private MapValueHolder _newKey;
    private MapValueHolder _newValue;

    public MapChangeEntry(NdfPropertyValue affectedPropertyValue, MapValueHolder newKey, MapValueHolder newValue)
        : base(affectedPropertyValue)
    {
        NewKey = newKey;
        NewValue = newValue;

    }

    public MapValueHolder NewKey
    {
        get => _newKey;
        set
        {
            _newKey = value;
            OnPropertyChanged("NewKey");
        }
    }

    public MapValueHolder NewValue
    {
        get => _newValue;
        set
        {
            _newValue = value;
            OnPropertyChanged("NewValue");
        }
    }

    private int min(int l, int r) => l < r ? l : r;

    private float min(float l, float r) => l < r ? l : r;

}
