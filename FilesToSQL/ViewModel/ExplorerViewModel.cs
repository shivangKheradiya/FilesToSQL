using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;


namespace FilesToSQL
{
    public class ExplorerViewModel : BaseViewModel
    {
        public ObservableCollection<FolderItem> FolderTree { get; set; }
        public ObservableCollection<FileItem> FileList { get; set; }
        public FolderItem SelectedFolder { get; set; }
        public ICommand LoadFilesCommand { get; set; }
        public ICommand OpenIfFolderCommand { get; set; }
        public ICommand LoadChildFoldersCommand { get; set; }

        public ExplorerViewModel()
        {
            FolderTree = new ObservableCollection<FolderItem>();
            FileList = new ObservableCollection<FileItem>();

            LoadFilesCommand = new RelayCommand(LoadFiles);
            OpenIfFolderCommand = new RelayCommand(OpenIfFolder);
            LoadChildFoldersCommand = new RelayCommand(LoadChildFolders);

            LoadDrives();
        }

        private void LoadDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    FolderTree.Add(new FolderItem
                    {
                        Name = drive.Name.TrimEnd('\\'), // Fixes issue with extra backslash in drive name (e.g. "C:\")
                        Path = drive.Name,
                        SubFolders = new ObservableCollection<FolderItem>(
                            Directory.GetDirectories(drive.Name)
                                .Select(d => new FolderItem { Name = Path.GetFileName(d), Path = d })
                        )
                    });
                }
            }
        }

        private void LoadFiles(object parameter)
        {
            try
            {
                if (parameter is FolderItem folder)
                {
                    SelectedFolder = folder;
                    FileList.Clear();

                    // Load folders
                    var directories = Directory.GetDirectories(folder.Path);
                    foreach (var directory in directories)
                    {
                        FileList.Add(new FileItem
                        {
                            Name = Path.GetFileName(directory),
                            Path = directory,
                            Type = "Folder",
                            Size = "-"
                        });
                    }

                    // Load files
                    var files = Directory.GetFiles(folder.Path);
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        FileList.Add(new FileItem
                        {
                            Name = fileInfo.Name,
                            Path = file,
                            Type = fileInfo.Extension,
                            Size = fileInfo.Length / 1024 + " KB"
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenIfFolder(object parameter)
        {
            if (parameter is FileItem file)
            {
                // Here, we handle the case where it's a file or a folder
                if (file.Type == "Folder")
                {
                    // If it's a folder, load its files and subdirectories
                    LoadFilesCommand.Execute(new FolderItem { Path = file.Path });
                }
                else
                {
                    try
                    {
                        // For files, we could handle file opening (e.g., using Process.Start)
                        System.Diagnostics.Process.Start(file.Path);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        public void LoadChildFolders(object folderObj)
        {
            try
            {
                FolderItem folder = (FolderItem)folderObj;
                var directories = Directory.GetDirectories(folder.Path);
                foreach (var directory in directories)
                {
                    FolderItem item = new FolderItem
                    {
                        Name = Path.GetFileName(directory),
                        Path = directory
                    };

                    FolderItem folderItem = folder.SubFolders.FirstOrDefault(x => x.Name == item.Name);
                    if (folderItem == null)
                    {
                        folder.SubFolders.Add(item);
                    }
                }

                // Optionally load files too (this could be expanded based on your needs)
                //var files = Directory.GetFiles(folder.Path);
                //foreach (var file in files)
                //{
                //    var fileInfo = new FileInfo(file);
                //    FileList.Add(new FileItem
                //    {
                //        Name = fileInfo.Name,
                //        Path = file,
                //        Type = fileInfo.Extension,
                //        Size = fileInfo.Length / 1024 + " KB"
                //    });
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}

