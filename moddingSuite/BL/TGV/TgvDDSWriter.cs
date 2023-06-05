using moddingSuite.Model.Textures;
using moddingSuite.Util;
using System;
using System.IO;
using System.Linq;

namespace moddingSuite.BL.TGV;

/// <summary>
/// Writes a DDS File     
/// </summary>
public class TgvDDSWriter
{
    public byte[] CreateDDSFile(TgvFile file)
    {
        using (MemoryStream ms = new())
        {
            byte[] buffer = BitConverter.GetBytes(DDS.DDS.MagicHeader);
            ms.Write(buffer, 0, buffer.Length);

            buffer = CreateDDSHeader(file);
            ms.Write(buffer, 0, buffer.Length);

            buffer = file.MipMaps.OrderByDescending(x => x.MipWidth).First().Content;
            ms.Write(buffer, 0, buffer.Length);

            return ms.ToArray();
        }
    }

    protected byte[] CreateDDSHeader(TgvFile file)
    {
        DDS.DDS.Header hd = new()
        {
            Size = 124,
            Flags = DDS.DDS.HeaderFlags.Texture,
            SurfaceFlags = DDS.DDS.SurfaceFlags.Texture,
            Width = file.ImageWidth,
            Height = file.ImageHeight,
            Depth = 0,
            MipMapCount = 1
        };

        DDS.DDS.PixelFormat ddpf = DDS.DDS.PixelFormatFromDXGIFormat(file.Format);

        int rowPitch, slicePitch;
        int widthCount, heightCount;
        DDS.DDS.ComputePitch(file.Format, (int)file.ImageWidth, (int)file.ImageHeight, out rowPitch, out slicePitch, out widthCount,
                     out heightCount);

        if (DDS.DDS.IsCompressedFormat(file.Format))
        {
            hd.Flags |= DDS.DDS.HeaderFlags.LinearSize;
            hd.PitchOrLinearSize = slicePitch;
        }
        else
        {
            hd.Flags |= DDS.DDS.HeaderFlags.Pitch;
            hd.PitchOrLinearSize = rowPitch;
        }

        hd.PixelFormat = ddpf;

        return Utils.StructToBytes(hd);
    }
}