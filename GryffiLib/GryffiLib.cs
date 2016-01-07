using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace GryffiLib
{
    public enum Channel
    {
        Dev,
        Beta,
        Release
    }
    [Serializable()]
    class Patchlist
    {

        public int Version { get; set; }
        public string DownloadDirectory { get; set; }
        public List<Directories> Directories = new List<Directories>();
        public List<Files> Files = new List<Files>();
        public Channel VersionChannel;
    }
    [Serializable()]
    class Files
    {
        public string Filename { get; set; }
        public DateTime CreationDate { get; set; }
        public string Md5Hash { get; set; }
    }
    [Serializable()]
    class Directories
    {
        public string DirectoryName { get; set; }
        public bool CheckFiles { get; set; }
    }
    public class Gryffi
    {
        /// <summary>
        /// The patchlist object
        /// </summary>
        static Patchlist LPatchlist = new Patchlist();
        /// <summary>
        /// The current programs directory
        /// </summary>
        static string SCurrentDirectory = Directory.GetCurrentDirectory() + "\\";
        /// <summary>
        /// The name of the patchlist
        /// </summary>
        static string SPatchlistName = "list.txt";
        /// <summary>
        /// Location of the patchlist
        /// </summary>
        static string SPatchlistFile = SCurrentDirectory + SPatchlistName;
        /// <summary>
        /// Base patchserver url
        /// </summary>
        public static string PatchServerUrl;
        /// <summary>
        /// If the client is patching a dev, release or beta build
        /// </summary>
        static public string SVersionChannel = Enum.GetName(typeof(Channel), LPatchlist.VersionChannel);
        /// <summary>
        // The url of the current patchlist
        /// </summary>
        static string SPatchServerUrl = PatchServerUrl + LPatchlist.DownloadDirectory + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/";
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
        public static void LoadPatchlist()
        {
            Stream stream = File.Open(SPatchlistName, FileMode.Open);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            Console.WriteLine("Loading Patchlist...");
            LPatchlist = (Patchlist)binaryFormatter.Deserialize(stream);
            stream.Close();
            Console.WriteLine(String.Format("{0} Directories and {1} Files loaded", LPatchlist.Directories.Count, LPatchlist.Files.Count));
        }

        /// <summary>
        /// Creates a patchlist from the LPatchlist object
        /// </summary>
        /// <param name="version">The version of the client</param>
        /// <param name="autoUpload">If after the patchlist is created should it be uploaded with all files.</param>
        public static void CreatePatchlist(int version, bool autoUpload = false, Channel channel = Channel.Dev)
        {
            FtpLib.url = FTPUrl;
            FtpLib.username = FTPUsername;
            FtpLib.password = FTPPassword;
            LPatchlist.VersionChannel = channel;
            LPatchlist.Version = version;
            SVersionChannel = Enum.GetName(typeof(Channel), LPatchlist.VersionChannel);

            foreach (var file in Directory.GetFiles(SCurrentDirectory))
            {
                LPatchlist.Files.Add(new Files
                {
                    Filename = file.Replace(SCurrentDirectory, null),
                    CreationDate = File.GetCreationTime(file),
                    Md5Hash = CheckMD5(file)
                });
            }
            //Add Root Directory
            LPatchlist.Directories.Add(new Directories
            {
                DirectoryName = "",
                CheckFiles = true
            });
            foreach (var directory in Directory.GetDirectories(SCurrentDirectory))
            {
                LPatchlist.Directories.Add(new Directories
                {
                    DirectoryName = directory.Replace(SCurrentDirectory, null),
                    CheckFiles = true
                });
                foreach (var file in Directory.GetFiles(directory))
                {
                    LPatchlist.Files.Add(new Files
                    {
                        Filename = file.Replace(SCurrentDirectory, null),
                        CreationDate = File.GetCreationTime(file),
                        Md5Hash = CheckMD5(file)
                    });
                }
            }
            Stream stream = File.Open(SPatchlistName, FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();
            Console.WriteLine("Writing patchlist");
            Console.WriteLine(String.Format("{0} Directories and {1} Files", LPatchlist.Directories.Count, LPatchlist.Files.Count));
            bformatter.Serialize(stream, LPatchlist);
            stream.Close();
            if (autoUpload == true)
            {
                foreach (var file in LPatchlist.Files)
                {
                    GZipLib.Compress(file.Filename);
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
            foreach (var directory in LPatchlist.Directories)
            {
                string folder = PatchFolder + "/" + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/" + directory.DirectoryName;
                Directory.CreateDirectory(folder);
                Console.WriteLine(string.Format("Creating dir {0}", folder));
            }
            foreach (var file in LPatchlist.Files)
            {
                string filename = file.Filename.Replace(SCurrentDirectory, null);
                string moveFrom = SCurrentDirectory + file.Filename + GzipExtension;
                string moveTo = PatchFolder + "/" + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/" + filename + GzipExtension;
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
        }

        /// <summary>
        /// Upload all the .gz files to the update server
        /// </summary>
        public static void UploadUpdate()
        {
            foreach (var file in LPatchlist.Files)
            {
                FtpLib.Upload(("/" + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/" +
                    file.Filename.Replace(SCurrentDirectory, null) + GzipExtension), file.Filename);
            }
        }

        /// <summary>
        /// Create the patchlist folders on the ftp server as per update
        /// </summary>
        public static void CreateRemoteFolders()
        {
            foreach (var directory in LPatchlist.Directories)
            {
                string folder = "/" + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/" + directory.DirectoryName;
                Console.WriteLine("Creating remote folder {0}", folder);
                FtpLib.CreateFolder(folder);
            }
        }
    }
}
