﻿using System.IO;

namespace LocalFileSystem.IO.FileComparers
{
    public interface IFileEqualityComprarer : IFileInfoEqualityComparer
    {
        bool Equals(FileInfo f1, FileStream fs1, FileInfo f2, FileStream fs2);
    }
}
