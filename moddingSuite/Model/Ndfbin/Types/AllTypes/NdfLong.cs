using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

internal class NdfLong : NdfFlatValueWrapper
{
    public NdfLong(long value)
        : base(NdfType.Long, value)
    {
    }

    public override byte[] GetBytes() => BitConverter.GetBytes(Convert.ToInt64(Value));

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
}
