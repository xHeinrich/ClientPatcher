using System;
using GryffiLib;
namespace PatchClient
{
 
    class Program
    {


        static void Main(string[] args)
        {
            // Variables needed to create patch

            /*
            Gryffi.FTPPassword = "";
            Gryffi.FTPUsername = "";
            Gryffi.FTPUrl = "";
            Gryffi.PatchServerUrl = "";
            Gryffi.PatchFolder = @"C:\Users\Administrator\Documents\visual studio 2015\Projects\PatchClient\PatchClient\bin\Patch";
            */
            Gryffi.FTPPassword = "test1234.";
            Gryffi.FTPUsername = "patchclient@nathan-dev.com";
            Gryffi.FTPUrl = "ftp://nathan-dev.com";
            Gryffi.PatchServerUrl = "http://www.nathan-dev.com/projects/patchclient/";
            Gryffi.PatchFolder = @"C:\Users\Administrator\Documents\visual studio 2015\Projects\PatchClient\PatchClient\bin\Patch";
            
            //If being patcher
            // ReadPatchlist();
            //Patch();
            //CreateRemoteFolders();
            //If being patchlist creator
            Gryffi.CreatePatchlist(9, true, true, Channel.Release);
            Console.ReadLine();
        }
        
    }
}
