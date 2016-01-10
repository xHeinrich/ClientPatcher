using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using GryffiLib;

namespace PatchCreator
{
    public class DirectoryViewModel
    {
        public ICollectionView DirectoryView { get; private set; }
        public ICollectionView FileView { get; private set; }
        public ICollectionView VersionNumber { get; private set; }

        public DirectoryViewModel()
        {
            FileView = CollectionViewSource.GetDefaultView(Gryffi.GryffiPatchlist.Files);
            DirectoryView = CollectionViewSource.GetDefaultView(Gryffi.GryffiPatchlist.Directories);
        }
    }
}
