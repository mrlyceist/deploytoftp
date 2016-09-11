using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeployToFtp
{
    class Program
    {
        private static string _userName = "bit";
        private static string _password = "18071978";
        private static string _host = "ursa.ooobit.com";
        private static string _configuration;
        private static string _outFile;

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Check your arguments!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            _configuration = args[0];
            _outFile       = args[1];
            _host          = args[2];
            if (args[3].Contains(';'))
            {
                _userName = args[3].Split(';')[0];
                _password = args[3].Split(';')[1];
            }
            else
            {
                Console.WriteLine("Wrong last argument format! It must be \"username;password\"");
                Console.ReadLine();
                Environment.Exit(0);
            }

            if (_configuration != "Release")
                Environment.Exit(0);

            FtpClient ftp = new FtpClient
            {
                UseSsl   = false,
                Host     = _host,
                UserName = _userName,
                Password = _password
            };

            List<FileStruct> binaries = new List<FileStruct>();
            List<FileStruct> docs     = new List<FileStruct>();

            var docFile = _outFile.Replace(Path.GetExtension(_outFile), ".xml");
            var compiledDocs = _outFile.Replace(Path.GetExtension(_outFile), "Docs.chm");

            Console.WriteLine("Checking server...");
            try
            {
                binaries = ftp.ListDirectory("software");
                docs = ftp.ListDirectory("devdocs");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                Environment.Exit(0);
            }

            string solution = GetSolution(_outFile).ToLower();
            if (!ListContains(binaries, solution))
            {
                try
                {
                    ftp.CreateDirectory("software/", solution);
                    Console.WriteLine($"Created directory \"{solution}\" for binaries");
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            if (File.Exists(compiledDocs))
            {
                if (!ListContains(docs, solution))
                {
                    try
                    {
                        ftp.CreateDirectory("devdocs/", solution);
                        Console.WriteLine($"Created directory \"{solution}\" for documentation");
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                } 
            }

            Console.WriteLine("Uploading binary to server...");
            try
            {
                ftp.UploadFile($"software/{solution}/", Path.GetFileName(_outFile));
                if (File.Exists(docFile))
                    ftp.UploadFile($"software/{solution}/", Path.GetFileName(docFile));
            }
            catch (Exception ex) { Console.WriteLine(ex.Message);}

            if (File.Exists(compiledDocs))
            {
                Console.WriteLine("Uploading documentation to server...");
                try
                {
                    ftp.UploadFile($"software/{solution}/", Path.GetFileName(compiledDocs));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
                Console.WriteLine("Documentation file not found");

            Console.WriteLine("Binaries uploaded successfully!");
            
            Console.ReadLine();
        }

        private static bool ListContains(List<FileStruct> files, string name)
        {
            if (files.Any(file => file.Name == name))
                return true;
            return false;
        }

        private static string GetSolution(string outFile)
        {
            return Path.GetFileNameWithoutExtension(outFile);
        }
    }
}