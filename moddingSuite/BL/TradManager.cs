﻿using moddingSuite.Model.Trad;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace moddingSuite.BL;

public class TradManager
{
    private ObservableCollection<TradEntry> _entries = new();

    private const ulong GlyphHash = (ulong)0x1 << 63;

    public TradManager(byte[] data)
    {
        ParseTradFile(data);

        _entries.CollectionChanged += EntriesCollectionChanged;
    }

    public ObservableCollection<TradEntry> Entries
    {
        get { return _entries; }
        private set { _entries = value; }
    }

    private void EntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            foreach (object tradEntry in e.NewItems)
                ((TradEntry)tradEntry).UserCreated = true;
    }

    protected void ParseTradFile(byte[] data)
    {
        using (MemoryStream ms = new(data))
        {
            uint entryCount = ReadHeader(ms);
            Entries = ReadDictionary(entryCount, ms);

            GetEntryContents(ms);
        }
    }

    protected void GetEntryContents(MemoryStream ms)
    {
        foreach (TradEntry entry in Entries)
        {
            ms.Seek(entry.OffsetCont, SeekOrigin.Begin);

            byte[] buffer = new byte[entry.ContLen * 2];

            ms.Read(buffer, 0, buffer.Length);

            entry.Content = Encoding.Unicode.GetString(buffer);
        }
    }

    protected ObservableCollection<TradEntry> ReadDictionary(uint entryCount, MemoryStream ms)
    {
        ObservableCollection<TradEntry> entries = new();

        byte[] buffer = new byte[4];

        for (int i = 0; i < entryCount; i++)
        {
            TradEntry entry = new()
            { OffsetDic = (uint)ms.Position };

            byte[] hashBuffer = new byte[8];

            ms.Read(hashBuffer, 0, hashBuffer.Length);
            entry.Hash = hashBuffer;

            ms.Read(buffer, 0, buffer.Length);
            entry.OffsetCont = BitConverter.ToUInt32(buffer, 0);

            ms.Read(buffer, 0, buffer.Length);
            entry.ContLen = BitConverter.ToUInt32(buffer, 0);

            entries.Add(entry);
        }

        return entries;
    }

    protected uint ReadHeader(MemoryStream ms)
    {
        byte[] buffer = new byte[4];

        ms.Read(buffer, 0, buffer.Length);

        if (Encoding.ASCII.GetString(buffer) != "TRAD")
            throw new ArgumentException("No valid Eugen Systems TRAD (*.dic) file.");

        ms.Read(buffer, 0, buffer.Length);

        return BitConverter.ToUInt32(buffer, 0);
    }

    public byte[] BuildTradFile()
    {
        using (MemoryStream ms = new())
        {
            byte[] buffer = Encoding.ASCII.GetBytes("TRAD");
            ms.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(Entries.Count);
            ms.Write(buffer, 0, buffer.Length);


            TradEntry glyphEntry = Entries.FirstOrDefault(x => BitConverter.ToUInt64(x.Hash, 0).Equals(GlyphHash));

            if (glyphEntry == null)
            {
                glyphEntry = new TradEntry() { Hash = BitConverter.GetBytes(GlyphHash) };
                Entries.Add(glyphEntry);
            }

            List<TradEntry> orderedEntried = Entries.OrderBy(x => BitConverter.ToUInt64(x.Hash, 0)).ToList();
            orderedEntried.Remove(glyphEntry);
            glyphEntry.Content = BuildGlyphContent(orderedEntried);
            orderedEntried.Add(glyphEntry);

            // Write dictionary
            foreach (TradEntry entry in orderedEntried)
            {
                entry.OffsetDic = (uint)ms.Position;

                // Hash
                ms.Write(entry.Hash, 0, entry.Hash.Length);

                // Content offset : we dont know it yet
                ms.Seek(4, SeekOrigin.Current);

                // Content length
                buffer = BitConverter.GetBytes(entry.Content.Length);
                ms.Write(buffer, 0, buffer.Length);
            }

            foreach (TradEntry entry in orderedEntried)
            {
                entry.OffsetCont = (uint)ms.Position;
                buffer = Encoding.Unicode.GetBytes(entry.Content);
                ms.Write(buffer, 0, buffer.Length);
            }

            foreach (TradEntry entry in orderedEntried)
            {
                ms.Seek(entry.OffsetDic, SeekOrigin.Begin);

                ms.Seek(8, SeekOrigin.Current);

                buffer = BitConverter.GetBytes(entry.OffsetCont);

                ms.Write(buffer, 0, buffer.Length);
            }

            //Util.Utils.SaveDebug("dicttest.diccmp",ms.ToArray());

            return ms.ToArray();
        }
    }

    protected string BuildGlyphContent(List<TradEntry> lst)
    {
        Dictionary<char, int> glyphOccurences = new();

        foreach (TradEntry e in lst)
            foreach (char chr in e.Content)

                if (glyphOccurences.ContainsKey(chr))
                    glyphOccurences[chr]++;
                else
                    glyphOccurences.Add(chr, 1);

        StringBuilder contentBuilder = new();

        IOrderedEnumerable<KeyValuePair<char, int>> tmp = glyphOccurences.OrderByDescending(x => x.Value);

        foreach (KeyValuePair<char, int> occurence in glyphOccurences.OrderByDescending(x => x.Value))
            contentBuilder.Append(occurence.Key);

        return contentBuilder.ToString();
    }
}