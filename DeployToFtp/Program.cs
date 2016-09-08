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
        //private static string _host = "ftp://ursa.ooobit.com";///software";
        private static string _host = "ftp://ursa.ooobit.com/software";

        static void Main(string[] args)
        {
            var ftpRequest = (FtpWebRequest)WebRequest.Create($"{_host}/wat");
            ftpRequest.Credentials = new NetworkCredential(_userName, _password);
            //ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            ftpRequest.EnableSsl = false;

            var ftpResponce = (FtpWebResponse)ftpRequest.GetResponse();
            ftpResponce.Close();
            string cont = string.Empty;

            ftpRequest = (FtpWebRequest)WebRequest.Create(_host);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            ftpResponce = (FtpWebResponse)ftpRequest.GetResponse();
            StreamReader reader = new StreamReader(ftpResponce.GetResponseStream(), Encoding.ASCII);
            cont = reader.ReadToEnd();
            reader.Close();
            ftpResponce.Close();

            Console.WriteLine(cont);
            Console.ReadLine();
        }
    }
}