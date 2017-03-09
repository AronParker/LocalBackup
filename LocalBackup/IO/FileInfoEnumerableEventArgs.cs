using System;
using System.Collections.Generic;
using System.IO;

namespace LocalBackup.IO
{
    public class FileInfoEnumerableEventArgs : EventArgs
    {
        public FileInfoEnumerableEventArgs(IEnumerable<FileInfo> files)
        {
            Files = files;
        }

        public IEnumerable<FileInfo> Files { get; }
    }
}
