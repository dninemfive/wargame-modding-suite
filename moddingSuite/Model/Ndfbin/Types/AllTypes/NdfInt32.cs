using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfInt32 : NdfFlatValueWrapper
{
    public NdfInt32(int value)
        : base(NdfType.Int32, value)
    {
    }

    public override byte[] GetBytes() => BitConverter.GetBytes(Convert.ToInt32(Value));

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
}