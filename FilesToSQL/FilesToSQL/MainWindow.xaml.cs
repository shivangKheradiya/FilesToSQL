using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace FilesToSQL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LoadDrives();
        }

        private void LoadDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    var driveItem = CreateTreeViewItem(drive.Name, drive.Name);
                    FolderTreeView.Items.Add(driveItem);
                }
            }
        }

        // Creates a TreeViewItem with a placeholder for lazy loading
        private TreeViewItem CreateTreeViewItem(string header, string tag)
        {
            var item = new TreeViewItem
            {
                Header = header,
                Tag = tag
            };
            item.Items.Add(null); // Placeholder for lazy loading
            item.Expanded += TreeViewItem_Expanded;
            return item;
        }

        // Expands a folder and loads subdirectories lazily
        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;

            if (item.Items.Count == 1 && item.Items[0] == null) // Check for placeholder
            {
                item.Items.Clear();

                try
                {
                    var path = item.Tag.ToString();
                    var directories = Directory.GetDirectories(path);

                    foreach (var directory in directories)
                    {
                        var subItem = CreateTreeViewItem(Path.GetFileName(directory), directory);
                        item.Items.Add(subItem);
                    }
                }
                catch (Exception)
                {
                    // Handle inaccessible directories or other exceptions silently
                }
            }
        }

        // Event triggered when a TreeView item is selected
        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (FolderTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                var path = selectedItem.Tag.ToString();
                LoadFilesAndFolders(path);
            }
        }

        // Loads files into the ListView
        private void LoadFilesAndFolders(string folderPath)
        {
            FileListView.Items.Clear();

            try
            {
                var directories = Directory.GetDirectories(folderPath);
                foreach (var directory in directories)
                {
                    var dirInfo = new DirectoryInfo(directory);
                    FileListView.Items.Add(new
                    {
                        Name = dirInfo.Name,
                        Type = "Folder",
                        Size = "-",
                        Path = dirInfo.FullName,
                    });
                }

                var files = Directory.GetFiles(folderPath);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    FileListView.Items.Add(new
                    {
                        Name = fileInfo.Name,
                        Type = fileInfo.Extension,
                        Size = fileInfo.Length / 1024 + " KB"
                    });
                }
            }
            catch (Exception)
            {
                // Handle inaccessible files or other exceptions silently
            }
        }

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileListView.SelectedItem != null)
            {
                // Retrieve the selected item
                var selectedItem = FileListView.SelectedItem;

                // Use reflection to get property values
                var typeProperty = selectedItem.GetType().GetProperty("Type")?.GetValue(selectedItem)?.ToString();
                var pathProperty = selectedItem.GetType().GetProperty("Path")?.GetValue(selectedItem)?.ToString();

                // If the item is a folder, navigate to that folder
                if (typeProperty == "Folder" && pathProperty != null)
                {
                    LoadFilesAndFolders(pathProperty);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool isStorageSelected = buMSSQL.IsChecked == true || !buSQLite.IsChecked == true ;
            if (!isStorageSelected)
            {
                MessageBox.Show("Select Storage Method.");
                return;
            }

            if (buMSSQL.IsChecked == true)
            {
                
            }

            if (buSQLite.IsChecked == true)
            {

            }
        }
    }
}
