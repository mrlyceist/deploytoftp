using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

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
        /// <param name="directory">Директория на сервере, содержимое которой будет запрошено</param>
        /// <returns>Массив файлов <seealso cref="FileStruct"/></returns>
        public List<FileStruct> ListDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                directory = "/";

            string content = string.Empty;
            
            _ftpRequest = Initialize(directory);
            _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            _ftpResponce = (FtpWebResponse)_ftpRequest.GetResponse();

            StreamReader reader = new StreamReader(_ftpResponce.GetResponseStream(), Encoding.ASCII);
            content = reader.ReadToEnd();
            reader.Close();
            _ftpResponce.Close();

            ResponseParser parser = new ResponseParser(content);
            return parser.Files;
        }

        /// <summary>
        /// Реализует команду LIST для получения с FTP-сервера подробного списка файлов
        /// </summary>
        /// <param name="directory">Каталог на сервере, содержимое которого будет запрошено</param>
        /// <returns>Строка-ответ, содержащая недесериализованную информацию о содержимом каталога.</returns>
        internal string ListDirectoryString(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                directory = "/";

            string content = string.Empty;
            
            _ftpRequest = Initialize(directory);
            _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            using (_ftpResponce = (FtpWebResponse)_ftpRequest.GetResponse())
            using (StreamReader reader = new StreamReader(_ftpResponce.GetResponseStream(), Encoding.ASCII))
            {
                content = reader.ReadToEnd();
            }
            return content;
        }

        /// <summary>
        /// Реализует метод протокола <code>FTP RETR</code> для загрузки файла с FTP-сервера.
        /// Сохраняет скачанный файл в папку программы.
        /// </summary>
        /// <param name="path">Путь к файлу на сервере</param>
        /// <param name="fileName">Имя файла</param>
        public void DownloadFile(string path, string fileName)
        {
            _ftpRequest = Initialize($"{path}{fileName}");
            _ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            FileStream downloadedFile = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            _ftpResponce = (FtpWebResponse) _ftpRequest.GetResponse();

            using (Stream responce = _ftpResponce.GetResponseStream())
            {
                byte[] buffer = new byte[1024];
                int size = 0;
                while ((size = responce.Read(buffer, 0, 1024)) > 0)
                    downloadedFile.Write(buffer, 0, size);
                _ftpResponce.Close();
                downloadedFile.Close();
            }
        }

        /// <summary>
        /// Реализует метод протокола <code>FTP STOR</code> для загрузки файла на FTP-сервер
        /// </summary>
        /// <param name="path">Путь к файлу на сервере</param>
        /// <param name="fileName">Имя загружаемого файла</param>
        public void UploadFile(string path, string fileName)
        {
            string shortName = Path.GetFileName(fileName);
            byte[] fileToBytes;
            using (FileStream uploadFile = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                _ftpRequest = Initialize($"{path}{shortName}");
                _ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                fileToBytes = new byte[uploadFile.Length];
                uploadFile.Read(fileToBytes, 0, fileToBytes.Length);
            }

            using (Stream writer = _ftpRequest.GetRequestStream())
            {
                writer.Write(fileToBytes, 0, fileToBytes.Length);
            }
        }

        /// <summary>
        /// Реализует метод протокола <code>FTP DELE</code> для удаления файла с FTP-сервера
        /// </summary>
        /// <param name="path">Путь до удаляемого файла на сервере</param>
        public void DeleteFile(string path)
        {
            _ftpRequest = Initialize(path);
            _ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            _ftpResponce = (FtpWebResponse) _ftpRequest.GetResponse();
            _ftpResponce.Close();
        }

        /// <summary>
        /// Реализует метод протокола <code>FTP MKD</code> для создания каталога на FTP-сервере
        /// </summary>
        /// <param name="path">Путь к создаваемому каталогу на сервере, включая завершающий слэш (/)</param>
        /// <param name="folderName">Имя создаваемого каталога</param>
        public void CreateDirectory(string path, string folderName)
        {
            _ftpRequest = Initialize($"{path}{folderName}");
            _ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

            _ftpResponce = (FtpWebResponse) _ftpRequest.GetResponse();
            _ftpResponce.Close();
        }

        /// <summary>
        /// Реализует метод протокола <code>FTP RMD</code> для удаления каталога с FTP-сервера
        /// </summary>
        /// <param name="path">Путь к удаляемому каталогу на сервере</param>
        public void RemoveDirectory(string path)
        {
            string fileName = path;
            _ftpRequest = Initialize(path);
            _ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;

            _ftpResponce = (FtpWebResponse) _ftpRequest.GetResponse();
            _ftpResponce.Close();
        }

        /// <summary>
        /// Инициализирует запрос к FTP-серверу, используя полученные реквизиты.
        /// </summary>
        /// <param name="path">Путь, передаваемый в запрос</param>
        /// <returns>Запрос к FTP-серверу</returns>
        private FtpWebRequest Initialize(string path)
        {
            var request = (FtpWebRequest) WebRequest.Create($"ftp://{Host}/{path}");
            request.Credentials = new NetworkCredential(UserName, Password);
            request.EnableSsl = UseSsl;
            return request;
        }
    }
}