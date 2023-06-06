using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfBoolean : NdfFlatValueWrapper
{
    public NdfBoolean(bool value)
        : base(NdfType.Boolean, value)
    {
    }

    public override byte[] GetBytes() => BitConverter.GetBytes(Convert.ToBoolean(Value));

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
}