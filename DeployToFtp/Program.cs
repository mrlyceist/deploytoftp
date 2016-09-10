using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeployToFtp
{
    class Program
    {
        private static string _userName = "bit";
        private static string _password = "18071978";
        private static string _host = "ursa.ooobit.com";///software";
        //private static string _host = "ursa.ooobit.com/software";

        static void Main(string[] args)
        {
            FtpClient ftp = new FtpClient();
            ftp.UseSsl = false;
            ftp.Host = _host;
            //ftp.Host = "192.168.1.2";
            ftp.UserName = _userName;
            ftp.Password = _password;

            string responce = ftp.ListDirectoryString(string.Empty);
            //string responce = ftp.ListDirectoryString("/dotNet");

            //FileStruct[] files = ftp.ListDirectory(string.Empty);

            //Console.WriteLine(files.Length);
            //foreach (var file in files)
            //{
            //var str = $"{file.Name}";
            //Console.WriteLine($"{file.Name}");//, dir:{file.IsDirectory}, {file.CreateTime}, {file.Owner}");
            //}
            Console.WriteLine(responce);
            Console.ReadLine();
        }
    }
}