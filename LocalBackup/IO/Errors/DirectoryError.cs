using System;
using System.IO;

namespace LocalBackup.IO.Errors
{
    public class DirectoryError : FileSystemError
    {
        private DirectoryInfo _dir;

        public DirectoryError(DirectoryInfo dir, Exception ex) : base(ex)
        {
            _dir = dir;
        }

        public override string Name => "Directory error";
        public override FileSystemItemType Type => FileSystemItemType.DirectoryError;
        public override string FileName => _dir.Name;
        public override string FullPath => _dir.FullName;
        public DirectoryInfo Directory => _dir;
    }
}
