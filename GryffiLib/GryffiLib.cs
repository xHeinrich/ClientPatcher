using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;

namespace GryffiLib
{
    public enum Channel
    {
        Dev,
        Beta,
        Release
    }

    [Serializable()]
    public class Patchlist
    {
        public int Version { get; set; }
        public string DownloadDirectory { get; set; }
        public List<Directories> Directories = new List<Directories>();
        public List<Files> Files = new List<Files>();
        public Channel VersionChannel;
    }
    [Serializable()]
    public class Files
    {
        public string _filename { get; set; }
        public DateTime _creationDate { get; set; }
        public string _md5Hash { get; set; }

        public string Filename { get { return _filename; }
            set
            {
                _filename = value;
                NotifyPropertyChanged("Filename");
            }
        }
        public DateTime CreationDate
        {
            get { return _creationDate; }
            set
            {
                _creationDate = value;
                NotifyPropertyChanged("CreationDate");
            }
        }
        public string Md5Hash
        {
            get { return _md5Hash; }
            set
            {
                _md5Hash = value;
                NotifyPropertyChanged("Md5Hash");
            }
        }
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Helpers

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
    [Serializable()]
    public class Directories : INotifyPropertyChanged
    {
        public string _directoryName { get; set; }
        public bool _checkFiles { get; set; }

        public string DirectoryName
        {
            get
            { return _directoryName; }
            set
            {
                _directoryName = value;
                NotifyPropertyChanged("DirectoryName");
            }
        }
        public bool CheckFiles
        {
            get
            { return _checkFiles; }
            set
            {
                _checkFiles = value;
                NotifyPropertyChanged("CheckFiles");
            }
        }
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Helpers

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
    public class Gryffi
    {

