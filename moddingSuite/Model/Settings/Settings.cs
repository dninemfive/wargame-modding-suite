using moddingSuite.ViewModel.Base;
using System.Collections.Generic;

namespace moddingSuite.Model.Settings;

public class Settings : ViewModelBase
{
    private string _lastOpenFolder;
    private List<string> _lastOpenedFile = new();
    private string _savePath;
    private int _lastHighlightedFileIndex;
    private string _wargamePath;
    private string _pythonPath;
    private bool _exportWithFullPath = true;
    private bool _initialSettings = true;

    public string SavePath
    {
        get => _savePath;
        set
        {
            _savePath = value;
            OnPropertyChanged(() => SavePath);
        }
    }

    public List<string> LastOpenedFiles
    {
        get => _lastOpenedFile;
        set
        {
            _lastOpenedFile = value;
            OnPropertyChanged(() => LastOpenedFiles);
        }
    }

    public string LastOpenFolder
    {
        get => _lastOpenFolder;
        set { _lastOpenFolder = value; OnPropertyChanged(() => LastOpenedFiles); }
    }

    public int LastHighlightedFileIndex
    {
        get => _lastHighlightedFileIndex;
        set { _lastHighlightedFileIndex = value; OnPropertyChanged(() => LastOpenedFiles); }
    }

    public string WargamePath
    {
        get => _wargamePath;
        set { _wargamePath = value; OnPropertyChanged(() => WargamePath); }
    }

    public string PythonPath
    {
        get => _pythonPath;
        set { _pythonPath = value; OnPropertyChanged(() => PythonPath); }
    }

    public bool ExportWithFullPath
    {
        get => _exportWithFullPath;
        set
        {
            _exportWithFullPath = value;
            OnPropertyChanged();
        }
    }

    public bool InitialSettings
    {
        get => _initialSettings;
        set
        {
            _initialSettings = value;
            OnPropertyChanged();
        }
    }
}