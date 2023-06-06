using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfUInt16 : NdfFlatValueWrapper
{
    public NdfUInt16(ushort value)
        : base(NdfType.UInt16, value)
    {
    }

    public override byte[] GetBytes() => BitConverter.GetBytes(Convert.ToUInt16(Value));

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
}