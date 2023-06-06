using System;
using System.Runtime.InteropServices;

namespace moddingSuite.BL.DDS;
public class DDS
{
    /// <summary>
    /// DDS Cubemap flags.
    /// </summary>
    [Flags]
    public enum CubemapFlags
    {
        /// <summary>
        /// DDSCAPS2_CUBEMAP
        /// </summary>
        CubeMap = 0x00000200,
        /// <summary>
        /// DDSCAPS2_VOLUME
        /// </summary>
        Volume = 0x00200000,
        /// <summary>
        /// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
        /// </summary>
        PositiveX = 0x00000600,
        /// <summary>
        /// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
        /// </summary>
        NegativeX = 0x00000a00,
        /// <summary>
        /// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
        /// </summary>
        PositiveY = 0x00001200,
        /// <summary>
        /// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
        /// </summary>
        NegativeY = 0x00002200,
        /// <summary>
        /// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
        /// </summary>
        PositiveZ = 0x00004200,
        /// <summary>
        /// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ
        /// </summary>
        NegativeZ = 0x00008200,

        AllFaces = PositiveX | NegativeX | PositiveY | NegativeY | PositiveZ | NegativeZ,
    }
    /// <summary>
    /// DDS Header flags.
    /// </summary>
    [Flags]
    public enum HeaderFlags
    {
        /// <summary>
        /// DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT 
        /// </summary>
        Texture = 0x00001007,
        /// <summary>
        /// DDSD_MIPMAPCOUNT
        /// </summary>
        Mipmap = 0x00020000,
        /// <summary>
        ///  DDSD_DEPTH
        /// </summary>
        Volume = 0x00800000,
        /// <summary>
        /// DDSD_PITCH
        /// </summary>
        Pitch = 0x00000008,
        /// <summary>
        /// DDSD_LINEARSIZE
        /// </summary>
        LinearSize = 0x00080000,
        /// <summary>
        /// DDSD_HEIGHT
        /// </summary>
        Height = 0x00000002,
        /// <summary>
        /// DDSD_WIDTH
        /// </summary>
        Width = 0x00000004,
    };
    /// <summary>
    /// PixelFormat flags.
    /// </summary>
    [Flags]
    public enum PixelFormatFlags
    {
        /// <summary>
        /// DDPF_FOURCC
        /// </summary>
        FourCC = 0x00000004,
        /// <summary>
        /// DDPF_RGB
        /// </summary>
        Rgb = 0x00000040,
        /// <summary>
        /// DDPF_RGB | DDPF_ALPHAPIXELS
        /// </summary>
        Rgba = 0x00000041,
        /// <summary>
        /// DDPF_LUMINANCE
        /// </summary>
        Luminance = 0x00020000,
        /// <summary>
        /// DDPF_LUMINANCE | DDPF_ALPHAPIXELS
        /// </summary>
        LuminanceAlpha = 0x00020001,
        /// <summary>
        /// DDPF_ALPHA
        /// </summary>
        Alpha = 0x00000002,
        /// <summary>
        /// DDPF_PALETTEINDEXED8
        /// </summary>
        Pal8 = 0x00000020,
    }
    /// <summary>
    /// DDS Surface flags.
    /// </summary>
    [Flags]
    public enum SurfaceFlags
    {
        /// <summary>
        /// DDSCAPS_TEXTURE
        /// </summary>
        Texture = 0x00001000,
        /// <summary>
        /// DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
        /// </summary>
        Mipmap = 0x00400008,
        /// <summary>
        /// DDSCAPS_COMPLEX
        /// </summary>
        Cubemap = 0x00000008,
    }
    /// <summary>
    /// Magic code to identify DDS header
    /// </summary>
    public const uint MagicHeader = 0x20534444; // "DDS "
    /// <summary>
    /// http://www.getcodesamples.com/src/16371480/B0244AD5
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static int GetBitsPerPixel(PixelFormats format) => format switch
    {
        PixelFormats.R32G32B32A32_TYPELESS or
        PixelFormats.R32G32B32A32_FLOAT or
        PixelFormats.R32G32B32A32_UINT or
        PixelFormats.R32G32B32A32_SINT
            => 128,
        PixelFormats.R32G32B32_TYPELESS or
        PixelFormats.R32G32B32_FLOAT or
        PixelFormats.R32G32B32_UINT or
        PixelFormats.R32G32B32_SINT
            => 96,
        PixelFormats.R16G16B16A16_TYPELESS or
        PixelFormats.R16G16B16A16_FLOAT or
        PixelFormats.R16G16B16A16_UNORM or
        PixelFormats.R16G16B16A16_UINT or
        PixelFormats.R16G16B16A16_SNORM or
        PixelFormats.R16G16B16A16_SINT or
        PixelFormats.R32G32_TYPELESS or
        PixelFormats.R32G32_FLOAT or
        PixelFormats.R32G32_UINT or
        PixelFormats.R32G32_SINT or
        PixelFormats.R32G8X24_TYPELESS or
        PixelFormats.D32_FLOAT_S8X24_UINT or
        PixelFormats.R32_FLOAT_X8X24_TYPELESS or
        PixelFormats.X32_TYPELESS_G8X24_UINT
            => 64,
        PixelFormats.R10G10B10A2_TYPELESS or
        PixelFormats.R10G10B10A2_UNORM or
        PixelFormats.R10G10B10A2_UINT or
        PixelFormats.R11G11B10_FLOAT or
        PixelFormats.R8G8B8A8_TYPELESS or
        PixelFormats.R8G8B8A8_UNORM or
        PixelFormats.R8G8B8A8_UNORM_SRGB or
        PixelFormats.R8G8B8A8_UINT or
        PixelFormats.R8G8B8A8_SNORM or
        PixelFormats.R8G8B8A8_SINT or
        PixelFormats.R16G16_TYPELESS or
        PixelFormats.R16G16_FLOAT or
        PixelFormats.R16G16_UNORM or
        PixelFormats.R16G16_UINT or
        PixelFormats.R16G16_SNORM or
        PixelFormats.R16G16_SINT or
        PixelFormats.R32_TYPELESS or
        PixelFormats.D32_FLOAT or
        PixelFormats.R32_FLOAT or
        PixelFormats.R32_UINT or
        PixelFormats.R32_SINT or
        PixelFormats.R24G8_TYPELESS or
        PixelFormats.D24_UNORM_S8_UINT or
        PixelFormats.R24_UNORM_X8_TYPELESS or
        PixelFormats.X24_TYPELESS_G8_UINT or
        PixelFormats.R9G9B9E5_SHAREDEXP or
        PixelFormats.R8G8_B8G8_UNORM or
        PixelFormats.G8R8_G8B8_UNORM or
        PixelFormats.B8G8R8A8_UNORM or
        PixelFormats.B8G8R8X8_UNORM or
        PixelFormats.R10G10B10_XR_BIAS_A2_UNORM or
        PixelFormats.B8G8R8A8_TYPELESS or
        PixelFormats.B8G8R8A8_UNORM_SRGB or
        PixelFormats.B8G8R8X8_TYPELESS or
        PixelFormats.B8G8R8X8_UNORM_SRGB
            => 32,
        PixelFormats.R8G8_TYPELESS or
        PixelFormats.R8G8_UNORM or
        PixelFormats.R8G8_UINT or
        PixelFormats.R8G8_SNORM or
        PixelFormats.R8G8_SINT or
        PixelFormats.R16_TYPELESS or
        PixelFormats.R16_FLOAT or
        PixelFormats.D16_UNORM or
        PixelFormats.R16_UNORM or
        PixelFormats.R16_UINT or
        PixelFormats.R16_SNORM or
        PixelFormats.R16_SINT or
        PixelFormats.B5G6R5_UNORM or
        PixelFormats.B5G5R5A1_UNORM or
        PixelFormats.B4G4R4A4_UNORM
            => 16,
        PixelFormats.R8_TYPELESS or
        PixelFormats.R8_UNORM or
        PixelFormats.R8_UINT or
        PixelFormats.R8_SNORM or
        PixelFormats.R8_SINT or
        PixelFormats.A8_UNORM
            => 8,
        PixelFormats.R1_UNORM => 1,
        PixelFormats.BC1_TYPELESS or
        PixelFormats.BC1_UNORM or
        PixelFormats.BC1_UNORM_SRGB or
        PixelFormats.BC4_TYPELESS or
        PixelFormats.BC4_UNORM or
        PixelFormats.BC4_SNORM
            => 4,
        PixelFormats.BC2_TYPELESS or
        PixelFormats.BC2_UNORM or
        PixelFormats.BC2_UNORM_SRGB or
        PixelFormats.BC3_TYPELESS or
        PixelFormats.BC3_UNORM or
        PixelFormats.BC3_UNORM_SRGB or
        PixelFormats.BC5_TYPELESS or
        PixelFormats.BC5_UNORM or
        PixelFormats.BC5_SNORM or
        PixelFormats.BC6H_TYPELESS or
        PixelFormats.BC6H_UF16 or
        PixelFormats.BC6H_SF16 or
        PixelFormats.BC7_TYPELESS or
        PixelFormats.BC7_UNORM or
        PixelFormats.BC7_UNORM_SRGB
            => 8,
        _ => 0,
    };
    public static void ComputePitch(PixelFormats fmt, int width, int height, out int rowPitch, out int slicePitch,
                              out int widthCount, out int heightCount, PitchFlags flags = PitchFlags.None)
    {
        widthCount = width;
        heightCount = height;

        if (IsCompressedFormat(fmt))
        {
            int bpb = (fmt is PixelFormats.BC1_TYPELESS
                           or PixelFormats.BC1_UNORM
                           or PixelFormats.BC1_UNORM_SRGB
                           or PixelFormats.BC4_TYPELESS
                           or PixelFormats.BC4_UNORM
                           or PixelFormats.BC4_SNORM)
                           ? 8 : 16;
            widthCount = Math.Max(1, (width + 3) / 4);
            heightCount = Math.Max(1, (height + 3) / 4);
            rowPitch = widthCount * bpb;
            slicePitch = rowPitch * heightCount;
        }
        else if (IsPacked(fmt))
        {
            rowPitch = ((width + 1) >> 1) * 4;
            slicePitch = rowPitch * height;
        }
        else
        {
            int bpp = (flags & PitchFlags.Bpp24) != 0
                ? 24
                : (flags & PitchFlags.Bpp16) != 0 ? 16 : (flags & PitchFlags.Bpp8) != 0 ? 8 : GetBitsPerPixel(fmt);

            if ((flags & PitchFlags.LegacyDword) != 0)
            {
                // Special computation for some incorrectly created DDS files based on
                // legacy DirectDraw assumptions about pitch alignment
                rowPitch = ((width * bpp) + 31) / 32 * sizeof(int);
                slicePitch = rowPitch * height;
            }
            else
            {
                rowPitch = ((width * bpp) + 7) / 8;
                slicePitch = rowPitch * height;
            }
        }
    }
    public static bool IsPacked(PixelFormats fmt) => fmt is PixelFormats.R8G8_B8G8_UNORM or PixelFormats.G8R8_G8B8_UNORM;
    public static bool IsCompressedFormat(PixelFormats format) => format switch
    {
        PixelFormats.BC1_TYPELESS or
        PixelFormats.BC1_UNORM or
        PixelFormats.BC1_UNORM_SRGB or
        PixelFormats.BC2_TYPELESS or
        PixelFormats.BC2_UNORM or
        PixelFormats.BC2_UNORM_SRGB or
        PixelFormats.BC3_TYPELESS or
        PixelFormats.BC3_UNORM or
        PixelFormats.BC3_UNORM_SRGB or
        PixelFormats.BC4_TYPELESS or
        PixelFormats.BC4_UNORM or
        PixelFormats.BC4_SNORM or
        PixelFormats.BC5_TYPELESS or
        PixelFormats.BC5_UNORM or
        PixelFormats.BC5_SNORM or
        PixelFormats.BC6H_TYPELESS or
        PixelFormats.BC6H_UF16 or
        PixelFormats.BC6H_SF16 or
        PixelFormats.BC7_TYPELESS or
        PixelFormats.BC7_UNORM or
        PixelFormats.BC7_UNORM_SRGB
            => true,
        _ => false,
    };
    public static PixelFormat PixelFormatFromDXGIFormat(PixelFormats format)
    {
        PixelFormat ddpf = default;

        switch (format)
        {
            case PixelFormats.R8G8B8A8_UNORM:
                ddpf = PixelFormat.A8B8G8R8;
                break;
            case PixelFormats.R16G16_UNORM:
                ddpf = PixelFormat.G16R16;
                break;
            case PixelFormats.R8G8_UNORM:
                ddpf = PixelFormat.A8L8;
                break;
            case PixelFormats.R16_UNORM:
                ddpf = PixelFormat.L16;
                break;
            case PixelFormats.R8_UNORM:
                ddpf = PixelFormat.L8;
                break;
            case PixelFormats.A8_UNORM:
                ddpf = PixelFormat.A8;
                break;
            case PixelFormats.R8G8_B8G8_UNORM:
                ddpf = PixelFormat.R8G8_B8G8;
                break;
            case PixelFormats.G8R8_G8B8_UNORM:
                ddpf = PixelFormat.G8R8_G8B8;
                break;
            case PixelFormats.BC1_UNORM:
            case PixelFormats.BC1_UNORM_SRGB:
                ddpf = PixelFormat.DXT1;
                break;
            case PixelFormats.BC2_UNORM:
            case PixelFormats.BC2_UNORM_SRGB:
                ddpf = PixelFormat.DXT3;
                break;
            case PixelFormats.BC3_UNORM:
            case PixelFormats.BC3_UNORM_SRGB:
                ddpf = PixelFormat.DXT5;
                break;
            case PixelFormats.BC4_UNORM:
                ddpf = PixelFormat.BC4_UNorm;
                break;
            case PixelFormats.BC4_SNORM:
                ddpf = PixelFormat.BC4_SNorm;
                break;
            case PixelFormats.BC5_UNORM:
                ddpf = PixelFormat.BC5_UNorm;
                break;
            case PixelFormats.BC5_SNORM:
                ddpf = PixelFormat.BC5_SNorm;
                break;
            case PixelFormats.B5G6R5_UNORM:
                ddpf = PixelFormat.R5G6B5;
                break;
            case PixelFormats.B5G5R5A1_UNORM:
                ddpf = PixelFormat.A1R5G5B5;
                break;
            case PixelFormats.B8G8R8A8_UNORM:
                ddpf = PixelFormat.A8R8G8B8;
                break; // DXGI 1.1
            case PixelFormats.B8G8R8X8_UNORM:
                ddpf = PixelFormat.X8R8G8B8;
                break; // DXGI 1.1

            // Legacy D3DX formats using D3DFMT enum value as FourCC
            case PixelFormats.R32G32B32A32_FLOAT:
                ddpf.Size = 32;
                ddpf.Flags = PixelFormatFlags.FourCC;
                ddpf.FourCC = 116; // D3DFMT_A32B32G32R32F
                break;
            case PixelFormats.R16G16B16A16_FLOAT:
                ddpf.Size = 32;
                ddpf.Flags = PixelFormatFlags.FourCC;
                ddpf.FourCC = 113; // D3DFMT_A16B16G16R16F
                break;
            case PixelFormats.R16G16B16A16_UNORM:
                ddpf.Size = 32;
                ddpf.Flags = PixelFormatFlags.FourCC;
                ddpf.FourCC = 36; // D3DFMT_A16B16G16R16
                break;
            case PixelFormats.R16G16B16A16_SNORM:
                ddpf.Size = 32;
                ddpf.Flags = PixelFormatFlags.FourCC;
                ddpf.FourCC = 110; // D3DFMT_Q16W16V16U16
                break;
            case PixelFormats.R32G32_FLOAT:
                ddpf.Size = 32;
                ddpf.Flags = PixelFormatFlags.FourCC;
                ddpf.FourCC = 115; // D3DFMT_G32R32F
                break;
            case PixelFormats.R16G16_FLOAT:
                ddpf.Size = 32;
                ddpf.Flags = PixelFormatFlags.FourCC;
                ddpf.FourCC = 112; // D3DFMT_G16R16F
                break;
            case PixelFormats.R32_FLOAT:
                ddpf.Size = 32;
                ddpf.Flags = PixelFormatFlags.FourCC;
                ddpf.FourCC = 114; // D3DFMT_R32F
                break;
            case PixelFormats.R16_FLOAT:
                ddpf.Size = 32;
                ddpf.Flags = PixelFormatFlags.FourCC;
                ddpf.FourCC = 111; // D3DFMT_R16F
                break;
        }
        return ddpf;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public int Size;
        public HeaderFlags Flags;
        public uint Height;
        public uint Width;
        public int PitchOrLinearSize;
        public int Depth; // only if DDS_HEADER_FLAGS_VOLUME is set in dwFlags
        public int MipMapCount;

        private readonly uint unused1;
        private readonly uint unused2;
        private readonly uint unused3;
        private readonly uint unused4;
        private readonly uint unused5;
        private readonly uint unused6;
        private readonly uint unused7;
        private readonly uint unused8;
        private readonly uint unused9;
        private readonly uint unused10;
        private readonly uint unused11;

        public PixelFormat PixelFormat;
        public SurfaceFlags SurfaceFlags;
        public CubemapFlags CubemapFlags;

        private readonly uint Unused12;
        private readonly uint Unused13;

        private readonly uint Unused14;
    }
    [Flags]
    public enum PitchFlags
    {
        /// <summary>
        /// Normal operation
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Assume pitch is DWORD aligned instead of BYTE aligned
        /// </summary>
        LegacyDword = 0x1,
        /// <summary>
        /// Override with a legacy 24 bits-per-pixel format size
        /// </summary>
        Bpp24 = 0x10000, // 
        /// <summary>
        /// Override with a legacy 16 bits-per-pixel format size
        /// </summary>
        Bpp16 = 0x20000,
        /// <summary>
        /// Override with a legacy 8 bits-per-pixel format size
        /// </summary>
        Bpp8 = 0x40000,
    };

