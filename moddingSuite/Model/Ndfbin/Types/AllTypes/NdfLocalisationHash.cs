using moddingSuite.Util;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfLocalisationHash : NdfFlatValueWrapper
{
    public NdfLocalisationHash(byte[] value)
        : base(NdfType.LocalisationHash, value)
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

    public override string ToString() => Utils.ByteArrayToBigEndianHexByteString(Value);

    public override byte[] GetNdfText() => throw new NotImplementedException();
}