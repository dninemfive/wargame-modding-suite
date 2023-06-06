using moddingSuite.BL.Ndf;
using moddingSuite.Model.Edata;
using moddingSuite.Model.Mesh;
using moddingSuite.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace moddingSuite.BL.Mesh;

public class MeshReader
{
    public const uint MeshMagic = 1213416781; // "MESH"
    public const uint ProxyMagic = 1498960464; // "PRXY"

    public MeshFile Read(byte[] data)
    {
        using MemoryStream ms = new(data);
        return Read(ms);
    }

    public MeshFile Read(Stream s)
    {
        MeshFile file = new()
        {
            Header = ReadHeader(s),
            SubHeader = ReadSubHeader(s)
        };

        file.MultiMaterialMeshFiles = ReadMeshDictionary(s, file);

        file.TextureBindings = ReadTextureBindings(s, file);

        return file;
    }

    private Model.Ndfbin.NdfBinary ReadTextureBindings(Stream s, MeshFile file)
    {
        byte[] buffer = new byte[file.SubHeader.MeshMaterial.Size];

        _ = s.Seek(file.SubHeader.MeshMaterial.Offset, SeekOrigin.Begin);
        _ = s.Read(buffer, 0, buffer.Length);

        NdfbinReader ndfReader = new();

        return ndfReader.Read(buffer);
    }

    protected MeshSubHeader ReadSubHeader(Stream ms)
    {
        MeshSubHeader shead = new();

        byte[] buffer = new byte[4];

        _ = ms.Read(buffer, 0, buffer.Length);
        shead.MeshCount = BitConverter.ToUInt32(buffer, 0);

        shead.Dictionary = ReadSubHeaderEntryWithCount(ms);
        shead.VertexTypeNames = ReadSubHeaderEntryWithCount(ms);
        shead.MeshMaterial = ReadSubHeaderEntryWithCount(ms);

        shead.KeyedMeshSubPart = ReadSubHeaderEntryWithCount(ms);
        shead.KeyedMeshSubPartVectors = ReadSubHeaderEntryWithCount(ms);
        shead.MultiMaterialMeshes = ReadSubHeaderEntryWithCount(ms);
        shead.SingleMaterialMeshes = ReadSubHeaderEntryWithCount(ms);
        shead.Index1DBufferHeaders = ReadSubHeaderEntryWithCount(ms);
        shead.Index1DBufferStreams = ReadSubHeaderEntry(ms);
        shead.Vertex1DBufferHeaders = ReadSubHeaderEntryWithCount(ms);
        shead.Vertex1DBufferStreams = ReadSubHeaderEntry(ms);

        return shead;
    }

    protected MeshHeaderEntry ReadSubHeaderEntry(Stream s)
    {
        MeshHeaderEntry entry = new();

        byte[] buffer = new byte[4];

        _ = s.Read(buffer, 0, buffer.Length);
        entry.Offset = BitConverter.ToUInt32(buffer, 0);

        _ = s.Read(buffer, 0, buffer.Length);
        entry.Size = BitConverter.ToUInt32(buffer, 0);

        return entry;
    }

    protected MeshHeaderEntryWithCount ReadSubHeaderEntryWithCount(Stream s)
    {
        MeshHeaderEntry entry = ReadSubHeaderEntry(s);

        MeshHeaderEntryWithCount entryWithCount = new()
        {
            Offset = entry.Offset,
            Size = entry.Size
        };

        byte[] buffer = new byte[4];
        _ = s.Read(buffer, 0, buffer.Length);

        entryWithCount.Count = BitConverter.ToUInt32(buffer, 0);

        return entryWithCount;
    }

