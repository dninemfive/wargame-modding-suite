namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public abstract class NdfFlatValueWrapper : NdfValueWrapper
{
    private object _value;

    protected NdfFlatValueWrapper(NdfType type, object value)
        : base(type)
    {
        Value = value;
    }

    public object Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged("Value");
        }
    }

    public override string ToString() => Value.ToString();
}