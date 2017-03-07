using System;
using System.IO;

namespace LocalBackup.IO.FileComparers
{
    public class NTFSFileComparer : IFileInfoEqualityComparer
    {
        public bool Equals(FileInfo f1, FileInfo f2)
        {
            if (f1 == null)
                throw new ArgumentNullException(nameof(f1));
            if (f2 == null)
                throw new ArgumentNullException(nameof(f2));

            return f1.Length == f2.Length && f1.LastWriteTimeUtc == f2.LastWriteTimeUtc;
        }
    }
}
