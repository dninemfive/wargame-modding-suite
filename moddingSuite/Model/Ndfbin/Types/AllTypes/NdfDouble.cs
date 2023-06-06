using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfDouble : NdfFlatValueWrapper
{
    public NdfDouble(double value)
        : base(NdfType.Float64, value)
    {
    }

    public new double Value
    {
        get => (double)base.Value;
        set
        {
            base.Value = value;
            OnPropertyChanged("Value");
        }
    }

    public override byte[] GetBytes() => BitConverter.GetBytes(Convert.ToDouble(Value));

    public override string ToString() => string.Format("{0:0.###################################}", Value);

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
}