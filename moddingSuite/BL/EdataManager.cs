using moddingSuite.Model.Edata;
using moddingSuite.Util;
using moddingSuite.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace moddingSuite.BL;

/// <summary>
/// Thanks to Giovanni Condello. He created the "WargameEE DAT unpacker" which is the base for my EdataManager.
/// TODO: implement virtual packages.
/// </summary>
public class EdataManager : ViewModelBase
{
    public static readonly byte[] EdataMagic = { 0x65, 0x64, 0x61, 0x74 };

    /// <summary>
    /// Creates a new Instance of a EdataManager.
    /// </summary>
    /// <param name="filePath">The package file which is to be managed.</param>
    public EdataManager(string filePath)
    {
        FilePath = filePath;
    }

    static EdataManager()
    {
        byte[] edataHeader = EdataMagic;
        byte[] ndfbinheader = { 0x45, 0x55, 0x47, 0x30, 0x00, 0x00, 0x00, 0x00, 0x43, 0x4E, 0x44, 0x46 };
        byte[] tradHeader = { 0x54, 0x52, 0x41, 0x44 };
        byte[] savHeader = { 0x53, 0x41, 0x56, 0x30, 0x00, 0x00, 0x00, 0x00 };
        byte[] tgvHeader = { 2 };
        byte[] meshHeader = { 0x4D, 0x45, 0x53, 0x48 };
        byte[] scenarioHeader = {0x53, 0x43, 0x45, 0x4E, 0x41, 0x52, 0x49, 0x4F, 0x0D, 0x0A};

        _knownHeaders = new List<KeyValuePair<EdataFileType, byte[]>>
            {
                new KeyValuePair<EdataFileType, byte[]>(EdataFileType.Ndfbin, ndfbinheader),
                new KeyValuePair<EdataFileType, byte[]>(EdataFileType.Package, edataHeader),
                new KeyValuePair<EdataFileType, byte[]>(EdataFileType.Dictionary, tradHeader),
                new KeyValuePair<EdataFileType, byte[]>(EdataFileType.Save, savHeader),
                new KeyValuePair<EdataFileType, byte[]>(EdataFileType.Image, tgvHeader),
                new KeyValuePair<EdataFileType, byte[]>(EdataFileType.Mesh, meshHeader),
                new KeyValuePair<EdataFileType, byte[]>(EdataFileType.Scenario, scenarioHeader),
            };
    }

    /// <summary>
    /// The current packages path on the hdd.
    /// </summary>
    public string FilePath { get; protected set; }

    /// <summary>
    /// Header information of the current package.
    /// </summary>
    public EdataHeader Header { get; protected set; }

    /// <summary>
    /// The Files the current package holds.
    /// </summary>
    public ObservableCollection<EdataContentFile> Files { get; protected set; }

    /// <summary>
    /// Reads the raw data of a file inside the current package.
    /// </summary>
    /// <param name="ofFile">A EdataFile of the current manager</param>
    /// <returns>The data of the desired EdataFile.</returns>
    public virtual byte[] GetRawData(EdataContentFile ofFile)
    {
        //if (ofFile.Manager != this)
        //    throw new ArgumentException("ofFile must be created by this instance of EdataManager");

        byte[] buffer;

        using (FileStream fs = File.Open(FilePath, FileMode.Open))
        {
            long offset = Header.FileOffset + ofFile.Offset;
            _ = fs.Seek(offset, SeekOrigin.Begin);

            buffer = new byte[ofFile.Size];
            _ = fs.Read(buffer, 0, buffer.Length);
        }

        return buffer;
    }

    /// <summary>
    /// Initiates the parsing of the current Edata file.
    /// </summary>
    public void ParseEdataFile()
    {
        Header = ReadEdataHeader();

        Files = Header.Version switch
        {
            1 => ReadEdatV1Dictionary(),
            2 => ReadEdatV2Dictionary(),
            _ => throw new NotSupportedException(string.Format("Edata Version {0} is currently not supported", Header.Version)),
        };
    }

