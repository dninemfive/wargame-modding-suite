using moddingSuite.Model.Settings;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace moddingSuite.ViewModel.Edata;

public class WorkspaceViewModel : FileSystemOverviewViewModelBase
{
    private readonly FileSystemWatcher _fileSystemWatcher;

    public WorkspaceViewModel(Settings settings)
    {
        RootPath = settings.SavePath;

        if (!Directory.Exists(RootPath))
            return;

        Root.Add(ParseRoot());

        _fileSystemWatcher = new FileSystemWatcher(RootPath)
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true
        };

        _fileSystemWatcher.Created += FileSystemChanged;
        _fileSystemWatcher.Changed += FileSystemChanged;
        _fileSystemWatcher.Renamed += FileSystemChanged;
        _fileSystemWatcher.Deleted += FileSystemChanged;
    }


    private void FileSystemChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            SyncronizeChange(e);
        }
        catch (Exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Root.Clear();
                Root.Add(ParseRoot());
            });
        }
    }

    private void SyncronizeChange(FileSystemEventArgs e)
    {
        FileAttributes attr = File.GetAttributes(e.FullPath);
        string changedDirName;
        FileSystemItemViewModel newVm;

        if (attr.HasFlag(FileAttributes.Directory))
        {
            DirectoryInfo info = new(e.FullPath);
            newVm = new DirectoryViewModel(info);

            changedDirName = info.Parent.FullName;
        }
        else
        {
            FileInfo info = new(e.FullPath);
            newVm = new FileViewModel(info);

            changedDirName = info.DirectoryName;
        }

        DirectoryViewModel changedDir = GetChangedDirVm(new DirectoryInfo(changedDirName));

        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
            case WatcherChangeTypes.Created:
                if (!changedDir.Items.Any(x => x.Name.Equals(newVm.Name)))
                {
                    Application.Current.Dispatcher.Invoke(() => changedDir.Items.Add(newVm));
                }
                break;
            case WatcherChangeTypes.Deleted:
                Application.Current.Dispatcher.Invoke(
                    () => changedDir.Items.Remove(changedDir.Items.Single(x => x.Name.Equals(newVm.Name))));
                break;
            case WatcherChangeTypes.Renamed:
                RenamedEventArgs renEa = e as RenamedEventArgs;
                string oldName = renEa.OldName.Split('\\').Last();

                FileSystemItemViewModel changedItem = changedDir.Items.Single(x => x.Name.Equals(oldName));

                if (changedItem is DirectoryViewModel)
                    (changedItem as DirectoryViewModel).Info = new DirectoryInfo(renEa.FullPath);
                else
                    (changedItem as FileViewModel).Info = new FileInfo(renEa.FullPath);

                changedItem.Invalidate();
                break;
        }
    }

    protected DirectoryViewModel GetChangedDirVm(DirectoryInfo info)
    {
        DirectoryViewModel changedDir = null;

        foreach (DirectoryViewModel r in Root)
        {
            CheckRecursive(r, info, out changedDir);
        }

        return changedDir;
    }

    protected bool CheckRecursive(DirectoryViewModel dir, DirectoryInfo searchedDir, out DirectoryViewModel find)
    {
        if (dir.Info.FullName.Equals(searchedDir.FullName))
        {
            find = dir;
            return true;
        }

        foreach (DirectoryViewModel item in dir.Items.OfType<DirectoryViewModel>())
        {
            if (item.Info.FullName.Equals(searchedDir.FullName))
            {
                find = item;
                return true;
            }

            DirectoryViewModel possibleFind;

            if (CheckRecursive(item, searchedDir, out possibleFind))
            {
                find = possibleFind;
                return true;
            }
        }

        find = null;

        return false;
    }
}