using System;

namespace LocalBackup.IO
{
    public class FileSystemInfoErrorEventArgs : EventArgs
    {
        public FileSystemInfoErrorEventArgs(FileSystemInfoException ex)
        {
            Error = ex;
        }

        public FileSystemInfoException Error { get; }
    }
}
