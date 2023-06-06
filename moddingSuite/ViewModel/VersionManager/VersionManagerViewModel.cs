using moddingSuite.BL;
using moddingSuite.Model.Settings;
using moddingSuite.ViewModel.Base;
using moddingSuite.ViewModel.Edata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace moddingSuite.ViewModel.VersionManager;

public class VersionManagerViewModel : ViewModelBase
{
    private EdataManagerViewModel _edataManagerViewModel;
    private ICollectionView _versionCollectionView;
    private int _versionFilter;
    private ObservableCollection<VersionInfoViewModel> _versions = new();

    public VersionManagerViewModel(EdataManagerViewModel edataManagerViewModel)
    {
        EdataManagerViewModel = edataManagerViewModel;
        Initialize();
    }

    public ICollectionView VersionCollectionView
    {
        get
        {
            BuildVersionCollectionView();

            return _versionCollectionView;
        }
    }

    public EdataManagerViewModel EdataManagerViewModel
    {
        get => _edataManagerViewModel;
        set
        {
            _edataManagerViewModel = value;
            OnPropertyChanged(() => EdataManagerViewModel);
        }
    }

    public ObservableCollection<VersionInfoViewModel> Versions
    {
        get => _versions;
        set
        {
            _versions = value;
            OnPropertyChanged(() => Versions);
        }
    }

    public int VersionFilter
    {
        get => _versionFilter;
        set
        {
            _versionFilter = value;
            VersionCollectionView.Refresh();
            OnPropertyChanged(() => VersionFilter);
        }
    }

    public List<int> VersionNumbers { get; } = new();

    protected void Initialize()
    {
        Settings s = SettingsManager.Load();

        if (!Directory.Exists(s.WargamePath))
            return;

        string dataPath = Path.Combine(s.WargamePath, "Data", "wargame", "PC");

        if (!Directory.Exists(dataPath))
            return;

        DirectoryInfo dataDir = new(dataPath);

        foreach (DirectoryInfo dir in dataDir.EnumerateDirectories())
        {
            VersionInfoViewModel v = new(dir, this);
            Versions.Add(v);
            VersionNumbers.Add(v.Version);
        }

        foreach (VersionInfoViewModel version in Versions)
        {
            foreach (DirectoryInfo directory in version.DirectoryInfo.EnumerateDirectories())
            {
                VersionInfoViewModel v = Versions.SingleOrDefault(x => x.Version == Convert.ToInt32(directory.Name));

                if (v != null)
                {
                    foreach (FileInfo fl in directory.EnumerateFiles())
                        v.VersionFiles.Add(new VersionFileViewModel(fl));
                }
            }
        }

        VersionFilter = Versions.OrderByDescending(x => x.Version).First().Version;
    }

    protected void BuildVersionCollectionView()
    {
        if (_versionCollectionView == null)
        {
            _versionCollectionView = CollectionViewSource.GetDefaultView(Versions);
            _versionCollectionView.Filter = FilterVersions;
            OnPropertyChanged(() => VersionCollectionView);
        }
    }

    protected bool FilterVersions(object x)
    {
        bool b = ((VersionInfoViewModel)x).Version <= VersionFilter;

        return b;
    }
}