using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesToSQL.Model
{
    public class FolderItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsExpanded { get; set; }
        public ObservableCollection<FolderItem> SubFolders { get; set; }

        public FolderItem()
        {
            SubFolders = new ObservableCollection<FolderItem>();
        }
    }
}
