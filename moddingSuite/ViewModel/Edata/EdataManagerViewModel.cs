using moddingSuite.BL;
using moddingSuite.BL.Ess;
using moddingSuite.BL.Mesh;
using moddingSuite.BL.Ndf;
using moddingSuite.BL.TGV;
using moddingSuite.Model.Edata;
using moddingSuite.Model.Settings;
using moddingSuite.View.Common;
using moddingSuite.View.DialogProvider;
using moddingSuite.View.Ndfbin;
using moddingSuite.ViewModel.About;
using moddingSuite.ViewModel.Base;
using moddingSuite.ViewModel.Media;
using moddingSuite.ViewModel.Mesh;
using moddingSuite.ViewModel.Ndf;
using moddingSuite.ViewModel.Scenario;
using moddingSuite.ViewModel.Trad;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace moddingSuite.ViewModel.Edata;

public class EdataManagerViewModel : ViewModelBase
{
    private string _statusText;

    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged(() => StatusText);
        }
    }

    public EdataManagerViewModel()
    {
        InitializeCommands();

        Settings settings = SettingsManager.Load();

        List<FileInfo> failedFiles = new();

        foreach (string file in settings.LastOpenedFiles)
        {
            FileInfo fileInfo = new(file);

            if (fileInfo.Exists)
            {
                try
                {
                    AddFile(fileInfo.FullName);
                }
                catch (IOException)
                {
                    failedFiles.Add(fileInfo);
                }
            }
        }

        if (failedFiles.Count > 0)
            StatusText = $"{failedFiles.Count} files failed to open. Did you start the modding suite while running the game?";

        if (settings.LastOpenedFiles.Count == 0)
            _ = CollectionViewSource.GetDefaultView(OpenFiles).MoveCurrentToFirst();

        Workspace = new WorkspaceViewModel(settings);
        Gamespace = new GameSpaceViewModel(settings);

        OpenFiles.CollectionChanged += OpenFilesCollectionChanged;
    }

    public ICommand ExportNdfCommand { get; set; }
    public ICommand ExportRawCommand { get; set; }
    public ICommand ReplaceRawCommand { get; set; }
    public ICommand ExportTextureCommand { get; set; }
    public ICommand ReplaceTextureCommand { get; set; }
    public ICommand ExportSoundCommand { get; set; }
    public ICommand ReplaceSoundCommand { get; set; }
    public ICommand OpenFileCommand { get; set; }
    public ICommand CloseFileCommand { get; set; }
    public ICommand ChangeExportPathCommand { get; set; }
    public ICommand ChangeWargamePathCommand { get; set; }
    public ICommand ChangePythonPathCommand { get; set; }
    public ICommand EditNdfbinCommand { get; set; }
    public ICommand EditTradFileCommand { get; set; }
    public ICommand EditMeshCommand { get; set; }
    public ICommand EditScenarioCommand { get; set; }
    public ICommand PlayMovieCommand { get; set; }
    public ICommand AboutUsCommand { get; set; }
    public ICommand ReplaceRawFromWorkspaceCommand { get; set; }
    public ICommand ReplaceTextureFromWorkspaceCommand { get; set; }
    public ICommand ReplaceSoundFromWorkspaceCommand { get; set; }
    public ICommand OpenEdataFromWorkspaceCommand { get; set; }
    public ICommand AddNewFileCommand { get; set; }

    public ObservableCollection<EdataFileViewModel> OpenFiles { get; } = new();

    public WorkspaceViewModel Workspace
    {
        get; set;
    }

    public GameSpaceViewModel Gamespace
    {
        get; set;
    }

    protected void OpenFilesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Settings set = SettingsManager.Load();
        set.LastOpenedFiles.Clear();
        set.LastOpenedFiles.AddRange(OpenFiles.Select(x => x.LoadedFile).ToList());
        _ = SettingsManager.Save(set);
    }

    public void AddFile(string path)
    {
        EdataFileViewModel vm = new(this);

        vm.LoadFile(path);

        OpenFiles.Add(vm);

        _ = CollectionViewSource.GetDefaultView(OpenFiles).MoveCurrentTo(vm);
    }

    public void CloseFile(EdataFileViewModel vm)
    {
        if (!OpenFiles.Contains(vm))
            return;

        _ = OpenFiles.Remove(vm);
    }

    protected void InitializeCommands()
    {
        OpenFileCommand = new ActionCommand(OpenFileExecute);
        CloseFileCommand = new ActionCommand(CloseFileExecute);

        ChangeExportPathCommand = new ActionCommand(ChangeExportPathExecute);
        ChangeWargamePathCommand = new ActionCommand(ChangeWargamePathExecute);
        ChangePythonPathCommand = new ActionCommand(ChangePythonPathExecute);

        ExportNdfCommand = new ActionCommand(ExportNdfExecute, () => IsOfType(EdataFileType.Ndfbin));
        ExportRawCommand = new ActionCommand(ExportRawExecute);
        ReplaceRawCommand = new ActionCommand(ReplaceRawExecute);
        ExportTextureCommand = new ActionCommand(ExportTextureExecute, () => IsOfType(EdataFileType.Image));
        ReplaceTextureCommand = new ActionCommand(ReplaceTextureExecute, () => IsOfType(EdataFileType.Image));

        ExportSoundCommand = new ActionCommand(ExportSoundExecute, () => HasEnding(".ess"));
        ReplaceSoundCommand = new ActionCommand(ReplaceSoundExecute, () => HasEnding(".ess"));

        PlayMovieCommand = new ActionCommand(PlayMovieExecute);

        AboutUsCommand = new ActionCommand(AboutUsExecute);

        EditTradFileCommand = new ActionCommand(EditTradFileExecute, () => IsOfType(EdataFileType.Dictionary));
        EditNdfbinCommand = new ActionCommand(EditNdfbinExecute, () => IsOfType(EdataFileType.Ndfbin));
        EditMeshCommand = new ActionCommand(EditMeshExecute, () => IsOfType(EdataFileType.Mesh));
        EditScenarioCommand = new ActionCommand(EditScenarioExecute, () => IsOfType(EdataFileType.Scenario));

        AddNewFileCommand = new ActionCommand(AddNewFileExecute);

        ReplaceRawFromWorkspaceCommand = new ActionCommand(ReplaceRawFromWorkspaceExecute);
        ReplaceTextureFromWorkspaceCommand = new ActionCommand(ReplaceTextureFromWorkspaceExecute, () => IsOfType(EdataFileType.Image));
        ReplaceSoundFromWorkspaceCommand = new ActionCommand(ReplaceSoundFromWorkspaceExecute, () => HasEnding(".ess"));
    }

    private void AddNewFileExecute(object obj)
    {
        if (obj is FileViewModel)
        {
            FileViewModel file = obj as FileViewModel;

            HandleNewFile(file.Info.FullName);
        }
    }

    private void EditScenarioExecute(object obj)
    {
        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile scenario)
            return;

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        Action<ViewModelBase, ViewModelBase> open = DialogProvider.ProvideView;
        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
        {
            try
            {
                _ = dispatcher.Invoke(() => IsUIBusy = true);
                _ = dispatcher.Invoke(report, "Reading scenario...");

                ScenarioEditorViewModel detailsVm = new(scenario, vm);

                _ = dispatcher.Invoke(open, detailsVm, this);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
            }
            finally
            {
                _ = dispatcher.Invoke(() => IsUIBusy = false);
                _ = dispatcher.Invoke(report, "Ready");
            }
        });

        s.Start();
    }

    private void EditMeshExecute(object obj)
    {
        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile mesh)
            return;

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        Action<ViewModelBase, ViewModelBase> open = DialogProvider.ProvideView;
        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
        {
            try
            {
                _ = dispatcher.Invoke(() => IsUIBusy = true);
                _ = dispatcher.Invoke(report, "Reading Mesh package...");

                MeshReader reader = new();
                Model.Mesh.MeshFile meshfile = reader.Read(vm.EdataManager.GetRawData(mesh));

                MeshEditorViewModel detailsVm = new(meshfile);

                _ = dispatcher.Invoke(open, detailsVm, this);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
            }
            finally
            {
                _ = dispatcher.Invoke(() => IsUIBusy = false);
                _ = dispatcher.Invoke(report, "Ready");
            }
        });

        s.Start();
    }

    private void ReplaceRawExecute(object obj)
    {
        Settings settings = SettingsManager.Load();

        OpenFileDialog openfDlg = new()
        {
            //DefaultExt = ".*",
            Multiselect = false,
            Filter = "All files (*.*)|*.*"
        };

        if (File.Exists(settings.LastOpenFolder))
            openfDlg.InitialDirectory = settings.LastOpenFolder;

        if (openfDlg.ShowDialog().Value)
        {
            settings.LastOpenFolder = new FileInfo(openfDlg.FileName).DirectoryName;
            _ = SettingsManager.Save(settings);

            ReplaceRawFile(File.ReadAllBytes(openfDlg.FileName));
        }
    }

    protected void ReplaceTextureExecute(object obj)
    {
        Settings settings = SettingsManager.Load();

        OpenFileDialog openfDlg = new()
        {
            DefaultExt = ".dds",
            Multiselect = false,
            Filter = "DDS files (.dds)|*.dds"
        };

        if (File.Exists(settings.LastOpenFolder))
            openfDlg.InitialDirectory = settings.LastOpenFolder;

        if (openfDlg.ShowDialog().Value)
        {
            settings.LastOpenFolder = new FileInfo(openfDlg.FileName).DirectoryName;
            _ = SettingsManager.Save(settings);

            ReplaceTextureFile(openfDlg.FileName);
        }
    }

    private void ReplaceSoundExecute(object obj)
    {
        Settings settings = SettingsManager.Load();

        OpenFileDialog openfDlg = new()
        {
            DefaultExt = ".wav",
            Multiselect = false,
            Filter = "WAV files (.wav)|*.wav"
        };

        if (File.Exists(settings.LastOpenFolder))
            openfDlg.InitialDirectory = settings.LastOpenFolder;

        if (openfDlg.ShowDialog().Value)
        {
            settings.LastOpenFolder = new FileInfo(openfDlg.FileName).DirectoryName;
            _ = SettingsManager.Save(settings);

            ReplaceSoundFile(openfDlg.FileName);
        }
    }

    private void ReplaceRawFromWorkspaceExecute(object obj)
    {
        string file = obj.ToString();

        if (File.Exists(file))
        {
            ReplaceRawFile(File.ReadAllBytes(file));
        }
    }

    private void ReplaceTextureFromWorkspaceExecute(object obj)
    {
        string file = obj.ToString();

        if (File.Exists(file))
        {
            ReplaceTextureFile(file);
        }
    }

    private void ReplaceSoundFromWorkspaceExecute(object obj)
    {
        string file = obj.ToString();

        if (File.Exists(file))
        {
            ReplaceSoundFile(file);
        }
    }

    private void ReplaceRawFile(byte[] newFileData)
    {
        EdataFileViewModel vm = CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem as EdataFileViewModel;

        if (vm?.FilesCollectionView.CurrentItem is not EdataContentFile file)
            return;

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
        {
            try
            {
                _ = dispatcher.Invoke(() => IsUIBusy = true);
                _ = dispatcher.Invoke(report, $"Replacing {file.Path}...");

                vm.EdataManager.ReplaceFile(file, newFileData);
                vm.LoadFile(vm.LoadedFile);

                _ = dispatcher.Invoke(report, "Ready");
            }
            catch (Exception ex)
            {
                _ = dispatcher.Invoke(report, $"Replacing failed {ex.Message}");
                Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
            }
            finally
            {
                _ = dispatcher.Invoke(() => IsUIBusy = false);
            }
        });

        s.Start();
    }

    protected void ReplaceTextureFile(string newFilePath)
    {
        EdataFileViewModel vm = CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem as EdataFileViewModel;

        if (vm?.FilesCollectionView.CurrentItem is not EdataContentFile destTgvFile)
            return;

        TgvReader tgvReader = new();
        byte[] data = vm.EdataManager.GetRawData(destTgvFile);
        Model.Textures.TgvFile tgv = tgvReader.Read(data);

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
        {
            try
            {
                _ = dispatcher.Invoke(() => IsUIBusy = true);
                _ = dispatcher.Invoke(report, $"Replacing {destTgvFile.Path}...");

                byte[] sourceDds = File.ReadAllBytes(newFilePath);

                _ = dispatcher.Invoke(report, "Converting DDS to TGV file format...");

                TgvDDSReader ddsReader = new();
                Model.Textures.TgvFile sourceTgvFile = ddsReader.ReadDDS(sourceDds);
                byte[] sourceTgvRawData;

                using (MemoryStream tgvwriterStream = new())
                {
                    TgvWriter tgvWriter = new();
                    tgvWriter.Write(tgvwriterStream, sourceTgvFile, tgv.SourceChecksum, tgv.IsCompressed);
                    sourceTgvRawData = tgvwriterStream.ToArray();
                }

                _ = dispatcher.Invoke(report, "Replacing file in edata container...");

                vm.EdataManager.ReplaceFile(destTgvFile, sourceTgvRawData);

                vm.LoadFile(vm.LoadedFile);
                _ = dispatcher.Invoke(report, "Ready");
            }
            catch (Exception ex)
            {
                _ = dispatcher.Invoke(report, $"Replacing failed {ex.Message}");
                Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
            }
            finally
            {
                _ = dispatcher.Invoke(() => IsUIBusy = false);
            }
        });

        s.Start();
    }

    protected void ReplaceSoundFile(string newFilePath)
    {
        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile file)
            return;

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
        {
            try
            {
                _ = dispatcher.Invoke(() => IsUIBusy = true);
                _ = dispatcher.Invoke(report, $"Replacing {file.Path}...");
                byte[] replacefile = File.ReadAllBytes(newFilePath);

                EssWriter writer = new();

                try
                {
                    replacefile = writer.Write(replacefile);
                    vm.EdataManager.ReplaceFile(file, replacefile);
                    vm.LoadFile(vm.LoadedFile);
                    _ = dispatcher.Invoke(report, "Ready");
                }
                catch (InvalidDataException ex)
                {
                    _ = dispatcher.Invoke(report, ex.Message);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
            }
            finally
            {
                _ = dispatcher.Invoke(() => IsUIBusy = false);
            }
        });

        s.Start();
    }

    protected void ExportTextureExecute(object obj)
    {
        EdataFileViewModel vm = CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem as EdataFileViewModel;

        if (vm?.FilesCollectionView.CurrentItem is not EdataContentFile sourceTgvFile)
            return;

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
        {
            try
            {
                _ = dispatcher.Invoke(() => IsUIBusy = true);

                Settings settings = SettingsManager.Load();

                FileInfo f = new(sourceTgvFile.Path);
                string exportPath = Path.Combine(settings.SavePath, f.Name + ".dds");

                _ = dispatcher.Invoke(report, string.Format("Exporting to {0}...", exportPath));

                TgvReader tgvReader = new();
                Model.Textures.TgvFile tgv = tgvReader.Read(vm.EdataManager.GetRawData(sourceTgvFile));

                TgvDDSWriter writer = new();

                byte[] content = writer.CreateDDSFile(tgv);

                using FileStream fs = new(Path.Combine(settings.SavePath, f.Name + ".dds"), FileMode.OpenOrCreate);
                fs.Write(content, 0, content.Length);
                fs.Flush();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
            }
            finally
            {
                _ = dispatcher.Invoke(report, "Ready");
                _ = dispatcher.Invoke(() => IsUIBusy = false);
            }
        });

        s.Start();
    }

    private void ExportSoundExecute(object obj)
    {
        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile sourceEssFile)
            return;

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
        {
            try
            {
                _ = dispatcher.Invoke(() => IsUIBusy = true);

                Settings settings = SettingsManager.Load();

                FileInfo f = new(sourceEssFile.Path);
                string exportPath = Path.Combine(settings.SavePath, f.Name + ".wav");

                _ = dispatcher.Invoke(report, string.Format("Exporting to {0}...", exportPath));

                EssReader tgvReader = new();
                byte[] tgv = tgvReader.ReadEss(vm.EdataManager.GetRawData(sourceEssFile));

                using FileStream fs = new(Path.Combine(settings.SavePath, f.Name + ".wav"), FileMode.OpenOrCreate);
                fs.Write(tgv, 0, tgv.Length);
                fs.Flush();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
            }
            finally
            {
                _ = dispatcher.Invoke(report, "Ready");
                _ = dispatcher.Invoke(() => IsUIBusy = false);
            }
        });

        s.Start();
    }

    protected bool IsOfType(EdataFileType type)
    {
        EdataFileViewModel vm = CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem as EdataFileViewModel;

        EdataContentFile ndf = vm?.FilesCollectionView.CurrentItem as EdataContentFile;

        return ndf?.FileType == type;
    }

    protected bool HasEnding(string ending)
    {
        EdataFileViewModel vm = CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem as EdataFileViewModel;

        return vm?.FilesCollectionView.CurrentItem is EdataContentFile ndf && ndf.Name.EndsWith(ending);
    }

    protected void EditTradFileExecute(object obj)
    {
        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile ndf)
            return;

        TradFileViewModel tradVm = new(ndf, vm);

        DialogProvider.ProvideView(tradVm, this);
    }

    protected void EditNdfbinExecute(object obj)
    {
        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile ndf)
            return;

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        Action<ViewModelBase, ViewModelBase> open = DialogProvider.ProvideView;
        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
            {
                try
                {
                    _ = dispatcher.Invoke(() => IsUIBusy = true);
                    _ = dispatcher.Invoke(report, "Decompiling ndf binary...");

                    NdfEditorMainViewModel detailsVm = new(ndf, vm);
                    _ = dispatcher.Invoke(open, detailsVm, this);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
                }
                finally
                {
                    _ = dispatcher.Invoke(() => IsUIBusy = false);
                    _ = dispatcher.Invoke(report, "Ready");
                }
            });

        s.Start();

    }

    protected void ExportNdfExecute(object obj)
    {
        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile ndf)
            return;

        Settings settings = SettingsManager.Load();

        byte[] content = new NdfbinReader().GetUncompressedNdfbinary(vm.EdataManager.GetRawData(ndf));

        FileInfo f = new(ndf.Path);

        using FileStream fs = new(Path.Combine(settings.SavePath, f.Name), FileMode.OpenOrCreate);
        fs.Write(content, 0, content.Length);
        fs.Flush();
    }

    protected void ExportRawExecute(object obj)
    {
        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile ndf)
            return;

        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        Action<string> report = msg => StatusText = msg;

        Task s = new(() =>
        {
            try
            {
                _ = dispatcher.Invoke(() => IsUIBusy = true);

                Settings settings = SettingsManager.Load();

                FileInfo f = new(ndf.Path);

                string exportFullName = Path.Combine(settings.SavePath, settings.ExportWithFullPath ? ndf.Path : f.Name);
                string exportDir = Path.GetDirectoryName(exportFullName);

                if (!Directory.Exists(exportDir))
                    _ = Directory.CreateDirectory(exportDir);

                _ = dispatcher.Invoke(report, string.Format("Exporting to {0}...", exportFullName));

                byte[] buffer = vm.EdataManager.GetRawData(ndf);

                using FileStream fs = new(exportFullName, FileMode.OpenOrCreate);
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unhandeled exception in Thread occoured: {0}", ex.ToString());
            }
            finally
            {
                _ = dispatcher.Invoke(report, "Ready");
                _ = dispatcher.Invoke(() => IsUIBusy = false);
            }
        });

        s.Start();
    }

    protected void ExportAll()
    {
        //foreach (var file in Files)
        //{
        //    var f = new FileInfo(file.Path);

        //    var dirToCreate = Path.Combine("c:\\temp\\", f.DirectoryName);

        //    if (!Directory.Exists(dirToCreate))
        //        Directory.CreateDirectory(dirToCreate);

        //    var buffer = NdfManager.GetRawData(file);
        //    using (var fs = new FileStream(Path.Combine(dirToCreate, f.Name), FileMode.OpenOrCreate))
        //    {
        //        fs.Write(buffer, 0, buffer.Length);
        //        fs.Flush();
        //    }

        //}
    }

    protected void ChangeExportPathExecute(object obj)
    {
        Settings settings = SettingsManager.Load();

        FolderBrowserDialog folderDlg = new()
        {
            SelectedPath = settings.SavePath,
            //RootFolder = Environment.SpecialFolder.MyComputer,
            ShowNewFolderButton = true,
        };

        if (folderDlg.ShowDialog() == DialogResult.OK)
        {
            settings.SavePath = folderDlg.SelectedPath;
            _ = SettingsManager.Save(settings);
        }
    }

    protected void ChangeWargamePathExecute(object obj)
    {
        Settings settings = SettingsManager.Load();

        FolderBrowserDialog folderDlg = new()
        {
            SelectedPath = settings.WargamePath,
            //RootFolder = Environment.SpecialFolder.MyComputer,
            ShowNewFolderButton = true,
        };

        if (folderDlg.ShowDialog() == DialogResult.OK)
        {
            settings.WargamePath = folderDlg.SelectedPath;
            _ = SettingsManager.Save(settings);
        }
    }

    private void ChangePythonPathExecute(object obj)
    {
        Settings settings = SettingsManager.Load();

        FolderBrowserDialog folderDlg = new()
        {
            SelectedPath = settings.PythonPath,
            RootFolder = Environment.SpecialFolder.MyComputer,
            ShowNewFolderButton = true,
        };

        if (folderDlg.ShowDialog() == DialogResult.OK)
        {
            settings.PythonPath = folderDlg.SelectedPath;
            _ = SettingsManager.Save(settings);
        }
    }

    protected void OpenFileExecute(object obj)
    {
        Settings settings = SettingsManager.Load();

        OpenFileDialog openfDlg = new()
        {
            DefaultExt = ".dat",
            Multiselect = true,
            Filter = "Edat (.dat)|*.dat|All Files|*.*"
        };

        if (File.Exists(settings.LastOpenFolder))
            openfDlg.InitialDirectory = settings.LastOpenFolder;

        if (openfDlg.ShowDialog().Value)
        {
            settings.LastOpenFolder = new FileInfo(openfDlg.FileName).DirectoryName;
            _ = SettingsManager.Save(settings);
            foreach (string fileName in openfDlg.FileNames)
            {
                HandleNewFile(fileName);
            }
        }
    }

    private void HandleNewFile(string fileName)
    {
        EdataFileType type;

        using (FileStream fs = new(fileName, FileMode.Open))
        {
            byte[] headerBuffer = new byte[12];
            _ = fs.Read(headerBuffer, 0, headerBuffer.Length);

            type = EdataManager.GetFileTypeFromHeaderData(headerBuffer);

            if (type == EdataFileType.Ndfbin)
            {
                byte[] buffer = new byte[fs.Length];

                _ = fs.Seek(0, SeekOrigin.Begin);
                _ = fs.Read(buffer, 0, buffer.Length);

                NdfEditorMainViewModel detailsVm = new(buffer);

                NdfbinView view = new()
                { DataContext = detailsVm };

                view.Show();
            }
        }

        if (type == EdataFileType.Package)
            AddFile(fileName);
    }

    protected void CloseFileExecute(object obj) => CloseFile(CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem as EdataFileViewModel);

    protected void PlayMovieExecute(object obj)
    {
        const string name = "temp.wmv";

        if (CollectionViewSource.GetDefaultView(OpenFiles).CurrentItem is not EdataFileViewModel vm)
            return;

        if (vm.FilesCollectionView.CurrentItem is not EdataContentFile ndf)
            return;

        Settings settings = SettingsManager.Load();

        byte[] buffer = vm.EdataManager.GetRawData(ndf);

        //var f = new FileInfo(ndf.Path);

        using (FileStream fs = new(Path.Combine(settings.SavePath, name), FileMode.OpenOrCreate))
        {
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }

        MoviePlaybackViewModel detailsVm = new(Path.Combine(settings.SavePath, name));

        MoviePlaybackView view = new()
        { DataContext = detailsVm };

        view.Show();
    }

    protected void AboutUsExecute(object obj) => DialogProvider.ProvideView(new AboutViewModel(), this);
}