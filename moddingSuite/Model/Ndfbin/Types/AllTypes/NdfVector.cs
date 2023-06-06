using moddingSuite.BL.Ndf;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfVector : NdfFlatValueWrapper
{
    public NdfVector(Point3D value)
        : base(NdfType.Vector, value)
    {
    }

    public override byte[] GetBytes()
    {
        Point3D pt = (Point3D) Value;

        List<byte> vector = new();

        vector.AddRange(BitConverter.GetBytes((float)pt.X));
        vector.AddRange(BitConverter.GetBytes((float)pt.Y));
        vector.AddRange(BitConverter.GetBytes((float)pt.Z));

        return vector.ToArray();
    }

    public override byte[] GetNdfText() => NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
}