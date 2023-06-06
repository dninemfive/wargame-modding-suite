﻿using moddingSuite.BL;
using moddingSuite.Model.Edata;
using moddingSuite.ViewModel.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;

namespace moddingSuite.ViewModel.Edata;

public class EdataFileViewModel : ViewModelBase
{
    private ObservableCollection<EdataContentFile> _files;
    private ICollectionView _filesCollectionView;
    private string _filterExpression = string.Empty;
    private string _loadedFile = string.Empty;

    public EdataManager EdataManager { get; protected set; }

    public ICommand CloseCommand { get; set; }

    public ICommand DetailsCommand { get; set; }

    public EdataManagerViewModel ParentVm { get; }

    public EdataFileViewModel(EdataManagerViewModel parentVm)
    {
        ParentVm = parentVm;

        CloseCommand = new ActionCommand((x) => ParentVm.CloseFile(this));
        DetailsCommand = new ActionCommand(DetailsExecute);
    }

    private void DetailsExecute(object obj)
    {
        if (obj is not EdataContentFile file)
            return;

        switch (file.FileType)
        {
            case EdataFileType.Ndfbin:
                ParentVm.EditNdfbinCommand.Execute(obj);
                break;
            case EdataFileType.Image:
                ParentVm.ExportTextureCommand.Execute(obj);
                break;
            case EdataFileType.Dictionary:
                ParentVm.EditTradFileCommand.Execute(obj);
                break;
            case EdataFileType.Mesh:
                ParentVm.EditMeshCommand.Execute(obj);
                break;
            case EdataFileType.Scenario:
                ParentVm.EditScenarioCommand.Execute(obj);
                break;
        }
    }

    public string LoadedFile
    {
        get => _loadedFile;
        set
        {
            _loadedFile = value;
            OnPropertyChanged(() => LoadedFile);
            OnPropertyChanged(() => HeaderText);
        }
    }

    public string HeaderText
    {
        get
        {
            FileInfo f = new(LoadedFile);

            return f.Name;
        }
    }

    public ObservableCollection<EdataContentFile> Files
    {
        get => _files;
        set
        {
            _files = value;
            OnPropertyChanged(() => Files);
        }
    }

    public ICollectionView FilesCollectionView
    {
        get
        {
            if (_filesCollectionView == null)
            {
                CreateFilesCollectionView();
            }

            return _filesCollectionView;
        }
    }

    public string FilterExpression
    {
        get => _filterExpression;
        set
        {
            _filterExpression = value;
            OnPropertyChanged(() => FilterExpression);
            FilesCollectionView.Refresh();
        }
    }

    public void LoadFile(string path)
    {
        EdataManager = new EdataManager(path);

        LoadedFile = EdataManager.FilePath;

        EdataManager.ParseEdataFile();
        Files = EdataManager.Files;
        CreateFilesCollectionView();
    }

    public bool FilterPath(object item) => item is not EdataContentFile file || FilterExpression == string.Empty || FilterExpression.Length < 3
|| file.Path.Contains(FilterExpression);

    private void CreateFilesCollectionView()
    {
        _filesCollectionView = CollectionViewSource.GetDefaultView(Files);
        _filesCollectionView.Filter = FilterPath;

        OnPropertyChanged(() => FilesCollectionView);
    }
}