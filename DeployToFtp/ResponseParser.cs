using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("DeployToFtpTests")]
namespace DeployToFtp
{
    /// <summary>
    /// Класс-парсер, разбирающий ответ сервера на файлы
    /// </summary>
    public class ResponseParser
    {
        private List<FileStruct> _files;

        /// <summary>
        /// Парсит ответ от сервера и вытаскивает из него информацию о файлах
        /// </summary>
        /// <param name="nixResponse">Строка ответа от сервера</param>
        public ResponseParser(string nixResponse)
        {
            _files = new List<FileStruct>();
            foreach (string nFile in nixResponse.Trim('\n').Split('\n'))
            {
                if (!string.IsNullOrEmpty(nFile))
                    _files.Add(ParseFile(nFile));
            }
        }

        /// <summary>
        /// Список всех файлов, полученных в ответе от сервера.
        /// </summary>
        public List<FileStruct> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        /// <summary>
        /// Cравнивает начало строки описания юникс-файла с регулярным выражением.
        /// Исходя из результатов сравнения делает вывод о том, на какой системе базируется FTP-сервер.
        /// </summary>
        /// <param name="unixFile">Строка описания юникс-файла</param>
        /// <returns>Тип файловой системы FTP-сервера</returns>
        internal static FileSystem SwitchFileSystem(string unixFile)
        {
            Regex nixStyle = new Regex(@"(-|d)((-|r)(-|w)(-|x)){3}");
            Regex winStyle = new Regex(@"[0-9]{2}-[0-9]{2}-[0-9]{2}");
            if (unixFile.Length > 10 && nixStyle.IsMatch(unixFile.Substring(0, 10)))
                return FileSystem.UnixStyle;
            if (unixFile.Length > 8 && winStyle.IsMatch(unixFile.Substring(0, 8)))
                return FileSystem.WindowsStyle;
            return FileSystem.Unknown;
        }

        /// <summary>
        /// Вытаскивает из строки описания юникс-файла флаги доступа в виде строки
        /// </summary>
        /// <param name="nixFile">Строка-описание юникс-файла</param>
        /// <returns>Строка флагов доступа в формате "rwxrwxrwx" или "notdeterm" (если файл пришел с Windows)</returns>
        internal static string GetFlags(string nixFile)
        {
            if (SwitchFileSystem(nixFile) == FileSystem.UnixStyle)
                return nixFile.Substring(1, 9);
            return "notdeterm";
        }

        /// <summary>
        /// Вытаскивает дату последней модификации файла из строки описания юникс-файла.
        /// В случае, если файл был создан не в текущем году - время создания будет 00:00
        /// </summary>
        /// <param name="nixFile">Строка-описание юникс-файла</param>
        /// <returns>Время создания файла</returns>
        internal static DateTime GetCreateTime(string nixFile)
        {
            int month   = 0;
            int day     = 0;
            int year    = 0;
            int hours   = 0;
            int minutes = 0;
            if (SwitchFileSystem(nixFile)==FileSystem.UnixStyle)
            {
                string timeString = nixFile.Substring(43, 12).Trim();
                Regex rMonth = new Regex(@"(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)", RegexOptions.IgnoreCase);
                Regex rYear  = new Regex(@"[1-2][0-9]{3}");
                Regex rTime  = new Regex(@"[0-9]{1,2}:[0-9]{2}");

                var sMonth = rMonth.Match(timeString).Value;
                switch (sMonth.ToLower())
                {
                    case "jan":
                        month = 01;
                        break;
                    case "feb":
                        month = 02;
                        break;
                    case "mar":
                        month = 03;
                        break;
                    case "apr":
                        month = 04;
                        break;
                    case "may":
                        month = 05;
                        break;
                    case "jun":
                        month = 06;
                        break;
                    case "jul":
                        month = 07;
                        break;
                    case "aug":
                        month = 08;
                        break;
                    case "sep":
                        month = 09;
                        break;
                    case "oct":
                        month = 10;
                        break;
                    case "nov":
                        month = 11;
                        break;
                    case "dec":
                        month = 12;
                        break;
                    default:
                        month = 00;
                        break;
                }
                day = int.Parse(timeString.Substring(4, 2));
                if (rYear.IsMatch(timeString.Substring(6)))
                {
                    year    = int.Parse(rYear.Match(timeString.Substring(6)).Value);
                    hours   = 0;
                    minutes = 0;
                }
                else
                {
                    var time = rTime.Match(timeString.Substring(6)).Value;
                    year     = int.Parse(DateTime.Now.Year.ToString());
                    hours    = int.Parse(time.Substring(0, 2));
                    minutes  = int.Parse(time.Substring(3, 2));
                } 
            }
            if (SwitchFileSystem(nixFile)==FileSystem.WindowsStyle)
            {
                month   = int.Parse(nixFile.Substring(0, 2));
                day     = int.Parse(nixFile.Substring(3, 2));
                year    = 2000 + int.Parse(nixFile.Substring(6, 2));
                bool pm = nixFile.Substring(15, 2).ToLower() == "pm";
                hours   = int.Parse(nixFile.Substring(10, 2));
                minutes = int.Parse(nixFile.Substring(13, 2));
                if (pm) hours += 12;
            }
            
            return new DateTime(year, month, day, hours, minutes, 0);
        }

        /// <summary>
        /// Вытаскивает из строки описания юникс-файла имя файла
        /// </summary>
        /// <param name="nixFile">Строка описания юникс-файла</param>
        /// <returns>Имя файла</returns>
        internal static string GetName(string nixFile)
        {
            return nixFile.Substring(nixFile.Trim().LastIndexOf(' ') + 1).Trim('\r', '\n');
        }

        /// <summary>
        /// Разбирает строку описания юникс-файла на аттрибуты.
        /// </summary>
        /// <param name="nixFile">Строка описания юникс-файла</param>
        /// <returns>Файл в виде сруктуры <seealso cref="FileStruct"/></returns>
        internal static FileStruct ParseFile(string nixFile)
        {
            var file = new FileStruct()
            {
                Name        = GetName(nixFile),
                Flags       = GetFlags(nixFile),
                CreateTime  = GetCreateTime(nixFile),
                IsDirectory = IsItDirectory(nixFile)
            };
            return file;
        }

        /// <summary>
        /// Вытаскивает из строки описания юникс-файла признак директории
        /// </summary>
        /// <param name="nixFile">Строка описания юникс-файла</param>
        /// <returns>Истина, если юникс-файл - директория, иначе - Ложь</returns>
        public static bool IsItDirectory(string nixFile)
        {
            if (SwitchFileSystem(nixFile) == FileSystem.UnixStyle)
                return nixFile[0] == 'd';
            return Regex.IsMatch(nixFile, @"(<DIR>)", RegexOptions.CultureInvariant);
        }
    }
}