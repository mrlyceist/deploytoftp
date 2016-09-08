using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DeployToFtp
{
    /// <summary>
    /// Класс, реализующий простой FTP-клиент.
    /// </summary>
    class FtpClient
    {
        /// <summary>
        /// Внутренний объект FTP-запроса
        /// </summary>
        private FtpWebRequest _ftpRequest;
        /// <summary>
        /// Внутренний объект ответа FTP-сервера
        /// </summary>
        private FtpWebResponse _ftpResponce;

        /// <summary>
        /// Адрес FTP-сервера
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Имя пользователя для подключения к FTP-серверу
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Пароль для подключения к FTP-серверу
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Флаг, отвечающий за использование FTP-сервером SSL-шифрования
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Реализует команду <code>LIST</code> для плолучения с FTP-сервера подробного списка файлов.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public FileStruct[] ListDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                directory = "/";

            string content = string.Empty;

            _ftpRequest = (FtpWebRequest)WebRequest.Create($"ftp://{Host}{directory}");
            _ftpRequest.Credentials = new NetworkCredential(UserName, Password);
            _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            _ftpRequest.EnableSsl = UseSsl;

            _ftpResponce = (FtpWebResponse)_ftpRequest.GetResponse();

            StreamReader reader = new StreamReader(_ftpResponce.GetResponseStream(), Encoding.ASCII);
            content = reader.ReadToEnd();
            reader.Close();
            _ftpResponce.Close();

            DirectoryListParser parser = new DirectoryListParser(content);
            return parser.FullListing;
        }

        
    }
}