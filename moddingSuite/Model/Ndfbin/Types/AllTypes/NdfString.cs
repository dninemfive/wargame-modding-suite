using moddingSuite.BL.Ndf;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfString : NdfFlatValueWrapper
{
    public NdfString(NdfStringReference value)
        : base(NdfType.TableString, value)
    {
    }

    public override byte[] GetBytes() => BitConverter.GetBytes(((NdfStringReference)Value).Id);

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(string.Format("\"{0}\"", ((NdfStringReference)Value).Value));
}