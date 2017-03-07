using System;

namespace LocalBackup.IO.Errors
{
    public abstract class FileSystemError : FileSystemItem
    {
        private Exception _ex;

        public FileSystemError(Exception ex)
        {
            _ex = ex;
        }

        public Exception Error => _ex;
    }
}