    /// <summary>
    /// Internal structure used to describe a DDS pixel format.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PixelFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PixelFormat" /> struct.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <param name="fourCC">The four CC.</param>
        /// <param name="rgbBitCount">The RGB bit count.</param>
        /// <param name="rBitMask">The r bit mask.</param>
        /// <param name="gBitMask">The g bit mask.</param>
        /// <param name="bBitMask">The b bit mask.</param>
        /// <param name="aBitMask">A bit mask.</param>
        public PixelFormat(PixelFormatFlags flags, int fourCC, int rgbBitCount, uint rBitMask, uint gBitMask,
                           uint bBitMask, uint aBitMask)
        {
            Size = 32;
            Flags = flags;
            FourCC = fourCC;
            RGBBitCount = rgbBitCount;
            RBitMask = rBitMask;
            GBitMask = gBitMask;
            BBitMask = bBitMask;
            ABitMask = aBitMask;
        }

        public int Size;
        public PixelFormatFlags Flags;
        public int FourCC;
        public int RGBBitCount;
        public uint RBitMask;
        public uint GBitMask;
        public uint BBitMask;
        public uint ABitMask;

        public static readonly PixelFormat DXT1 = new(PixelFormatFlags.FourCC,
                                                      new FourCC('D', 'X', 'T', '1'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat DXT2 = new(PixelFormatFlags.FourCC,
                                                      new FourCC('D', 'X', 'T', '2'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat DXT3 = new(PixelFormatFlags.FourCC,
                                                                  new FourCC('D', 'X', 'T', '3'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat DXT4 = new(PixelFormatFlags.FourCC,
                                                                  new FourCC('D', 'X', 'T', '4'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat DXT5 = new(PixelFormatFlags.FourCC,
                                                                  new FourCC('D', 'X', 'T', '5'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat BC4_UNorm = new(PixelFormatFlags.FourCC,
                                                                       new FourCC('B', 'C', '4', 'U'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat BC4_SNorm = new(PixelFormatFlags.FourCC,
                                                                       new FourCC('B', 'C', '4', 'S'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat BC5_UNorm = new(PixelFormatFlags.FourCC,
                                                                       new FourCC('B', 'C', '5', 'U'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat BC5_SNorm = new(PixelFormatFlags.FourCC,
                                                                       new FourCC('B', 'C', '5', 'S'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat R8G8_B8G8 = new(PixelFormatFlags.FourCC,
                                                                       new FourCC('R', 'G', 'B', 'G'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat G8R8_G8B8 = new(PixelFormatFlags.FourCC,
                                                                       new FourCC('G', 'R', 'G', 'B'), 0, 0, 0, 0, 0);

        public static readonly PixelFormat A8R8G8B8 = new(PixelFormatFlags.Rgba, 0, 32, 0x00ff0000,
                                                                      0x0000ff00, 0x000000ff, 0xff000000);

        public static readonly PixelFormat X8R8G8B8 = new(PixelFormatFlags.Rgb, 0, 32, 0x00ff0000,
                                                                      0x0000ff00, 0x000000ff, 0x00000000);

        public static readonly PixelFormat A8B8G8R8 = new(PixelFormatFlags.Rgba, 0, 32, 0x000000ff,
                                                                      0x0000ff00, 0x00ff0000, 0xff000000);

        public static readonly PixelFormat X8B8G8R8 = new(PixelFormatFlags.Rgb, 0, 32, 0x000000ff,
                                                                      0x0000ff00, 0x00ff0000, 0x00000000);

        public static readonly PixelFormat G16R16 = new(PixelFormatFlags.Rgb, 0, 32, 0x0000ffff,
                                                                    0xffff0000, 0x00000000, 0x00000000);

        public static readonly PixelFormat R5G6B5 = new(PixelFormatFlags.Rgb, 0, 16, 0x0000f800,
                                                                    0x000007e0, 0x0000001f, 0x00000000);

        public static readonly PixelFormat A1R5G5B5 = new(PixelFormatFlags.Rgba, 0, 16, 0x00007c00,
                                                                      0x000003e0, 0x0000001f, 0x00008000);

        public static readonly PixelFormat A4R4G4B4 = new(PixelFormatFlags.Rgba, 0, 16, 0x00000f00,
                                                                      0x000000f0, 0x0000000f, 0x0000f000);

        public static readonly PixelFormat R8G8B8 = new(PixelFormatFlags.Rgb, 0, 24, 0x00ff0000,
                                                                    0x0000ff00, 0x000000ff, 0x00000000);

        public static readonly PixelFormat L8 = new(PixelFormatFlags.Luminance, 0, 8, 0xff, 0x00, 0x00,
                                                                0x00);

        public static readonly PixelFormat L16 = new(PixelFormatFlags.Luminance, 0, 16, 0xffff, 0x0000,
                                                                 0x0000, 0x0000);

        public static readonly PixelFormat A8L8 = new(PixelFormatFlags.LuminanceAlpha, 0, 16, 0x00ff,
                                                                  0x0000, 0x0000, 0xff00);

        public static readonly PixelFormat A8 = new(PixelFormatFlags.Alpha, 0, 8, 0x00, 0x00, 0x00, 0xff);
    }
}