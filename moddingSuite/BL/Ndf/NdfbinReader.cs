using moddingSuite.BL.Compressing;
using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace moddingSuite.BL.Ndf;

public class NdfbinReader : INdfReader
{
    public NdfBinary Read(byte[] data)
    {
        NdfBinary ndf = new();

        using (MemoryStream ms = new(data))
        {
            ndf.Header = ReadHeader(ms);

            if (ndf.Header.IsCompressedBody)
            {
                using MemoryStream uncompStream = new();
                _ = ms.Seek(0, SeekOrigin.Begin);
                byte[] headBuffer = new byte[ndf.Header.HeaderSize];
                _ = ms.Read(headBuffer, 0, headBuffer.Length);
                uncompStream.Write(headBuffer, 0, headBuffer.Length);

                _ = ms.Seek((long)ndf.Header.HeaderSize, SeekOrigin.Begin);

                byte[] buffer = new byte[4];
                _ = ms.Read(buffer, 0, buffer.Length);
                uint compressedblocklen = BitConverter.ToUInt32(buffer, 0);

                byte[] contentBuffer = new byte[(ulong)data.Length - ndf.Header.HeaderSize];
                _ = ms.Read(contentBuffer, 0, contentBuffer.Length);

                byte[] da = Compressor.Decomp(contentBuffer);
                uncompStream.Write(da, 0, da.Length);

                data = uncompStream.ToArray();
            }
        }

        using (MemoryStream ms = new(data))
        {
            ndf.Footer = ReadFooter(ms, ndf.Header);
            ndf.Classes = ReadClasses(ms, ndf);
            ReadProperties(ms, ndf);

            ndf.Strings = ReadStrings(ms, ndf);
            ndf.Trans = ReadTrans(ms, ndf);

            ndf.TopObjects = new HashSet<uint>(ReadUIntList(ms, ndf, "TOPO"));
            ndf.Import = ReadUIntList(ms, ndf, "IMPR");
            ndf.Export = ReadUIntList(ms, ndf, "EXPR");

            ndf.Instances = ReadObjects(ms, ndf);
        }

        return ndf;
    }

    public byte[] GetUncompressedNdfbinary(byte[] data)
    {
        using (MemoryStream ms = new(data))
        {
            NdfHeader header = ReadHeader(ms);

            if (header.IsCompressedBody)
            {
                using MemoryStream uncompStream = new();
                _ = ms.Seek(0, SeekOrigin.Begin);
                byte[] headBuffer = new byte[header.HeaderSize];
                _ = ms.Read(headBuffer, 0, headBuffer.Length);
                uncompStream.Write(headBuffer, 0, headBuffer.Length);

                _ = ms.Seek((long)header.HeaderSize, SeekOrigin.Begin);

                byte[] buffer = new byte[4];
                _ = ms.Read(buffer, 0, buffer.Length);
                uint compressedblocklen = BitConverter.ToUInt32(buffer, 0);

                byte[] contentBuffer = new byte[(ulong)data.Length - header.HeaderSize];
                _ = ms.Read(contentBuffer, 0, contentBuffer.Length);
                //Compressor.Decomp(contentBuffer, uncompStream);
                byte[] da = Compressor.Decomp(contentBuffer);

                uncompStream.Write(da, 0, da.Length);

                data = uncompStream.ToArray();
            }
        }

        return data;
    }

