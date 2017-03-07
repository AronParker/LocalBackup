using System;
using System.IO;

namespace LocalBackup.IO.Errors
{
    public class FileError : FileSystemError
    {
        private FileInfo _file;

        public FileError(FileInfo file, Exception ex) : base(ex)
        {
            _file = file;
        }

        public override string Name => "File error";
        public override FileSystemItemType Type => FileSystemItemType.FileError;
        public override string FileName => _file.Name;
        public override string FullPath => _file.FullName;
        public FileInfo File => _file;
    }
}
