using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalBackup.IO.FileComparers
{
    public interface IFileInfoEqualityComparer
    {
        bool Equals(FileInfo f1, FileInfo f2);
    }
}
