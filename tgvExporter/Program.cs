using moddingSuite.BL.TGV;
using moddingSuite.Model.Textures;
using System;
using System.IO;

namespace tgvExporter;

internal class Program
{
    private static void Main(string[] args)
    {
        //var inpath = @"C:\Users\Anders\wargameexport\port_Wonsan";
        //var inFile = Path.Combine(inpath, "lowdef.tmst_chunk_pc");
        if (args == null || args.Length == 0)
            return;
        string inFile = args[0];
        string inpath = Directory.GetCurrentDirectory();
        TgvReader tgvReader = new();

        _ = new
        FileInfo(inFile);

        TgvFile tgv;

        using FileStream fs = new(inFile, FileMode.Open);
        TgvDDSWriter writer = new();

        int index = 1;

        const uint fatMagic = 810828102;
        Console.WriteLine("start");

        while (fs.Position + 4 < fs.Length)
        {
            _ = fs.Seek(4, SeekOrigin.Current);

            byte[] buffer = new byte[4];
            _ = fs.Read(buffer, 0, buffer.Length);

            if (BitConverter.ToUInt32(buffer, 0) != fatMagic)
                break;

            // Always 1
            _ = fs.Read(buffer, 0, buffer.Length);
            uint int1 = BitConverter.ToUInt32(buffer, 0);

            // Always 16
            _ = fs.Read(buffer, 0, buffer.Length);
            uint int2 = BitConverter.ToUInt32(buffer, 0);

            //Console.WriteLine("{0} - {1}", int1, int2);

            //fs.Seek(8, SeekOrigin.Current);

            _ = fs.Read(buffer, 0, buffer.Length);
            uint blockSize = BitConverter.ToUInt32(buffer, 0);

            if (fs.Position >= fs.Length)
                continue;

            byte[] tileBuffer = new byte[blockSize];

            _ = fs.Read(tileBuffer, 0, tileBuffer.Length);

            tgv = tgvReader.Read(tileBuffer);
            Console.WriteLine(index);
            byte[] content = writer.CreateDDSFile(tgv);

            FileInfo f = new(inFile);

            string path = Path.Combine(inpath, string.Format("{0}_{1}", f.Name, "export"));

            if (!Directory.Exists(path))
                _ = Directory.CreateDirectory(path);
            //if (index%21==3)
            using (FileStream outFs = new(Path.Combine(path, string.Format("{0}.dds", index)), FileMode.OpenOrCreate))
            {
                outFs.Write(content, 0, content.Length);
                outFs.Flush();
            }

            index++;
        }
    }
}
