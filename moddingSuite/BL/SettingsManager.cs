using moddingSuite.Model.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace moddingSuite.BL;

public static class SettingsManager
{
    public static readonly string SettingsPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "edataFileManager", "settings.xml");

    //private static Settings LastLoadedSettings { get; set; }

    public static Settings Load()
    {
        Settings settings = new();

        if (!File.Exists(SettingsPath))
        {
            return settings;
        }

        XmlSerializer serializer = new(typeof (Settings));
        using (FileStream fs = new(SettingsPath, FileMode.Open))
        {
            try
            {
                settings = serializer.Deserialize(fs) as Settings;
                settings.InitialSettings = false;
            }
            catch (InvalidOperationException ex)
            {
                Trace.TraceError($"Error while loading Settings: {ex}");
            }
        }

        return settings;
    }

    public static bool Save(Settings settingsToSave)
    {
        if (settingsToSave == null)
            return false;

        string dir = Path.GetDirectoryName(SettingsPath);

        if (dir != null && !Directory.Exists(dir))
            _ = Directory.CreateDirectory(dir);

        try
        {
            using FileStream fs = File.Create(SettingsPath);
            XmlSerializer serializer = new(typeof (Settings));

            serializer.Serialize(fs, settingsToSave);

            fs.Flush();
        }
        catch (UnauthorizedAccessException uaex)
        {
            Trace.TraceError("Error while saving settings: {0}", uaex);
            return false;
        }
        catch (IOException ioex)
        {
            Trace.TraceError("Error while saving settings: {0}", ioex);
            return false;
        }

        return true;
    }
}