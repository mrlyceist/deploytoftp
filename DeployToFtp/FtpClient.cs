using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeployToFtp
{
    class FtpClient
    {
        private FtpWebRequest _ftpRequest;
        private FtpWebResponse _ftpResponce;

        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }

        public FileStruct[] ListDirectory(string directory)
        {
            if (directory == null || directory == string.Empty)
                directory = "/";

            _ftpRequest = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{directory}");
            _ftpRequest.Credentials = new NetworkCredential(UserName, Password);
            _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            _ftpRequest.EnableSsl = UseSsl;

            _ftpResponce = (FtpWebResponse)_ftpRequest.GetResponse();

            string content = string.Empty;

            StreamReader reader = new StreamReader(_ftpResponce.GetResponseStream(), Encoding.ASCII);
            content = reader.ReadToEnd();
            reader.Close();
            _ftpResponce.Close();

            DirectoryListParser parser = new DirectoryListParser(content);
            return parser.FullListing;
        }

    }

    struct FileStruct
    {
        public string Flags;
        public string Owner;
        public bool IsDirectory;
        public string CreateTime;
        public string Name;
    }
}
