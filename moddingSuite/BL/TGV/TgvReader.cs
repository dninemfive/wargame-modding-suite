using moddingSuite.BL.Compressing;
using moddingSuite.BL.DDS;
using moddingSuite.Model.Textures;
using moddingSuite.Util;
using System;
using System.IO;
using System.Text;

namespace moddingSuite.BL.TGV;

public class TgvReader
{
    public TgvFile Read(Stream ms)
    {
        TgvFile file = new();

        byte[] buffer = new byte[4];

        _ = ms.Read(buffer, 0, buffer.Length);
        file.Version = BitConverter.ToUInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        file.IsCompressed = BitConverter.ToInt32(buffer, 0) > 0;

        _ = ms.Read(buffer, 0, buffer.Length);
        file.Width = BitConverter.ToUInt32(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        file.Height = BitConverter.ToUInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        file.ImageWidth = BitConverter.ToUInt32(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        file.ImageHeight = BitConverter.ToUInt32(buffer, 0);

        buffer = new byte[2];

        _ = ms.Read(buffer, 0, buffer.Length);
        file.MipMapCount = BitConverter.ToUInt16(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        ushort pixelFormatLen = BitConverter.ToUInt16(buffer, 0);

        buffer = new byte[pixelFormatLen];

        _ = ms.Read(buffer, 0, buffer.Length);
        file.PixelFormatStr = Encoding.ASCII.GetString(buffer);

        _ = ms.Seek(Utils.RoundToNextDivBy4(pixelFormatLen) - pixelFormatLen, SeekOrigin.Current);

        buffer = new byte[16];
        _ = ms.Read(buffer, 0, buffer.Length);
        file.SourceChecksum = (byte[])buffer.Clone();

        buffer = new byte[4];

        for (int i = 0; i < file.MipMapCount; i++)
        {
            _ = ms.Read(buffer, 0, buffer.Length);
            uint offset = BitConverter.ToUInt32(buffer, 0);
            file.Offsets.Add(offset);
        }

        for (int i = 0; i < file.MipMapCount; i++)
        {
            _ = ms.Read(buffer, 0, buffer.Length);
            uint offset = BitConverter.ToUInt32(buffer, 0);
            file.Sizes.Add(offset);
        }

        for (int i = 0; i < file.MipMapCount; i++)
            file.MipMaps.Add(ReadMip(ms, file, i));

        file.Format = TranslatePixelFormat(file.PixelFormatStr);

        //if (file.Width != file.ImageWidth || file.Height != file.ImageHeight)
        //{
        //    throw new InvalidDataException("something interresting happened here");
        //}

        return file;
    }

    public TgvFile Read(byte[] data)
    {
        using MemoryStream ms = new(data);
        return Read(ms);

        //var file = new TgvFile();

        //using (var ms = new MemoryStream(data))
        //{
        //    var buffer = new byte[4];

        //    ms.Read(buffer, 0, buffer.Length);
        //    file.Version = BitConverter.ToUInt32(buffer, 0);

        //    ms.Read(buffer, 0, buffer.Length);
        //    file.IsCompressed = BitConverter.ToInt32(buffer, 0) > 0;

        //    ms.Read(buffer, 0, buffer.Length);
        //    file.Width = BitConverter.ToUInt32(buffer, 0);
        //    ms.Read(buffer, 0, buffer.Length);
        //    file.Height = BitConverter.ToUInt32(buffer, 0);

        //    ms.Read(buffer, 0, buffer.Length);
        //    file.ImageWidth = BitConverter.ToUInt32(buffer, 0);
        //    ms.Read(buffer, 0, buffer.Length);
        //    file.ImageHeight = BitConverter.ToUInt32(buffer, 0);

        //    buffer = new byte[2];

        //    ms.Read(buffer, 0, buffer.Length);
        //    file.MipMapCount = BitConverter.ToUInt16(buffer, 0);

        //    ms.Read(buffer, 0, buffer.Length);
        //    ushort pixelFormatLen = BitConverter.ToUInt16(buffer, 0);

        //    buffer = new byte[pixelFormatLen];

        //    ms.Read(buffer, 0, buffer.Length);
        //    file.PixelFormatStr = Encoding.ASCII.GetString(buffer);

        //    ms.Seek(Utils.RoundToNextDivBy4(pixelFormatLen) - pixelFormatLen, SeekOrigin.Current);

        //    buffer = new byte[16];
        //    ms.Read(buffer, 0, buffer.Length);
        //    file.SourceChecksum = (byte[])buffer.Clone();

        //    buffer = new byte[4];

        //    for (int i = 0; i < file.MipMapCount; i++)
        //    {
        //        ms.Read(buffer, 0, buffer.Length);
        //        uint offset = BitConverter.ToUInt32(buffer, 0);
        //        file.Offsets.Add(offset);
        //    }

        //    for (int i = 0; i < file.MipMapCount; i++)
        //    {
        //        ms.Read(buffer, 0, buffer.Length);
        //        uint offset = BitConverter.ToUInt32(buffer, 0);
        //        file.Sizes.Add(offset);
        //    }

        //    for (int i = 0; i < file.MipMapCount; i++)
        //        file.MipMaps.Add(ReadMip(i, data, file));
        //}

        //file.Format = TranslatePixelFormat(file.PixelFormatStr);

        ////if (file.Width != file.ImageWidth || file.Height != file.ImageHeight)
        ////{
        ////    throw new InvalidDataException("something interresting happened here");
        ////}

        //return file;
    }

    /// <summary>
    /// This method is stream and order dependant, don't use outside.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="file"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    private TgvMipMap ReadMip(Stream ms, TgvFile file, int id)
    {
        if (id > file.MipMapCount)
            throw new ArgumentException("id");

        byte[] zipo = new byte[] { 0x5A, 0x49, 0x50, 0x4F };

        TgvMipMap mipMap = new(file.Offsets[id], file.Sizes[id], 0);

        byte[] buffer;

        if (file.IsCompressed)
        {
            buffer = new byte[4];

            _ = ms.Read(buffer, 0, buffer.Length);
            if (!Utils.ByteArrayCompare(buffer, zipo))
                throw new InvalidDataException("Mipmap has to start with \"ZIPO\"!");

            _ = ms.Read(buffer, 0, buffer.Length);
            mipMap.MipWidth = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[mipMap.Size - 8];
        }
        else
        {
            buffer = new byte[mipMap.Size];
        }

        _ = ms.Read(buffer, 0, buffer.Length);

        _ = ms.Seek(Utils.RoundToNextDivBy4((int)mipMap.Size) - mipMap.Size, SeekOrigin.Current);

        if (file.IsCompressed)
            buffer = Compressor.Decomp(buffer);

        mipMap.Content = buffer;

        return mipMap;
    }

    protected PixelFormats TranslatePixelFormat(string pixelFormat) => pixelFormat switch
    {
        "A8R8G8B8_HDR" or "A8R8G8B8_LIN" or "A8R8G8B8_LIN_HDR" or "A8R8G8B8" => PixelFormats.R8G8B8A8_UNORM,
        "X8R8G8B8" or "X8R8G8B8_LE" => PixelFormats.B8G8R8X8_UNORM,
        "X8R8G8B8_SRGB" => PixelFormats.B8G8R8X8_UNORM_SRGB,
        "A8R8G8B8_SRGB" or "A8R8G8B8_SRGB_HDR" => PixelFormats.R8G8B8A8_UNORM_SRGB,
        "A16B16G16R16" or "A16B16G16R16_EDRAM" => PixelFormats.R16G16B16A16_UNORM,
        "A16B16G16R16F" or "A16B16G16R16F_LIN" => PixelFormats.R16G16B16A16_FLOAT,
        "A32B32G32R32F" or "A32B32G32R32F_LIN" => PixelFormats.R32G32B32A32_FLOAT,
        "A8" or "A8_LIN" => PixelFormats.A8_UNORM,
        "A8P8" => PixelFormats.A8P8,
        "P8" => PixelFormats.P8,
        "L8" or "L8_LIN" => PixelFormats.R8_UNORM,
        "L16" or "L16_LIN" => PixelFormats.R16_UNORM,
        "D16_LOCKABLE" or "D16" or "D16F" => PixelFormats.D16_UNORM,
        "V8U8" => PixelFormats.R8G8_SNORM,
        "V16U16" => PixelFormats.R16G16_SNORM,
        "DXT1" or "DXT1_LIN" => PixelFormats.BC1_UNORM,
        "DXT1_SRGB" => PixelFormats.BC1_UNORM_SRGB,
        "DXT2" or "DXT3" or "DXT3_LIN" => PixelFormats.BC2_UNORM,
        "DXT3_SRGB" => PixelFormats.BC2_UNORM_SRGB,
        "DXT4" or "DXT5" or "DXT5_LIN" or "DXT5_FROM_ENCODE" => PixelFormats.BC3_UNORM,
        "DXT5_SRGB" => PixelFormats.BC3_UNORM_SRGB,
        "R5G6B5_LIN" or "R5G6B5" or "R8G8B8" or "X1R5G5B5" or "X1R5G5B5_LIN" or "A1R5G5B5" or "A4R4G4B4" or "R3G3B2" or "A8R3G3B2" or "X4R4G4B4" or "A8L8" or "A4L4" or "L6V5U5" or "X8L8V8U8" or "Q8W8U8V8" or "W11V11U10" or "UYVY" or "YUY2" or "D32" or "D32F_LOCKABLE" or "D15S1" or "D24S8" or "R16F" or "R32F" or "R32F_LIN" or "A2R10G10B10" or "D24X8" or "D24X8F" or "D24X4S4" or "G16R16" or "G16R16_EDRAM" or "G16R16F" or "G16R16F_LIN" or "G32R32F" or "G32R32F_LIN" or "A2R10G10B10_LE" or "CTX1" or "CTX1_LIN" or "DXN" or "DXN_LIN" or "INTZ" or "RAWZ" or "DF24" or "PIXNULL" => throw new NotSupportedException(string.Format("Pixelformat {0} not supported", pixelFormat)),
        _ => throw new NotSupportedException(string.Format("Unknown Pixelformat {0} ", pixelFormat)),
    };
}
