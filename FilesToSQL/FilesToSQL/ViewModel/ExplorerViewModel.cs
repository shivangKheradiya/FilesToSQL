using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using FilesToSQL.Model;
using System.Windows.Input;


namespace FilesToSQL.ViewModel
{
    public class ExplorerViewModel : BaseViewModel
    {
        public ObservableCollection<FolderItem> FolderTree { get; set; }
        public ObservableCollection<FileItem> FileList { get; set; }
        public FolderItem SelectedFolder { get; set; }

        public ICommand LoadFilesCommand { get; set; }

        public ExplorerViewModel()
        {
            FolderTree = new ObservableCollection<FolderItem>();
            FileList = new ObservableCollection<FileItem>();

            LoadFilesCommand = new RelayCommand(LoadFiles);

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
                        Name = drive.Name,
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
    }
}

