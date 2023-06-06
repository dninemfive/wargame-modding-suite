using moddingSuite.BL.ImageService;
using moddingSuite.BL.ImageService.BlockDXT;
using moddingSuite.Model.Textures;
using moddingSuite.Util;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace moddingSuite.BL.TGV;

public class TgvBitmapReader
{
    public RawImage GetMip(TgvFile file, uint mip)
    {
        RawImage ret;

        uint width = file.ImageWidth;
        uint height = file.ImageHeight;

        // Compute width and height.
        for (uint m = 0; m < mip; m++)
        {
            width = Math.Max(1, width / 2);
            height = Math.Max(1, height / 2);
        }

        ret = new RawImage(width, height);

        using (MemoryStream ms = new(file.MipMaps[(int)mip].Content))
        {
            if (DDS.DDS.IsCompressedFormat(file.Format))
            {
                ReadBlockFormat(ret, ms);
            }
        }

        return ret;
    }

    private static void ReadBlockFormat(RawImage ret, Stream ms)
    {
        ret.ColFormat = RawImage.Format.Format_ARGB;

        uint w = ret.Width;
        uint h = ret.Height;

        uint bw = (w + 3) / 4;
        uint bh = (h + 3) / 4;

        for (uint by = 0; by < bh; by++)
        {
            for (uint bx = 0; bx < bw; bx++)
            {
                ColorBlock block = new();

                // Read color block.
                ReadBlock(block, ms);

                // Write color block.
                for (uint y = 0; y < Math.Min(4, h - (4 * by)); y++)
                {
                    for (uint x = 0; x < Math.Min(4, w - (4 * bx)); x++)
                        ret.Data[(((4 * by) + y) * ret.Width) + (4 * bx) + x] = block.Color(x, y);
                }
            }
        }
    }

    private static void ReadBlock(ColorBlock rgba, Stream ms)
    {
        byte[] blockBuffer = new byte[Marshal.SizeOf(typeof(BlockDXT5))];

        _ = ms.Read(blockBuffer, 0, blockBuffer.Length);

        BlockDXT5 blockdxt5 = Utils.ByteArrayToStructure<BlockDXT5>(blockBuffer);

        blockdxt5.decodeBlock(ref rgba);
    }
}