    protected MeshHeader ReadHeader(Stream ms)
    {
        MeshHeader head = new();

        byte[] buffer = new byte[4];

        _ = ms.Read(buffer, 0, buffer.Length);

        if (BitConverter.ToUInt32(buffer, 0) != MeshMagic)
            throw new InvalidDataException("Wrong header magic");

        _ = ms.Read(buffer, 0, buffer.Length);
        head.Platform = BitConverter.ToUInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        head.Version = BitConverter.ToUInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        head.FileSize = BitConverter.ToUInt32(buffer, 0);

        byte[] chkSumBuffer = new byte[16];

        _ = ms.Read(chkSumBuffer, 0, chkSumBuffer.Length);
        head.Checksum = chkSumBuffer;

        _ = ms.Read(buffer, 0, buffer.Length);
        head.HeaderOffset = BitConverter.ToUInt32(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        head.HeaderSize = BitConverter.ToUInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        head.ContentOffset = BitConverter.ToUInt32(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        head.ContentSize = BitConverter.ToUInt32(buffer, 0);

        return head;
    }

    protected ObservableCollection<MeshContentFile> ReadMeshDictionary(Stream s, MeshFile f)
    {
        ObservableCollection<MeshContentFile> files = new();
        List<EdataDir> dirs = new();
        List<long> endings = new();

        _ = s.Seek(f.SubHeader.Dictionary.Offset, SeekOrigin.Begin);

        long dirEnd = f.SubHeader.Dictionary.Offset + f.SubHeader.Dictionary.Size;

        while (s.Position < dirEnd)
        {
            byte[] buffer = new byte[4];
            _ = s.Read(buffer, 0, 4);
            int fileGroupId = BitConverter.ToInt32(buffer, 0);

            if (fileGroupId == 0)
            {
                MeshContentFile file = new();
                _ = s.Read(buffer, 0, 4);
                file.FileEntrySize = BitConverter.ToUInt32(buffer, 0);

                Point3D minp = new();
                _ = s.Read(buffer, 0, buffer.Length);
                minp.X = BitConverter.ToSingle(buffer, 0);
                _ = s.Read(buffer, 0, buffer.Length);
                minp.Y = BitConverter.ToSingle(buffer, 0);
                _ = s.Read(buffer, 0, buffer.Length);
                minp.Z = BitConverter.ToSingle(buffer, 0);
                file.MinBoundingBox = minp;

                Point3D maxp = new();
                _ = s.Read(buffer, 0, buffer.Length);
                maxp.X = BitConverter.ToSingle(buffer, 0);
                _ = s.Read(buffer, 0, buffer.Length);
                maxp.Y = BitConverter.ToSingle(buffer, 0);
                _ = s.Read(buffer, 0, buffer.Length);
                maxp.Z = BitConverter.ToSingle(buffer, 0);
                file.MaxBoundingBox = maxp;

                _ = s.Read(buffer, 0, buffer.Length);
                file.Flags = BitConverter.ToUInt32(buffer, 0);

                buffer = new byte[2];

                _ = s.Read(buffer, 0, buffer.Length);
                file.MultiMaterialMeshIndex = BitConverter.ToUInt16(buffer, 0);

                _ = s.Read(buffer, 0, buffer.Length);
                file.HierarchicalAseModelSkeletonIndex = BitConverter.ToUInt16(buffer, 0);

                file.Name = Utils.ReadString(s);
                file.Path = MergePath(dirs, file.Name);

                if (file.Name.Length % 2 == 0)
                    _ = s.Seek(1, SeekOrigin.Current);

                files.Add(file);

                while (endings.Count > 0 && s.Position == endings.Last())
                {
                    _ = dirs.Remove(dirs.Last());
                    _ = endings.Remove(endings.Last());
                }
            }
            else if (fileGroupId > 0)
            {
                EdataDir dir = new();

                _ = s.Read(buffer, 0, 4);
                dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                if (dir.FileEntrySize != 0)
                    endings.Add(dir.FileEntrySize + s.Position - 8);
                else if (endings.Count > 0)
                    endings.Add(endings.Last());

                dir.Name = Utils.ReadString(s);

                if (dir.Name.Length % 2 == 0)
                    _ = s.Seek(1, SeekOrigin.Current);

                dirs.Add(dir);
            }
        }

        return files;
    }

    protected string MergePath(IEnumerable<EdataDir> dirs, string fileName)
    {
        StringBuilder b = new();

        foreach (EdataDir dir in dirs)
            _ = b.Append(dir.Name);

        _ = b.Append(fileName);

        return b.ToString();
    }
}
