using moddingSuite.BL.Ndf;
using moddingSuite.Model.Common;
using moddingSuite.Model.Scenario;
using moddingSuite.Model.Settings;
using moddingSuite.Util;
using System;
using System.IO;
using System.Text;
using System.Windows.Media.Media3D;

namespace moddingSuite.BL.Scenario;

public class ScenarioReader
{
    public ScenarioFile Read(byte[] data)
    {
        using MemoryStream ms = new(data);
        return Read(ms);
    }

    public ScenarioFile Read(Stream s)
    {
        ScenarioFile f = new();

        byte[] buffer = new byte[10];
        _ = s.Read(buffer, 0, buffer.Length);

        if (!Utils.ByteArrayCompare(Encoding.ASCII.GetBytes("SCENARIO\r\n"), buffer))
            throw new InvalidDataException("Wrong scenario header magic!");

        buffer = new byte[16];
        _ = s.Read(buffer, 0, buffer.Length);
        f.Checksum = buffer;

        _ = s.Seek(2, SeekOrigin.Current);

        buffer = new byte[4];
        _ = s.Read(buffer, 0, buffer.Length);
        f.Version = BitConverter.ToInt32(buffer, 0);

        _ = s.Read(buffer, 0, buffer.Length);
        int subFilesCount = BitConverter.ToInt32(buffer, 0);

        for (int i = 0; i < subFilesCount; i++)
        {
            f.lastPartStartByte = s.Position;
            _ = s.Read(buffer, 0, buffer.Length);
            byte[] contentFileBuffer = new byte[BitConverter.ToUInt32(buffer, 0)];
            _ = s.Read(contentFileBuffer, 0, contentFileBuffer.Length);
            f.ContentFiles.Add(contentFileBuffer);
        }

        NdfbinReader reader = new();
        f.NdfBinary = reader.Read(f.ContentFiles[1]);

        f.ZoneData = ReadZoneData(f.ContentFiles[0]);
        uncompressedPrintToFile(f.ContentFiles[2], "thirdPart");
        return f;
    }

    public AreaFile ReadZoneData(byte[] data)
    {
        AreaFile areaFile = new();

        using (MemoryStream ms = new(data))
        {
            byte[] buffer = new byte[4];

            ms.AssertAreaMagic();

            _ = ms.Read(buffer, 0, buffer.Length);
            int version = BitConverter.ToInt32(buffer, 0);
            if (version != 0)
                throw new InvalidDataException("Not supported version of area format!");

            _ = ms.Seek(4, SeekOrigin.Current);
            //ms.Read(buffer, 0, buffer.Length);
            //uint dataLen = BitConverter.ToUInt32(buffer, 0);

            _ = ms.Read(buffer, 0, buffer.Length);
            int layerCount = BitConverter.ToInt32(buffer, 0);

            for (int lc = 0; lc < layerCount; lc++)
            {
                AreaColletion areaList = new();

                _ = ms.Read(buffer, 0, buffer.Length);
                int areasToRead = BitConverter.ToInt32(buffer, 0);

                for (int a = 0; a < areasToRead; a++)
                    areaList.Add(ReadArea(ms));

                areaFile.AreaManagers.Add(areaList);
            }
        }

        return areaFile;
    }

