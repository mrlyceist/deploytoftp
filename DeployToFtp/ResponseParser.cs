using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeployToFtp
{
    public class ResponseParser
    {
        private string _nixResponce;
        
        /// <summary>
        /// Разбирает входной массив на строки и начало каждой сравнивает с регулярным выражением.
        /// Исходя из результатов сравнения делает вывод о том, на какой системе базируется FTP-сервер.
        /// </summary>
        /// <param name="fileData">Массив строк, на который методом <seealso cref="GetList"/> будет разобран ответ сервера</param>
        /// <returns>Тип файловой системы FTP-сервера</returns>
        internal static FileListStyle SwitchFileSystem(string[] response)
        {
            Regex nixStyle = new Regex(@"(-|d)((-|r)(-|w)(-|x)){3}");
            Regex winStyle = new Regex(@"[0-9]{2}-[0-9]{2}-[0-9]{2}");
            foreach (string unixFile in response)
            {
                if (unixFile.Length > 10 && nixStyle.IsMatch(unixFile.Substring(0, 10)))
                    return FileListStyle.UnixStyle;
                if (unixFile.Length > 8 && winStyle.IsMatch(unixFile.Substring(0, 8)))
                    return FileListStyle.WindowsStyle;
            }
            return FileListStyle.Unknown;
        }

        /// <summary>
        /// Вытаскивает из строки описания юникс-файла в nix-формате флаги доступа в виде строки
        /// </summary>
        /// <param name="nixFile">Строка-описание юникс-файла в nix-формате</param>
        /// <returns>Строка флагов доступа в формате "rwxrwxrwx"</returns>
        internal static string GetFlags(string nixFile)
        {
            return nixFile.Substring(1, 9);
        }

        /// <summary>
        /// Вытаскивает дату последней модификации файла из строки описания юникс-файла в nix-формате.
        /// В случае, если файл был создан не в текущем году - время создания будет 00:00
        /// </summary>
        /// <param name="nixFile">Строка-описание юникс-файла в nix-формате</param>
        /// <returns>Строка времени в формате "YYMMDD-HH:MM"</returns>
        internal static string GetCreateTime(string nixFile)
        {
            string timeString = nixFile.Substring(43, 12).Trim();
            Regex rMonth = new Regex(@"(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)", RegexOptions.IgnoreCase);
            Regex rYear  = new Regex(@"[1-2][0-9]{3}");
            Regex rTime  = new Regex(@"[0-9]{1,2}:[0-9]{2}");

            var sMonth = rMonth.Match(timeString).Value;
            string month;
            switch (sMonth.ToLower())
            {
                case "jan":
                    month = "01";
                    break;
                case "feb":
                    month = "02";
                    break;
                case "mar":
                    month = "03";
                    break;
                case "apr":
                    month = "04";
                    break;
                case "may":
                    month = "05";
                    break;
                case "jun":
                    month = "06";
                    break;
                case "jul":
                    month = "07";
                    break;
                case "aug":
                    month = "08";
                    break;
                case "sep":
                    month = "09";
                    break;
                case "oct":
                    month = "10";
                    break;
                case "nov":
                    month = "11";
                    break;
                case "dec":
                    month = "12";
                    break;
                default:
                    month = "00";
                    break;
            }
            var day = timeString.Substring(4, 2);
            string year;
            string time;
            if (rYear.IsMatch(timeString.Substring(6)))
            {
                year = rYear.Match(timeString.Substring(6)).Value;
                time = "00:00";
            }
            else
            {
                year = DateTime.Now.Year.ToString();
                time = rTime.Match(timeString.Substring(6)).Value;
            }

            return string.Concat(year.Substring(2), month, day, "-", time);
        }

        internal static string GetName(string nixFile)
        {
            return nixFile.Substring(nixFile.Trim().LastIndexOf(' ')+1);
        }

        internal static FileStruct ParseFile(string nixFile)
        {
            var file = new FileStruct()
            {
                Name = GetName(nixFile),
                Flags = GetFlags(nixFile),
                CreateTime = GetCreateTime(nixFile),
                IsDirectory = nixFile[0] == 'd'
            };
            return file;
        }
    }
}