using System;
using System.Collections.Generic;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfZipBlob : NdfFlatValueWrapper
{
    public NdfZipBlob(byte[] value)
        : base(NdfType.ZipBlob, value)
    {
    }

    public override byte[] GetBytes()
    {
        List<byte> val = new();

        val.AddRange(BitConverter.GetBytes((uint)((byte[])Value).Length));
        val.Add(1);
        val.AddRange((byte[])Value);

        return val.ToArray();
    }

    public override byte[] GetNdfText()
    {
        throw new NotImplementedException();
    }
}
