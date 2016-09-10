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
            if (!string.IsNullOrEmpty(args[0]))
                _configuration = args[0];
            else
            {
                Console.WriteLine("Lack of arguments!");
                return;
            }
            if (!string.IsNullOrEmpty(args[1]))
                _outFile = args[1];
            else
            {
                Console.WriteLine("Lack of arguments!");
                return;
            }
            if (!string.IsNullOrEmpty(args[2]))
                _host = args[2];
            else
            {
                Console.WriteLine("Lack of arguments!");
                return;
            }
            if (!string.IsNullOrEmpty(args[3]))
            {
                if (args[3].Contains(';'))
                {
                    _userName = args[3].Split(';')[0];
                    _password = args[3].Split(';')[1];
                }
            }
            else
            {
                Console.WriteLine("Lack of arguments!");
                return;
            }
            FtpClient ftp = new FtpClient
            {
                UseSsl   = false,
                Host     = _host,
                UserName = _userName,
                Password = _password
            };

            List<FileStruct> binaries = new List<FileStruct>();
            List<FileStruct> docs     = new List<FileStruct>();

            Console.WriteLine("Checking server...");
            try
            {
                binaries = ftp.ListDirectory("software");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(0);
            }

            string solution = GetSolution(_outFile);
            bool flag = false;
            if (!ListContains(binaries, solution))
            {
                try
                {
                    ftp.CreateDirectory("software/", solution);
                    Console.WriteLine($"Created directory \"{solution}\" on server");
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }

            Console.WriteLine("Uploading binary to server");
            try
            {
                ftp.UploadFile($"software/{solution}/", Path.GetFileName(_outFile));
            }
            catch (Exception ex) { Console.WriteLine(ex.Message);}

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