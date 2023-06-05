﻿using moddingSuite.BL;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace moddingSuite.Util;

public static class Utils
{
    public static string ReadString(Stream fs)
    {
        StringBuilder b = new();
        int c;

        do
        {
            c = fs.ReadByte();
            b.Append((char)c);
        } while (c != '\0');

        return StripString(b.ToString());
    }

    public static string StripString(string s)
    {
        return s.Split('\0')[0].TrimEnd();
    }

    public static string Int32ToBigEndianHexByteString(Int32 i)
    {
        byte[] bytes = BitConverter.GetBytes(i);
        string format = BitConverter.IsLittleEndian
            ? "{0:X2} {1:X2} {2:X2} {3:X2}"
            : "{3:X2} {2:X2} {1:X2} {0:X2}";
        return String.Format(format, bytes[0], bytes[1], bytes[2], bytes[3]);
    }

    public static string ByteArrayToBigEndianHexByteString(byte[] data)
    {
        if (data == null)
            return string.Empty;

        StringBuilder stringBuilderb = new();

        stringBuilderb.Append(string.Empty);

        foreach (byte b in data)
            stringBuilderb.Append(string.Format("{0:X2}", b));

        return stringBuilderb.ToString();
    }

    public static byte[] StringToByteArrayFastest(string hex)
    {
        if (hex.Length % 2 == 1)
            throw new Exception("The binary key cannot have an odd number of digits");

        byte[] arr = new byte[hex.Length >> 1];

        for (int i = 0; i < (hex.Length >> 1); ++i)
            arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));

        return arr;
    }

    public static int GetHexVal(char hex)
    {
        int val = (int)hex;
        //For uppercase A-F letters:
        //return val - (val < 58 ? 48 : 55);
        //For lowercase a-f letters:
        //return val - (val < 58 ? 48 : 87);
        //Or the two combined, but a bit slower:
        return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
    }

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int memcmp(byte[] b1, byte[] b2, long count);

    public static bool ByteArrayCompare(byte[] b1, byte[] b2)
    {
        // Validate buffers are the same length.
        // This also ensures that the count does not exceed the length of either buffer.  
        return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
    }

    public static void SaveDebug(string fileName, byte[] contentData)
    {
        string path = SettingsManager.Load().SavePath;
        string file = Path.Combine(path, string.Format("{0}_{1}", DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ff"), fileName));

        if (!File.Exists(file))
            using (File.Create(file))
            { }

        using (FileStream fs = new(file, FileMode.Truncate))
            fs.Write(contentData, 0, contentData.Length);
    }

    public static string GenerateCoupon(int length, Random random)
    {
        const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        StringBuilder result = new(length);

        for (int i = 0; i < length; i++)
            result.Append(characters[random.Next(characters.Length)]);

        return result.ToString();
    }

    public static byte[] CreateLocalisationHash(string text, int maxSize = 8)
    {
        long hash = 0;
        for (int i = 0; i < maxSize; ++i)
        {
            int value;
            ushort chr = text[i];

            if (chr == 0)
                break;

            if (chr is >= '0' and <= '9')
                value = 1 + chr - '0';
            else if (chr is >= 'A' and <= 'Z')
                value = 2 + '9' - '0' + chr - 'A';
            else if (chr == '_')
                value = 3 + '9' - '0' + 'Z' - 'A';
            else if (chr is >= 'a' and <= 'z')
                value = 4 + '9' - '0' + 'Z' - 'A' + chr - 'a';
            else
                throw new InvalidDataException("");

            hash = (hash << 6) | value;
        }

        return BitConverter.GetBytes(hash);
    }

    public static bool IsValueType(object obj)
    {
        return obj != null && obj.GetType().IsValueType;
    }

    public static byte[] StructToBytes(object str)
    {
        if (!IsValueType(str))
            throw new ArgumentException("str");

        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();

        return stuff;
    }

    public static int RoundToNextDivBy4(int number)
    {
        while (number % 4 != 0)
            number++;

        return number;
    }

    public static void Swap<T>(T a, T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }
}