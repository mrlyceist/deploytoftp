using System;

namespace DeployToFtp
{
    /// <summary>
    /// Структура для хранения аттрибутов файла
    /// </summary>
    public struct FileStruct
    {
        public string Flags;
        public string Owner;
        public bool IsDirectory;
        public DateTime CreateTime;
        public string Name;
    }
}