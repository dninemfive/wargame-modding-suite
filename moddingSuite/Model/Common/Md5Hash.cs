using System.Collections.Generic;

namespace moddingSuite.Model.Common;

public struct Md5Hash
{
    public byte B_1;
    public byte B_2;
    public byte B_3;
    public byte B_4;
    public byte B_5;
    public byte B_6;
    public byte B_7;
    public byte B_8;
    public byte B_9;
    public byte B_10;
    public byte B_11;
    public byte B_12;
    public byte B_13;
    public byte B_14;
    public byte B_15;
    public byte B_16;

    public static Md5Hash GetHash(byte[] arr)
    {
        Md5Hash h = new()
        {
            B_1 = arr[0],
            B_2 = arr[1],
            B_3 = arr[2],
            B_4 = arr[3],
            B_5 = arr[4],
            B_6 = arr[5],
            B_7 = arr[6],
            B_8 = arr[7],
            B_9 = arr[8],
            B_10 = arr[9],
            B_11 = arr[10],
            B_12 = arr[11],
            B_13 = arr[12],
            B_14 = arr[13],
            B_15 = arr[14],
            B_16 = arr[15]
        };

        return h;
    }

    public static byte[] GetBytes(Md5Hash hash)
    {
        List<byte> l = new()
        {
            hash.B_1,
            hash.B_2,
            hash.B_3,
            hash.B_4,
            hash.B_5,
            hash.B_6,
            hash.B_7,
            hash.B_8,
            hash.B_9,
            hash.B_10,
            hash.B_11,
            hash.B_12,
            hash.B_13,
            hash.B_14,
            hash.B_15,
            hash.B_16
        };

        return l.ToArray();
    }
}
