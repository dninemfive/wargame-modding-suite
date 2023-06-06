using moddingSuite.BL;
using moddingSuite.BL.Compressing;
using moddingSuite.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfBlob : NdfFlatValueWrapper
{
    public ICommand ExportRawCommand { get; set; }
    public ICommand ExportKdTreeCommand { get; set; }
    public NdfBlob(byte[] value)
        : base(NdfType.Blob, value)
    {
        ExportRawCommand = new ActionCommand(ExportRawExecute);
        ExportKdTreeCommand = new ActionCommand(ExportKdTreeExecute);
    }
    private readonly List<byte[]> chunks = new();
    private readonly uint mysteryNumber;
    private readonly byte[] mysteryChunk;
    private const uint mysteryChunkLenth = 70;
    private void ExportKdTreeExecute(object obj)
    {
        Settings.Settings settings = SettingsManager.Load();
        StreamWriter logFile=new(new FileStream(Path.Combine(settings.SavePath, "exportLogFile.txt"), FileMode.OpenOrCreate));
        logFile.WriteLine("kdf values:");
        foreach (NdfPropertyValue propValue in ParentProperty.Instance.PropertyValues)
        {
            logFile.WriteLine($"{propValue.Property.Name}:\t{propValue.Value}");
        }

        byte[] data = GetBytes();
        logFile.WriteLine("blob export data, start byte/end byte/uncompressed length:");
        using (MemoryStream ms = new(data))
        {

            byte[] buffer = new byte[4];
            _ = ms.Read(buffer, 0, buffer.Length);
            uint fileLength = BitConverter.ToUInt32(buffer, 0);
            readVertexes(ms);
            printToFile(ms, "VertexNormaleBuffer", logFile);
            uncompressedPrintToFile(ms, "3rdPartUnComp", 8, logFile);
            printToFile(ms, "IndexBuffer", logFile);
            uncompressedPrintToFile(ms, "5thPartUnComp", 4, logFile);
            printToFile(ms, "TriangleIndexBuffer", logFile);
            long off = ms.Position % 8;
            if (off == 0)
                off = 8;
            uncompressedPrintToFile(ms, "7thPartUncomp", (int)(72 - off), logFile);
            printToFile(ms, "Subtrees", logFile);

        }
        logFile.Flush();
    }
    private void uncompressedPrintToFile(MemoryStream ms, string name, int length, StreamWriter logFile = null)
    {
        Settings.Settings settings = SettingsManager.Load();
        using FileStream fs = new(Path.Combine(settings.SavePath, name), FileMode.OpenOrCreate);
        byte[] buffer = new byte[length];
        long start = ms.Position;
        _ = ms.Read(buffer, 0, length);
        long end = ms.Position;
        fs.Write(buffer, 0, length);
        fs.Flush();
        logFile?.WriteLine("{0}: {1}/{2}/{3}", name, start, end, length);
    }
    private void printToFile(MemoryStream ms, string name, StreamWriter logFile = null)
    {
        //Console.WriteLine(ms.Position);
        Settings.Settings settings = SettingsManager.Load();
        byte[] buffer = new byte[4];
        long start = ms.Position;
        _ = ms.Read(buffer, 0, buffer.Length);
        uint blockLenComp = BitConverter.ToUInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);

        byte[] subBlockBuffer = new byte[blockLenComp - 4];

        _ = ms.Read(subBlockBuffer, 0, subBlockBuffer.Length);
        long end = ms.Position;
        _ = ms.Seek(0, SeekOrigin.Current);
        byte[] subBlockData = Compressor.Decomp(subBlockBuffer);
        int uncompLength = subBlockData.Length;
        using (FileStream fs = new(Path.Combine(settings.SavePath, name), FileMode.OpenOrCreate))
        {
            fs.Write(subBlockData, 0, subBlockData.Length);
            fs.Flush();
        }
        logFile?.WriteLine($"{name}: {start}/{end}/{uncompLength}");
    }
    private void ExportRawExecute(object obj)
    {
        Settings.Settings settings = SettingsManager.Load();

        byte[] buffer = GetBytes();

        using FileStream fs = new(Path.Combine(settings.SavePath, string.Format("blobdump_{0}", DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ff"))), FileMode.OpenOrCreate);
        fs.Write(buffer, 0, buffer.Length);
        fs.Flush();
    }

    public override byte[] GetBytes()
    {
        List<byte> val = new();
        val.AddRange(BitConverter.GetBytes((uint)((byte[])Value).Length));
        val.AddRange((byte[])Value);

        return val.ToArray();
    }

    public override byte[] GetNdfText() => throw new NotImplementedException();
    /*private float getHalfFloat(byte[] bytes){
        
    }*/
    public void readVertexes(MemoryStream ms)
    {
        Settings.Settings settings = SettingsManager.Load();
        byte[] buffer = new byte[4];

        _ = ms.Position;
        _ = ms.Read(buffer, 0, buffer.Length);
        uint blockLenComp = BitConverter.ToUInt32(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);

        _ = BitConverter.ToUInt32(buffer, 0);

        byte[] subBlockBuffer = new byte[blockLenComp - 4];

        _ = ms.Read(subBlockBuffer, 0, subBlockBuffer.Length);

        _ = ms.Position;
        _ = ms.Seek(0, SeekOrigin.Current);
        byte[] subBlockData = Compressor.Decomp(subBlockBuffer);
        MemoryStream bs = new(subBlockData);
        uncompressedPrintToFile(bs, "vertexBinary", (int)bs.Length);
        StreamWriter vertexFile = new(new FileStream(Path.Combine(settings.SavePath, "vertexes.csv"), FileMode.Create));
        _ = bs.Read(buffer, 0, buffer.Length);
        int nbVertexes = BitConverter.ToInt32(buffer, 0);
        _ = bs.Read(buffer, 0, buffer.Length);

        _ = BitConverter.ToInt32(buffer, 0);
        uint xMask = 0xFFFC0000;
        uint yMask = 0x0003FFF0;
        Console.WriteLine("M=[");
        for (int i = 0; i < nbVertexes; i++)
        {
            _ = bs.Read(buffer, 0, buffer.Length);
            Array.Reverse(buffer);
            uint coord = BitConverter.ToUInt32(buffer, 0);
            _ = bs.Seek(2, SeekOrigin.Current);
            uint x = (coord & xMask) >> 18;
            uint y = (coord & yMask) >> 4;
            Console.WriteLine("{0}\t{1};", x, y);
            vertexFile.WriteLine("{0}\t{1}", x, y);

        }
        Console.WriteLine("]");
    }
}
