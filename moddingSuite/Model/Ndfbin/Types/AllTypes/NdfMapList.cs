using System;
using System.Collections.Generic;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfMapList : NdfCollection
{
    public NdfMapList()
    {
        Type = NdfType.MapList;
    }

    public override byte[] GetBytes()
    {
        List<byte> data = new();

        data.AddRange(BitConverter.GetBytes(InnerList.Count));

        foreach (CollectionItemValueHolder valueHolder in InnerList)
        {
            byte[] valueDat = valueHolder.Value.GetBytes();

            //if (valueHolder.Value.Type == NdfType.ObjectReference || valueHolder.Value.Type == NdfType.TransTableReference)
            //    data.AddRange(BitConverter.GetBytes((uint)NdfType.Reference));

            //data.AddRange(BitConverter.GetBytes((uint)valueHolder.Value.Type));
            data.AddRange(valueDat);
        }

        return data.ToArray();
    }
}