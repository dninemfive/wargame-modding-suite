using moddingSuite.BL;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace moddingSuite.Model.Edata;

public class EdataPackage : EdataManager
{
    private EdataDir _root;

    public EdataPackage(string path)
        : base(path)
    {

    }

    public EdataDir Root
    {
        get => _root;
        set
        {
            _root = value;
            OnPropertyChanged("Root");
        }
    }

    public Dictionary<EdataContentFile, byte[]> LoadedFiles { get; } = new();

    public byte[] GetRawData(EdataContentFile ofFile, bool cacheResult = true)
    {
        byte[] rawData;

        if (LoadedFiles.ContainsKey(ofFile))
        {
            rawData = LoadedFiles[ofFile];
            ofFile.Checksum = MD5.Create().ComputeHash(rawData);
        }
        else
        {
            rawData = base.GetRawData(ofFile);
            if (cacheResult)
                LoadedFiles.Add(ofFile, rawData);
        }

        return rawData;
    }

    public override void ReplaceFile(EdataContentFile oldFile, byte[] newContent)
    {
        if (LoadedFiles.ContainsKey(oldFile))
            LoadedFiles[oldFile] = newContent;
        else
            LoadedFiles.Add(oldFile, newContent);
    }
}
