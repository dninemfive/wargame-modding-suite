using moddingSuite.BL.Compressing;
using moddingSuite.BL.DDS;
using moddingSuite.Model.Textures;
using moddingSuite.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace moddingSuite.BL.TGV;

public class TgvWriter
{
    // We do write TGV files in version 2.
    public const uint Version = 0x00000002;

    public void Write(Stream destStream, TgvFile sourceFile, byte[] sourceChecksum, bool compress = true)
    {
        if (sourceChecksum.Length > 16)
            throw new ArgumentException("sourceChecksum");

        sourceFile.PixelFormatStr = TranslatePixelFormatBack(sourceFile.Format);
        byte[] zipoMagic = Encoding.ASCII.GetBytes("ZIPO");

        byte[] buffer = BitConverter.GetBytes(Version);
        destStream.Write(buffer, 0, buffer.Length);

        buffer = BitConverter.GetBytes(compress ? 1 : 0);
        destStream.Write(buffer, 0, buffer.Length);

        buffer = BitConverter.GetBytes(sourceFile.Width);
        destStream.Write(buffer, 0, buffer.Length);
        buffer = BitConverter.GetBytes(sourceFile.Height);
        destStream.Write(buffer, 0, buffer.Length);

        buffer = BitConverter.GetBytes(sourceFile.Width);
        destStream.Write(buffer, 0, buffer.Length);
        buffer = BitConverter.GetBytes(sourceFile.Height);
        destStream.Write(buffer, 0, buffer.Length);

        buffer = BitConverter.GetBytes((short)sourceFile.MipMapCount);
        destStream.Write(buffer, 0, buffer.Length);

        short fmtLen = (short)sourceFile.PixelFormatStr.Length;
        buffer = BitConverter.GetBytes(fmtLen);
        destStream.Write(buffer, 0, buffer.Length);

        buffer = Encoding.ASCII.GetBytes(sourceFile.PixelFormatStr);
        destStream.Write(buffer, 0, buffer.Length);
        destStream.Seek(Utils.RoundToNextDivBy4(fmtLen) - fmtLen, SeekOrigin.Current);

        destStream.Write(sourceChecksum, 0, sourceChecksum.Length);

        long mipdefOffset = (destStream.Position);

        List<int> mipImgsizes = new();
        uint tileSize = sourceFile.Width - sourceFile.Width / sourceFile.MipMapCount;

        for (int i = 0; i < sourceFile.MipMapCount; i++)
        {
            destStream.Seek(8, SeekOrigin.Current);
            mipImgsizes.Add((int)tileSize);
            tileSize /= 4;
        }

        List<TgvMipMap> sortedMipMaps = sourceFile.MipMaps.OrderBy(x => x.Content.Length).ToList();

        //mipImgsizes = mipImgsizes.OrderBy(x => x).ToList();

        // Create the content and write all MipMaps, 
        // since we compress on this part its the first part where we know the size of a MipMap
        foreach (TgvMipMap sortedMipMap in sortedMipMaps)
        {
            sortedMipMap.Offset = (uint)destStream.Position;
            if (compress)
            {
                destStream.Write(zipoMagic, 0, zipoMagic.Length);

                //buffer = BitConverter.GetBytes(mipImgsizes[sortedMipMaps.IndexOf(sortedMipMap)]);
                buffer = BitConverter.GetBytes((int)Math.Pow(4, sortedMipMaps.IndexOf(sortedMipMap)));
                destStream.Write(buffer, 0, buffer.Length);

                buffer = Compressor.Comp(sortedMipMap.Content);
                destStream.Write(buffer, 0, buffer.Length);
                sortedMipMap.Size = (uint)buffer.Length;
            }
            else
            {
                destStream.Write(sortedMipMap.Content, 0, sortedMipMap.Content.Length);
                sortedMipMap.Size = (uint)sortedMipMap.Content.Length;
            }
        }

        destStream.Seek(mipdefOffset, SeekOrigin.Begin);

        // Write the offset collection in the header.
        for (int i = 0; i < sourceFile.MipMapCount; i++)
        {
            buffer = BitConverter.GetBytes(sortedMipMaps[i].Offset);
            destStream.Write(buffer, 0, buffer.Length);
        }

        // Write the size collection into the header.
        for (int i = 0; i < sourceFile.MipMapCount; i++)
        {
            buffer = BitConverter.GetBytes(sortedMipMaps[i].Size + 8);
            destStream.Write(buffer, 0, buffer.Length);
        }
    }

    protected string TranslatePixelFormatBack(PixelFormats pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormats.R8G8B8A8_UNORM => "A8R8G8B8",
            PixelFormats.B8G8R8X8_UNORM => "X8R8G8B8",
            PixelFormats.B8G8R8X8_UNORM_SRGB => "X8R8G8B8_SRGB",
            PixelFormats.R8G8B8A8_UNORM_SRGB => "A8R8G8B8_SRGB",
            PixelFormats.R16G16B16A16_UNORM => "A16B16G16R16",
            PixelFormats.R16G16B16A16_FLOAT => "A16B16G16R16F",
            PixelFormats.R32G32B32A32_FLOAT => "A32B32G32R32F",
            PixelFormats.A8_UNORM => "A8",
            PixelFormats.A8P8 => "A8P8",
            PixelFormats.P8 => "P8",
            PixelFormats.R8_UNORM => "L8",
            PixelFormats.R16_UNORM => "L16",
            PixelFormats.D16_UNORM => "D16",
            PixelFormats.R8G8_SNORM => "V8U8",
            PixelFormats.R16G16_SNORM => "V16U16",
            PixelFormats.BC1_UNORM => "DXT1",
            PixelFormats.BC1_UNORM_SRGB => "DXT1_SRGB",
            PixelFormats.BC2_UNORM => "DXT3",
            PixelFormats.BC2_UNORM_SRGB => "DXT3_SRGB",
            PixelFormats.BC3_UNORM => "DXT5",
            PixelFormats.BC3_UNORM_SRGB => "DXT5_SRGB",
            _ => throw new NotSupportedException(string.Format("Unsupported PixelFormat {0}", pixelFormat)),
        };
    }
}