    protected Area ReadArea(Stream ms)
    {
        Area currentZone = new()
        {Content = new AreaContent()};
        byte[] buffer = new byte[4];

        ms.AssertAreaMagic();

        _ = ms.Read(buffer, 0, buffer.Length);
        int zoneDataVersion = BitConverter.ToInt32(buffer, 0);
        if (zoneDataVersion != 2)
            throw new InvalidDataException("Zone data version != 2 not supported!");

        _ = ms.Read(buffer, 0, buffer.Length);
        currentZone.Id = BitConverter.ToInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        int idStrLen = BitConverter.ToInt32(buffer, 0);
        byte[] idStrBuffer = new byte[Utils.RoundToNextDivBy4(idStrLen)];
        _ = ms.Read(idStrBuffer, 0, idStrBuffer.Length);
        currentZone.Name = Encoding.UTF8.GetString(idStrBuffer).TrimEnd('\0');

        ms.AssertAreaMagic();

        Point3D attachmentPt = new();
        _ = ms.Read(buffer, 0, buffer.Length);
        attachmentPt.X = BitConverter.ToSingle(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        attachmentPt.Y = BitConverter.ToSingle(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        attachmentPt.Z = BitConverter.ToSingle(buffer, 0);
        currentZone.AttachmentPoint = attachmentPt;

        ms.AssertAreaMagic();

        _ = ms.Read(buffer, 0, buffer.Length);
        int subParts = BitConverter.ToInt32(buffer, 0);

        for (int sp = 0; sp < subParts; sp++)
        {
            AreaClipped aced = new();
            _ = ms.Read(buffer, 0, buffer.Length);
            aced.StartTriangle = BitConverter.ToInt32(buffer, 0);
            _ = ms.Read(buffer, 0, buffer.Length);
            aced.TriangleCount = BitConverter.ToInt32(buffer, 0);
            _ = ms.Read(buffer, 0, buffer.Length);
            aced.StartVertex = BitConverter.ToInt32(buffer, 0);
            _ = ms.Read(buffer, 0, buffer.Length);
            aced.VertexCount = BitConverter.ToInt32(buffer, 0);
            currentZone.Content.ClippedAreas.Add(aced);
        }

        ms.AssertAreaMagic();

        _ = ms.Read(buffer, 0, buffer.Length);
        currentZone.Content.BorderTriangle.StartTriangle = BitConverter.ToInt32(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        currentZone.Content.BorderTriangle.TriangleCount = BitConverter.ToInt32(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        currentZone.Content.BorderTriangle.StartVertex = BitConverter.ToInt32(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        currentZone.Content.BorderTriangle.VertexCount = BitConverter.ToInt32(buffer, 0);

        ms.AssertAreaMagic();

        _ = ms.Read(buffer, 0, buffer.Length);
        currentZone.Content.BorderVertex.StartVertex = BitConverter.ToInt32(buffer, 0);
        _ = ms.Read(buffer, 0, buffer.Length);
        currentZone.Content.BorderVertex.VertexCount = BitConverter.ToInt32(buffer, 0);

        ms.AssertAreaMagic();

        _ = ms.Read(buffer, 0, buffer.Length);
        int vertexCount = BitConverter.ToInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        int trianglesCount = BitConverter.ToInt32(buffer, 0);

        for (int v = 0; v < vertexCount; v++)
        {
            AreaVertex curVertex = new();

            _ = ms.Read(buffer, 0, buffer.Length);
            curVertex.X = BitConverter.ToSingle(buffer, 0);

            _ = ms.Read(buffer, 0, buffer.Length);
            curVertex.Y = BitConverter.ToSingle(buffer, 0);

            _ = ms.Read(buffer, 0, buffer.Length);
            curVertex.Z = BitConverter.ToSingle(buffer, 0);

            _ = ms.Read(buffer, 0, buffer.Length);
            curVertex.W = BitConverter.ToSingle(buffer, 0);

            _ = ms.Read(buffer, 0, buffer.Length);
            curVertex.Center = BitConverter.ToSingle(buffer, 0);

            currentZone.Content.Vertices.Add(curVertex);
        }

        ms.AssertAreaMagic();

        for (int f = 0; f < trianglesCount; f++)
        {
            MeshTriangularFace currentTriangle = new();

            _ = ms.Read(buffer, 0, buffer.Length);
            currentTriangle.Point1 = BitConverter.ToInt32(buffer, 0);

            _ = ms.Read(buffer, 0, buffer.Length);
            currentTriangle.Point2 = BitConverter.ToInt32(buffer, 0);

            _ = ms.Read(buffer, 0, buffer.Length);
            currentTriangle.Point3 = BitConverter.ToInt32(buffer, 0);

            currentZone.Content.Triangles.Add(currentTriangle);
        }

        ms.AssertAreaMagic();
        _ = ms.Seek(4, SeekOrigin.Current);

        ms.AssertAreaMagic();

        _ = ms.Read(buffer, 0, buffer.Length);
        return BitConverter.ToUInt32(buffer, 0) != 809782853 ? throw new InvalidDataException("END0 expected!") : currentZone;
    }
    private void uncompressedPrintToFile(byte[] buffer, string name, StreamWriter logFile = null)
    {
        Settings settings = SettingsManager.Load();
        using FileStream fs = new(Path.Combine(settings.SavePath, name), FileMode.OpenOrCreate);
        //var buffer = new byte[length];
        //var start = ms.Position;
        //ms.Read(buffer, 0, length);
        //var end = ms.Position;
        fs.Write(buffer, 0, buffer.Length);
        fs.Flush();

        //if (logFile != null) logFile.WriteLine("{0}: {1}/{2}/{3}", name, start, end, length);
    }
}
