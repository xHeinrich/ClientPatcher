using System;
using System.IO;
using System.Net;
using System.Text;
namespace GryffiLib
{
    static class FtpLib
    {
        public static string username;
        public static string password;
        public static string url;

        public static void CreateFolder(string directory)
        {
            Console.WriteLine(directory);
            WebRequest request = WebRequest.Create(url + directory);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(username, password);
            try
            {
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine(resp.StatusCode);
                }
            }catch(WebException e)
            {
                string status = ((FtpWebResponse)e.Response).StatusDescription;
                Console.WriteLine(status.ToString());
            }

        }
        public static void Upload(string filename, string localfile)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url + filename);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(username, password);

            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader(localfile);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }
    }
}