    /// <summary>
    /// Reads the header of the current package.
    /// </summary>
    /// <returns>A instance of the current header.</returns>
    protected EdataHeader ReadEdataHeader()
    {
        EdataHeader header;

        byte[] buffer = new byte[Marshal.SizeOf(typeof(EdataHeader))];

        using (FileStream fs = File.Open(FilePath, FileMode.Open))
            _ = fs.Read(buffer, 0, buffer.Length);

        header = Utils.ByteArrayToStructure<EdataHeader>(buffer);

        return header.Version > 2
            ? throw new NotSupportedException(string.Format("Edata version {0} not supported", header.Version))
            : header;
    }

    /// <summary>
    /// The only tricky part about that algorythm is that you have to skip one byte if the length of the File/Dir name PLUS nullbyte is an odd number.
    /// </summary>
    /// <returns>A Collection of the Files found in the Dictionary</returns>
    protected ObservableCollection<EdataContentFile> ReadEdatV2Dictionary()
    {
        ObservableCollection<EdataContentFile> files = new();
        List<EdataDir> dirs = new();
        List<long> endings = new();

        using (FileStream fileStream = File.Open(FilePath, FileMode.Open))
        {
            _ = fileStream.Seek(Header.DictOffset, SeekOrigin.Begin);

            long dirEnd = Header.DictOffset + Header.DictLength;
            uint id = 0;

            while (fileStream.Position < dirEnd)
            {
                byte[] buffer = new byte[4];
                _ = fileStream.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    EdataContentFile file = new();
                    _ = fileStream.Read(buffer, 0, 4);
                    file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    buffer = new byte[8];
                    _ = fileStream.Read(buffer, 0, buffer.Length);
                    file.Offset = BitConverter.ToInt64(buffer, 0);

                    _ = fileStream.Read(buffer, 0, buffer.Length);
                    file.Size = BitConverter.ToInt64(buffer, 0);

                    byte[] checkSum = new byte[16];
                    _ = fileStream.Read(checkSum, 0, checkSum.Length);
                    file.Checksum = checkSum;

                    file.Name = Utils.ReadString(fileStream);
                    file.Path = MergePath(dirs, file.Name);

                    if (file.Name.Length % 2 == 0)
                        _ = fileStream.Seek(1, SeekOrigin.Current);

                    file.Id = id;
                    id++;

                    ResolveFileType(fileStream, file);

                    files.Add(file);

                    while (endings.Count > 0 && fileStream.Position == endings.Last())
                    {
                        _ = dirs.Remove(dirs.Last());
                        _ = endings.Remove(endings.Last());
                    }
                }
                else if (fileGroupId > 0)
                {
                    EdataDir dir = new();

                    _ = fileStream.Read(buffer, 0, 4);
                    dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    if (dir.FileEntrySize != 0)
                        endings.Add(dir.FileEntrySize + fileStream.Position - 8);
                    else if (endings.Count > 0)
                        endings.Add(endings.Last());

                    dir.Name = Utils.ReadString(fileStream);

                    if (dir.Name.Length % 2 == 0)
                        _ = fileStream.Seek(1, SeekOrigin.Current);

                    dirs.Add(dir);
                }
            }
        }
        return files;
    }

    protected ObservableCollection<EdataContentFile> ReadEdatV1Dictionary()
    {
        ObservableCollection<EdataContentFile> files = new();
        List<EdataDir> dirs = new();
        List<long> endings = new();

        using (FileStream fileStream = File.Open(FilePath, FileMode.Open))
        {
            _ = fileStream.Seek(Header.DictOffset, SeekOrigin.Begin);

            long dirEnd = Header.DictOffset + Header.DictLength;
            uint id = 0;

            while (fileStream.Position < dirEnd)
            {
                byte[] buffer = new byte[4];
                _ = fileStream.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    EdataContentFile file = new();
                    _ = fileStream.Read(buffer, 0, 4);
                    file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    //buffer = new byte[8];  - it's [4] now, so no need to change
                    _ = fileStream.Read(buffer, 0, 4);
                    file.Offset = BitConverter.ToUInt32(buffer, 0);

                    _ = fileStream.Read(buffer, 0, 4);
                    file.Size = BitConverter.ToUInt32(buffer, 0);

                    //var checkSum = new byte[16];
                    //fileStream.Read(checkSum, 0, checkSum.Length);
                    //file.Checksum = checkSum;
                    _ = fileStream.Seek(1, SeekOrigin.Current);  //instead, skip 1 byte - as in WEE DAT unpacker

                    file.Name = Utils.ReadString(fileStream);
                    file.Path = MergePath(dirs, file.Name);

                    if ((file.Name.Length + 1) % 2 == 0)
                        _ = fileStream.Seek(1, SeekOrigin.Current);

                    file.Id = id;
                    id++;

                    ResolveFileType(fileStream, file);

                    files.Add(file);

                    while (endings.Count > 0 && fileStream.Position == endings.Last())
                    {
                        _ = dirs.Remove(dirs.Last());
                        _ = endings.Remove(endings.Last());
                    }
                }
                else if (fileGroupId > 0)
                {
                    EdataDir dir = new();

                    _ = fileStream.Read(buffer, 0, 4);
                    dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    if (dir.FileEntrySize != 0)
                        endings.Add(dir.FileEntrySize + fileStream.Position - 8);
                    else if (endings.Count > 0)
                        endings.Add(endings.Last());

                    dir.Name = Utils.ReadString(fileStream);

                    if ((dir.Name.Length + 1) % 2 == 1)
                        _ = fileStream.Seek(1, SeekOrigin.Current);

                    dirs.Add(dir);
                }
            }
        }
        return files;
    }

    /// <summary>
    /// Merges a filename in its dictionary tree.
    /// </summary>
    /// <param name="dirs"></param>
    /// <param name="fileName"></param>
    /// <returns>The valid Path inside the package.</returns>
    protected string MergePath(IEnumerable<EdataDir> dirs, string fileName)
    {
        StringBuilder b = new();

        foreach (EdataDir dir in dirs)
            _ = b.Append(dir.Name);

        _ = b.Append(fileName);

        return b.ToString();
    }

    /// <summary>
    /// Replaces a file and rebuilds the Edata File with
    /// </summary>
    /// <param name="oldFile">The EdataFile object which is to be replaced.</param>
    /// <param name="newContent">The data of the new File including Header and content.</param>
    /// <returns>The data of the completly rebuilt EdataFile. This has to be saved back to the file.</returns>
    protected string ReplaceRebuildV2(EdataContentFile oldFile, byte[] newContent)
    {
        byte[] reserveBuffer = new byte[200];

        FileInfo tmp = new(FilePath);

        string tmpPath = Path.Combine(tmp.DirectoryName, string.Format("{0}_{1}", tmp.FullName, "temp"));

        if (!File.Exists(tmpPath))
        {
            using (File.Create(tmpPath))
            { }
        }

        using (FileStream fs = new(FilePath, FileMode.Open))
        {
            using FileStream newFile = new(tmpPath, FileMode.Truncate);
            byte[] headerPart = new byte[Header.FileOffset];
            _ = fs.Read(headerPart, 0, headerPart.Length);
            newFile.Write(headerPart, 0, headerPart.Length);

            _ = fs.Seek(Header.FileOffset, SeekOrigin.Begin);

            uint filesContentLength = 0;
            byte[] fileBuffer;

            foreach (EdataContentFile file in Files)
            {
                long oldOffset = file.Offset;
                file.Offset = newFile.Position - Header.FileOffset;

                if (file == oldFile)
                {
                    fileBuffer = newContent;
                    file.Size = newContent.Length;
                    file.Checksum = MD5.Create().ComputeHash(fileBuffer);
                }
                else
                {
                    fileBuffer = new byte[file.Size];
                    _ = fs.Seek(oldOffset + Header.FileOffset, SeekOrigin.Begin);
                    _ = fs.Read(fileBuffer, 0, fileBuffer.Length);
                }

                newFile.Write(fileBuffer, 0, fileBuffer.Length);
                newFile.Write(reserveBuffer, 0, reserveBuffer.Length);

                filesContentLength += (uint)fileBuffer.Length + (uint)reserveBuffer.Length;
            }

            _ = newFile.Seek(0x25, SeekOrigin.Begin);
            newFile.Write(BitConverter.GetBytes(filesContentLength), 0, 4);

            _ = newFile.Seek(Header.DictOffset, SeekOrigin.Begin);
            long dirEnd = Header.DictOffset + Header.DictLength;
            uint id = 0;

            while (newFile.Position < dirEnd)
            {
                byte[] buffer = new byte[4];
                _ = newFile.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    EdataContentFile curFile = Files[(int)id];

                    // FileEntrySize
                    _ = newFile.Seek(4, SeekOrigin.Current);

                    buffer = BitConverter.GetBytes(curFile.Offset);
                    newFile.Write(buffer, 0, buffer.Length);

                    buffer = BitConverter.GetBytes(curFile.Size);
                    newFile.Write(buffer, 0, buffer.Length);

                    byte[] checkSum = curFile.Checksum;
                    newFile.Write(checkSum, 0, checkSum.Length);

                    string name = Utils.ReadString(newFile);

                    if ((name.Length + 1) % 2 == 1)
                        _ = newFile.Seek(1, SeekOrigin.Current);

                    id++;
                }
                else if (fileGroupId > 0)
                {
                    _ = newFile.Seek(4, SeekOrigin.Current);
                    string name = Utils.ReadString(newFile);

                    if ((name.Length + 1) % 2 == 1)
                        _ = newFile.Seek(1, SeekOrigin.Current);
                }
            }

            _ = newFile.Seek(Header.DictOffset, SeekOrigin.Begin);
            byte[] dirBuffer = new byte[Header.DictLength];
            _ = newFile.Read(dirBuffer, 0, dirBuffer.Length);

            byte[] dirCheckSum = MD5.Create().ComputeHash(dirBuffer);

            _ = newFile.Seek(0x31, SeekOrigin.Begin);

            newFile.Write(dirCheckSum, 0, dirCheckSum.Length);
        }

        return tmpPath;
    }

    protected string ReplaceRebuildV1(EdataContentFile oldFile, byte[] newContent)
    {
        //var reserveBuffer = new byte[200]; // RUSE doesn't like the reserve buffer between files.

        FileInfo tmp = new(FilePath);

        string tmpPath = Path.Combine(tmp.DirectoryName, string.Format("{0}_{1}", tmp.FullName, "temp"));

        if (!File.Exists(tmpPath))
        {
            using (File.Create(tmpPath))
            { }
        }

        using (FileStream fs = new(FilePath, FileMode.Open))
        {
            using FileStream newFile = new(tmpPath, FileMode.Truncate);
            byte[] oldHead = new byte[Header.FileOffset];
            _ = fs.Read(oldHead, 0, (int)Header.FileOffset);
            newFile.Write(oldHead, 0, oldHead.Length);

            byte[] fileBuffer;
            uint filesContentLength = 0;

            // Write Filecontent and replace selected file
            foreach (EdataContentFile file in Files)
            {
                long oldOffset = file.Offset;
                file.Offset = newFile.Position - Header.FileOffset;

                if (file == oldFile)
                {
                    fileBuffer = newContent;
                    file.Size = newContent.Length;
                }
                else
                {
                    fileBuffer = new byte[file.Size];
                    _ = fs.Seek(oldOffset + Header.FileOffset, SeekOrigin.Begin);
                    _ = fs.Read(fileBuffer, 0, fileBuffer.Length);
                }

                newFile.Write(fileBuffer, 0, fileBuffer.Length);
                //newFile.Write(reserveBuffer, 0, reserveBuffer.Length);

                filesContentLength += (uint)fileBuffer.Length; // +(uint)reserveBuffer.Length;
            }

            _ = newFile.Seek(Header.DictOffset, SeekOrigin.Begin);
            long dirEnd = Header.DictOffset + Header.DictLength;
            uint id = 0;

            byte[] buffer = new byte[4];

            while (newFile.Position < dirEnd)
            {
                _ = newFile.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    EdataContentFile curFile = Files[(int)id];

                    // FileEntrySize
                    _ = newFile.Seek(4, SeekOrigin.Current);

                    buffer = BitConverter.GetBytes((uint)curFile.Offset);
                    newFile.Write(buffer, 0, buffer.Length);

                    buffer = BitConverter.GetBytes((uint)curFile.Size);
                    newFile.Write(buffer, 0, buffer.Length);

                    _ = newFile.Seek(1, SeekOrigin.Current);

                    string name = Utils.ReadString(newFile);

                    if ((name.Length + 1) % 2 == 0)
                        _ = newFile.Seek(1, SeekOrigin.Current);

                    id++;
                }
                else if (fileGroupId > 0)
                {
                    _ = newFile.Seek(4, SeekOrigin.Current);
                    string name = Utils.ReadString(newFile);

                    if ((name.Length + 1) % 2 == 1)
                        _ = newFile.Seek(1, SeekOrigin.Current);
                }
            }

            _ = newFile.Seek(Header.DictOffset, SeekOrigin.Begin);
            byte[] dirBuffer = new byte[Header.DictLength];
            _ = newFile.Read(dirBuffer, 0, dirBuffer.Length);
            byte[] dirCheckSum = MD5.Create().ComputeHash(dirBuffer);

            _ = newFile.Seek(8, SeekOrigin.Begin);
            newFile.Write(dirCheckSum, 0, dirCheckSum.Length);

            _ = newFile.Seek(13, SeekOrigin.Current);
            newFile.Write(BitConverter.GetBytes(filesContentLength), 0, 4);
        }

        return tmpPath;
    }

    /// <summary>
    /// Replaces a file in the current Edata package and saves the changes back.
    /// </summary>
    /// <param name="oldFile">The EdataFile object which is to be replaced.</param>
    /// <param name="newContent">The data of the new File including Header and content.</param>
    public virtual void ReplaceFile(EdataContentFile oldFile, byte[] newContent)
    {
        if (!File.Exists(FilePath))
            throw new InvalidOperationException("The Edata file does not exist anymore.");

        string newFile = Header.Version switch
        {
            1 => ReplaceRebuildV1(oldFile, newContent),
            2 => ReplaceRebuildV2(oldFile, newContent),
            _ => throw new NotSupportedException(string.Format("Edata Version {0} is currently not supported", Header.Version)),
        };
        FileInfo oldFileInfo = new(FilePath);

        string backupFile = Path.Combine(oldFileInfo.DirectoryName, "to_delete.dat");

        File.Move(FilePath, backupFile);
        File.Move(newFile, FilePath);

        File.Delete(backupFile);
    }

    protected void ResolveFileType(FileStream fs, EdataContentFile file)
    {
        // save original offset
        long origOffset = fs.Position;

        _ = fs.Seek(file.Offset + Header.FileOffset, SeekOrigin.Begin);

        byte[] headerBuffer = new byte[12];
        _ = fs.Read(headerBuffer, 0, headerBuffer.Length);

        file.FileType = GetFileTypeFromHeaderData(headerBuffer);

        // set offset back to original
        _ = fs.Seek(origOffset, SeekOrigin.Begin);
    }

    private static readonly List<KeyValuePair<EdataFileType, byte[]>> _knownHeaders;

    public static IEnumerable<KeyValuePair<EdataFileType, byte[]>> KnownHeaders => _knownHeaders;

    public static EdataFileType GetFileTypeFromHeaderData(byte[] headerData)
    {
        foreach (KeyValuePair<EdataFileType, byte[]> knownHeader in KnownHeaders)
        {
            if (knownHeader.Value.Length < headerData.Length)
            {
                byte[] tmp = new byte[knownHeader.Value.Length];
                Array.Copy(headerData, tmp, knownHeader.Value.Length);
                //headerData = tmp;

                if (Utils.ByteArrayCompare(tmp, knownHeader.Value))
                    return knownHeader.Key;
            }
            else
                if (Utils.ByteArrayCompare(headerData, knownHeader.Value))
            {
                return knownHeader.Key;
            }
        }

        return EdataFileType.Unknown;
    }
}