        /// <summary>
        /// The patchlist object
        /// </summary>
        public static Patchlist GryffiPatchlist = new Patchlist();
        /// <summary>
        /// The current programs directory
        /// </summary>
       public static string SCurrentDirectory = Directory.GetCurrentDirectory() + "\\";
        /// <summary>
        /// The name of the patchlist
        /// </summary>
        static string SPatchlistName = "list.txt";
        /// <summary>
        /// Base patchserver url
        /// </summary>
        public static string PatchServerUrl;
        /// <summary>
        /// If the client is patching a dev, release or beta build
        /// </summary>
        static public string SVersionChannel = Enum.GetName(typeof(Channel), GryffiPatchlist.VersionChannel);
        /// <summary>
        // The url of the current patchlist
        /// </summary>
        static string SPatchServerUrl = PatchServerUrl + GryffiPatchlist.DownloadDirectory + SVersionChannel + "/" + GryffiPatchlist.Version.ToString() + "/";
        /// <summary>
        /// FTP Password
        /// </summary>
        public static string FTPPassword;
        /// <summary>
        /// FTP Username
        /// </summary>
        public static string FTPUsername;
        /// <summary>
        /// Ftp url
        /// </summary>
        public static string FTPUrl;
        /// <summary>
        /// Extension of all gziped archives
        /// </summary>
        static string GzipExtension = ".gz";
        /// <summary>
        /// Folder all patch files will copy to after upload
        /// </summary>
        public static string PatchFolder;
        /// <summary>
        /// Load the patchlist after it is downloaded
        /// </summary>
        public static void DeserializePatchlist()
        {
            string json = File.ReadAllText(SCurrentDirectory + SPatchlistName);
            GryffiPatchlist = JsonConvert.DeserializeObject<Patchlist>(json);
            Console.WriteLine(String.Format("{0} Directories and {1} Files loaded", GryffiPatchlist.Directories.Count, GryffiPatchlist.Files.Count));
        }
        public static void SerializePatchlist()
        {
            var json = JsonConvert.SerializeObject(GryffiPatchlist);
            File.WriteAllText(SCurrentDirectory + SPatchlistName, json);
        }
        public static void PopulatePatchList()
        {

            foreach (var file in Directory.GetFiles(SCurrentDirectory))
            {
                string filename = file.Replace(SCurrentDirectory, null);
                //if (filename != ExecutableName && filename != "GryffiLib.dll")
                //{
                GryffiPatchlist.Files.Add(new Files
                {
                    _filename = filename,
                    _creationDate = File.GetCreationTime(file),
                    _md5Hash = CheckMD5(file)
                });
                //}

            }
            //Add Root Directory
            GryffiPatchlist.Directories.Add(new Directories
            {
                _directoryName = "",
                CheckFiles = true
            });
            foreach (var directory in Directory.GetDirectories(SCurrentDirectory))
            {
                GryffiPatchlist.Directories.Add(new Directories
                {
                    _directoryName = directory.Replace(SCurrentDirectory, null),
                    CheckFiles = true
                });
                foreach (var file in Directory.GetFiles(directory))
                {
                    GryffiPatchlist.Files.Add(new Files
                    {
                        _filename = file.Replace(SCurrentDirectory, null),
                        _creationDate = File.GetCreationTime(file),
                        _md5Hash = CheckMD5(file)
                    });
                }
            }
        }
        /// <summary>
        /// Creates a patchlist from the LPatchlist object
        /// </summary>
        /// <param name="version">The version of the client</param>
        /// <param name="autoUpload">If after the patchlist is created should it be uploaded with all files.</param>
        public static void CreatePatchlist(int version, bool populateList, bool autoUpload = false, Channel channel = Channel.Dev)
        {
            FtpLib.url = FTPUrl;
            FtpLib.username = FTPUsername;
            FtpLib.password = FTPPassword;
            GryffiPatchlist.VersionChannel = channel;
            GryffiPatchlist.Version = version;
            SVersionChannel = Enum.GetName(typeof(Channel), GryffiPatchlist.VersionChannel);

            if(populateList == true)
            {
                PopulatePatchList();
            }
            SerializePatchlist();

            if (autoUpload == true)
            {
                foreach (var file in GryffiPatchlist.Files)
                {
                    GZipLib.Compress(file._filename);
                }
                CreateRemoteFolders();
                UploadUpdate();
                CleanPatchCreation();
            }
            Console.WriteLine("Patch creation finished!");
        }
        /// <summary>
        /// Create an md5 hash for a file
        /// </summary>
        /// <param name="filename">File you want to check the md5 hash on</param>
        /// <returns></returns>
        public static string CheckMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        /// <summary>
        /// Move all .gz files to the update folder on the local machine
        /// </summary>
        public static void CleanPatchCreation()
        {
            foreach (var directory in GryffiPatchlist.Directories)
            {
                string folder = PatchFolder + "/" + SVersionChannel + "/" + GryffiPatchlist.Version.ToString() + "/" + directory._directoryName;
                Directory.CreateDirectory(folder);
                Console.WriteLine(string.Format("Creating dir {0}", folder));
            }
            foreach (var file in GryffiPatchlist.Files)
            {
                string filename = file._filename.Replace(SCurrentDirectory, null);
                string moveFrom = SCurrentDirectory + file._filename + GzipExtension;
                string moveTo = PatchFolder + "/" + SVersionChannel + "/" + GryffiPatchlist.Version.ToString() + "/" + filename + GzipExtension;
                if (File.Exists(moveTo))
                {
                    File.Delete(moveFrom);
                    Console.WriteLine(string.Format("File {0} already exists, deleting.", moveFrom));
                }
                else
                {
                    File.Move(moveFrom, moveTo);
                    Console.WriteLine(String.Format("Moving file {0} to {1}", moveFrom, moveTo));
                }
            }
            Console.WriteLine("Moving patchlist");
            File.Move(SCurrentDirectory + SPatchlistName, PatchFolder + "/" + SVersionChannel + "/" + GryffiPatchlist.Version.ToString() + "/" + SPatchlistName);
        }

        /// <summary>
        /// Upload all the .gz files to the update server
        /// </summary>
        public static void UploadUpdate()
        {
            string remoteDirectory = "/" + SVersionChannel + "/" + GryffiPatchlist.Version.ToString() + "/";

            foreach (var file in GryffiPatchlist.Files)
            {
                FtpLib.Upload(( remoteDirectory + file._filename.Replace(SCurrentDirectory, null) + GzipExtension), file._filename);
            }
            FtpLib.Upload(remoteDirectory + SPatchlistName, SCurrentDirectory + SPatchlistName);
        }

        /// <summary>
        /// Create the patchlist folders on the ftp server as per update
        /// </summary>
        public static void CreateRemoteFolders()
        {
            foreach (var directory in GryffiPatchlist.Directories)
            {
                string folder = "/" + SVersionChannel + "/" + GryffiPatchlist.Version.ToString() + "/" + directory._directoryName;
                Console.WriteLine("Creating remote folder {0}", folder);
                FtpLib.CreateFolder(folder);
            }
        }
    }
}
