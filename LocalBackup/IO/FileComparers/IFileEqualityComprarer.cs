using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalBackup.IO.FileComparers
{
    public interface IFileEqualityComprarer : IFileInfoEqualityComparer
    {
        bool Equals(FileInfo f1, FileStream fs1, FileInfo f2, FileStream fs2);
    }
}
