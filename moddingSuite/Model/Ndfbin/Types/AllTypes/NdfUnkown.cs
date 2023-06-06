﻿using moddingSuite.Util;
using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfUnkown : NdfFlatValueWrapper
{
    public NdfUnkown(byte[] value)
        : base(NdfType.Unknown, value)
    {
    }

    public override byte[] GetBytes() => throw new InvalidOperationException("Cant serialize ndfunknown.");

    public override byte[] GetNdfText() => throw new NotImplementedException();

    public override string ToString() => string.Format("{0}", Utils.ByteArrayToBigEndianHexByteString((byte[])Value));
}