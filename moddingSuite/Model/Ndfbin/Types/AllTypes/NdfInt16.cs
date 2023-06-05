using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfInt16 : NdfFlatValueWrapper
{
    public NdfInt16(short value)
        : base(NdfType.Int16, value)
    {

    }


    public override byte[] GetBytes()
    {
        return BitConverter.GetBytes(Convert.ToInt16(Value));
    }

    public override byte[] GetNdfText()
    {
        return NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
    }
}
