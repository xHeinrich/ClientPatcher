using System;
using System.IO;
using System.IO.Compression;

namespace PatchClient
{
    class GZipLib
    {
        public static void Compress(string filename)
        {
            FileInfo fileToCompress = new FileInfo(filename);
                using (FileStream originalFileStream = File.OpenRead(filename))
                {
                    if ((File.GetAttributes(fileToCompress.FullName) &
                       FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                    {
                        using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                        {
                            using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                               CompressionMode.Compress))
                            {
                                originalFileStream.CopyTo(compressionStream);

                            }
                        }
                        FileInfo info = new FileInfo(filename + ".gz");
                        Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                        fileToCompress.Name, fileToCompress.Length.ToString(), info.Length.ToString());
                    }                
            }
        }
    }
}
