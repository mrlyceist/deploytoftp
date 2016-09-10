using System;

namespace DeployToFtp
{
    public struct FileStruct
    {
        public string Flags;
        public string Owner;
        public bool IsDirectory;
        public DateTime CreateTime;
        public string Name;
    }
}