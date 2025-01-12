using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace FilesToSQL
{
    public class DataProcessor
    {
        public IList SeletedFolderItem { get; set; }
        public List<string> allFiles {  get; set; }
        public bool? IsMSSQL { get; internal set; }
        public bool? IsSQLite { get; internal set; }

        public SQLiteDBStore SQLiteDBStoreObj { get; set; } = new SQLiteDBStore();
        public MSSQLDBStore MSSQLDBStoreObj { get; set; } = new MSSQLDBStore();

        public DataProcessor() { }

        internal void ExecuteStorageProcess()
        {
            if (IsMSSQL == true)
            {
                MSSQLDBStoreObj.SetConnectionString();
            }

            if (IsSQLite == true)
            {
                if (SQLiteDBStoreObj.SetConnectionString())
                {
                    SQLiteDBStoreObj.CreateSQLiteTable();
                    SQLiteDBStoreObj.ExcuteAction();
                }
            }
        }

        internal void ExecuteRetrivalProcess()
        {
            if (IsMSSQL == true)
            {

            }

            if (IsSQLite == true)
            {
                if (SQLiteDBStoreObj.SetConnectionStringForRetrival())
                {
                    SQLiteDBStoreObj.RetrieveAllFilesFromDatabase();
                }
            }
        }

        internal bool ValidateStorage()
        {
            bool isStorageSelected = IsMSSQL == true || IsSQLite == true;
            if (!isStorageSelected)
            {
                MessageBox.Show("Select Storage Method.");
                return false;
            }

            List<string> filePaths = new List<string>(); 
            foreach (var item in SeletedFolderItem)
            {
                if (item is FileItem fileItem && fileItem.Type == "Folder")
                {
                    // Retrieve all file paths within the folder and subfolders
                    var folderFiles = Directory.GetFiles(fileItem.Path, "*", SearchOption.AllDirectories);

                    // Add the file paths to the list
                    filePaths.AddRange(folderFiles);
                }
                else
                {
                    filePaths.Add(((FileItem)item).Path);
                }
            }

            if (IsMSSQL == true)
            {
                MSSQLDBStoreObj.filesToStore = filePaths;
                MSSQLDBStoreObj.SetConnectionString();
            }

            if (IsSQLite == true)
            {
                SQLiteDBStoreObj.filesToStore = filePaths;
            }

            return true;
        }
    }
}