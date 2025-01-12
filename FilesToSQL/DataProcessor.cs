using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FilesToSQL
{
    public class DataProcessor
    {
        public IList SeletedFolderItem { get; set; }
        public List<string> allFiles {  get; set; }
        public bool? IsMSSQL { get; internal set; }
        public bool? IsSQLite { get; internal set; }

        public SQLiteDBStore sQLiteDBStore { get; set; } = new SQLiteDBStore();
        public MSSQLDBStore msSQLDBStore { get; set; } = new MSSQLDBStore();

        public DataProcessor()
        {
            //allFiles.Add = Directory.GetFiles("folderPath", "*" , SearchOption.AllDirectories);
        }

        internal void ExecuteProcess()
        {

        }

        internal bool ValidateStorage()
        {
            bool isStorageSelected = IsMSSQL == true || IsSQLite == true;
            if (!isStorageSelected)
            {
                MessageBox.Show("Select Storage Method.");
                return false;
            }

            if (IsMSSQL == true)
            {
            }

            if (IsSQLite == true)
            {
            }

            return true;
        }
    }
}