    /// <summary>
    /// Reads the header data of the compiled Ndf binary.
    /// </summary>
    /// <returns>A valid instance of the Headerfile.</returns>
    protected NdfHeader ReadHeader(Stream ms)
    {
        NdfHeader header = new();

        byte[] buffer = new byte[4];
        _ = ms.Read(buffer, 0, buffer.Length);

        if (BitConverter.ToUInt32(buffer, 0) != 809981253)
            throw new InvalidDataException("No EUG0 found on top of this file!");

        _ = ms.Read(buffer, 0, buffer.Length);

        if (BitConverter.ToUInt32(buffer, 0) != 0)
            throw new InvalidDataException("Bytes between EUG0 and CNDF have to be 0");

        _ = ms.Read(buffer, 0, buffer.Length);

        if (BitConverter.ToUInt32(buffer, 0) != 1178881603)
            throw new InvalidDataException("No CNDF (Compiled NDF)!");

        _ = ms.Read(buffer, 0, buffer.Length);
        header.IsCompressedBody = BitConverter.ToInt32(buffer, 0) == 128;

        buffer = new byte[8];

        _ = ms.Read(buffer, 0, buffer.Length);
        header.FooterOffset = BitConverter.ToUInt64(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        header.HeaderSize = BitConverter.ToUInt64(buffer, 0);

        _ = ms.Read(buffer, 0, buffer.Length);
        header.FullFileSizeUncomp = BitConverter.ToUInt64(buffer, 0);

        return header;
    }

    /// <summary>
    /// Reads the footer data which is the Ndfbin Dictionary.
    /// </summary>
    /// <returns></returns>
    protected NdfFooter ReadFooter(Stream ms, NdfHeader head)
    {
        NdfFooter footer = new();

        _ = ms.Seek((long)head.FooterOffset, SeekOrigin.Begin);

        byte[] dwdBuffer = new byte[4];
        byte[] qwdbuffer = new byte[8];

        _ = ms.Read(dwdBuffer, 0, dwdBuffer.Length);
        if (BitConverter.ToUInt32(dwdBuffer, 0) != 809717588)
            throw new InvalidDataException("Footer doesnt start with TOC0");

        _ = ms.Read(dwdBuffer, 0, dwdBuffer.Length);
        uint footerEntryCount = BitConverter.ToUInt32(dwdBuffer, 0);

        for (int i = 0; i < footerEntryCount; i++)
        {
            NdfFooterEntry entry = new();

            _ = ms.Read(qwdbuffer, 0, qwdbuffer.Length);
            entry.Name = Encoding.ASCII.GetString(qwdbuffer).TrimEnd('\0');

            _ = ms.Read(qwdbuffer, 0, qwdbuffer.Length);
            entry.Offset = BitConverter.ToInt64(qwdbuffer, 0);

            _ = ms.Read(qwdbuffer, 0, qwdbuffer.Length);
            entry.Size = BitConverter.ToInt64(qwdbuffer, 0);

            footer.Entries.Add(entry);
        }

        return footer;
    }

    /// <summary>
    /// Reads the Classes dictionary.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected ObservableCollection<NdfClass> ReadClasses(Stream ms, NdfBinary owner)
    {
        ObservableCollection<NdfClass> classes = new();

        NdfFooterEntry classEntry = owner.Footer.Entries.Single(x => x.Name == "CLAS");

        _ = ms.Seek(classEntry.Offset, SeekOrigin.Begin);

        uint i = 0;
        byte[] buffer = new byte[4];

        while (ms.Position < classEntry.Offset + classEntry.Size)
        {
            NdfClass nclass = new(owner, i);

            _ = ms.Read(buffer, 0, buffer.Length);
            int strLen = BitConverter.ToInt32(buffer, 0);

            byte[] strBuffer = new byte[strLen];
            _ = ms.Read(strBuffer, 0, strBuffer.Length);

            nclass.Name = Encoding.GetEncoding("ISO-8859-1").GetString(strBuffer);

            i++;
            classes.Add(nclass);
        }

        return classes;
    }

    /// <summary>
    /// Reads the Properties dictionary and relates each one to its owning class.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="owner"></param>
    protected void ReadProperties(Stream ms, NdfBinary owner)
    {
        NdfFooterEntry propEntry = owner.Footer.Entries.Single(x => x.Name == "PROP");
        _ = ms.Seek(propEntry.Offset, SeekOrigin.Begin);

        int i = 0;
        byte[] buffer = new byte[4];
        while (ms.Position < propEntry.Offset + propEntry.Size)
        {
            NdfProperty property = new(i);

            _ = ms.Read(buffer, 0, buffer.Length);
            int strLen = BitConverter.ToInt32(buffer, 0);

            byte[] strBuffer = new byte[strLen];
            _ = ms.Read(strBuffer, 0, strBuffer.Length);

            property.Name = Encoding.GetEncoding("ISO-8859-1").GetString(strBuffer);

            _ = ms.Read(buffer, 0, buffer.Length);

            NdfClass cls = owner.Classes.Single(x => x.Id == BitConverter.ToUInt32(buffer, 0));
            property.Class = cls;

            cls.Properties.Add(property);

            i++;
        }
    }

    /// <summary>
    /// Reads the string list.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected ObservableCollection<NdfStringReference> ReadStrings(Stream ms, NdfBinary owner)
    {
        ObservableCollection<NdfStringReference> strings = new();

        NdfFooterEntry stringEntry = owner.Footer.Entries.Single(x => x.Name == "STRG");
        _ = ms.Seek(stringEntry.Offset, SeekOrigin.Begin);

        int i = 0;
        byte[] buffer = new byte[4];
        while (ms.Position < stringEntry.Offset + stringEntry.Size)
        {
            NdfStringReference nstring = new()
            { Id = i };

            _ = ms.Read(buffer, 0, buffer.Length);
            int strLen = BitConverter.ToInt32(buffer, 0);

            byte[] strBuffer = new byte[strLen];
            _ = ms.Read(strBuffer, 0, strBuffer.Length);

            nstring.Value = Encoding.GetEncoding("ISO-8859-1").GetString(strBuffer);

            i++;
            strings.Add(nstring);
        }

        return strings;
    }

    /// <summary>
    /// Reads the trans list
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected ObservableCollection<NdfTranReference> ReadTrans(Stream ms, NdfBinary owner)
    {
        ObservableCollection<NdfTranReference> trans = new();

        NdfFooterEntry stringEntry = owner.Footer.Entries.Single(x => x.Name == "TRAN");
        _ = ms.Seek(stringEntry.Offset, SeekOrigin.Begin);

        int i = 0;
        byte[] buffer = new byte[4];
        while (ms.Position < stringEntry.Offset + stringEntry.Size)
        {
            NdfTranReference ntran = new()
            { Id = i };

            _ = ms.Read(buffer, 0, buffer.Length);
            int strLen = BitConverter.ToInt32(buffer, 0);

            byte[] strBuffer = new byte[strLen];
            _ = ms.Read(strBuffer, 0, strBuffer.Length);

            ntran.Value = Encoding.GetEncoding("ISO-8859-1").GetString(strBuffer);

            i++;
            trans.Add(ntran);
        }

        // TODO: Trans is actually more a tree than a list, this is still not fully implemented/reversed.

        return trans;
    }

    /// <summary>
    /// Reads the amount of instances this file contains.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected uint ReadChunk(Stream ms, NdfBinary owner)
    {
        NdfFooterEntry chnk = owner.Footer.Entries.Single(x => x.Name == "CHNK");
        _ = ms.Seek(chnk.Offset, SeekOrigin.Begin);

        byte[] buffer = new byte[4];

        _ = ms.Read(buffer, 0, buffer.Length);
        _ = ms.Read(buffer, 0, buffer.Length);

        return BitConverter.ToUInt32(buffer, 0);
    }

    /// <summary>
    /// Reads a list of UInt32, this is needed for the topobjects, import and export tables.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="owner"></param>
    /// <param name="lst"></param>
    /// <returns></returns>
    protected List<uint> ReadUIntList(Stream ms, NdfBinary owner, string lst)
    {
        List<uint> uintList = new();

        NdfFooterEntry uintEntry = owner.Footer.Entries.Single(x => x.Name == lst);
        _ = ms.Seek(uintEntry.Offset, SeekOrigin.Begin);

        byte[] buffer = new byte[4];
        while (ms.Position < uintEntry.Offset + uintEntry.Size)
        {
            _ = ms.Read(buffer, 0, buffer.Length);
            uintList.Add(BitConverter.ToUInt32(buffer, 0));
        }

        return uintList;
    }

    /// <summary>
    /// Reads the object instances.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected List<NdfObject> ReadObjects(Stream ms, NdfBinary owner)
    {
        List<NdfObject> objects = new();

        uint instanceCount = ReadChunk(ms, owner);

        NdfFooterEntry objEntry = owner.Footer.Entries.Single(x => x.Name == "OBJE");
        _ = ms.Seek(objEntry.Offset, SeekOrigin.Begin);

        for (uint i = 0; i < instanceCount; i++)
        {
            long objOffset = ms.Position;
            try
            {
                NdfObject obj = ReadObject(ms, i, owner);

                obj.Offset = objOffset;

                objects.Add(obj);
            }
            catch (Exception)
            {
                throw;
            }
        }

        return objects;
    }

    /// <summary>
    /// Reads one object instance.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="index"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    protected NdfObject ReadObject(Stream ms, uint index, NdfBinary owner)
    {
        NdfObject instance = new()
        { Id = index };

        if (owner.TopObjects.Contains(index))
            instance.IsTopObject = true;

        byte[] buffer = new byte[4];
        _ = ms.Read(buffer, 0, buffer.Length);
        int classId = BitConverter.ToInt32(buffer, 0);

        if (owner.Classes.Count < classId)
            throw new InvalidDataException("Object without class found.");

        NdfClass cls = instance.Class = owner.Classes[classId];

        cls.Instances.Add(instance);

        // Read properties
        for (; ; )
        {
            _ = ms.Read(buffer, 0, buffer.Length);
            uint propertyId = BitConverter.ToUInt32(buffer, 0);

            if (propertyId == 0xABABABAB)
                break;

            NdfPropertyValue propVal = new(instance)
            {
                Property = cls.Properties.SingleOrDefault(x => x.Id == propertyId)
            };

            if (propVal.Property == null)
            {
                // throw new InvalidDataException("Found a value for a property which doens't exist in this class.");
                foreach (NdfClass c in owner.Classes)
                {
                    foreach (NdfProperty p in c.Properties)
                    {
                        if (p.Id == propertyId)
                        {
                            propVal.Property = p;
                            break;
                        }
                    }
                }
            }

            instance.PropertyValues.Add(propVal);
            try
            {
                NdfValueWrapper res = ReadValue(ms, owner);
                propVal.Value = res;
            }
            catch (Exception)
            {
                throw;
            }
        }

        owner.AddEmptyProperties(instance);

        return instance;
    }

    /// <summary>
    /// Reads the value of a Property inside a object instance.
    /// </summary>
    /// <param name="ms"></param>
    /// <param name="binary"></param>
    /// <returns>A NdfValueWrapper Instance.</returns>
    protected NdfValueWrapper ReadValue(Stream ms, NdfBinary binary)
    {
        uint contBufferlen;
        NdfValueWrapper value;
        byte[] buffer = new byte[4];

        _ = ms.Read(buffer, 0, buffer.Length);
        NdfType type=NdfTypeManager.GetType(buffer);

        if (type == NdfType.Unknown)
        {
            using (FileStream file = File.Create("dump.bin"))
            {
                int k = 64;
                byte[] buf = new byte[k];
                _ = ms.Read(buf, 0, k);
                file.Write(buf, 0, k);
                file.Flush();
                Console.WriteLine("dumped");
            }

            throw new InvalidDataException("Unknown datatypes are not supported!");

        }
        if (type == NdfType.Reference)
        {
            _ = ms.Read(buffer, 0, buffer.Length);
            type = NdfTypeManager.GetType(buffer);
        }

        switch (type)
        {
            case NdfType.WideString:
            case NdfType.List:
            case NdfType.MapList:
            case NdfType.Blob:
            case NdfType.ZipBlob:
                _ = ms.Read(buffer, 0, buffer.Length);
                contBufferlen = BitConverter.ToUInt32(buffer, 0);

                if (type == NdfType.ZipBlob)
                {
                    if (ms.ReadByte() != 1)
                        throw new InvalidDataException("has to be checked.");
                }

                break;
            default:
                contBufferlen = NdfTypeManager.SizeofType(type);
                break;
        }

        switch (type)
        {
            case NdfType.MapList:
            case NdfType.List:
                NdfCollection lstValue = type == NdfType.List ? new NdfCollection() : new NdfMapList();

                for (int i = 0; i < contBufferlen; i++)
                {
                    CollectionItemValueHolder res = type == NdfType.List
                        ? new CollectionItemValueHolder(ReadValue(ms, binary), binary)
                        : new CollectionItemValueHolder(
                            new NdfMap(
                                new MapValueHolder(ReadValue(ms, binary), binary),
                                new MapValueHolder(ReadValue(ms, binary), binary),
                                binary), binary);

                    lstValue.Add(res);
                }

                value = lstValue;
                break;
            case NdfType.Map:
                value = new NdfMap(
                    new MapValueHolder(ReadValue(ms, binary), binary),
                    new MapValueHolder(ReadValue(ms, binary), binary),
                    binary);
                break;
            default:
                byte[] contBuffer = new byte[contBufferlen];
                _ = ms.Read(contBuffer, 0, contBuffer.Length);

                value = NdfTypeManager.GetValue(contBuffer, type, binary);
                break;
        }

        return value;
    }
}
