using moddingSuite.ViewModel.Base;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public abstract class NdfValueWrapper : ViewModelBase, INdfScriptSerializable
{
    private NdfType _type;

    protected NdfValueWrapper(NdfType type)
    {
        Type = type;
    }

    public NdfType Type
    {
        get => _type;
        protected set
        {
            _type = value;
            OnPropertyChanged("Type");
        }
    }
    public NdfPropertyValue ParentProperty { get; set; }
    #region INdfScriptSerializable Members

    public abstract byte[] GetNdfText();

    #endregion

    public abstract byte[] GetBytes();
}