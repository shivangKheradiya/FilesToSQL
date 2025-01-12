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
        }

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileListView.SelectedItem is FileItem selectedFolder)
            {
                // Retrieve the ViewModel
                var viewModel = DataContext as ExplorerViewModel;

                if (viewModel != null)
                {
                    // Use the ViewModel to load the folder's content
                    viewModel.OpenIfFolderCommand.Execute(selectedFolder);
                }

                if (selectedFolder.Type == "Folder")
                {
                    FolderItem folderItem = (FolderItem)FolderTreeView.SelectedItem;
                    FolderItem seletedFolderItem = folderItem.SubFolders.FirstOrDefault(x => x.Name == selectedFolder.Name);
                    viewModel.LoadChildFoldersCommand.Execute(seletedFolderItem);
                }
            }

        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FolderItem selectedFolder)
            {
                var viewModel = DataContext as ExplorerViewModel;
                viewModel.LoadFilesCommand.Execute(selectedFolder);
                viewModel.LoadChildFoldersCommand.Execute(selectedFolder);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DataProcessor dataProcessor = new DataProcessor();
            dataProcessor.IsMSSQL = buMSSQL.IsChecked;
            dataProcessor.IsSQLite = buSQLite.IsChecked;
            dataProcessor.SeletedFolderItem = FileListView.SelectedItems;
            bool isStorageSelected = dataProcessor.ValidateStorage();
            if (isStorageSelected)
            {
                dataProcessor.ExecuteStorageProcess();
            }
        }

        private void RetriveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hii");
        }
    }
}
