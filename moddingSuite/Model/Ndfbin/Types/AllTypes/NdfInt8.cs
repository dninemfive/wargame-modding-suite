using moddingSuite.BL.Ndf;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfInt8 : NdfFlatValueWrapper
{
    public NdfInt8(byte value)
        : base(NdfType.Int8, value)
    {
    }

    public override byte[] GetBytes() => new byte[1] { (byte)Value };

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
}