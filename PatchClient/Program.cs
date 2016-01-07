using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
namespace PatchClient
{
    [Serializable()]
    class Patchlist
    {
        public enum Channel
        {
            Dev,
            Beta,
            Release
        }

        public int Version { get; set;}
        public string DownloadDirectory { get; set; }
        public List<Directories> Directories = new List<PatchClient.Directories>();
        public List<Files> Files = new List<PatchClient.Files>();
        public Channel VersionChannel = new Channel();
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
    class Program
    {
        //Patchlist object
        static Patchlist LPatchlist = new Patchlist();
        //Computer directories
        static string SCurrentDirectory = Directory.GetCurrentDirectory() + "\\";
        static string SPatchlistName = "list.txt";
        static string SPatchlisChant = SCurrentDirectory + SPatchlistName;
        //Server urls
        static string SPatchServer = "http://www.nathan-dev.com/projects/patchclient/";
        static string SVersionChannel = Enum.GetName(typeof(Patchlist.Channel), LPatchlist.VersionChannel);
        static string SPatchServerUrl = SPatchServer + LPatchlist.DownloadDirectory + SVersionChannel + "/" +  LPatchlist.Version.ToString() + "/";
        //Ftp Credentials
        static string SPassword;
        static string SUsername;
        static string SUrl;
        //Other stuff
        static string GzipExtension = ".gz";
        //Patch Folder(folder all patch files will copy to)
        static string SPatchFolder = @"C:\Users\Administrator\Documents\visual studio 2015\Projects\PatchClient\PatchClient\bin\Patch\";


        static void Main(string[] args)
        {
            //FTP STUFF
            FtpLib.password = SPassword;
            FtpLib.username = SUsername;
            FtpLib.url = SUrl;
            //If being patcher
           // ReadPatchlist();
            //Patch();
            //CreateRemoteFolders();
            Console.WriteLine(SPatchServerUrl);
            //If being patchlist creator
            CreatePatchlist(2, true);
            Console.ReadLine();
        }
        public static void CheckFiles()
        {

        }

        public static void ReadPatchlist()
        {
            Stream stream = File.Open(SPatchlistName, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            Console.WriteLine("Reading Patchlist");
            LPatchlist = (Patchlist)bformatter.Deserialize(stream);
            stream.Close();
            Console.WriteLine(String.Format("{0} Directories and {1} Files loaded", LPatchlist.Directories.Count, LPatchlist.Files.Count));
        }
        public static void Patch()
        {
            Console.WriteLine("Starting Patch");
            foreach(var directory in LPatchlist.Directories)
            {
                if(directory.DirectoryName != "")
                {
                    Directory.CreateDirectory(directory.DirectoryName);
                }
            }
            foreach(var file in LPatchlist.Files)
            {
                if(File.Exists(SCurrentDirectory + file.Filename))
                {
                    if(CheckMD5(SCurrentDirectory + file.Filename) != file.Md5Hash)
                    {
                        Console.WriteLine("Patch file: " + file.Filename);
                        Console.WriteLine("Downloading file: " + SPatchServerUrl + file.Filename + GzipExtension);
                        Console.WriteLine("Decompressing file: " + file.Filename + GzipExtension);
                    }
                }
            }
            RemoveUneededFiles();
        }
        public static void RemoveUneededFiles()
        {
            Console.WriteLine("Removing files");
            foreach(var directory in LPatchlist.Directories)
            {
                if(directory.CheckFiles == true)
                {
                    foreach (var file in Directory.GetFiles(SCurrentDirectory + directory.DirectoryName))
                    {
                        bool IsFileInClient = false;
                        foreach (var pfile in LPatchlist.Files)
                        {
                            if (pfile.Filename == file.Replace(SCurrentDirectory, null))
                            {
                                IsFileInClient = true;
                            }
                        }
                        if (IsFileInClient == false)
                        {
                            Console.WriteLine("Deleting file " + file);
                        }
                    }
                }
            }
            
        }
        public static void CreatePatchlist(int version, bool autoUpload = false)
        {
            LPatchlist.Version = version;
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
            if(autoUpload == true)
            {
                foreach (var file in LPatchlist.Files)
                {
                    GZipLib.Compress(file.Filename);
                }
                CreateRemoteFolders();
                UploadUpdate();
                CleanPatchCreation();
            }

        }

        public static void CleanPatchCreation()
        {
            foreach(var directory in LPatchlist.Directories)
            {
                string folder = SPatchFolder + "/" + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/" + directory.DirectoryName;
                Directory.CreateDirectory(folder);
                Console.WriteLine(string.Format("Creating dir {0}", folder));
            }
            foreach(var file in LPatchlist.Files)
            {
                string filename = file.Filename.Replace(SCurrentDirectory, null);
                string moveFrom = SCurrentDirectory + file.Filename + GzipExtension;
                string moveTo = SPatchFolder + "/" + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/" + filename + GzipExtension;
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

        public static void CreateRemoteFolders()
        {
            foreach(var directory in LPatchlist.Directories)
            {
                string folder = "/" + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/" + directory.DirectoryName;
                Console.WriteLine("Creating remote folder {0}", folder);
                FtpLib.CreateFolder(folder);
            }
        }
        public static void UploadUpdate()
        {
            foreach(var file in LPatchlist.Files)
            {
                FtpLib.Upload(("/" + SVersionChannel + "/" + LPatchlist.Version.ToString() + "/" + 
                    file.Filename.Replace(SCurrentDirectory, null) + GzipExtension), file.Filename);
            }
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
    }
}
