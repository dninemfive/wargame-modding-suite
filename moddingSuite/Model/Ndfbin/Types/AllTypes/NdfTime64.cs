using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfTime64 : NdfFlatValueWrapper
{
    public NdfTime64(DateTime value)
        : base(NdfType.Time64, value)
    {
    }

    public override byte[] GetBytes()
    {
        DateTime unixdt = new(1970, 1, 1);
        DateTime msdt = (DateTime)Value;

        ulong res = (ulong)msdt.Subtract(unixdt).TotalSeconds;

        return BitConverter.GetBytes(res);
    }

    public override byte[] GetNdfText() => throw new NotImplementedException();
}
