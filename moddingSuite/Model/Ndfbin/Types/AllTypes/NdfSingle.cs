using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfSingle : NdfFlatValueWrapper
{
    public NdfSingle(float value)
        : base(NdfType.Float32, value)
    {
    }

    public new float Value
    {
        get => (float)base.Value;
        set
        {
            base.Value = value;
            OnPropertyChanged("Value");
        }
    }

    public override byte[] GetBytes() => BitConverter.GetBytes(Convert.ToSingle(Value));

    public override string ToString() => string.Format("{0:0.####################}", Value);

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
}