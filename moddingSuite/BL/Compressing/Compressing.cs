using System.IO;
using Ionic.Zlib;

namespace moddingSuite.BL.Compressing;
public static class Compressor
{
    public static byte[] Decomp(byte[] input)
    {
        using MemoryStream output = new();
        Decomp(input, output);
        return output.ToArray();
    }
    public static void Decomp(byte[] input, Stream outputStream)
    {
        using ZlibStream zipStream = new(outputStream, CompressionMode.Decompress);
        using MemoryStream inputStream = new(input);
        byte[] buffer = input.Length > 4096 ? new byte[4096] : new byte[input.Length];
        int size;
        while ((size = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            zipStream.Write(buffer, 0, size);
    }
    public static byte[] Comp(byte[] input)
    {
        using MemoryStream sourceStream = new(input);
        using MemoryStream compressed = new();
        using ZlibStream zipSteam = new(compressed, CompressionMode.Compress, CompressionLevel.Level9, true);
        zipSteam.FlushMode = FlushType.Full;
        sourceStream.CopyTo(zipSteam);
        zipSteam.Flush();
        return compressed.ToArray();
    }
}
