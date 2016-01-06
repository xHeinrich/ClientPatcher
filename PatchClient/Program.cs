using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace PatchClient
{
    [Serializable()]
    class Patchlist
    {
        public int Version { get; set;}
        public List<string> Directories = new List<string>();
        public List<Files> Files = new List<PatchClient.Files>();
    }
    [Serializable()]
    class Files
    {
        public string Filename { get; set; }
        public DateTime CreationDate { get; set; }
        public string Md5Hash { get; set; }
    }
    class Program
    {
        static Patchlist LPatchlist = new Patchlist();

        static string SCurrentDirectory = Directory.GetCurrentDirectory() + "\\";
        static string SPatchlistName = "list.txt";
        static string SPatchlist = SCurrentDirectory + SPatchlistName;


        static void Main(string[] args)
        {
            ReadPatchlist();
            Patch();
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
            foreach(var dir in LPatchlist.Directories)
            {
                    Directory.CreateDirectory(dir);
            }
            foreach(var file in LPatchlist.Files)
            {
                if(File.Exists(SCurrentDirectory + file.Filename))
                {
                    if(CheckMD5(SCurrentDirectory + file.Filename) != file.Md5Hash)
                    {
                        Console.WriteLine("Patch file: " + file.Filename);
                    }
                }
            }
            RemoveUneededFiles();
        }
        public static void RemoveUneededFiles()
        {
            Console.WriteLine("Removing files");
            foreach(var file in Directory.GetFiles(SCurrentDirectory))
            {
                bool IsFileInClient = false;
                foreach(var pfile in LPatchlist.Files)
                {
                    if(pfile.Filename == file.Replace(SCurrentDirectory, null))
                    {
                        IsFileInClient = true;
                    }
                }
                if(IsFileInClient == false)
                {
                    Console.WriteLine("Deleting file " + file);
                }
            }
        }
        public static void CreatePatchlist(int version)
        {
            if (File.Exists(SPatchlist))
            {
                File.Delete(SPatchlist);
            }
            foreach (var file in Directory.GetFiles(SCurrentDirectory))
            {
                LPatchlist.Files.Add(new Files
                {
                    Filename = file.Replace(SCurrentDirectory, null),
                    CreationDate = File.GetCreationTime(file),
                    Md5Hash = CheckMD5(file)
                });
            }
            foreach (var directory in Directory.GetDirectories(SCurrentDirectory))
            {
                LPatchlist.Directories.Add(directory);
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
