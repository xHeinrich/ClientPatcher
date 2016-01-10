using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Collections.ObjectModel;
using GryffiLib;
using System.ComponentModel;

namespace PatchCreator
{
    static class stuffToTry
    {
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<Directories> patchlist = new List<Directories>();

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                GryffiLib.Gryffi.DeserializePatchlist();
            }
            catch (Exception)
            {
                MessageBox.Show("list.txt could not be found.");
            }
            DataContext = new DirectoryViewModel();

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt";
            Nullable<bool> result = dlg.ShowDialog();
            string filename = "";

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                filename = dlg.FileName.Replace("list.txt", null);
                try
                {
                    Gryffi.SCurrentDirectory = filename;
                    Gryffi.GryffiPatchlist = null;
                    GryffiLib.Gryffi.DeserializePatchlist();
                    DataContext = new DirectoryViewModel();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "list"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                Gryffi.SCurrentDirectory = dlg.FileName.Replace("list.txt", null);
                Gryffi.CreatePatchlist(Gryffi.GryffiPatchlist.Version, false);
            }
        }
    }
}
