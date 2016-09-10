using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("DeployToFtpTests")]
namespace DeployToFtp
{
    /// <summary>
    /// Класс-парсер, извлекающий информацию о файлах и директориях из строки-ответа FTP-сервера
    /// </summary>
    internal class DirectoryListParser
    {
        /// <summary>
        /// Список файлов
        /// </summary>
        private readonly List<FileStruct> _filesArray;

        /// <summary>
        /// Список всех файлов в распаршенном ответе сервера
        /// </summary>
        public FileStruct[] FullListing => _filesArray.ToArray();

        /// <summary>
        /// Разбирает массив unix-файлов, полученный в <seealso cref="DirectoryListParser">конструкторе</seealso> и вытаскивает из него файлы.
        /// </summary>
        public FileStruct[] FileList
        {
            get { return _filesArray.Where(@struct => !@struct.IsDirectory).ToArray(); }
        }

        /// <summary>
        /// Разбирает массив unix-файлов, полученный в <seealso cref="DirectoryListParser">конструкторе</seealso> и вытаскивает из него директории.
        /// </summary>
        public FileStruct[] DirectoryList
        {
            get { return _filesArray.Where(@struct => @struct.IsDirectory).ToArray(); }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:DeployToFtp.DirectoryListParser"/>.
        /// </summary>
        public DirectoryListParser(string filesString)
        {
            _filesArray = GetList(filesString);
        }

        /// <summary>
        /// Разбирает полученную в ответе от сервера строку на unix-файлы.
        /// </summary>
        /// <param name="filesString">Строка-ответ, полученная от сервера</param>
        /// <returns>Массив unix-файлов</returns>
        internal List<FileStruct> GetList(string filesString)
        {
            var filesArray = new List<FileStruct>();
            string[] fileData = filesString.Split('\n');
            FileListStyle directoryListStyle = GuessFileListStyle(fileData);
            foreach (string data in fileData)
            {
                if (directoryListStyle != FileListStyle.Unknown && data != string.Empty)
                {
                    FileStruct @struct = new FileStruct {Name = ".."};
                    switch (directoryListStyle)
                    {
                        case FileListStyle.UnixStyle:
                            @struct = ParseFileStructFromNix(data);
                            break;
                        case FileListStyle.WindowsStyle:
                            @struct = ParseFileStructFromWin(data);
                            break;
                    }
                    if (@struct.Name != string.Empty && @struct.Name != "." && @struct.Name != "..")
                        filesArray.Add(@struct);
                }
            }
            return filesArray;
        }

        /// <summary>
        /// Разбирает виндовую строку описания файла на свойства файла
        /// </summary>
        /// <param name="data">Строка описания файла в windows-формате</param>
        /// <returns>Файл со всеми его свойствами</returns>
        internal FileStruct ParseFileStructFromWin(string data)
        {
            FileStruct file = new FileStruct();
            string processString = data.Trim();
            string dateString = processString.Substring(0, 8);
            processString = ReduceString(processString, 8);
            string timeString = processString.Substring(0, 7);
            processString = ReduceString(processString, 7);
            //file.CreateTime = $"{dateString} {timeString}";

            if (processString.Substring(0, 5) == "<DIR>")
            {
                file.IsDirectory = true;
                processString = ReduceString(processString, 5);
            }
            else
            {
                string[] strs = processString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                processString = strs[1];
                file.IsDirectory = false;
            }

            file.Name = processString;
            return file;
        }

        /// <summary>
        /// Укорачивает строку сначала и тримит ее.
        /// </summary>
        /// <param name="processString">Строка, которую надо укоротить</param>
        /// <param name="i">Количество символов от начала строки, на которые будем укорачивать</param>
        /// <returns>Укороченная на i символов от начала строка</returns>
        internal static string ReduceString(string processString, int i)
        {
            return processString.Substring(i, processString.Length - i).Trim();
        }

        /// <summary>
        /// Разбирает никсовую строку описания файла на свойства файла
        /// </summary>
        /// <param name="data">Строка описания файла в unix-формате</param>
        /// <returns>Файл со всеми его свойствами</returns>
        internal FileStruct ParseFileStructFromNix(string data)
        {
            FileStruct file = new FileStruct();
            if (data[0]=='-'||data[0]=='d')
            {
                string processString = data.Trim();
                file.Flags = processString.Substring(0, 9);
                file.IsDirectory = (file.Flags[0] == 'd');
                processString = processString.Substring(11).Trim();
                CutString(ref processString, ' ', 0);
                file.Owner = CutString(ref processString, ' ', 0);
                //file.CreateTime = GetCreateTime(data);
                //int fileNameIndex = data.IndexOf(file.CreateTime) + file.CreateTime.Length;
                //file.Name = data.Substring(fileNameIndex).Trim();
            }
            else
                file.Name = string.Empty;
            return file;
        }

        /// <summary>
        /// Разбирает полученную строку на компоненты и вытаскивает из них дату и время создания файла
        /// </summary>
        /// <param name="data">Строка описания файла в unix-формате</param>
        /// <returns>Строка, содержащая дату и время создания файла</returns>
        internal string GetCreateTime(string data)
        {
            const string month = @"(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)";
            const string space = @"(\040)+";
            const string day = "([0-9]|[1-3][0-9])";
            const string year = "[1-2][0-9]{3}";
            const string time = "[0-9]{1,2}:[0-9]{2}";
            Regex dateTime = new Regex($"{month}{space}{day}{space}({year}|{time})", RegexOptions.IgnoreCase);
            Match match = dateTime.Match(data);
            return match.Value;
        }

        internal string CutString(ref string s, char c, int startIndex)
        {
            int pos1 = s.IndexOf(c, startIndex);
            string resultString = s.Substring(0, pos1);
            s = s.Substring(pos1).Trim();
            return resultString;
        }

        /// <summary>
        /// Разбирает входной массив на строки и начало каждой сравнивает с регулярным выражением.
        /// Исходя из результатов сравнения делает вывод о том, на какой системе базируется FTP-сервер.
        /// </summary>
        /// <param name="fileData">Массив строк, на который методом <seealso cref="GetList"/> будет разобран ответ сервера</param>
        /// <returns>Тип файловой системы FTP-сервера</returns>
        internal FileListStyle GuessFileListStyle(string[] fileData)
        {
            Regex nixStyle = new Regex(@"(-|d)((-|r)(-|w)(-|x)){3}");
            Regex winStyle = new Regex(@"[0-9]{2}-[0-9]{2}-[0-9]{2}");
            foreach (string unixFile in fileData)
            {
                if (unixFile.Length > 10 && nixStyle.IsMatch(unixFile.Substring(0, 10)))
                    return FileListStyle.UnixStyle;
                if (unixFile.Length > 8 && winStyle.IsMatch(unixFile.Substring(0, 8)))
                    return FileListStyle.WindowsStyle;
            }
            return FileListStyle.Unknown;
        }
    }
}