using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfTrans : NdfFlatValueWrapper
{
    public NdfTrans(NdfTranReference value)
        : base(NdfType.TransTableReference, value)
    {
    }

    public override byte[] GetBytes() => BitConverter.GetBytes(((NdfTranReference)Value).Id);

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(string.Format("\"{0}\"", ((NdfStringReference)Value).Value));
}