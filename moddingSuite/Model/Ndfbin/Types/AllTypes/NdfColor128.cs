using moddingSuite.Util;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfColor128 : NdfFlatValueWrapper
{
    public NdfColor128(byte[] value) : base(NdfType.Color128, value)
    {
    }

    public new byte[] Value
    {
        get => (byte[])base.Value;
        set
        {
            base.Value = value;
            OnPropertyChanged(() => Value);
        }
    }

    public override byte[] GetBytes() => Value;

    public override string ToString() => string.Format("Vec4: {0}", Utils.ByteArrayToBigEndianHexByteString(Value));

    public override byte[] GetNdfText() => throw new NotImplementedException();
}